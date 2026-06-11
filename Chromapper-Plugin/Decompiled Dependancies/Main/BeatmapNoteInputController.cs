using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BeatmapNoteInputController : BeatmapInputController<NoteContainer>, CMInput.INoteObjectsActions
{
	[FormerlySerializedAs("noteAppearanceSO")]
	[SerializeField]
	private NoteAppearanceSO noteAppearanceSo;

	[SerializeField]
	private ArcAppearanceSO arcAppearanceSo;

	[SerializeField]
	private ChainAppearanceSO chainAppearanceSo;

	public bool QuickModificationActive;

	private readonly Dictionary<int, int> cutDirectionMovedBackward = new Dictionary<int, int>
	{
		{ 8, 8 },
		{ 6, 1 },
		{ 2, 6 },
		{ 4, 2 },
		{ 0, 4 },
		{ 5, 0 },
		{ 3, 5 },
		{ 7, 3 },
		{ 1, 7 },
		{ 9, 9 }
	};

	private readonly Dictionary<int, int> cutDirectionMovedForward = new Dictionary<int, int>
	{
		{ 8, 8 },
		{ 1, 6 },
		{ 6, 2 },
		{ 2, 4 },
		{ 4, 0 },
		{ 0, 5 },
		{ 5, 3 },
		{ 3, 7 },
		{ 7, 1 },
		{ 9, 9 }
	};

	public void OnInvertNoteColors(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && KeybindsController.IsMouseInWindow && context.performed)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging)
			{
				InvertNote(firstObject);
			}
		}
	}

	public void OnQuickDirectionModifier(InputAction.CallbackContext context)
	{
		QuickModificationActive = context.performed;
	}

	public void OnUpdateNoteDirection(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && context.performed)
		{
			bool shiftForward = context.ReadValue<float>() > 0f;
			RaycastFirstObject(out var firstObject);
			if (firstObject != null)
			{
				UpdateNoteDirection(firstObject, shiftForward);
			}
		}
	}

	public void OnUpdateNotePreciseDirection(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && context.performed)
		{
			bool shiftForward = context.ReadValue<float>() > 0f;
			RaycastFirstObject(out var firstObject);
			if (firstObject != null)
			{
				UpdateNotePreciseDirection(firstObject, shiftForward);
			}
		}
	}

	public void InvertNote(NoteContainer note)
	{
		if (note.NoteData.Type != 3)
		{
			BaseObject baseObject = BeatmapFactory.Clone(note.ObjectData);
			int type = ((note.NoteData.Type == 0) ? 1 : 0);
			note.NoteData.Type = type;
			noteAppearanceSo.SetNoteAppearance(note);
			NoteGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note);
			collectionForType.RefreshSpecialAngles(note.ObjectData, objectWasSpawned: false, isNatural: false);
			collectionForType.RefreshSpecialAngles(baseObject, objectWasSpawned: false, isNatural: false);
			List<BeatmapAction> list = new List<BeatmapAction>
			{
				new BeatmapObjectModifiedAction(note.ObjectData, note.ObjectData, baseObject)
			};
			InvertAttachedSliders(note, list);
			BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: true, "Note inversion"));
		}
	}

	private void InvertAttachedSliders(NoteContainer note, ICollection<BeatmapAction> actions)
	{
		BaseNote noteData = note.NoteData;
		float epsilon = BeatmapObjectContainerCollection.Epsilon;
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer in BeatmapObjectContainerCollection.GetCollectionForType<ArcGridContainer>(ObjectType.Arc).LoadedContainers)
		{
			BaseArc baseArc = loadedContainer.Key as BaseArc;
			bool num = Mathf.Abs(baseArc.JsonTime - noteData.JsonTime) < epsilon && baseArc.GetPosition() == noteData.GetPosition();
			bool flag = Mathf.Abs(baseArc.TailJsonTime - noteData.JsonTime) < epsilon && baseArc.GetTailPosition() == noteData.GetPosition();
			if (num || flag)
			{
				BaseArc originalData = BeatmapFactory.Clone(baseArc);
				baseArc.Color = noteData.Color;
				arcAppearanceSo.SetArcAppearance(loadedContainer.Value as ArcContainer);
				actions.Add(new BeatmapObjectModifiedAction(baseArc, baseArc, originalData));
			}
		}
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer2 in BeatmapObjectContainerCollection.GetCollectionForType<ChainGridContainer>(ObjectType.Chain).LoadedContainers)
		{
			BaseChain baseChain = loadedContainer2.Key as BaseChain;
			if (Mathf.Abs(baseChain.JsonTime - noteData.JsonTime) < epsilon && baseChain.GetPosition() == noteData.GetPosition())
			{
				BaseChain originalData2 = BeatmapFactory.Clone(baseChain);
				baseChain.Color = noteData.Color;
				chainAppearanceSo.SetChainAppearance(loadedContainer2.Value as ChainContainer);
				actions.Add(new BeatmapObjectModifiedAction(baseChain, baseChain, originalData2));
			}
		}
	}

	public void UpdateNoteDirection(NoteContainer note, bool shiftForward)
	{
		BaseObject originalData = BeatmapFactory.Clone(note.ObjectData);
		note.NoteData.CutDirection = ((shiftForward ^ Settings.Instance.InvertScrollNoteAngle) ? cutDirectionMovedBackward : cutDirectionMovedForward)[note.NoteData.CutDirection];
		BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note).RefreshSpecialAngles(note.ObjectData, objectWasSpawned: false, isNatural: false);
		List<BeatmapAction> list = new List<BeatmapAction>
		{
			new BeatmapObjectModifiedAction(note.ObjectData, note.ObjectData, originalData, "Update Note Direction", keepSelection: false, ActionMergeType.NoteDirectionChange)
		};
		CommonNotePlacement.UpdateAttachedSlidersDirection(note.NoteData, list);
		if (list.Count > 1)
		{
			BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: true, "Update Note Direction", ActionMergeType.NoteDirectionChange));
		}
		else
		{
			BeatmapActionContainer.AddAction(list[0]);
		}
	}

	public void UpdateNotePreciseDirection(NoteContainer note, bool shiftForward)
	{
		BaseObject originalData = BeatmapFactory.Clone(note.ObjectData);
		if (Settings.Instance.MapVersion >= 3)
		{
			note.NoteData.AngleOffset += ((shiftForward ^ Settings.Instance.InvertScrollNoteAngle) ? 5 : (-5));
			BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note).RefreshSpecialAngles(note.ObjectData, objectWasSpawned: false, isNatural: false);
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(note.ObjectData, note.ObjectData, originalData, "No comment.", keepSelection: false, ActionMergeType.NotePreciseDirectionTweak));
		}
	}
}
