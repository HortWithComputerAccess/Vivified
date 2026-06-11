using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcPlacement : PlacementController<BaseArc, ArcContainer, ArcGridContainer>, CMInput.IArcPlacementActions
{
	private static HashSet<BaseObject> SelectedObjects => SelectionController.SelectedObjects;

	public void OnSpawnArc(InputAction.CallbackContext context)
	{
		if (!context.performed && !context.canceled)
		{
			SpawnArcsFromSelection();
		}
	}

	public int SpawnArcsFromSelection()
	{
		List<BaseNote> list = SelectedObjects.Where(IsColorNote).Cast<BaseNote>().ToList();
		list.Sort((BaseNote a, BaseNote b) => a.JsonTime.CompareTo(b.JsonTime));
		if (Settings.Instance.MapVersion == 2 && list.Count > 1)
		{
			PersistentUI.Instance.ShowDialogBox("Arc placement is not supported in v2 format.\nConvert map to v3 to place arcs.", null, PersistentUI.DialogBoxPresetType.Ok);
			return 0;
		}
		List<BaseArc> list2 = new List<BaseArc>();
		List<BaseNote> list3 = list.Where((BaseNote n) => n.Color == 0).ToList();
		List<BaseNote> list4 = list.Where((BaseNote n) => n.Color == 1).ToList();
		for (int num = 1; num < list3.Count; num++)
		{
			list2.Add(CreateArcData(list3[num - 1], list3[num]));
		}
		for (int num2 = 1; num2 < list4.Count; num2++)
		{
			list2.Add(CreateArcData(list4[num2 - 1], list4[num2]));
		}
		if (list2.Count > 0)
		{
			foreach (BaseArc item in list2)
			{
				objectContainerCollection.SpawnObject(item, removeConflicting: false);
			}
			SelectionController.DeselectAll();
			SelectionController.SelectedObjects = new HashSet<BaseObject>(list2);
			SelectionController.SelectionChangedEvent?.Invoke();
			SelectionController.RefreshSelectionMaterial(triggersAction: false);
			BeatmapActionContainer.AddAction(new BeatmapObjectPlacementAction(list2.ToArray(), new List<BaseObject>(), $"Placed {list2.Count} arcs"));
		}
		return list2.Count;
	}

	public static bool IsColorNote(BaseObject o)
	{
		if (o is BaseNote baseNote)
		{
			return baseNote.Type != 3;
		}
		return false;
	}

	public override BaseArc GenerateOriginalData()
	{
		return new BaseArc();
	}

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, "Placed an arc.");
	}

	public BaseArc CreateArcData(BaseNote head, BaseNote tail)
	{
		if (head.JsonTime > tail.JsonTime)
		{
			BaseNote baseNote = tail;
			BaseNote baseNote2 = head;
			head = baseNote;
			tail = baseNote2;
		}
		return new BaseArc(head, tail);
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 transformedPoint)
	{
	}

	public override void TransferQueuedToDraggedObject(ref BaseArc dragged, BaseArc queued)
	{
	}
}
