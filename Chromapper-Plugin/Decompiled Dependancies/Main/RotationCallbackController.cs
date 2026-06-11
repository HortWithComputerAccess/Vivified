using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.Serialization;

public class RotationCallbackController : MonoBehaviour
{
	[SerializeField]
	private BeatmapObjectCallbackController interfaceCallback;

	[FormerlySerializedAs("atsc")]
	public AudioTimeSyncController Atsc;

	[FormerlySerializedAs("events")]
	[SerializeField]
	private EventGridContainer eventGrid;

	private readonly string[] enabledCharacteristics = new string[3] { "360Degree", "90Degree", "Lawless" };

	public Action<bool, float> RotationChangedEvent;

	public bool IsActive { get; private set; }

	public BaseEvent LatestRotationEvent { get; private set; }

	public float Rotation { get; private set; }

	internal void Start()
	{
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		IsActive = Enumerable.Contains(enabledCharacteristics, mapDifficultyInfo.Characteristic);
		if (IsActive && Settings.Instance.Reminder_Loading360Levels)
		{
			PersistentUI.Instance.ShowDialogBox("PersistentUI", "360warning", Handle360LevelReminder, PersistentUI.DialogBoxPresetType.OkIgnore);
		}
		BeatmapObjectCallbackController beatmapObjectCallbackController = interfaceCallback;
		beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassedThreshold));
		AudioTimeSyncController atsc = Atsc;
		atsc.PlayToggle = (Action<bool>)Delegate.Combine(atsc.PlayToggle, new Action<bool>(PlayToggle));
		AudioTimeSyncController atsc2 = Atsc;
		atsc2.TimeChanged = (Action)Delegate.Combine(atsc2.TimeChanged, new Action(OnTimeChanged));
		Settings.NotifyBySettingName("RotateTrack", UpdateRotateTrack);
	}

	private void OnDestroy()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = interfaceCallback;
		beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassedThreshold));
		AudioTimeSyncController atsc = Atsc;
		atsc.PlayToggle = (Action<bool>)Delegate.Remove(atsc.PlayToggle, new Action<bool>(PlayToggle));
		AudioTimeSyncController atsc2 = Atsc;
		atsc2.TimeChanged = (Action)Delegate.Remove(atsc2.TimeChanged, new Action(OnTimeChanged));
		Settings.ClearSettingNotifications("RotateTrack");
	}

	private void UpdateRotateTrack(object obj)
	{
		if (!Settings.Instance.RotateTrack)
		{
			RotationChangedEvent?.Invoke(arg1: false, 0f);
		}
	}

	private void Handle360LevelReminder(int res)
	{
		Settings.Instance.Reminder_Loading360Levels = res == 0;
	}

	private void OnTimeChanged()
	{
		if (!Atsc.IsPlaying)
		{
			PlayToggle(isPlaying: false);
		}
	}

	private void PlayToggle(bool isPlaying)
	{
		if (!IsActive)
		{
			return;
		}
		float currentJsonTime = Atsc.CurrentJsonTime;
		Span<BaseEvent> span = eventGrid.AllRotationEvents.AsSpan();
		int num = span.BinarySearchBy(currentJsonTime, (BaseEvent e) => e.JsonTime);
		int num2 = ((num >= 0) ? num : (~num));
		for (float epsilon = BeatmapObjectContainerCollection.Epsilon; num2 < span.Length && span[num2].JsonTime <= currentJsonTime - epsilon; num2++)
		{
		}
		Rotation = 0f;
		if (num2 > 0)
		{
			for (int num3 = 0; num3 < num2; num3++)
			{
				Rotation += span[num3].Rotation;
			}
			LatestRotationEvent = span[num2 - 1];
		}
		else
		{
			LatestRotationEvent = null;
		}
		RotationChangedEvent(arg1: false, Rotation);
	}

	private void EventPassedThreshold(bool initial, int index, BaseObject obj)
	{
		if (IsActive && obj is BaseEvent baseEvent && baseEvent.IsLaneRotationEvent() && baseEvent != LatestRotationEvent)
		{
			Rotation += baseEvent.Rotation;
			LatestRotationEvent = baseEvent;
			RotationChangedEvent(arg1: true, Rotation);
		}
	}
}
