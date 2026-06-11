using System;
using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ObstaclePlacement : PlacementController<BaseObstacle, ObstacleContainer, ObstacleGridContainer>
{
	public static readonly string ChromaColorKey = "PlaceChromaObjects";

	[FormerlySerializedAs("obstacleAppearanceSO")]
	[SerializeField]
	private ObstacleAppearanceSO obstacleAppearanceSo;

	[SerializeField]
	private PrecisionPlacementGridController precisionPlacement;

	[SerializeField]
	private ColorPicker colorPicker;

	[SerializeField]
	private ToggleColourDropdown dropdown;

	private int originIndex;

	private Vector2 originPosition;

	private float startJsonTime;

	private float startSongBpmTime;

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

	public static bool IsPlacing { get; private set; }

	public override int PlacementXMin => base.PlacementXMax * -1;

	private float SmallestRankableWallDuration => Atsc.GetBeatFromSeconds(0.016f);

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> container)
	{
		return new BeatmapObjectPlacementAction(spawned, container, "Place a Wall.");
	}

	public override BaseObstacle GenerateOriginalData()
	{
		return new BaseObstacle();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 transformedPoint)
	{
		Bounds = default(Bounds);
		TestForType<ObstaclePlacement>(hit, ObjectType.Obstacle);
		instantiatedContainer.ObstacleData = queuedData;
		instantiatedContainer.ObstacleData.Duration = base.RoundedJsonTime - startJsonTime;
		obstacleAppearanceSo.SetObstacleAppearance(instantiatedContainer);
		Vector3 vector = ParentTrack.InverseTransformPoint(hit.Point);
		float num = BeatSaberSongContainer.Instance.Map.JsonTimeToSongBpmTime(base.RoundedJsonTime).Value - startSongBpmTime;
		queuedData.CustomColor = ((CanPlaceChromaObjects && dropdown.Visible) ? new Color?(colorPicker.CurrentColor) : ((Color?)null));
		Transform localTarget = instantiatedContainer.Animator.LocalTarget;
		if (IsPlacing)
		{
			if (UsePrecisionPlacement)
			{
				int precisionPlacementGridPrecision = Settings.Instance.PrecisionPlacementGridPrecision;
				float num2 = 1f / (float)precisionPlacementGridPrecision;
				vector.x = Mathf.Floor(vector.x * (float)precisionPlacementGridPrecision) * num2;
				vector.y = Mathf.Floor((vector.y - 0.6f) * (float)precisionPlacementGridPrecision) * num2 + 0.5f;
				vector.z = num * EditorScaleController.EditorScale;
				Vector3 vector2 = originPosition;
				Vector3 scale = vector - vector2 + new Vector3(num2, num2, 0f);
				if (scale.x <= 0f)
				{
					vector2.x = originPosition.x + scale.x - num2;
					queuedData.CustomCoordinate[0] = vector2.x;
					scale.x = 2f * num2 - scale.x;
				}
				if (scale.y <= 0f)
				{
					vector2.y = originPosition.y + scale.y - num2;
					queuedData.CustomCoordinate[1] = vector2.y;
					scale.y = 2f * num2 - scale.y;
				}
				Vector3 localPosition = new Vector3(vector2.x + scale.x * 0.5f, vector2.y, 0f);
				localTarget.localPosition = localPosition;
				instantiatedContainer.transform.localPosition = new Vector3(0f, 0.1f, startSongBpmTime * EditorScaleController.EditorScale);
				instantiatedContainer.SetScale(scale);
				if (queuedData.CustomSize == null)
				{
					queuedData.CustomSize = new JSONArray();
				}
				queuedData.CustomSize[0] = scale.x;
				queuedData.CustomSize[1] = scale.y;
				precisionPlacement.TogglePrecisionPlacement(isVisible: true);
				precisionPlacement.UpdateMousePosition(hit.Point);
			}
			else
			{
				queuedData.CustomCoordinate = null;
				queuedData.CustomSize = null;
				vector = new Vector3(Mathf.Ceil(Math.Min(Math.Max(vector.x, Bounds.min.x + 0.01f), Bounds.max.x)), Mathf.Ceil(Math.Min(Math.Max(vector.y, 0.01f), 3f)), base.SongBpmTime * EditorScaleController.EditorScale);
				queuedData.Width = Mathf.CeilToInt(vector.x + 2f) - originIndex;
				if (queuedData.Width <= 0)
				{
					queuedData.PosX = originIndex + queuedData.Width - 1;
					queuedData.Width = 2 - queuedData.Width;
				}
				else
				{
					queuedData.PosX = originIndex;
				}
				localTarget.localPosition = new Vector3((float)queuedData.PosX - 2f + (float)queuedData.Width / 2f, (queuedData.Type != 0) ? 2 : 0, 0f);
				instantiatedContainer.transform.localPosition = new Vector3(0f, 0.1f, startSongBpmTime * EditorScaleController.EditorScale);
				instantiatedContainer.SetScale(new Vector3(queuedData.Width, localTarget.localScale.y, num * EditorScaleController.EditorScale));
				precisionPlacement.TogglePrecisionPlacement(isVisible: false);
			}
			return;
		}
		startJsonTime = base.RoundedJsonTime;
		instantiatedContainer.ObstacleData.Duration = SmallestRankableWallDuration;
		if (UsePrecisionPlacement)
		{
			int precisionPlacementGridPrecision2 = Settings.Instance.PrecisionPlacementGridPrecision;
			vector.x = Mathf.Floor(vector.x * (float)precisionPlacementGridPrecision2) / (float)precisionPlacementGridPrecision2;
			vector.y = Mathf.Floor((vector.y - 0.6f) * (float)precisionPlacementGridPrecision2) / (float)precisionPlacementGridPrecision2 + 0.5f;
			Vector3 scale2 = Vector3.one / precisionPlacementGridPrecision2;
			localTarget.localPosition = vector + new Vector3(scale2.x * 0.5f, 0f, 0f);
			instantiatedContainer.SetScale(scale2);
			BaseObstacle baseObstacle = queuedData;
			int posX = (queuedData.Type = 0);
			baseObstacle.PosX = posX;
			if (queuedData.CustomData == null)
			{
				queuedData.CustomData = new JSONObject();
			}
			queuedData.CustomCoordinate = (Vector2)vector;
			precisionPlacement.TogglePrecisionPlacement(isVisible: true);
			precisionPlacement.UpdateMousePosition(hit.Point);
		}
		else
		{
			queuedData.CustomCoordinate = null;
			queuedData.CustomSize = null;
			int num4 = ((!(vector.y <= 2f)) ? 1 : 0);
			queuedData.PosX = Mathf.RoundToInt(transformedPoint.x);
			queuedData.PosY = ((num4 != 0) ? 2 : 0);
			queuedData.Height = ((num4 == 0) ? 5 : 3);
			queuedData.Type = num4;
			localTarget.localPosition = new Vector3(transformedPoint.x - 1.5f, queuedData.PosY, 0f);
			instantiatedContainer.transform.localPosition = new Vector3(0f, 0.1f, transformedPoint.z);
			instantiatedContainer.SetScale(new Vector3(1f, (num4 == 0) ? 5f : 3f, 0f));
			precisionPlacement.TogglePrecisionPlacement(isVisible: false);
		}
	}

	public override void OnMousePositionUpdate(InputAction.CallbackContext context)
	{
		base.OnMousePositionUpdate(context);
		if (IsPlacing)
		{
			Vector3 scale = instantiatedContainer.GetScale();
			instantiatedContainer.SetScale(new Vector3(scale.x, scale.y, (base.SongBpmTime - startSongBpmTime) * EditorScaleController.EditorScale));
		}
	}

	internal override void ApplyToMap()
	{
		if (IsPlacing)
		{
			IsPlacing = false;
			queuedData.JsonTime = startJsonTime;
			float num = startSongBpmTime + instantiatedContainer.GetScale().z / EditorScaleController.EditorScale;
			if (num - startSongBpmTime < SmallestRankableWallDuration)
			{
				num = startSongBpmTime + SmallestRankableWallDuration;
				float num2 = BeatSaberSongContainer.Instance.Map.SongBpmTimeToJsonTime(num).Value;
				queuedData.Duration = num2 - startJsonTime;
			}
			objectContainerCollection.SpawnObject(queuedData, out var conflicting);
			BeatmapActionContainer.AddAction(GenerateAction(queuedData, conflicting));
			queuedData = BeatmapFactory.Clone(queuedData);
			instantiatedContainer.ObstacleData = queuedData;
			obstacleAppearanceSo.SetObstacleAppearance(instantiatedContainer);
			instantiatedContainer.SetScale(new Vector3(1f, (instantiatedContainer.ObstacleData.Type == 0) ? 5f : 3f, 0f));
		}
		else
		{
			IsPlacing = true;
			originIndex = queuedData.PosX;
			originPosition = queuedData.CustomCoordinate?.ReadVector2() ?? new Vector2(originIndex, 5 - queuedData.Height);
			startJsonTime = base.RoundedJsonTime;
			startSongBpmTime = base.SongBpmTime;
		}
	}

	public override void TransferQueuedToDraggedObject(ref BaseObstacle dragged, BaseObstacle queued)
	{
		dragged.JsonTime = queued.JsonTime;
		dragged.PosX = queued.PosX;
		dragged.CustomCoordinate = queued.CustomCoordinate;
	}

	public override void CancelPlacement()
	{
		if (IsPlacing)
		{
			IsPlacing = false;
			queuedData = GenerateOriginalData();
			instantiatedContainer.ObstacleData = queuedData;
			obstacleAppearanceSo.SetObstacleAppearance(instantiatedContainer);
			instantiatedContainer.SetScale(new Vector3(1f, (instantiatedContainer.ObstacleData.Type == 0) ? 5f : 3f, 0f));
		}
	}
}
