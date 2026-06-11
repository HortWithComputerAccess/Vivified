using System;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;

public class NJSEventGridContainer : BeatmapObjectContainerCollection<BaseNJSEvent>
{
	[SerializeField]
	private GameObject njsEventPrefab;

	[SerializeField]
	private CountersPlusController countersPlus;

	private static readonly int currentHJDShaderID = Shader.PropertyToID("_CurrentHJD");

	private static readonly int DisplayHJDLine = Shader.PropertyToID("_DisplayHJDLine");

	private float currentNJS;

	public override ObjectType ContainerType => ObjectType.NJSEvent;

	public float CurrentNJS
	{
		get
		{
			return currentNJS;
		}
		private set
		{
			if (currentNJS != value)
			{
				currentNJS = value;
				countersPlus.UpdateStatistic(CountersPlusStatistic.NJSEvents);
			}
		}
	}

	internal override void SubscribeToCallbacks()
	{
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		AudioTimeSyncController audioTimeSyncController2 = AudioTimeSyncController;
		audioTimeSyncController2.TimeChanged = (Action)Delegate.Combine(audioTimeSyncController2.TimeChanged, new Action(UpdateHJDLine));
		UIMode.PreviewModeSwitched = (Action)Delegate.Combine(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.NotifyBySettingName("DisplayHJDLine", UpdateDisplayHJDLine);
		UpdateHJDLine();
		UpdateDisplayHJDLine(Settings.Instance.DisplayHJDLine);
	}

	internal override void UnsubscribeToCallbacks()
	{
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		AudioTimeSyncController audioTimeSyncController2 = AudioTimeSyncController;
		audioTimeSyncController2.TimeChanged = (Action)Delegate.Remove(audioTimeSyncController2.TimeChanged, new Action(UpdateHJDLine));
		UIMode.PreviewModeSwitched = (Action)Delegate.Remove(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.ClearSettingNotifications("DisplayHJDLine");
	}

	private void OnPlayToggle(bool isPlaying)
	{
		if (!isPlaying)
		{
			RefreshPool();
		}
	}

	private void OnUIPreviewModeSwitch()
	{
		RefreshPool(forceRefresh: true);
	}

	public override ObjectContainer CreateContainer()
	{
		return NJSEventContainer.SpawnNJSEvent(null, ref njsEventPrefab);
	}

	protected override void OnObjectSpawned(BaseObject _, bool __ = false)
	{
		UpdateHJDLine();
	}

	protected override void OnObjectDelete(BaseObject _, bool __ = false)
	{
		UpdateHJDLine();
	}

	public void UpdateHJDLine()
	{
		float noteJumpSpeed = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
		float num = SpawnParameterHelper.CalculateHalfJumpDuration(noteJumpSpeed, BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteStartBeatOffset, BeatSaberSongContainer.Instance.Info.BeatsPerMinute);
		if (MapObjects.Count == 0)
		{
			if (CurrentNJS != noteJumpSpeed)
			{
				CurrentNJS = noteJumpSpeed;
				Shader.SetGlobalFloat(currentHJDShaderID, num);
			}
			return;
		}
		BaseNJSEvent previousNJSEvent = MapObjects.FindLast((BaseNJSEvent x) => x.JsonTime <= AudioTimeSyncController.CurrentJsonTime + 0.01f);
		BaseNJSEvent baseNJSEvent = MapObjects.Find((BaseNJSEvent x) => x.JsonTime >= AudioTimeSyncController.CurrentJsonTime - 0.01f);
		float num2 = (previousNJSEvent?.RelativeNJS ?? 0f) + noteJumpSpeed;
		BaseNJSEvent baseNJSEvent2 = previousNJSEvent;
		if (baseNJSEvent2 != null && baseNJSEvent2.UsePrevious == 1)
		{
			num2 = (MapObjects.FindLast((BaseNJSEvent x) => x.UsePrevious == 0 && x.JsonTime <= previousNJSEvent.JsonTime)?.RelativeNJS ?? 0f) + noteJumpSpeed;
		}
		float b = ((baseNJSEvent != null && baseNJSEvent.UsePrevious == 1) ? num2 : ((baseNJSEvent?.RelativeNJS ?? previousNJSEvent?.RelativeNJS ?? 0f) + noteJumpSpeed));
		float num3 = previousNJSEvent?.JsonTime ?? 0f;
		float num4 = baseNJSEvent?.JsonTime ?? num3;
		float k = (Mathf.Approximately(num3, num4) ? 0f : ((AudioTimeSyncController.CurrentJsonTime - num3) / (num4 - num3)));
		float t = Easing.BeatSaber.EaseVNJS(baseNJSEvent?.Easing, k);
		float num5 = Mathf.Lerp(num2, b, t);
		if (num5 > noteJumpSpeed)
		{
			Shader.SetGlobalFloat(currentHJDShaderID, num);
		}
		else
		{
			float num6 = noteJumpSpeed / num5;
			Shader.SetGlobalFloat(currentHJDShaderID, num * num6);
		}
		CurrentNJS = num5;
		countersPlus.UpdateStatistic(CountersPlusStatistic.NJSEvents);
	}

	private void UpdateDisplayHJDLine(object value)
	{
		Shader.SetGlobalInt(DisplayHJDLine, ((bool)value) ? 1 : 0);
	}
}
