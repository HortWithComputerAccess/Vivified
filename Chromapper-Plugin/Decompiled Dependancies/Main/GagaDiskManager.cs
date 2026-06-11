using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class GagaDiskManager : MonoBehaviour
{
	private const int minEventValue = 0;

	private const int maxEventValue = 8;

	private readonly int[] heightEventTypes = new int[6] { 18, 16, 12, 13, 17, 19 };

	public List<GagaDisk> Disks = new List<GagaDisk>();

	private EventGridContainer eventGridContainer;

	private AudioTimeSyncController atsc;

	private Dictionary<int, List<BaseEvent>> cachedHeightEvents = new Dictionary<int, List<BaseEvent>>();

	public void Start()
	{
		atsc = UnityEngine.Object.FindObjectOfType<AudioTimeSyncController>();
		eventGridContainer = UnityEngine.Object.FindObjectOfType<EventGridContainer>();
		foreach (GagaDisk disk in Disks)
		{
			disk.Init();
			UpdateEventCache(disk.HeightEventType);
		}
		eventGridContainer.ObjectSpawnedEvent += UpdateEventCache;
		eventGridContainer.ObjectDeletedEvent += UpdateEventCache;
	}

	public void OnDestroy()
	{
		eventGridContainer.ObjectSpawnedEvent -= UpdateEventCache;
		eventGridContainer.ObjectDeletedEvent -= UpdateEventCache;
	}

	private void LateUpdate()
	{
		foreach (GagaDisk disk in Disks)
		{
			disk.LateUpdateDisk(atsc.CurrentJsonTime);
		}
	}

	public void HandlePositionEvent(BaseEvent evt)
	{
		foreach (GagaDisk item in Disks.Where((GagaDisk d) => d.HeightEventType == evt.Type))
		{
			BaseEvent nextHeightEvent = GetNextHeightEvent(evt);
			if (nextHeightEvent == null)
			{
				break;
			}
			int value = evt.Value;
			int value2 = nextHeightEvent.Value;
			float jsonTime = nextHeightEvent.JsonTime;
			item.SetPosition(ClampEventValue(value), ClampEventValue(value2), evt.JsonTime, jsonTime);
		}
	}

	private int ClampEventValue(int value)
	{
		return Math.Clamp(value, 0, 8);
	}

	private List<BaseEvent> GetHeightEventsFromGrid()
	{
		return (from x in eventGridContainer.AllUtilityEvents.Where((BaseEvent x) => Enumerable.Contains(heightEventTypes, x.Type)).Concat(eventGridContainer.AllLaserRotationEvents)
			where Enumerable.Contains(heightEventTypes, x.Type)
			orderby x.JsonTime
			select x).ToList();
	}

	private List<BaseEvent> GetCachedHeightEvents(int type)
	{
		if (!cachedHeightEvents.TryGetValue(type, out var value))
		{
			return new List<BaseEvent>();
		}
		return value;
	}

	private BaseEvent GetNextHeightEvent(BaseEvent e)
	{
		List<BaseEvent> source = GetCachedHeightEvents(e.Type);
		if (!source.Any())
		{
			return null;
		}
		return source.FirstOrDefault((BaseEvent ev) => ev.JsonTime > e.JsonTime);
	}

	private BaseEvent GetNextHeightEvent(int type)
	{
		List<BaseEvent> source = GetCachedHeightEvents(type);
		if (!source.Any())
		{
			return null;
		}
		return source.FirstOrDefault((BaseEvent ev) => ev.JsonTime >= atsc.CurrentJsonTime);
	}

	private BaseEvent GetPreviousHeightEvent(int type)
	{
		List<BaseEvent> source = Enumerable.Reverse(GetCachedHeightEvents(type)).ToList();
		if (!source.Any())
		{
			return null;
		}
		return source.FirstOrDefault((BaseEvent ev) => ev.JsonTime >= atsc.CurrentJsonTime);
	}

	private void UpdateEventCache(BaseEvent evt)
	{
		if (!Enumerable.Contains(heightEventTypes, evt.Type))
		{
			return;
		}
		IEnumerable<BaseEvent> collection = from x in GetHeightEventsFromGrid()
			where x.Type == evt.Type
			select x;
		if (cachedHeightEvents.ContainsKey(evt.Type))
		{
			cachedHeightEvents[evt.Type].Clear();
		}
		else
		{
			cachedHeightEvents[evt.Type] = new List<BaseEvent>();
		}
		cachedHeightEvents[evt.Type].AddRange(collection);
		foreach (GagaDisk disk in Disks)
		{
			if (disk.HeightEventType != evt.Type)
			{
				continue;
			}
			BaseEvent previousHeightEvent = GetPreviousHeightEvent(evt.Type);
			BaseEvent nextHeightEvent = GetNextHeightEvent(evt);
			int value = 4;
			int value2 = 4;
			float timeStart = 0f;
			float timeDest = 0.1f;
			if (previousHeightEvent != null)
			{
				value = previousHeightEvent.Value;
				timeStart = previousHeightEvent.JsonTime;
				if (nextHeightEvent != null)
				{
					value2 = nextHeightEvent.Value;
					timeDest = nextHeightEvent.JsonTime;
				}
			}
			disk.SetPosition(ClampEventValue(value), ClampEventValue(value2), timeStart, timeDest);
			break;
		}
	}

	private void UpdateEventCache(int eventType)
	{
		IEnumerable<BaseEvent> collection = from x in GetHeightEventsFromGrid()
			where x.Type == eventType
			select x;
		if (cachedHeightEvents.ContainsKey(eventType))
		{
			cachedHeightEvents[eventType].Clear();
		}
		else
		{
			cachedHeightEvents[eventType] = new List<BaseEvent>();
		}
		cachedHeightEvents[eventType].AddRange(collection);
	}
}
