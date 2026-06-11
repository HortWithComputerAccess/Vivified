using System;
using Beatmap.Base;
using UnityEngine;

internal class PluginEventHandler : MonoBehaviour
{
	[SerializeField]
	private BeatmapObjectCallbackController interfaceCallback;

	private void Awake()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = interfaceCallback;
		beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassedThreshold));
		BeatmapObjectCallbackController beatmapObjectCallbackController2 = interfaceCallback;
		beatmapObjectCallbackController2.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController2.NotePassedThreshold, new Action<bool, int, BaseObject>(NotePassedThreshold));
	}

	private void OnDestroy()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = interfaceCallback;
		beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassedThreshold));
		BeatmapObjectCallbackController beatmapObjectCallbackController2 = interfaceCallback;
		beatmapObjectCallbackController2.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController2.NotePassedThreshold, new Action<bool, int, BaseObject>(NotePassedThreshold));
	}

	private void EventPassedThreshold(bool _, int __, BaseObject newlyAdded)
	{
		PluginLoader.BroadcastEvent<EventPassedThresholdAttribute, BaseObject>(newlyAdded);
	}

	private void NotePassedThreshold(bool _, int __, BaseObject newlyAdded)
	{
		PluginLoader.BroadcastEvent<NotePassedThresholdAttribute, BaseObject>(newlyAdded);
	}
}
