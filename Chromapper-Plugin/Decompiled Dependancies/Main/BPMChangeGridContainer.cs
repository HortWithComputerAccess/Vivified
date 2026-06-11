using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;

public class BPMChangeGridContainer : BeatmapObjectContainerCollection<BaseBpmEvent>
{
	public static readonly int MaxBpmChangesInShader = 170;

	private static readonly int times = Shader.PropertyToID("_BPMChange_Times");

	private static readonly int jsonTimes = Shader.PropertyToID("_BPMChange_Json_Times");

	private static readonly int bpMs = Shader.PropertyToID("_BPMChange_BPMs");

	private static readonly int bpmCount = Shader.PropertyToID("_BPMChange_Count");

	private static readonly int songBpm = Shader.PropertyToID("_SongBPM");

	private static readonly int editorScale = Shader.PropertyToID("_EditorScale");

	private static readonly float firstVisibleBeatTime = 2f;

	private static readonly float[] bpmShaderTimes = new float[MaxBpmChangesInShader];

	private static readonly float[] bpmShaderJsonTimes = new float[MaxBpmChangesInShader];

	private static readonly float[] bpmShaderBpMs = new float[MaxBpmChangesInShader];

	[SerializeField]
	private Transform gridRendererParent;

	[SerializeField]
	private GameObject bpmPrefab;

	[SerializeField]
	private MeasureLinesController measureLinesController;

	[SerializeField]
	private CountersPlusController countersPlus;

	public override ObjectType ContainerType => ObjectType.BpmChange;

	private void Start()
	{
		Shader.SetGlobalFloat(songBpm, BeatSaberSongContainer.Instance.Info.BeatsPerMinute);
	}

	internal override void SubscribeToCallbacks()
	{
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Combine(EditorScaleController.EditorScaleChangedEvent, new Action<float>(EditorScaleChanged));
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(RefreshModifiedBeat));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.TimeChanged = (Action)Delegate.Combine(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
	}

	private void EditorScaleChanged(float obj)
	{
		Shader.SetGlobalFloat(editorScale, EditorScaleController.EditorScale);
	}

	private void OnTimeChanged()
	{
		if (!AudioTimeSyncController.IsPlaying)
		{
			RefreshGridProperties();
		}
	}

	internal override void UnsubscribeToCallbacks()
	{
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Remove(EditorScaleController.EditorScaleChangedEvent, new Action<float>(EditorScaleChanged));
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(RefreshModifiedBeat));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.TimeChanged = (Action)Delegate.Remove(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
	}

	protected override void OnObjectDelete(BaseObject obj, bool _ = false)
	{
		OnObjectDeleteOrSpawn(obj);
	}

	protected override void OnObjectSpawned(BaseObject obj, bool _ = false)
	{
		OnObjectDeleteOrSpawn(obj);
	}

	private void OnObjectDeleteOrSpawn(BaseObject obj)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.BpmEvents);
		obj.RecomputeSongBpmTime();
		BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(obj.JsonTime);
		RefreshModifiedBeat();
	}

	public void RefreshModifiedBeat()
	{
		BaseBpmEvent baseBpmEvent = null;
		foreach (BaseBpmEvent mapObject in MapObjects)
		{
			if (baseBpmEvent == null)
			{
				mapObject.Beat = Mathf.CeilToInt(mapObject.JsonTime);
			}
			else
			{
				float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
				float f = (mapObject.JsonTime - baseBpmEvent.JsonTime - 0.01f) / beatsPerMinute * baseBpmEvent.Bpm;
				mapObject.Beat = baseBpmEvent.Beat + Mathf.CeilToInt(f);
			}
			baseBpmEvent = mapObject;
		}
		RefreshGridProperties();
		measureLinesController.RefreshMeasureLines();
	}

	public void RefreshGridProperties()
	{
		BeatSaberSongContainer instance = BeatSaberSongContainer.Instance;
		int num = 1;
		bpmShaderTimes[0] = 0f;
		bpmShaderJsonTimes[0] = 0f;
		bpmShaderBpMs[0] = instance.Info.BeatsPerMinute;
		BaseBpmEvent baseBpmEvent = instance.Map.FindLastBpmEventBySongBpmTime(AudioTimeSyncController.CurrentSongBpmTime - firstVisibleBeatTime);
		if (baseBpmEvent != null)
		{
			num = 2;
			bpmShaderTimes[1] = baseBpmEvent.SongBpmTime;
			bpmShaderJsonTimes[1] = baseBpmEvent.JsonTime;
			bpmShaderBpMs[1] = baseBpmEvent.Bpm;
		}
		if (LoadedContainers.Count > 0)
		{
			foreach (KeyValuePair<BaseObject, ObjectContainer> item in LoadedContainers.OrderBy((KeyValuePair<BaseObject, ObjectContainer> x) => x.Key.JsonTime))
			{
				if (num >= MaxBpmChangesInShader)
				{
					Debug.LogError($":hyperPepega: :mega: THE CAP FOR BPM CHANGES IN THE SHADER IS {MaxBpmChangesInShader - 1}");
					break;
				}
				BaseBpmEvent baseBpmEvent2 = item.Key as BaseBpmEvent;
				bpmShaderTimes[num] = baseBpmEvent2.SongBpmTime;
				bpmShaderJsonTimes[num] = baseBpmEvent2.JsonTime;
				bpmShaderBpMs[num] = baseBpmEvent2.Bpm;
				num++;
			}
		}
		Shader.SetGlobalFloatArray(times, bpmShaderTimes);
		Shader.SetGlobalFloatArray(jsonTimes, bpmShaderJsonTimes);
		Shader.SetGlobalFloatArray(bpMs, bpmShaderBpMs);
		Shader.SetGlobalInt(bpmCount, num);
	}

	protected override void OnContainerSpawn(ObjectContainer container, BaseObject obj)
	{
		RefreshGridProperties();
	}

	protected override void OnContainerDespawn(ObjectContainer container, BaseObject obj)
	{
		RefreshGridProperties();
	}

	public override ObjectContainer CreateContainer()
	{
		return BpmEventContainer.SpawnBpmChange(null, ref bpmPrefab);
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		(con as BpmEventContainer).UpdateBpmText();
	}
}
