using System;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class ObstacleGridContainer : BeatmapObjectContainerCollection<BaseObstacle>
{
	[SerializeField]
	private GameObject obstaclePrefab;

	[FormerlySerializedAs("obstacleAppearanceSO")]
	[SerializeField]
	private ObstacleAppearanceSO obstacleAppearanceSo;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private CountersPlusController countersPlus;

	public BaseObstacle[] SpawnSortedObjects;

	private int spawnIndex;

	public BaseObstacle[] DespawnSortedObjects;

	private int despawnIndex;

	private static readonly int outsideAlpha = Shader.PropertyToID("_OutsideAlpha");

	private static readonly int mainAlpha = Shader.PropertyToID("_MainAlpha");

	private bool updateFrame;

	public override ObjectType ContainerType => ObjectType.Obstacle;

	internal override void SubscribeToCallbacks()
	{
		Shader.SetGlobalFloat(outsideAlpha, 0.25f);
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		AudioTimeSyncController audioTimeSyncController2 = AudioTimeSyncController;
		audioTimeSyncController2.TimeChanged = (Action)Delegate.Combine(audioTimeSyncController2.TimeChanged, new Action(OnTimeChanged));
		UIMode.PreviewModeSwitched = (Action)Delegate.Combine(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.NotifyBySettingName("ObstacleOpacity", ObstacleOpacityChanged);
		ObstacleOpacityChanged(Settings.Instance.ObstacleOpacity);
	}

	internal override void UnsubscribeToCallbacks()
	{
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		AudioTimeSyncController audioTimeSyncController2 = AudioTimeSyncController;
		audioTimeSyncController2.TimeChanged = (Action)Delegate.Remove(audioTimeSyncController2.TimeChanged, new Action(OnTimeChanged));
		UIMode.PreviewModeSwitched = (Action)Delegate.Remove(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.ClearSettingNotifications("ObstacleOpacity");
	}

	private void ObstacleOpacityChanged(object obj)
	{
		Shader.SetGlobalFloat(mainAlpha, (float)obj);
	}

	private void OnPlayToggle(bool playing)
	{
		Shader.SetGlobalFloat(outsideAlpha, playing ? 0f : 0.25f);
	}

	public override void RefreshPool(bool force)
	{
		if (UIMode.AnimationMode)
		{
			SpawnSortedObjects = MapObjects.OrderBy((BaseObstacle o) => o.SpawnSongBpmTime).ToArray();
			DespawnSortedObjects = MapObjects.OrderBy((BaseObstacle o) => o.DespawnSongBpmTime).ToArray();
			RefreshWalls();
		}
		else
		{
			base.RefreshPool(force);
		}
	}

	private void OnUIPreviewModeSwitch()
	{
		RefreshPool(true);
	}

	public void UpdateColor(Color obstacle)
	{
		obstacleAppearanceSo.DefaultObstacleColor = obstacle;
	}

	internal override void LateUpdate()
	{
		if (!UIMode.AnimationMode)
		{
			base.LateUpdate();
		}
	}

	private void OnTimeChanged()
	{
		if (!UIMode.AnimationMode)
		{
			return;
		}
		float currentSongBpmTime = AudioTimeSyncController.CurrentSongBpmTime;
		if (AudioTimeSyncController.IsPlaying)
		{
			while (spawnIndex < SpawnSortedObjects.Length && currentSongBpmTime + 2f >= SpawnSortedObjects[spawnIndex].SpawnSongBpmTime)
			{
				if (SpawnSortedObjects[spawnIndex].HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
				{
					CreateContainerFromPool(SpawnSortedObjects[spawnIndex]);
				}
				spawnIndex++;
			}
			while (despawnIndex < DespawnSortedObjects.Length && currentSongBpmTime >= DespawnSortedObjects[despawnIndex].DespawnSongBpmTime)
			{
				BaseObstacle baseObstacle = DespawnSortedObjects[despawnIndex];
				if (LoadedContainers.ContainsKey(baseObstacle))
				{
					if (!LoadedContainers[baseObstacle].Animator.AnimatedLife)
					{
						RecycleContainer(baseObstacle);
					}
					else
					{
						LoadedContainers[baseObstacle].Animator.ShouldRecycle = true;
					}
				}
				despawnIndex++;
			}
		}
		else
		{
			RefreshWalls();
		}
	}

	private void RefreshWalls()
	{
		float time = AudioTimeSyncController.CurrentSongBpmTime;
		foreach (ObjectContainer item in LoadedContainers.Values.ToList())
		{
			RecycleContainer(item.ObjectData);
		}
		GetIndexes(time, (int i) => SpawnSortedObjects[i].SpawnSongBpmTime, SpawnSortedObjects.Length, out spawnIndex, out var next);
		GetIndexes(time, (int i) => DespawnSortedObjects[i].DespawnSongBpmTime, DespawnSortedObjects.Length, out despawnIndex, out next);
		foreach (BaseObstacle item2 in SpawnSortedObjects.Where((BaseObstacle o) => o.SpawnSongBpmTime <= time && time < o.DespawnSongBpmTime))
		{
			if (item2.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				CreateContainerFromPool(item2);
			}
		}
	}

	protected override void OnObjectSpawned(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Obstacles);
	}

	protected override void OnObjectDelete(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Obstacles);
	}

	public override ObjectContainer CreateContainer()
	{
		ObstacleContainer obstacleContainer = ObstacleContainer.SpawnObstacle(null, tracksManager, ref obstaclePrefab);
		obstacleContainer.Animator.Atsc = AudioTimeSyncController;
		obstacleContainer.Animator.TracksManager = tracksManager;
		return obstacleContainer;
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		ObstacleContainer obstacleContainer = con as ObstacleContainer;
		if (!obstacleContainer.IsRotatedByNoodleExtensions && !obstacleContainer.Animator.AnimatedTrack)
		{
			tracksManager.GetTrackAtTime(obj.SongBpmTime).AttachContainer(con);
		}
		obstacleAppearanceSo.SetObstacleAppearance(obstacleContainer);
	}

	private void GetIndexes(float time, Func<int, float> getter, int count, out int prev, out int next)
	{
		prev = 0;
		next = count;
		while (prev < next - 1)
		{
			int num = (prev + next) / 2;
			if (getter(num) < time)
			{
				prev = num;
			}
			else
			{
				next = num;
			}
		}
	}
}
