using System;
using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class ArcGridContainer : BeatmapObjectContainerCollection<BaseArc>
{
	[SerializeField]
	private GameObject arcPrefab;

	[FormerlySerializedAs("arcAppearanceSO")]
	[SerializeField]
	private ArcAppearanceSO arcAppearanceSO;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private CountersPlusController countersPlus;

	private bool isPlaying;

	private Queue<ArcContainer> queuedUpdatingArcs = new Queue<ArcContainer>();

	private const int maxRecomputePerFrame = 2;

	public override ObjectType ContainerType => ObjectType.Arc;

	public override ObjectContainer CreateContainer()
	{
		return ArcContainer.SpawnArc(null, ref arcPrefab);
	}

	protected override void OnObjectSpawned(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Arcs);
	}

	protected override void OnObjectDelete(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Arcs);
	}

	internal override void SubscribeToCallbacks()
	{
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	internal override void UnsubscribeToCallbacks()
	{
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	internal override void LateUpdate()
	{
		base.LateUpdate();
		ScheduleRecomputePosition();
	}

	private void SpawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (!LoadedContainers.ContainsKey(objectData))
		{
			CreateContainerFromPool(objectData);
		}
	}

	private void RecursiveCheckFinished(bool natural, int lastPassedIndex)
	{
		RefreshPool();
	}

	private void DespawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (LoadedContainers.ContainsKey(objectData))
		{
			RecycleContainer(objectData);
		}
	}

	private void OnPlayToggle(bool isPlaying)
	{
		this.isPlaying = isPlaying;
		foreach (ArcContainer value in LoadedContainers.Values)
		{
			value.SetIndicatorBlocksActive(!this.isPlaying);
		}
	}

	public void UpdateColor(Color red, Color blue)
	{
		arcAppearanceSO.UpdateColor(red, blue);
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		ArcContainer arcContainer = con as ArcContainer;
		BaseArc baseArc = obj as BaseArc;
		arcContainer.NotifySplineChanged(baseArc);
		arcAppearanceSO.SetArcAppearance(arcContainer);
		arcContainer.Setup();
		arcContainer.SetIndicatorBlocksActive(visible: false);
		tracksManager.GetTrackAtTime(baseArc.SongBpmTime).AttachContainer(con);
	}

	public void RequestForSplineRecompute(ArcContainer container)
	{
		queuedUpdatingArcs.Enqueue(container);
	}

	private void ScheduleRecomputePosition()
	{
		for (int i = 0; i < 2; i++)
		{
			if (queuedUpdatingArcs.Count == 0)
			{
				break;
			}
			ArcContainer arcContainer = queuedUpdatingArcs.Dequeue();
			arcContainer.RecomputePosition();
			arcContainer.SetIndicatorBlocksActive(!isPlaying);
		}
	}
}
