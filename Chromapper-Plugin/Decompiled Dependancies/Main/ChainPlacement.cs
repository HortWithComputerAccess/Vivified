using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ChainPlacement : PlacementController<BaseChain, ChainContainer, ChainGridContainer>, CMInput.IChainPlacementActions
{
	[SerializeField]
	private SelectionController selectionController;

	[FormerlySerializedAs("notesContainer")]
	[SerializeField]
	private NoteGridContainer noteGridContainer;

	private static HashSet<BaseObject> SelectedObjects => SelectionController.SelectedObjects;

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, "Placed a chain.");
	}

	public override BaseChain GenerateOriginalData()
	{
		return new BaseChain();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 transformedPoint)
	{
		throw new NotImplementedException();
	}

	public void OnSpawnChain(InputAction.CallbackContext context)
	{
		if (!context.performed && !context.canceled)
		{
			SpawnChainFromSelection();
		}
	}

	public int SpawnChainFromSelection()
	{
		List<BaseNote> list = SelectedObjects.Where(IsColorNote).Cast<BaseNote>().ToList();
		list.Sort((BaseNote a, BaseNote b) => a.JsonTime.CompareTo(b.JsonTime));
		if (Settings.Instance.MapVersion == 2 && list.Count > 1)
		{
			PersistentUI.Instance.ShowDialogBox("Chain placement is not supported in v2 format.\nConvert map to v3 to place chains.", null, PersistentUI.DialogBoxPresetType.Ok);
			return 0;
		}
		List<BaseNote> list2 = new List<BaseNote>();
		List<BaseChain> list3 = new List<BaseChain>();
		List<BaseNote> list4 = list.Where((BaseNote n) => n.Color == 0).ToList();
		List<BaseNote> list5 = list.Where((BaseNote n) => n.Color == 1).ToList();
		for (int num = 1; num < list4.Count; num++)
		{
			if (TryCreateChainData(list4[num - 1], list4[num], out var chain, out var tailNote))
			{
				list2.Add(tailNote);
				list3.Add(chain);
			}
		}
		for (int num2 = 1; num2 < list5.Count; num2++)
		{
			if (TryCreateChainData(list5[num2 - 1], list5[num2], out var chain2, out var tailNote2))
			{
				list2.Add(tailNote2);
				list3.Add(chain2);
			}
		}
		if (list3.Count > 0)
		{
			SelectionController.DeselectAll();
			SelectionController.SelectedObjects = new HashSet<BaseObject>(list2);
			selectionController.Delete(triggersAction: false);
			foreach (BaseChain item in list3)
			{
				objectContainerCollection.SpawnObject(item, removeConflicting: false);
			}
			SelectionController.SelectedObjects = new HashSet<BaseObject>(list3);
			SelectionController.SelectionChangedEvent?.Invoke();
			SelectionController.RefreshSelectionMaterial(triggersAction: false);
			BeatmapActionContainer.AddAction(new BeatmapObjectPlacementAction(list3.ToArray(), list2, $"Placed {list3.Count} chains"));
		}
		return list3.Count;
	}

	private static bool IsColorNote(BaseObject o)
	{
		return ArcPlacement.IsColorNote(o);
	}

	public bool TryCreateChainData(BaseNote head, BaseNote tail, out BaseChain chain, out BaseNote tailNote)
	{
		if (head.JsonTime > tail.JsonTime)
		{
			BaseNote baseNote = tail;
			BaseNote baseNote2 = head;
			head = baseNote;
			tail = baseNote2;
		}
		tailNote = tail;
		if (head.CutDirection == 8)
		{
			chain = null;
			return false;
		}
		chain = new BaseChain(head, tail);
		return true;
	}

	public override void TransferQueuedToDraggedObject(ref BaseChain dragged, BaseChain queued)
	{
	}
}
