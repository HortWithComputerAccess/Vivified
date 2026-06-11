using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventGridContainer : BeatmapObjectContainerCollection<BaseEvent>, CMInput.IEventGridActions
{
	public enum PropMode
	{
		Off,
		Prop,
		Light
	}

	[SerializeField]
	private GameObject eventPrefab;

	[SerializeField]
	private EventAppearanceSO eventAppearanceSo;

	[SerializeField]
	private GameObject eventGridLabels;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private EventPlacement eventPlacement;

	[SerializeField]
	private CreateEventTypeLabels labels;

	[SerializeField]
	private BoxSelectionPlacementController boxSelectionPlacementController;

	[SerializeField]
	private LaserSpeedController laserSpeedController;

	[SerializeField]
	private CountersPlusController countersPlus;

	public int EventTypeToPropagate = 1;

	public int EventTypePropagationSize;

	public List<BaseEvent> AllRotationEvents = new List<BaseEvent>();

	public List<BaseEvent> AllBoostEvents = new List<BaseEvent>();

	public List<BaseEvent> AllBpmEvents = new List<BaseEvent>();

	public List<BaseEvent> AllUtilityEvents = new List<BaseEvent>();

	public List<BaseEvent> AllLaserRotationEvents = new List<BaseEvent>();

	private HashSet<BaseEvent> lightEventsWithKnownPrevNext = new HashSet<BaseEvent>();

	private Dictionary<int, List<BaseEvent>> allLightEvents = new Dictionary<int, List<BaseEvent>>();

	internal PlatformDescriptor platformDescriptor;

	private PropMode propagationEditing;

	public Dictionary<int, List<BaseEvent>> AllLightEvents
	{
		get
		{
			return allLightEvents;
		}
		set
		{
			allLightEvents = value;
			foreach (KeyValuePair<int, List<BaseEvent>> allLightEvent in allLightEvents)
			{
				List<BaseEvent> value2 = allLightEvent.Value;
				if (Settings.Instance.EmulateChromaAdvanced && Settings.Instance.LightIDTransitionSupport)
				{
					LinkEventsForChroma(value2);
				}
				else
				{
					LinkEventsForVanilla(value2);
				}
			}
		}
	}

	public override ObjectType ContainerType => ObjectType.Event;

	private static int ExtraInterscopeLanes
	{
		get
		{
			if (EventContainer.ModifyTypeMode != 2)
			{
				return 0;
			}
			return 2;
		}
	}

	private static int ExtraGagaLanes
	{
		get
		{
			if (EventContainer.ModifyTypeMode != 3)
			{
				return 0;
			}
			return 4;
		}
	}

	private int SpecialEventTypeCount => 7 + labels.NoRotationLaneOffset + ExtraInterscopeLanes + ExtraGagaLanes;

	public PropMode PropagationEditing
	{
		get
		{
			return propagationEditing;
		}
		set
		{
			propagationEditing = value;
			boxSelectionPlacementController.CancelPlacement();
			int num = ((!(platformDescriptor.LightingManagers[EventTypeToPropagate] == null)) ? ((value != PropMode.Light) ? platformDescriptor.LightingManagers[EventTypeToPropagate].LightsGroupedByZ?.Length : platformDescriptor.LightingManagers[EventTypeToPropagate].LightIDPlacementMapReverse?.Count).GetValueOrDefault() : 0);
			int num2 = ExtraInterscopeLanes + ExtraGagaLanes;
			labels.UpdateLabels(value, EventTypeToPropagate, (value == PropMode.Off) ? (16 + num2) : (num + 1));
			eventPlacement.SetGridSize((value != PropMode.Off) ? (num + 1) : (SpecialEventTypeCount + platformDescriptor.LightingManagers.Count((LightsManager s) => s != null)));
			EventTypePropagationSize = num;
			UpdatePropagationMode();
		}
	}

	private void Start()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	public void OnToggleLightPropagation(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PropagationEditing = ((PropagationEditing != PropMode.Prop) ? PropMode.Prop : PropMode.Off);
		}
	}

	public void OnToggleLightIdMode(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PropagationEditing = ((PropagationEditing != PropMode.Light) ? PropMode.Light : PropMode.Off);
		}
	}

	public void OnResetRings(InputAction.CallbackContext context)
	{
		if (context.performed && !laserSpeedController.Activated)
		{
			if (platformDescriptor.BigRingManager is TrackLaneRingsManager trackLaneRingsManager)
			{
				trackLaneRingsManager.RotationEffect.Reset();
			}
			if (platformDescriptor.SmallRingManager != null && platformDescriptor.SmallRingManager.RotationEffect != null)
			{
				platformDescriptor.SmallRingManager.RotationEffect.Reset();
			}
		}
	}

	public void OnCycleLightPropagationUp(InputAction.CallbackContext context)
	{
		if (!context.performed || PropagationEditing == PropMode.Off)
		{
			return;
		}
		int num = EventTypeToPropagate + 1;
		if (num == platformDescriptor.LightingManagers.Length)
		{
			num = 0;
		}
		while (platformDescriptor.LightingManagers[num] == null)
		{
			num++;
			if (num == platformDescriptor.LightingManagers.Length)
			{
				num = 0;
			}
		}
		EventTypeToPropagate = num;
		PropagationEditing = PropagationEditing;
	}

	public void OnCycleLightPropagationDown(InputAction.CallbackContext context)
	{
		if (!context.performed || PropagationEditing == PropMode.Off)
		{
			return;
		}
		int num = EventTypeToPropagate - 1;
		if (num == -1)
		{
			num = platformDescriptor.LightingManagers.Length - 1;
		}
		while (platformDescriptor.LightingManagers[num] == null)
		{
			num--;
			if (num == -1)
			{
				num = platformDescriptor.LightingManagers.Length - 1;
			}
		}
		EventTypeToPropagate = num;
		PropagationEditing = PropagationEditing;
	}

	public static string GetKeyForProp(PropMode mode)
	{
		return mode switch
		{
			PropMode.Light => "_lightID", 
			PropMode.Prop => "_propID", 
			_ => null, 
		};
	}

	private void PlatformLoaded(PlatformDescriptor descriptor)
	{
		platformDescriptor = descriptor;
		StartCoroutine(AfterPlatformLoaded());
	}

	private IEnumerator AfterPlatformLoaded()
	{
		yield return null;
		PropagationEditing = PropMode.Off;
	}

	internal override void SubscribeToCallbacks()
	{
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(spawnCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveEventCheckFinished = (Action<bool, int>)Delegate.Combine(spawnCallbackController2.RecursiveEventCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(despawnCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	internal override void UnsubscribeToCallbacks()
	{
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(spawnCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveEventCheckFinished = (Action<bool, int>)Delegate.Remove(spawnCallbackController2.RecursiveEventCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(despawnCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	protected override void OnObjectDelete(BaseObject obj, bool inCollection = false)
	{
		if (obj is BaseEvent baseEvent)
		{
			if (baseEvent.IsLaneRotationEvent())
			{
				AllRotationEvents.Remove(baseEvent);
				tracksManager.RefreshTracks();
			}
			else if (baseEvent.IsColorBoostEvent())
			{
				AllBoostEvents.Remove(baseEvent);
			}
			else if (baseEvent.IsBpmEvent())
			{
				AllBpmEvents.Remove(baseEvent);
			}
			else if (baseEvent.IsLightEvent() && !inCollection)
			{
				RemoveLinkedLightEvents(baseEvent);
				if (AllLightEvents.TryGetValue(baseEvent.Type, out var value))
				{
					value.Remove(baseEvent);
				}
			}
			else if (baseEvent.IsUtilityEvent())
			{
				AllUtilityEvents.Remove(baseEvent);
			}
			else if (baseEvent.IsLaserRotationEvent())
			{
				AllLaserRotationEvents.Remove(baseEvent);
			}
			MarkEventToBeRelinked(baseEvent);
		}
		countersPlus.UpdateStatistic(CountersPlusStatistic.Events);
	}

	public override void DoPostObjectsDeleteWorkflow()
	{
		LinkAllLightEvents();
		RefreshPool();
	}

	protected override void OnObjectSpawned(BaseObject obj, bool inCollection = false)
	{
		if (obj is BaseEvent baseEvent)
		{
			if (baseEvent.IsLaneRotationEvent())
			{
				AllRotationEvents.Add(baseEvent);
			}
			else if (baseEvent.IsColorBoostEvent())
			{
				AllBoostEvents.Add(baseEvent);
			}
			else if (baseEvent.IsBpmEvent())
			{
				AllBpmEvents.Add(baseEvent);
			}
			else if (baseEvent.IsLightEvent() && !inCollection)
			{
				RemoveLinkedLightEvents(baseEvent);
				LinkLightEvents(baseEvent);
				AddToAllLightEvents(baseEvent);
				lightEventsWithKnownPrevNext.Add(baseEvent);
			}
			else if (baseEvent.IsUtilityEvent())
			{
				AllUtilityEvents.Add(baseEvent);
			}
			else if (baseEvent.IsLaserRotationEvent())
			{
				AllLaserRotationEvents.Add(baseEvent);
			}
		}
		countersPlus.UpdateStatistic(CountersPlusStatistic.Events);
	}

	public override void DoPostObjectsSpawnedWorkflow()
	{
		LinkAllLightEvents();
	}

	private void LinkLightEvents(BaseEvent e)
	{
		BaseEvent previousEventWithSameLightIDOrDefault = GetPreviousEventWithSameLightIDOrDefault(e);
		if (previousEventWithSameLightIDOrDefault != null)
		{
			previousEventWithSameLightIDOrDefault.Next = e;
			if (LoadedContainers.TryGetValue(previousEventWithSameLightIDOrDefault, out var value))
			{
				(value as EventContainer).RefreshAppearance();
			}
		}
		BaseEvent nextEventWithSameLightIDOrDefault = GetNextEventWithSameLightIDOrDefault(e);
		if (nextEventWithSameLightIDOrDefault != null)
		{
			nextEventWithSameLightIDOrDefault.Prev = e;
		}
		e.Prev = previousEventWithSameLightIDOrDefault;
		e.Next = nextEventWithSameLightIDOrDefault;
	}

	private void RemoveLinkedLightEvents(BaseEvent e)
	{
		if (e.Prev != null)
		{
			if (e.Next != null)
			{
				BaseEvent prev = e.Prev;
				BaseEvent next = e.Next;
				BaseEvent next2 = e.Next;
				BaseEvent prev2 = e.Prev;
				BaseEvent baseEvent = (prev.Next = next2);
				baseEvent = (next.Prev = prev2);
			}
			else
			{
				e.Prev.Next = null;
			}
			if (LoadedContainers.TryGetValue(e.Prev, out var value))
			{
				(value as EventContainer).RefreshAppearance();
			}
		}
	}

	private void AddToAllLightEvents(BaseEvent e)
	{
		if (AllLightEvents.TryGetValue(e.Type, out var value))
		{
			if (e.Prev == null)
			{
				value.Add(e);
			}
			else
			{
				value.Insert(value.IndexOf(e.Prev) + 1, e);
			}
		}
		else
		{
			AllLightEvents.Add(e.Type, new List<BaseEvent> { e });
		}
	}

	private BaseEvent GetPreviousEventWithSameLightIDOrDefault(BaseEvent e)
	{
		if (!AllLightEvents.TryGetValue(e.Type, out var value))
		{
			return null;
		}
		if (Settings.Instance.EmulateChromaAdvanced && Settings.Instance.LightIDTransitionSupport)
		{
			int? thisLightID = e.CustomLightID?.FirstOrDefault();
			return value.FindLast((BaseEvent x) => x.JsonTime < e.JsonTime && thisLightID == x.CustomLightID?.FirstOrDefault());
		}
		return value.FindLast((BaseEvent x) => x.JsonTime < e.JsonTime);
	}

	private BaseEvent GetNextEventWithSameLightIDOrDefault(BaseEvent e)
	{
		if (!AllLightEvents.TryGetValue(e.Type, out var value))
		{
			return null;
		}
		if (Settings.Instance.EmulateChromaAdvanced && Settings.Instance.LightIDTransitionSupport)
		{
			int? thisLightID = e.CustomLightID?.FirstOrDefault();
			return value.Find((BaseEvent x) => x.JsonTime > e.JsonTime && thisLightID == x.CustomLightID?.FirstOrDefault());
		}
		return value.Find((BaseEvent x) => x.JsonTime > e.JsonTime);
	}

	private void UpdatePropagationMode()
	{
		foreach (ObjectContainer value in LoadedContainers.Values)
		{
			if (value is EventContainer eventContainer)
			{
				if (propagationEditing != PropMode.Off)
				{
					value.SafeSetActive(eventContainer.EventData.Type == EventTypeToPropagate);
				}
				else
				{
					value.SafeSetActive(active: true);
				}
				value.UpdateGridPosition();
			}
		}
		if (propagationEditing == PropMode.Off)
		{
			OnPlayToggle(AudioTimeSyncController.IsPlaying);
		}
	}

	private void SpawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (!LoadedContainers.ContainsKey(objectData))
		{
			CreateContainerFromPool(objectData);
		}
	}

	private void DespawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (LoadedContainers.ContainsKey(objectData))
		{
			BaseEvent baseEvent = objectData as BaseEvent;
			if (baseEvent.CustomLightGradient != null && Settings.Instance.VisualizeChromaGradients && base.isActiveAndEnabled)
			{
				StartCoroutine("WaitForGradientThenRecycle", baseEvent);
			}
			else
			{
				RecycleContainer(objectData);
			}
		}
	}

	private IEnumerator WaitForGradientThenRecycle(BaseEvent @event)
	{
		float endTime = @event.JsonTime + @event.CustomLightGradient.Duration;
		yield return new WaitUntil(() => endTime < AudioTimeSyncController.CurrentJsonTime + DespawnCallbackController.Offset);
		RecycleContainer(@event);
	}

	private void OnPlayToggle(bool playing)
	{
		if (!playing)
		{
			StopCoroutine("WaitForGradientThenRecycle");
			RefreshPool();
		}
	}

	private void RecursiveCheckFinished(bool natural, int lastPassedIndex)
	{
		float num = Mathf.Pow(10f, -9f);
		RefreshPool(AudioTimeSyncController.CurrentSongBpmTime + DespawnCallbackController.Offset - num, AudioTimeSyncController.CurrentSongBpmTime + SpawnCallbackController.Offset + num);
	}

	public override ObjectContainer CreateContainer()
	{
		return EventContainer.SpawnEvent(this, null, ref eventPrefab, ref eventAppearanceSo, ref labels);
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		EventAppearanceSO eventAppearanceSO = eventAppearanceSo;
		EventContainer e = con as EventContainer;
		BaseEvent baseEvent = AllBoostEvents.FindLast((BaseEvent x) => x.JsonTime <= obj.JsonTime);
		eventAppearanceSO.SetEventAppearance(e, final: true, baseEvent != null && baseEvent.Value == 1);
		BaseEvent baseEvent2 = obj as BaseEvent;
		if (PropagationEditing != PropMode.Off && baseEvent2.Type != EventTypeToPropagate)
		{
			con.SafeSetActive(active: false);
		}
	}

	private void LinkEventsForChroma(List<BaseEvent> events)
	{
		Dictionary<int, BaseEvent> dictionary = new Dictionary<int, BaseEvent>();
		for (int i = 0; i < events.Count; i++)
		{
			BaseEvent baseEvent = events[i];
			int? num = baseEvent.CustomLightID?.FirstOrDefault();
			if (lightEventsWithKnownPrevNext.Add(baseEvent))
			{
				baseEvent.Prev = null;
				if (dictionary.TryGetValue(num ?? int.MinValue, out var value))
				{
					baseEvent.Prev = value;
					value.Next = baseEvent;
				}
				baseEvent.Next = null;
				for (int j = i + 1; j < events.Count; j++)
				{
					if (num == events[j].CustomLightID?.FirstOrDefault())
					{
						events[j].Prev = baseEvent;
						baseEvent.Next = events[j];
						break;
					}
				}
			}
			dictionary[num ?? int.MinValue] = baseEvent;
		}
	}

	private void LinkEventsForVanilla(List<BaseEvent> events)
	{
		if (events.Count == 0)
		{
			return;
		}
		if (events.Count == 1)
		{
			events[0].Prev = null;
			events[0].Next = null;
			return;
		}
		events[0].Prev = null;
		events[0].Next = events[1];
		for (int i = 1; i < events.Count - 1; i++)
		{
			events[i].Prev = events[i - 1];
			events[i].Next = events[i + 1];
		}
		events[^1].Prev = events[^2];
		events[^1].Next = null;
	}

	public void MarkEventsToBeRelinked(IEnumerable<BaseEvent> events)
	{
		foreach (BaseEvent @event in events)
		{
			MarkEventToBeRelinked(@event);
		}
	}

	public void MarkEventToBeRelinked(BaseEvent e)
	{
		lightEventsWithKnownPrevNext.Remove(e.Prev);
		lightEventsWithKnownPrevNext.Remove(e);
		lightEventsWithKnownPrevNext.Remove(e.Next);
	}

	public void LinkAllLightEvents()
	{
		AllLightEvents = (from x in MapObjects
			where x.IsLightEvent()
			group x by x.Type).ToDictionary((IGrouping<int, BaseEvent> g) => g.Key, (IGrouping<int, BaseEvent> g) => g.ToList());
	}

	public void RefreshEventsAppearance(IEnumerable<BaseEvent> events)
	{
		foreach (BaseEvent @event in events)
		{
			if (@event.Prev != null && LoadedContainers.TryGetValue(@event.Prev, out var value))
			{
				(value as EventContainer).RefreshAppearance();
			}
			if (LoadedContainers.TryGetValue(@event, out var value2))
			{
				(value2 as EventContainer).RefreshAppearance();
			}
		}
	}

	public void UpdateColor(Color red, Color redBoost, Color blue, Color blueBoost, Color white, Color whiteBoost)
	{
		eventAppearanceSo.RedColor = red;
		eventAppearanceSo.RedBoostColor = redBoost;
		eventAppearanceSo.BlueColor = blue;
		eventAppearanceSo.BlueBoostColor = blueBoost;
		eventAppearanceSo.WhiteColor = white;
		eventAppearanceSo.WhiteBoostColor = whiteBoost;
	}
}
