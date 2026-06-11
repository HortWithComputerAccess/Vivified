using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BeatmapObstacleInputController : BeatmapInputController<ObstacleContainer>, CMInput.IObstacleObjectsActions
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[FormerlySerializedAs("bpmChangesContainer")]
	[SerializeField]
	private BPMChangeGridContainer bpmChangeGridContainer;

	[FormerlySerializedAs("obstacleAppearanceSO")]
	[SerializeField]
	private ObstacleAppearanceSO obstacleAppearanceSo;

	public void OnChangeWallDuration(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging && context.performed)
			{
				BaseObject originalData = BeatmapFactory.Clone(firstObject.ObjectData);
				float num = 1f / (float)atsc.GridMeasureSnapping;
				num *= (float)(((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollWallDuration) ? 1 : (-1));
				firstObject.ObstacleData.Duration += num;
				firstObject.UpdateGridPosition();
				obstacleAppearanceSo.SetObstacleAppearance(firstObject);
				BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(firstObject.ObjectData, firstObject.ObjectData, originalData, "No comment.", keepSelection: false, ActionMergeType.WallDurationTweak));
			}
		}
	}

	public void OnChangeWallLowerBound(InputAction.CallbackContext context)
	{
		if (Settings.Instance.MapVersion < 3 || CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			return;
		}
		RaycastFirstObject(out var firstObject);
		if (firstObject != null && !firstObject.Dragging && context.performed)
		{
			BaseObject baseObject = BeatmapFactory.Clone(firstObject.ObjectData);
			int num = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollWallDuration) ? 1 : (-1));
			BaseObstacle baseObstacle = firstObject.ObjectData as BaseObstacle;
			baseObstacle.PosY = Mathf.Clamp(baseObstacle.PosY + num, 0, 2);
			baseObstacle.Height = Mathf.Min(baseObstacle.Height, 5 - baseObstacle.PosY);
			if (baseObstacle.CompareTo(baseObject) != 0)
			{
				firstObject.UpdateGridPosition();
				obstacleAppearanceSo.SetObstacleAppearance(firstObject);
				BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(firstObject.ObjectData, firstObject.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.WallLowerBoundTweak));
			}
		}
	}

	public void OnChangeWallUpperBound(InputAction.CallbackContext context)
	{
		if (Settings.Instance.MapVersion < 3 || CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			return;
		}
		RaycastFirstObject(out var firstObject);
		if (firstObject != null && !firstObject.Dragging && context.performed)
		{
			BaseObject baseObject = BeatmapFactory.Clone(firstObject.ObjectData);
			int num = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollWallDuration) ? 1 : (-1));
			BaseObstacle baseObstacle = firstObject.ObjectData as BaseObstacle;
			baseObstacle.Height = Mathf.Clamp(baseObstacle.Height + num, 1, 5 - baseObstacle.PosY);
			if (baseObstacle.CompareTo(baseObject) != 0)
			{
				firstObject.UpdateGridPosition();
				obstacleAppearanceSo.SetObstacleAppearance(firstObject);
				BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(firstObject.ObjectData, firstObject.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.WallUpperBoundTweak));
			}
		}
	}

	public void OnToggleHyperWall(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging && context.performed)
			{
				ToggleHyperWall(firstObject);
			}
		}
	}

	public void ToggleHyperWall(ObstacleContainer obs)
	{
		BaseObstacle obj = BeatmapFactory.Clone(obs.ObjectData) as BaseObstacle;
		obj.JsonTime += obs.ObstacleData.Duration;
		obj.Duration *= -1f;
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(obj, obs.ObjectData, obs.ObjectData), perform: true);
	}
}
