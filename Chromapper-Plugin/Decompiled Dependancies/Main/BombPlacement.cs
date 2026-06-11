using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Containers;
using SimpleJSON;
using UnityEngine;

public class BombPlacement : PlacementController<BaseNote, NoteContainer, NoteGridContainer>
{
	public static readonly string ChromaColorKey = "PlaceChromaObjects";

	[SerializeField]
	private PrecisionPlacementGridController precisionPlacement;

	[SerializeField]
	private ColorPicker colorPicker;

	[SerializeField]
	private ToggleColourDropdown dropdown;

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

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> container)
	{
		return new BeatmapObjectPlacementAction(spawned, container, "Placed a Bomb.");
	}

	public override BaseNote GenerateOriginalData()
	{
		return new BaseNote
		{
			Type = 3
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
		instantiatedContainer.MaterialPropertyBlock.SetFloat(alwaysTranslucent, 1f);
		instantiatedContainer.UpdateMaterials();
		instantiatedContainer.NoteData = queuedData;
		instantiatedContainer.UpdateGridPosition();
	}

	public override void TransferQueuedToDraggedObject(ref BaseNote dragged, BaseNote queued)
	{
		dragged.JsonTime = queued.JsonTime;
		dragged.PosX = queued.PosX;
		dragged.PosY = queued.PosY;
		dragged.CustomCoordinate = queued.CustomCoordinate;
	}
}
