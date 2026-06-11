using System;
using Beatmap.Base;
using UnityEngine;

public class MetronomeHandler : MonoBehaviour
{
	private static readonly int animatorBpm = Animator.StringToHash("BPM");

	[SerializeField]
	private BPMChangeGridContainer bpmChangeGridContainer;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private AudioClip metronomeSound;

	[SerializeField]
	private AudioClip moreCowbellSound;

	[SerializeField]
	private AudioClip cowbellSound;

	[SerializeField]
	private AudioUtil audioUtil;

	[SerializeField]
	private GameObject metronomeUI;

	public bool CowBell;

	private float queuedDingSongBpmTime;

	private Animator metronomeUIAnimator;

	private bool metronomeUIDirection = true;

	private bool metronomeActive = true;

	private float songSpeed = 1f;

	private void Start()
	{
		metronomeUIAnimator = metronomeUI.GetComponent<Animator>();
		metronomeActive = Settings.Instance.MetronomeVolume != 0f;
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
		Settings.NotifyBySettingName("MetronomeVolume", delegate(object value)
		{
			if ((float)value != 0f && !metronomeActive)
			{
				metronomeActive = true;
			}
			else if (metronomeActive && (float)value == 0f)
			{
				metronomeActive = false;
			}
		});
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void LateUpdate()
	{
		if (metronomeActive && atsc.IsPlaying && !atsc.StopScheduled)
		{
			if (atsc.CurrentAudioBeats > queuedDingSongBpmTime)
			{
				float num = Mathf.Ceil(atsc.CurrentJsonTime);
				BaseDifficulty map = BeatSaberSongContainer.Instance.Map;
				if (Mathf.Abs(Mathf.Floor(map.SongBpmTimeToJsonTime(atsc.CurrentAudioBeats).Value) - Mathf.Floor(atsc.CurrentJsonTime)) > 0.01f)
				{
					num = Mathf.Ceil(num + 1f);
				}
				queuedDingSongBpmTime = map.JsonTimeToSongBpmTime(num).Value;
				float num2 = atsc.GetSecondsFromBeat(queuedDingSongBpmTime - atsc.CurrentAudioBeats) / songSpeed;
				audioUtil.PlayOneShotSound(CowBell ? cowbellSound : metronomeSound, Settings.Instance.MetronomeVolume, 1f, num2);
				if (!metronomeUI.activeInHierarchy)
				{
					metronomeUI.SetActive(value: true);
				}
				RunAnimation(60f / num2);
			}
		}
		else
		{
			metronomeUI.SetActive(value: false);
		}
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

	private void RunAnimation(float inferredBpm)
	{
		if (metronomeUIAnimator.gameObject.activeInHierarchy)
		{
			metronomeUIAnimator.StopPlayback();
			metronomeUIAnimator.SetFloat(animatorBpm, inferredBpm);
			metronomeUIAnimator.Play(metronomeUIDirection ? "Metronome_R2L" : "Metronome_L2R");
			metronomeUIDirection = !metronomeUIDirection;
		}
	}

	private void OnPlayToggle(bool playing)
	{
		if (metronomeActive && !playing)
		{
			queuedDingSongBpmTime = 0f;
		}
	}
}
