using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class PauseToggleLights : MonoBehaviour
{
	private class LastEvents
	{
		public BaseEvent LastEvent;

		public readonly Dictionary<int, BaseEvent> LastLightIdEvents = new Dictionary<int, BaseEvent>();

		public readonly Dictionary<int, BaseEvent> LastPropEvents = new Dictionary<int, BaseEvent>();
	}

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private EventGridContainer eventGrid;

	private readonly BaseEvent defaultBoostEvent = new BaseEvent
	{
		Type = 5
	};

	private readonly List<BaseEvent> lastChromaEvents = new List<BaseEvent>();

	private readonly Dictionary<int, LastEvents> lastEvents = new Dictionary<int, LastEvents>();

	private PlatformDescriptor descriptor;

	private void Awake()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(PlayToggle));
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(PlayToggle));
	}

	private void PlatformLoaded(PlatformDescriptor platform)
	{
		descriptor = platform;
	}

	private void PlayToggle(bool isPlaying)
	{
		this.lastEvents.Clear();
		lastChromaEvents.Clear();
		if (descriptor == null)
		{
			return;
		}
		if (isPlaying)
		{
			foreach (BaseEvent item in Enumerable.Reverse(eventGrid.MapObjects))
			{
				if (item.JsonTime <= atsc.CurrentJsonTime && !item.IsLegacyChroma)
				{
					if (!this.lastEvents.ContainsKey(item.Type))
					{
						this.lastEvents.Add(item.Type, new LastEvents());
					}
					LastEvents d = this.lastEvents[item.Type];
					if (item.CustomLightID != null && d.LastEvent == null)
					{
						int[] array = (from x in item.CustomLightID.Distinct()
							where !d.LastLightIdEvents.ContainsKey(x)
							select x).ToArray();
						foreach (int key in array)
						{
							d.LastLightIdEvents.Add(key, item);
						}
					}
					else if (item.CustomLightID == null && d.LastEvent == null)
					{
						d.LastEvent = item;
					}
				}
				else if (this.lastEvents.ContainsKey(item.Type) && item.IsLegacyChroma)
				{
					lastChromaEvents.Add(item);
				}
			}
			descriptor.EventPassed(isPlaying, 0, this.lastEvents.ContainsKey(5) ? this.lastEvents[5].LastEvent : defaultBoostEvent);
			BaseEvent baseEvent = new BaseEvent();
			int i;
			for (i = 0; i < 16; i++)
			{
				if (i == 5)
				{
					continue;
				}
				baseEvent.Type = i;
				if (this.lastEvents.ContainsKey(i) && this.lastEvents[i].LastEvent == null)
				{
					this.lastEvents[i].LastEvent = baseEvent;
				}
				if (descriptor.DiskManager != null && (baseEvent.IsLaserRotationEvent() || baseEvent.IsUtilityEvent()))
				{
					continue;
				}
				if (!this.lastEvents.ContainsKey(i))
				{
					if (!baseEvent.IsRingEvent() && !baseEvent.IsLaneRotationEvent())
					{
						descriptor.EventPassed(isPlaying, 0, baseEvent);
					}
					continue;
				}
				LastEvents lastEvents = this.lastEvents[i];
				BaseEvent lastEvent = lastEvents.LastEvent;
				BaseEvent baseEvent2 = lastChromaEvents.Find((BaseEvent x) => x.Type == i);
				if (lastEvent != null && (!lastEvent.IsLightEvent() || (lastEvent.Value != 3 && lastEvent.Value != 7)) && !lastEvent.IsRingEvent())
				{
					descriptor.EventPassed(isPlaying, 0, lastEvent);
				}
				else if (lastEvent == null || (!lastEvent.IsRingEvent() && !lastEvent.IsLaneRotationEvent()))
				{
					descriptor.EventPassed(isPlaying, 0, new BaseEvent
					{
						Type = i
					});
					continue;
				}
				foreach (KeyValuePair<int, BaseEvent> lastPropEvent in lastEvents.LastPropEvents)
				{
					descriptor.EventPassed(isPlaying, 0, lastPropEvent.Value);
				}
				foreach (KeyValuePair<int, BaseEvent> lastLightIdEvent in lastEvents.LastLightIdEvents)
				{
					descriptor.EventPassed(isPlaying, 0, lastLightIdEvent.Value);
				}
				if (lastEvent.IsLightEvent() && Settings.Instance.EmulateChromaLite)
				{
					descriptor.EventPassed(isPlaying, 0, baseEvent2 ?? new BaseEvent
					{
						JsonTime = 0f,
						Type = i,
						Value = 1900000001
					});
				}
			}
		}
		else
		{
			if (descriptor.DiskManager == null)
			{
				BaseEvent obj = new BaseEvent
				{
					Type = 12,
					CustomLockRotation = true
				};
				BaseEvent obj2 = new BaseEvent
				{
					Type = 13,
					CustomLockRotation = true
				};
				descriptor.EventPassed(isPlaying, 0, obj);
				descriptor.EventPassed(isPlaying, 0, obj2);
			}
			descriptor.KillChromaLights();
			descriptor.KillLights();
		}
	}
}
