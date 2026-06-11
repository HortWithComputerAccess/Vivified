using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChainIndicatorPlacement : PlacementController<BaseChain, ChainIndicatorContainer, ChainGridContainer>, CMInput.INotePlacementActions
{
	[SerializeField]
	private DeleteToolController deleteToolController;

	[SerializeField]
	private PrecisionPlacementGridController precisionPlacement;

	[SerializeField]
	private LaserSpeedController laserSpeedController;

	private readonly float diagonalStickMAXTime = 0.3f;

	private readonly List<bool> heldKeys = new List<bool> { false, false, false, false };

	private const int upKey = 0;

	private const int leftKey = 1;

	private const int downKey = 2;

	private const int rightKey = 3;

	private bool diagonal;

	private bool flagDirectionsUpdate;

	public override int PlacementXMin => PlacementXMax * -1;

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, "Edited a chain.");
	}

	public override BaseChain GenerateOriginalData()
	{
		return new BaseChain();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 roundedHit)
	{
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
			if (IsDraggingObject || IsDraggingObjectAtTime)
			{
				if (DraggedObjectContainer.IndicatorType == IndicatorType.Head)
				{
					queuedData.CustomCoordinate = (Vector2)roundedHit;
				}
				if (DraggedObjectContainer.IndicatorType == IndicatorType.Tail)
				{
					queuedData.CustomTailCoordinate = (Vector2)roundedHit;
				}
			}
			precisionPlacement.TogglePrecisionPlacement(isVisible: true);
			precisionPlacement.UpdateMousePosition(hit.Point);
			return;
		}
		precisionPlacement.TogglePrecisionPlacement(isVisible: false);
		if (IsDraggingObject || IsDraggingObjectAtTime)
		{
			if (DraggedObjectContainer.IndicatorType == IndicatorType.Head)
			{
				queuedData.CustomCoordinate = ((!flag) ? ((JSONNode)((Vector2)roundedHit - vanillaOffset + precisionOffset)) : null);
			}
			if (DraggedObjectContainer.IndicatorType == IndicatorType.Tail)
			{
				queuedData.CustomTailCoordinate = ((!flag) ? ((JSONNode)((Vector2)roundedHit - vanillaOffset + precisionOffset)) : null);
			}
		}
	}

	public override void TransferQueuedToDraggedObject(ref BaseChain dragged, BaseChain queued)
	{
		if (DraggedObjectContainer.IndicatorType == IndicatorType.Head)
		{
			dragged.JsonTime = queued.JsonTime;
			dragged.PosX = queued.PosX;
			dragged.PosY = queued.PosY;
			dragged.CutDirection = queued.CutDirection;
			dragged.CustomCoordinate = queued.CustomCoordinate;
		}
		if (DraggedObjectContainer.IndicatorType == IndicatorType.Tail)
		{
			dragged.TailJsonTime = queued.JsonTime;
			dragged.TailPosX = queued.PosX;
			dragged.TailPosY = queued.PosY;
			dragged.CustomTailCoordinate = queued.CustomTailCoordinate;
		}
		DraggedObjectContainer.ParentChain.AdjustTimePlacement();
		DraggedObjectContainer.ParentChain.GenerateChain(dragged);
	}

	public override void OnPlaceObject(InputAction.CallbackContext context)
	{
	}

	protected override float GetContainerPosZ(ObjectContainer con)
	{
		if (con is ChainIndicatorContainer chainIndicatorContainer)
		{
			if (chainIndicatorContainer.IndicatorType == IndicatorType.Head)
			{
				return (chainIndicatorContainer.ParentChain.ChainData.SongBpmTime - Atsc.CurrentSongBpmTime) * EditorScaleController.EditorScale;
			}
			if (chainIndicatorContainer.IndicatorType == IndicatorType.Tail)
			{
				return (chainIndicatorContainer.ParentChain.ChainData.TailSongBpmTime - Atsc.CurrentSongBpmTime) * EditorScaleController.EditorScale;
			}
		}
		return base.GetContainerPosZ(con);
	}

	protected override float GetDraggedObjectJsonTime()
	{
		if (DraggedObjectContainer.IndicatorType == IndicatorType.Tail)
		{
			return draggedObjectData.TailJsonTime;
		}
		return draggedObjectData.JsonTime;
	}

	public void UpdateCut(int value)
	{
		if (DraggedObjectContainer != null && DraggedObjectContainer.ParentChain != null && DraggedObjectContainer.IndicatorType == IndicatorType.Head)
		{
			queuedData.CutDirection = value;
			DraggedObjectContainer.ParentChain.ChainData.CutDirection = value;
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

	private void LateUpdate()
	{
		if (flagDirectionsUpdate)
		{
			HandleDirectionValues();
			flagDirectionsUpdate = false;
		}
	}
}
