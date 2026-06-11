using System;
using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class DingOnNotePassingGrid : MonoBehaviour
{
	public static Dictionary<int, bool> NoteTypeToDing = new Dictionary<int, bool>
	{
		{ 0, true },
		{ 1, true },
		{ 3, false }
	};

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private AudioSource source;

	[SerializeField]
	private SoundList[] soundLists;

	[FormerlySerializedAs("DensityCheckOffset")]
	[SerializeField]
	private int densityCheckOffset = 2;

	[FormerlySerializedAs("ThresholdInNoteTime")]
	[SerializeField]
	private float thresholdInNoteTime = 0.25f;

	[SerializeField]
	private AudioUtil audioUtil;

	[SerializeField]
	private NoteGridContainer container;

	[SerializeField]
	private BeatmapObjectCallbackController defaultCallbackController;

	[SerializeField]
	private BeatmapObjectCallbackController beatSaberCutCallbackController;

	[SerializeField]
	private BongoCat bongocat;

	[SerializeField]
	private GameObject discordPingPrefab;

	[SerializeField]
	private float difference;

	private float lastCheckedTime;

	private float offset;

	private float songSpeed = 1f;

	private void Start()
	{
		NoteTypeToDing[0] = Settings.Instance.Ding_Red_Notes;
		NoteTypeToDing[1] = Settings.Instance.Ding_Blue_Notes;
		NoteTypeToDing[3] = Settings.Instance.Ding_Bombs;
		beatSaberCutCallbackController.Offset = container.AudioTimeSyncController.GetBeatFromSeconds(0.5f);
		beatSaberCutCallbackController.UseAudioTime = true;
		UpdateHitSoundType(Settings.Instance.NoteHitSound);
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnEnable()
	{
		Settings.NotifyBySettingName("Ding_Red_Notes", UpdateRedNoteDing);
		Settings.NotifyBySettingName("Ding_Blue_Notes", UpdateBlueNoteDing);
		Settings.NotifyBySettingName("Ding_Bombs", UpdateBombDing);
		Settings.NotifyBySettingName("NoteHitSound", UpdateHitSoundType);
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
		BeatmapObjectCallbackController beatmapObjectCallbackController = beatSaberCutCallbackController;
		beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(PlaySound));
		BeatmapObjectCallbackController beatmapObjectCallbackController2 = defaultCallbackController;
		beatmapObjectCallbackController2.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController2.NotePassedThreshold, new Action<bool, int, BaseObject>(TriggerBongoCat));
		BeatmapObjectCallbackController beatmapObjectCallbackController3 = beatSaberCutCallbackController;
		beatmapObjectCallbackController3.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController3.ChainPassedThreshold, new Action<bool, int, BaseObject>(PlaySound));
	}

	private void OnDisable()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = beatSaberCutCallbackController;
		beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(PlaySound));
		BeatmapObjectCallbackController beatmapObjectCallbackController2 = defaultCallbackController;
		beatmapObjectCallbackController2.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController2.NotePassedThreshold, new Action<bool, int, BaseObject>(TriggerBongoCat));
		BeatmapObjectCallbackController beatmapObjectCallbackController3 = beatSaberCutCallbackController;
		beatmapObjectCallbackController3.ChainPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController3.ChainPassedThreshold, new Action<bool, int, BaseObject>(PlaySound));
		Settings.ClearSettingNotifications("Ding_Red_Notes");
		Settings.ClearSettingNotifications("Ding_Blue_Notes");
		Settings.ClearSettingNotifications("Ding_Bombs");
		Settings.ClearSettingNotifications("NoteHitSound");
		Settings.ClearSettingNotifications("SongSpeed");
	}

	private void OnDestroy()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void UpdateSongSpeed(object value)
	{
		float num = (float)Convert.ChangeType(value, typeof(float));
		songSpeed = num / 10f;
	}

	private void OnPlayToggle(bool playing)
	{
		lastCheckedTime = -1f;
		audioUtil.StopOneShot();
		if (playing)
		{
			BaseDifficulty map = BeatSaberSongContainer.Instance.Map;
			float jsonTime = map.SongBpmTimeToJsonTime(atsc.CurrentAudioBeats).Value;
			float jsonTime2 = map.SongBpmTimeToJsonTime(atsc.CurrentAudioBeats + beatSaberCutCallbackController.Offset).Value;
			Span<BaseNote> between = container.GetBetween(jsonTime, jsonTime2);
			for (int i = 0; i < between.Length; i++)
			{
				BaseNote objectData = between[i];
				PlaySound(initial: false, 0, objectData);
			}
		}
	}

	private void UpdateRedNoteDing(object obj)
	{
		NoteTypeToDing[0] = (bool)obj;
	}

	private void UpdateBlueNoteDing(object obj)
	{
		NoteTypeToDing[1] = (bool)obj;
	}

	private void UpdateBombDing(object obj)
	{
		NoteTypeToDing[3] = (bool)obj;
	}

	private void UpdateHitSoundType(object obj)
	{
		if ((int)obj == 1)
		{
			offset = 0.18f;
		}
		else
		{
			offset = 0f;
		}
	}

	private void TriggerBongoCat(bool initial, int index, BaseObject objectData)
	{
		if (!(objectData.SongBpmTime - container.AudioTimeSyncController.CurrentSongBpmTime <= -0.5f))
		{
			if (Settings.Instance.NoteHitSound == 9)
			{
				UnityEngine.Object.Instantiate(discordPingPrefab, base.gameObject.transform, worldPositionStays: true);
			}
			bongocat.TriggerArm(objectData as BaseNote, container);
		}
	}

	private void PlaySound(bool initial, int index, BaseObject objectData)
	{
		if (!(objectData.SongBpmTime - container.AudioTimeSyncController.CurrentSongBpmTime <= -0.5f) && !(objectData is BaseChain) && !(objectData as BaseNote).CustomFake && objectData.SongBpmTime != lastCheckedTime && NoteTypeToDing[((BaseNote)objectData).Type])
		{
			bool isShortCut = objectData.SongBpmTime - lastCheckedTime < thresholdInNoteTime;
			lastCheckedTime = objectData.SongBpmTime;
			int noteHitSound = Settings.Instance.NoteHitSound;
			SoundList soundList = soundLists[noteHitSound];
			float beat = objectData.SongBpmTime - atsc.CurrentAudioBeats;
			float delay = atsc.GetSecondsFromBeat(beat) / songSpeed - offset;
			audioUtil.PlayOneShotSound(soundList.GetRandomClip(isShortCut), Settings.Instance.NoteHitVolume, 1f, delay);
		}
	}
}
