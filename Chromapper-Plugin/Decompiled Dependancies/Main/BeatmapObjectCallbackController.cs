using System;
using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class BeatmapObjectCallbackController : MonoBehaviour
{
	private static readonly int eventsToLookAhead = 75;

	private static readonly int notesToLookAhead = 25;

	[FormerlySerializedAs("notesContainer")]
	[SerializeField]
	private NoteGridContainer noteGridContainer;

	[FormerlySerializedAs("eventsContainer")]
	[SerializeField]
	private EventGridContainer eventGridContainer;

	[SerializeField]
	private AudioTimeSyncController timeSyncController;

	[SerializeField]
	private UIMode uiMode;

	[SerializeField]
	private bool useOffsetFromConfig = true;

	[Tooltip("Whether or not to use the Despawn or Spawn offset from settings.")]
	[SerializeField]
	private bool useDespawnOffset;

	[FormerlySerializedAs("offset")]
	public float Offset;

	[SerializeField]
	private int nextNoteIndex;

	[SerializeField]
	private int nextEventIndex;

	[SerializeField]
	private int nextChainIndex;

	[FormerlySerializedAs("useAudioTime")]
	public bool UseAudioTime;

	private float curTime;

	public Action<bool, int, BaseObject> NotePassedThreshold;

	public Action<bool, int> RecursiveNoteCheckFinished;

	public Action<bool, int, BaseObject> EventPassedThreshold;

	public Action<bool, int> RecursiveEventCheckFinished;

	public Action<bool, int, BaseObject> ChainPassedThreshold;

	public Action<bool, int> RecursiveChainCheckFinished;

	[FormerlySerializedAs("chainsContainer")]
	[SerializeField]
	private ChainGridContainer chainGridContainer;

	private static readonly int obstacleFadeRadius = Shader.PropertyToID("_ObstacleFadeRadius");

	private void Start()
	{
		noteGridContainer.ObjectSpawnedEvent += NoteGridContainerObjectSpawnedEvent;
		noteGridContainer.ObjectDeletedEvent += NoteGridContainerObjectDeletedEvent;
		eventGridContainer.ObjectSpawnedEvent += EventGridContainerObjectSpawnedEventGrid;
		eventGridContainer.ObjectDeletedEvent += EventGridContainerObjectDeletedEventGrid;
		chainGridContainer.ObjectSpawnedEvent += ChainGridContainerObjectSpawnedEvent;
		chainGridContainer.ObjectDeletedEvent += ChainGridContainerObjectDeletedEvent;
	}

	private void LateUpdate()
	{
		if (useOffsetFromConfig)
		{
			UIModeType selectedMode = UIMode.SelectedMode;
			if (selectedMode == UIModeType.Playing || selectedMode == UIModeType.Preview)
			{
				if (useDespawnOffset)
				{
					Offset = 0f;
				}
				else
				{
					float noteJumpSpeed = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
					float noteStartBeatOffset = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteStartBeatOffset;
					float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
					Offset = SpawnParameterHelper.CalculateHalfJumpDuration(noteJumpSpeed, noteStartBeatOffset, beatsPerMinute);
				}
			}
			else
			{
				Offset = (useDespawnOffset ? (Settings.Instance.Offset_Despawning * -1) : Settings.Instance.Offset_Spawning);
			}
			if (!useDespawnOffset)
			{
				Shader.SetGlobalFloat(obstacleFadeRadius, Offset * EditorScaleController.EditorScale);
			}
		}
		if (timeSyncController.IsPlaying)
		{
			curTime = (UseAudioTime ? timeSyncController.CurrentAudioBeats : timeSyncController.CurrentSongBpmTime);
			RecursiveCheckNotes(init: true, natural: true);
			RecursiveCheckEvents(init: true, natural: true);
			if (chainGridContainer != null)
			{
				RecursiveCheckChains(init: true, natural: true);
			}
		}
	}

	private void OnEnable()
	{
		AudioTimeSyncController audioTimeSyncController = timeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnDisable()
	{
		AudioTimeSyncController audioTimeSyncController = timeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnPlayToggle(bool playing)
	{
		if (playing)
		{
			CheckAllNotes(natural: false);
			CheckAllEvents(natural: false);
			if (chainGridContainer != null)
			{
				CheckAllChains(natural: false);
			}
		}
	}

	private void CheckAllNotes(bool natural)
	{
		float num = (UseAudioTime ? timeSyncController.CurrentAudioBeats : timeSyncController.CurrentSongBpmTime);
		nextNoteIndex = noteGridContainer.MapObjects.BinarySearchBy(num + Offset, (BaseNote obj) => obj.SongBpmTime);
		if (nextNoteIndex < 0)
		{
			nextNoteIndex = ~nextNoteIndex;
		}
		RecursiveNoteCheckFinished?.Invoke(natural, nextNoteIndex - 1);
	}

	private void CheckAllEvents(bool natural)
	{
		float num = (UseAudioTime ? timeSyncController.CurrentAudioBeats : timeSyncController.CurrentSongBpmTime);
		nextEventIndex = eventGridContainer.MapObjects.BinarySearchBy(num + Offset, (BaseEvent obj) => obj.SongBpmTime);
		if (nextEventIndex < 0)
		{
			nextEventIndex = ~nextEventIndex;
		}
		RecursiveEventCheckFinished?.Invoke(natural, nextEventIndex - 1);
	}

	private void CheckAllChains(bool natural)
	{
		float num = (UseAudioTime ? timeSyncController.CurrentAudioBeats : timeSyncController.CurrentSongBpmTime);
		nextChainIndex = chainGridContainer.MapObjects.BinarySearchBy(num + Offset, (BaseChain obj) => obj.SongBpmTime);
		if (nextChainIndex < 0)
		{
			nextChainIndex = ~nextChainIndex;
		}
		RecursiveChainCheckFinished?.Invoke(natural, nextChainIndex - 1);
	}

	private void RecursiveCheckNotes(bool init, bool natural)
	{
		List<BaseNote> mapObjects = noteGridContainer.MapObjects;
		bool flag = useOffsetFromConfig && !useDespawnOffset && UIMode.AnimationMode;
		while (nextNoteIndex < mapObjects.Count)
		{
			BaseNote baseNote = mapObjects[nextNoteIndex];
			float num = (flag ? (baseNote.Hjd + 2f) : Offset);
			if (baseNote.SongBpmTime > curTime + num)
			{
				break;
			}
			if (baseNote.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				NotePassedThreshold?.Invoke(natural, nextNoteIndex, baseNote);
			}
			nextNoteIndex++;
		}
	}

	private void RecursiveCheckEvents(bool init, bool natural)
	{
		List<BaseEvent> mapObjects = eventGridContainer.MapObjects;
		while (nextEventIndex < mapObjects.Count)
		{
			BaseEvent baseEvent = mapObjects[nextEventIndex];
			if (baseEvent.SongBpmTime > curTime + Offset)
			{
				break;
			}
			EventPassedThreshold?.Invoke(natural, nextEventIndex, baseEvent);
			nextEventIndex++;
		}
	}

	private void RecursiveCheckChains(bool init, bool natural)
	{
		List<BaseChain> mapObjects = chainGridContainer.MapObjects;
		bool flag = useOffsetFromConfig && !useDespawnOffset && UIMode.AnimationMode;
		while (nextChainIndex < mapObjects.Count)
		{
			BaseChain baseChain = mapObjects[nextChainIndex];
			float num = (flag ? (baseChain.Hjd + 2f) : Offset);
			if (baseChain.TailSongBpmTime > curTime + num)
			{
				break;
			}
			ChainPassedThreshold?.Invoke(natural, nextChainIndex, baseChain);
			nextChainIndex++;
		}
	}

	private void NoteGridContainerObjectSpawnedEvent(BaseObject obj)
	{
		OnObjSpawn(obj, ref nextNoteIndex);
	}

	private void NoteGridContainerObjectDeletedEvent(BaseObject obj)
	{
		OnObjDeleted(obj, ref nextNoteIndex);
	}

	private void EventGridContainerObjectSpawnedEventGrid(BaseObject obj)
	{
		OnObjSpawn(obj, ref nextEventIndex);
	}

	private void EventGridContainerObjectDeletedEventGrid(BaseObject obj)
	{
		OnObjDeleted(obj, ref nextEventIndex);
	}

	private void ChainGridContainerObjectSpawnedEvent(BaseObject obj)
	{
		OnObjSpawn(obj, ref nextChainIndex);
	}

	private void ChainGridContainerObjectDeletedEvent(BaseObject obj)
	{
		OnObjDeleted(obj, ref nextChainIndex);
	}

	private void OnObjSpawn(BaseObject obj, ref int idx)
	{
		if (timeSyncController.IsPlaying && !(obj.SongBpmTime >= curTime + Offset))
		{
			idx++;
		}
	}

	private void OnObjDeleted(BaseObject obj, ref int idx)
	{
		if (timeSyncController.IsPlaying && !(obj.SongBpmTime >= curTime + Offset))
		{
			idx--;
		}
	}

	private void OnDestroy()
	{
		noteGridContainer.ObjectSpawnedEvent -= NoteGridContainerObjectSpawnedEvent;
		noteGridContainer.ObjectDeletedEvent -= NoteGridContainerObjectDeletedEvent;
		eventGridContainer.ObjectSpawnedEvent -= EventGridContainerObjectSpawnedEventGrid;
		eventGridContainer.ObjectDeletedEvent -= EventGridContainerObjectDeletedEventGrid;
		chainGridContainer.ObjectSpawnedEvent -= ChainGridContainerObjectSpawnedEvent;
		chainGridContainer.ObjectDeletedEvent -= ChainGridContainerObjectDeletedEvent;
	}
}
