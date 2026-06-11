using System;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;

public class ChainGridContainer : BeatmapObjectContainerCollection<BaseChain>
{
	[SerializeField]
	private GameObject chainPrefab;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private ChainAppearanceSO chainAppearanceSO;

	[SerializeField]
	private CountersPlusController countersPlus;

	public const float ViewEpsilon = 0.1f;

	private bool isPlaying;

	public override ObjectType ContainerType => ObjectType.Chain;

	public override ObjectContainer CreateContainer()
	{
		ChainContainer chainContainer = ChainContainer.SpawnChain(null, ref chainPrefab);
		chainContainer.Animator.Atsc = AudioTimeSyncController;
		chainContainer.Animator.TracksManager = tracksManager;
		return chainContainer;
	}

	public void UpdateColor(Color red, Color blue)
	{
		chainAppearanceSO.UpdateColor(red, blue);
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		ChainContainer chainContainer = con as ChainContainer;
		BaseChain baseChain = (chainContainer.ChainData = obj as BaseChain);
		chainAppearanceSO.SetChainAppearance(chainContainer);
		chainContainer.Setup();
		chainContainer.SetIndicatorBlocksActive(!isPlaying);
		if (!chainContainer.Animator.AnimatedTrack)
		{
			tracksManager.GetTrackAtTime(baseChain.SongBpmTime).AttachContainer(con);
		}
	}

	protected override void OnObjectSpawned(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Chains);
	}

	protected override void OnObjectDelete(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Chains);
	}

	internal override void SubscribeToCallbacks()
	{
		(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note) as NoteGridContainer).ContainerSpawnedEvent += CheckUpdatedNote;
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(spawnCallbackController.ChainPassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveChainCheckFinished = (Action<bool, int>)Delegate.Combine(spawnCallbackController2.RecursiveChainCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(despawnCallbackController.ChainPassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		UIMode.PreviewModeSwitched = (Action)Delegate.Combine(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.NotifyBySettingName("NoteColorMultiplier", AppearanceChanged);
		Settings.NotifyBySettingName("ArrowColorMultiplier", AppearanceChanged);
		Settings.NotifyBySettingName("ArrowColorWhiteBlend", AppearanceChanged);
		Settings.NotifyBySettingName("AccurateNoteSize", AppearanceChanged);
	}

	internal override void UnsubscribeToCallbacks()
	{
		NoteGridContainer noteGridContainer = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note) as NoteGridContainer;
		if (noteGridContainer != null)
		{
			noteGridContainer.ContainerSpawnedEvent -= CheckUpdatedNote;
		}
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(spawnCallbackController.ChainPassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveChainCheckFinished = (Action<bool, int>)Delegate.Remove(spawnCallbackController2.RecursiveChainCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(despawnCallbackController.ChainPassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		UIMode.PreviewModeSwitched = (Action)Delegate.Remove(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.ClearSettingNotifications("NoteColorMultiplier");
		Settings.ClearSettingNotifications("ArrowColorMultiplier");
		Settings.ClearSettingNotifications("ArrowColorWhiteBlend");
		Settings.ClearSettingNotifications("AccurateNoteSize");
	}

	private void OnPlayToggle(bool isPlaying)
	{
		if (!isPlaying)
		{
			RefreshPool();
		}
		this.isPlaying = isPlaying;
		foreach (ChainContainer value in LoadedContainers.Values)
		{
			value.SetIndicatorBlocksActive(!this.isPlaying);
		}
	}

	private void OnUIPreviewModeSwitch()
	{
		RefreshPool(forceRefresh: true);
	}

	private void RecursiveCheckFinished(bool natural, int lastPassedIndex)
	{
		RefreshPool();
	}

	private void AppearanceChanged(object _)
	{
		RefreshPool(forceRefresh: true);
	}

	protected override void OnContainerSpawn(ObjectContainer container, BaseObject obj)
	{
		(container as ChainContainer).DetectHeadNote();
	}

	protected override void OnContainerDespawn(ObjectContainer container, BaseObject obj)
	{
		(container as ChainContainer).DetachHeadNote();
	}

	private void CheckUpdatedNote(BaseObject obj)
	{
		BaseNote baseNote = obj as BaseNote;
		if (baseNote.Type == 3)
		{
			return;
		}
		Span<BaseChain> between = GetBetween(baseNote.JsonTime - 0.1f, baseNote.JsonTime + 0.1f);
		for (int i = 0; i < between.Length; i++)
		{
			BaseChain key = between[i];
			LoadedContainers.TryGetValue(key, out var value);
			ChainContainer chainContainer = value as ChainContainer;
			if (!(chainContainer == null) && chainContainer.IsHeadNote(baseNote))
			{
				BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note).LoadedContainers.TryGetValue(baseNote, out var value2);
				chainContainer.AttachedHead = value2 as NoteContainer;
				chainContainer.DetectHeadNote(detect: false);
				break;
			}
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
			RecycleContainer(objectData);
		}
	}
}
