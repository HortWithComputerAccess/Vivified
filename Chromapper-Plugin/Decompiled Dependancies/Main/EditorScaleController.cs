using System;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.InputSystem;

public class EditorScaleController : MonoBehaviour, CMInput.IEditorScaleActions
{
	private const float keybindMultiplyValue = 1.25f;

	private const float baseBpm = 160f;

	public static float EditorScale = 4f;

	public static Action<float> EditorScaleChangedEvent;

	[SerializeField]
	private Transform moveableGridTransform;

	[SerializeField]
	private Transform[] scalingOffsets;

	[SerializeField]
	private AudioTimeSyncController atsc;

	private BeatmapObjectContainerCollection[] collections;

	private float currentBpm = 160f;

	private float previousEditorScale = -1f;

	private void Start()
	{
		collections = moveableGridTransform.GetComponents<BeatmapObjectContainerCollection>();
		currentBpm = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
		SetAccurateEditorScale(Settings.Instance.NoteJumpSpeedForEditorScale);
		Settings.NotifyBySettingName("EditorScale", UpdateEditorScale);
		Settings.NotifyBySettingName("EditorScaleBPMIndependent", RecalcEditorScale);
		Settings.NotifyBySettingName("NoteJumpSpeedForEditorScale", SetAccurateEditorScale);
		UIMode.UIModeSwitched = (Action<UIModeType>)Delegate.Combine(UIMode.UIModeSwitched, new Action<UIModeType>(UpdateByUIMode));
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("EditorScale");
		Settings.ClearSettingNotifications("EditorScaleBPMIndependent");
		Settings.ClearSettingNotifications("NoteJumpSpeedForEditorScale");
		UIMode.UIModeSwitched = (Action<UIModeType>)Delegate.Remove(UIMode.UIModeSwitched, new Action<UIModeType>(UpdateByUIMode));
	}

	public void OnDecreaseEditorScale(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Settings.Instance.EditorScale /= 1.25f;
			Settings.ManuallyNotifySettingUpdatedEvent("EditorScale", Settings.Instance.EditorScale);
		}
	}

	public void OnIncreaseEditorScale(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Settings.Instance.EditorScale *= 1.25f;
			Settings.ManuallyNotifySettingUpdatedEvent("EditorScale", Settings.Instance.EditorScale);
		}
	}

	public void UpdateEditorScale(object value)
	{
		if (!Settings.Instance.NoteJumpSpeedForEditorScale)
		{
			float num = (float)value;
			if (Settings.Instance.EditorScaleBPMIndependent)
			{
				EditorScale = num * 160f / currentBpm;
			}
			else
			{
				EditorScale = num;
			}
			if (previousEditorScale != EditorScale)
			{
				Apply();
			}
		}
	}

	private void RecalcEditorScale(object obj)
	{
		UpdateEditorScale(Settings.Instance.EditorScale);
	}

	private void SetAccurateEditorScale(object obj)
	{
		if ((bool)obj)
		{
			float num = 60f / currentBpm;
			float noteJumpSpeed = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
			EditorScale = 1.6666666f * noteJumpSpeed * num;
			Apply();
		}
		else
		{
			UpdateEditorScale(Settings.Instance.EditorScale);
		}
	}

	private void UpdateByUIMode(UIModeType mode)
	{
		switch (mode)
		{
		case UIModeType.Normal:
			SetAccurateEditorScale(Settings.Instance.NoteJumpSpeedForEditorScale);
			break;
		case UIModeType.HideUI:
			SetAccurateEditorScale(Settings.Instance.NoteJumpSpeedForEditorScale);
			break;
		case UIModeType.HideGrids:
			SetAccurateEditorScale(Settings.Instance.NoteJumpSpeedForEditorScale);
			break;
		case UIModeType.Preview:
			SetAccurateEditorScale(true);
			break;
		case UIModeType.Playing:
			SetAccurateEditorScale(true);
			break;
		}
	}

	private void Apply()
	{
		BeatmapObjectContainerCollection[] array = collections;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (ObjectContainer value in array[i].LoadedContainers.Values)
			{
				value.UpdateGridPosition();
			}
		}
		atsc.MoveToSongBpmTime(atsc.CurrentSongBpmTime);
		EditorScaleChangedEvent?.Invoke(EditorScale);
		previousEditorScale = EditorScale;
		Transform[] array2 = scalingOffsets;
		foreach (Transform transform in array2)
		{
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, (float)Settings.Instance.TrackLength * EditorScale);
		}
	}
}
