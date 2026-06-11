using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotePlacement : PlacementController<BaseNote, NoteContainer, NoteGridContainer>, CMInput.INotePlacementActions
{
	private const int upKey = 0;

	private const int leftKey = 1;

	private const int downKey = 2;

	private const int rightKey = 3;

	public static readonly string ChromaColorKey = "PlaceChromaObjects";

	[SerializeField]
	private NoteAppearanceSO noteAppearanceSo;

	[SerializeField]
	private DeleteToolController deleteToolController;

	[SerializeField]
	private PrecisionPlacementGridController precisionPlacement;

	[SerializeField]
	private LaserSpeedController laserSpeedController;

	[SerializeField]
	private BeatmapNoteInputController beatmapNoteInputController;

	[SerializeField]
	private ColorPicker colorPicker;

	[SerializeField]
	private ToggleColourDropdown dropdown;

	private readonly float diagonalStickMAXTime = 0.3f;

	private readonly List<bool> heldKeys = new List<bool> { false, false, false, false };

	private bool diagonal;

	private bool flagDirectionsUpdate;

	private bool updateAttachedSliderDirection;

	private static readonly int alwaysTranslucent = Shader.PropertyToID("_AlwaysTranslucent");

	public static bool CanPlaceChromaObjects
	{
		get
		{
			if (Settings.NonPersistentSettings.ContainsKey(ChromaColorKey))
			{
				return (bool)Settings.NonPersistentSettings[ChromaColorKey];
			}
			return false;
		}
	}

	public override int PlacementXMin => base.PlacementXMax * -1;

	private void LateUpdate()
	{
		if (flagDirectionsUpdate)
		{
			HandleDirectionValues();
			flagDirectionsUpdate = false;
		}
	}

	public void OnDownNote(InputAction.CallbackContext context)
	{
		HandleKeyUpdate(context, 2);
	}

	public void OnLeftNote(InputAction.CallbackContext context)
	{
		HandleKeyUpdate(context, 1);
	}

	public void OnUpNote(InputAction.CallbackContext context)
	{
		HandleKeyUpdate(context, 0);
	}

	public void OnRightNote(InputAction.CallbackContext context)
	{
		HandleKeyUpdate(context, 3);
	}

	public void OnDotNote(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			deleteToolController.UpdateDeletion(enabled: false);
			UpdateCut(8);
		}
	}

	public void OnUpLeftNote(InputAction.CallbackContext context)
	{
		if (context.performed && !laserSpeedController.Activated)
		{
			UpdateCut(4);
		}
	}

	public void OnUpRightNote(InputAction.CallbackContext context)
	{
		if (context.performed && !laserSpeedController.Activated)
		{
			UpdateCut(5);
		}
	}

	public void OnDownRightNote(InputAction.CallbackContext context)
	{
		if (context.performed && !laserSpeedController.Activated)
		{
			UpdateCut(7);
		}
	}

	public void OnDownLeftNote(InputAction.CallbackContext context)
	{
		if (context.performed && !laserSpeedController.Activated)
		{
			UpdateCut(6);
		}
	}

	public void PlaceChromaObjects(bool v)
	{
		Settings.NonPersistentSettings[ChromaColorKey] = v;
	}

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> container)
	{
		return new BeatmapObjectPlacementAction(spawned, container, "Placed a note.");
	}

	public override BaseNote GenerateOriginalData()
	{
		return new BaseNote
		{
			Color = 0,
			CutDirection = 1
		};
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 roundedHit)
	{
		queuedData.CustomColor = ((CanPlaceChromaObjects && dropdown.Visible) ? new Color?(colorPicker.CurrentColor) : ((Color?)null));
		int num = (int)roundedHit.x;
		int num2 = (int)roundedHit.y;
		int num3 = Mathf.Clamp(num, 0, 3);
		int num4 = Mathf.Clamp(num2, 0, 2);
		bool flag = num3 == num && num4 == num2;
		queuedData.PosX = num3;
		queuedData.PosY = num4;
		if (UsePrecisionPlacement)
		{
			Vector3 vector = ParentTrack.InverseTransformPoint(hit.Point);
			vector.z = base.SongBpmTime * EditorScaleController.EditorScale;
			int precisionPlacementGridPrecision = Settings.Instance.PrecisionPlacementGridPrecision;
			roundedHit = (Vector2)Vector2Int.RoundToInt((precisionOffset + (Vector2)vector) * precisionPlacementGridPrecision) / (float)precisionPlacementGridPrecision;
			instantiatedContainer.transform.localPosition = roundedHit;
			queuedData.CustomCoordinate = (Vector2)roundedHit;
			precisionPlacement.TogglePrecisionPlacement(isVisible: true);
			precisionPlacement.UpdateMousePosition(hit.Point);
		}
		else
		{
			precisionPlacement.TogglePrecisionPlacement(isVisible: false);
			queuedData.CustomCoordinate = ((!flag) ? ((JSONNode)((Vector2)roundedHit - vanillaOffset + precisionOffset)) : null);
		}
		UpdateAppearance();
	}

	public void UpdateCut(int value)
	{
		ToggleDiagonalAngleOffset(queuedData, value);
		queuedData.CutDirection = value;
		if (DraggedObjectContainer != null && DraggedObjectContainer.NoteData != null)
		{
			ToggleDiagonalAngleOffset(DraggedObjectContainer.NoteData, value);
			DraggedObjectContainer.NoteData.CutDirection = value;
			noteAppearanceSo.SetNoteAppearance(DraggedObjectContainer);
			updateAttachedSliderDirection = true;
		}
		else if (IsActive && beatmapNoteInputController.QuickModificationActive && Settings.Instance.QuickNoteEditing)
		{
			NoteContainer noteContainer = ObjectUnderCursor();
			if (noteContainer != null && noteContainer.ObjectData is BaseNote baseNote)
			{
				BaseNote originalData = BeatmapFactory.Clone(baseNote);
				ToggleDiagonalAngleOffset(baseNote, value);
				baseNote.CutDirection = value;
				List<BeatmapAction> list = new List<BeatmapAction>
				{
					new BeatmapObjectModifiedAction(baseNote, baseNote, originalData, "Quick edit", keepSelection: true, ActionMergeType.NoteDirectionChange)
				};
				CommonNotePlacement.UpdateAttachedSlidersDirection(baseNote, list);
				if (list.Count > 1)
				{
					BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: false, "Quick edit", ActionMergeType.NoteDirectionChange), perform: true);
					SelectionController.SelectionChangedEvent?.Invoke();
				}
				else
				{
					BeatmapActionContainer.AddAction(list[0], perform: true);
				}
			}
		}
		UpdateAppearance();
	}

	private void ToggleDiagonalAngleOffset(BaseNote note, int newCutDirection)
	{
		if (note.CutDirection == 8 && newCutDirection == 8 && note.AngleOffset != 45)
		{
			note.AngleOffset = 45;
		}
		else
		{
			note.AngleOffset = 0;
		}
	}

	public void UpdateType(int type)
	{
		queuedData.Type = type;
		UpdateAppearance();
	}

	private void UpdateAppearance()
	{
		if ((object)instantiatedContainer != null)
		{
			instantiatedContainer.NoteData = queuedData;
			noteAppearanceSo.SetNoteAppearance(instantiatedContainer);
			instantiatedContainer.MaterialPropertyBlock.SetFloat(alwaysTranslucent, 1f);
			instantiatedContainer.UpdateMaterials();
			instantiatedContainer.DirectionTarget.localEulerAngles = NoteContainer.Directionalize(queuedData);
		}
	}

	public override void TransferQueuedToDraggedObject(ref BaseNote dragged, BaseNote queued)
	{
		dragged.JsonTime = queued.JsonTime;
		dragged.PosX = queued.PosX;
		dragged.PosY = queued.PosY;
		dragged.CutDirection = queued.CutDirection;
		dragged.CustomCoordinate = queued.CustomCoordinate;
		if (DraggedObjectContainer != null)
		{
			DraggedObjectContainer.DirectionTarget.localEulerAngles = NoteContainer.Directionalize(dragged);
			DraggedObjectContainer.DirectionTargetEuler = NoteContainer.Directionalize(dragged);
		}
		noteAppearanceSo.SetNoteAppearance(DraggedObjectContainer);
		TransferQueuedToAttachedDraggedSliders(queued);
	}

	private void TransferQueuedToAttachedDraggedSliders(BaseNote queued)
	{
		_ = BeatmapObjectContainerCollection.Epsilon;
		foreach (BaseSlider item in DraggedAttachedSliderDatas[IndicatorType.Head])
		{
			item.JsonTime = queued.JsonTime;
			item.PosX = queued.PosX;
			item.PosY = queued.PosY;
			if (updateAttachedSliderDirection)
			{
				item.CutDirection = queued.CutDirection;
			}
			item.CustomCoordinate = queued.CustomCoordinate;
		}
		foreach (BaseSlider item2 in DraggedAttachedSliderDatas[IndicatorType.Tail])
		{
			item2.TailJsonTime = queued.JsonTime;
			item2.TailPosX = queued.PosX;
			item2.TailPosY = queued.PosY;
			item2.CustomTailCoordinate = queued.CustomCoordinate;
			if (item2 is BaseArc baseArc && updateAttachedSliderDirection)
			{
				baseArc.TailCutDirection = queued.CutDirection;
			}
		}
		foreach (ObjectContainer draggedAttachedSliderContainer in DraggedAttachedSliderContainers)
		{
			if (!(draggedAttachedSliderContainer is ArcContainer arcContainer))
			{
				if (draggedAttachedSliderContainer is ChainContainer chainContainer)
				{
					chainContainer.AdjustTimePlacement();
					chainContainer.GenerateChain();
				}
			}
			else
			{
				arcContainer.NotifySplineChanged();
			}
		}
		updateAttachedSliderDirection = false;
	}

	internal override void RefreshVisuals()
	{
		base.RefreshVisuals();
		instantiatedContainer.SetArcVisible(showArcVisualizer: false);
	}

	private void HandleKeyUpdate(InputAction.CallbackContext context, int id)
	{
		if (context.performed ^ heldKeys[id])
		{
			flagDirectionsUpdate = true;
		}
		heldKeys[id] = context.performed;
	}

	private void HandleDirectionValues()
	{
		deleteToolController.UpdateDeletion(enabled: false);
		bool flag = heldKeys[0];
		bool flag2 = heldKeys[2];
		bool flag3 = heldKeys[1];
		bool flag4 = heldKeys[3];
		bool flag5 = diagonal;
		bool flag6 = flag ^ flag2;
		bool flag7 = flag3 ^ flag4;
		diagonal = flag6 && flag7;
		if (flag5 && !diagonal)
		{
			StartCoroutine(CheckForDiagonalUpdate());
		}
		else if (flag6 && !flag7)
		{
			if (flag)
			{
				UpdateCut(0);
			}
			else
			{
				UpdateCut(1);
			}
		}
		else if (!flag6 && flag7)
		{
			if (flag3)
			{
				UpdateCut(2);
			}
			else
			{
				UpdateCut(3);
			}
		}
		else
		{
			if (!diagonal)
			{
				return;
			}
			if (flag3)
			{
				if (flag)
				{
					UpdateCut(4);
				}
				else
				{
					UpdateCut(6);
				}
			}
			else if (flag)
			{
				UpdateCut(5);
			}
			else
			{
				UpdateCut(7);
			}
		}
	}

	private IEnumerator CheckForDiagonalUpdate()
	{
		List<bool> previousHeldKeys = new List<bool>(heldKeys);
		yield return new WaitForSeconds(diagonalStickMAXTime);
		if (!previousHeldKeys.Except(heldKeys).Any())
		{
			flagDirectionsUpdate = true;
		}
	}
}
