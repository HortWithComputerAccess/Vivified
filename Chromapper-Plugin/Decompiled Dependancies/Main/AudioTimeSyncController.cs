using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AudioTimeSyncController : MonoBehaviour, CMInput.IPlaybackActions, CMInput.ITimelineActions, CMInput.ITimelineNavigationActions
{
	public static readonly string PrecisionSnapName = "PrecisionSnap";

	private static readonly int songTime = Shader.PropertyToID("_SongTime");

	private static readonly int songTimeSeconds = Shader.PropertyToID("_SongTimeSeconds");

	private static readonly int viewStart = Shader.PropertyToID("_ViewStart");

	private static readonly int viewEnd = Shader.PropertyToID("_ViewEnd");

	private const float cancelPlayInputDuration = 0.3f;

	[FormerlySerializedAs("songAudioSource")]
	public AudioSource SongAudioSource;

	[SerializeField]
	private AudioSource waveformSource;

	[SerializeField]
	private GameObject moveables;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private Track[] otherTracks;

	[FormerlySerializedAs("bpmChangesContainer")]
	[SerializeField]
	private BPMChangeGridContainer bpmChangeGridContainer;

	[SerializeField]
	private GridRenderingController gridRenderingController;

	[SerializeField]
	private CustomStandaloneInputModule customStandaloneInputModule;

	public BaseInfo MapInfo;

	[SerializeField]
	private float currentSeconds;

	[FormerlySerializedAs("stopScheduled")]
	public bool StopScheduled;

	[FormerlySerializedAs("initialized")]
	public bool Initialized;

	private int gridMeasureSnapping = 1;

	private float audioLatencyCompensationSeconds;

	private AudioClip clip;

	private bool controlSnap;

	public Action<int> GridMeasureSnappingChanged;

	private bool levelLoaded;

	public Action<bool> PlayToggle;

	public Action TimeChanged;

	private float playStartTime;

	private bool preciselyControlSnap;

	private float songSpeed = 10f;

	[SerializeField]
	private float currentJsonTime;

	[SerializeField]
	private float currentSongBpmTime;

	private bool toggledPlayingPreviousFrame;

	public int GridMeasureSnapping
	{
		get
		{
			return gridMeasureSnapping;
		}
		set
		{
			int num = gridMeasureSnapping;
			gridMeasureSnapping = value;
			Settings.NonPersistentSettings[PrecisionSnapName] = value;
			if (gridMeasureSnapping != num)
			{
				GridMeasureSnappingChanged?.Invoke(value);
			}
		}
	}

	public float CurrentJsonTime
	{
		get
		{
			return currentJsonTime;
		}
		private set
		{
			currentJsonTime = value;
			currentSongBpmTime = BeatSaberSongContainer.Instance.Map.JsonTimeToSongBpmTime(value).Value;
			currentSeconds = GetSecondsFromBeat(currentSongBpmTime);
			ValidatePosition();
			UpdateMovables();
		}
	}

	[Obsolete("This is for existing dev plugin compatibility. Use CurrentSongBpmTime, CurrentJsonTime, or CurrentSeconds.", true)]
	public float CurrentBeat => CurrentSongBpmTime;

	public float CurrentSongBpmTime
	{
		get
		{
			return currentSongBpmTime;
		}
		private set
		{
			currentSongBpmTime = value;
			currentJsonTime = BeatSaberSongContainer.Instance.Map.SongBpmTimeToJsonTime(value).Value;
			currentSeconds = GetSecondsFromBeat(value);
			ValidatePosition();
			UpdateMovables();
		}
	}

	public float CurrentSeconds
	{
		get
		{
			return currentSeconds;
		}
		private set
		{
			currentSeconds = value;
			currentSongBpmTime = GetBeatFromSeconds(value);
			currentJsonTime = BeatSaberSongContainer.Instance.Map.SongBpmTimeToJsonTime(currentSongBpmTime).Value;
			ValidatePosition();
			UpdateMovables();
		}
	}

	public float CurrentAudioSeconds
	{
		get
		{
			if ((object)SongAudioSource.clip != null)
			{
				return (float)SongAudioSource.timeSamples / (float)SongAudioSource.clip.frequency;
			}
			return 0f;
		}
	}

	public float CurrentAudioBeats => GetBeatFromSeconds(CurrentAudioSeconds);

	public bool IsPlaying { get; private set; }

	public bool IsSnapped
	{
		get
		{
			if (IsPlaying)
			{
				return false;
			}
			return Mathf.Approximately(currentJsonTime, (float)Math.Round(currentJsonTime * (float)gridMeasureSnapping, MidpointRounding.AwayFromZero) / (float)gridMeasureSnapping);
		}
	}

	private void Start()
	{
		try
		{
			clip = BeatSaberSongContainer.Instance.LoadedSong;
			MapInfo = BeatSaberSongContainer.Instance.Info;
			ResetTime();
			IsPlaying = false;
			SongAudioSource.clip = clip;
			SongAudioSource.volume = Settings.Instance.SongVolume;
			waveformSource.clip = clip;
			UpdateMovables();
			if (Settings.NonPersistentSettings.ContainsKey(PrecisionSnapName))
			{
				GridMeasureSnapping = (int)Settings.NonPersistentSettings[PrecisionSnapName];
			}
			GridMeasureSnappingChanged?.Invoke(GridMeasureSnapping);
			LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(OnLevelLoaded));
			Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
			Settings.NotifyBySettingName("SongVolume", UpdateSongVolume);
			Settings.NotifyBySettingName("TrackLength", UpdateTrackLength);
			Initialized = true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		try
		{
			if (!levelLoaded || !IsPlaying)
			{
				return;
			}
			float num = currentSeconds + audioLatencyCompensationSeconds * (songSpeed / 10f);
			float currentAudioSeconds = CurrentAudioSeconds;
			float num2 = ((num > 1f) ? (currentAudioSeconds / num) : 1f);
			if (SongAudioSource.isPlaying)
			{
				float num3 = Mathf.Max(0.04f, Time.smoothDeltaTime * 2f);
				if (Mathf.Abs(currentAudioSeconds - num) >= num3 * (songSpeed / 10f))
				{
					num = currentAudioSeconds;
					num2 = 1f;
				}
			}
			else
			{
				num2 = 1f;
				if (!StopScheduled)
				{
					StartCoroutine(StopPlayingDelayed(audioLatencyCompensationSeconds));
				}
			}
			CurrentSeconds = num + num2 * (Time.deltaTime * (songSpeed / 10f)) - audioLatencyCompensationSeconds * (songSpeed / 10f);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnDestroy()
	{
		clip = null;
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(OnLevelLoaded));
		Settings.ClearSettingNotifications("SongSpeed");
		Settings.ClearSettingNotifications("SongVolume");
	}

	private IEnumerator TrackToggledPlayingPreviousFrame()
	{
		toggledPlayingPreviousFrame = true;
		yield return null;
		toggledPlayingPreviousFrame = false;
	}

	public void OnTogglePlaying(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			TogglePlaying();
			if (IsPlaying)
			{
				StartCoroutine(TrackToggledPlayingPreviousFrame());
			}
		}
		if (!CMInputCallbackInstaller.IsActionMapDisabled(typeof(CMInput.IPlaybackActions)) && context.canceled && context.duration >= 0.30000001192092896 && !toggledPlayingPreviousFrame)
		{
			CancelPlaying();
		}
	}

	public void OnResetTime(InputAction.CallbackContext context)
	{
		if (context.performed && !IsPlaying)
		{
			ResetTime();
		}
	}

	public void OnChangeTimeandPrecision(InputAction.CallbackContext context)
	{
		if (!KeybindsController.IsMouseInWindow || customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			return;
		}
		float num = context.ReadValue<float>();
		if (!context.performed)
		{
			return;
		}
		if (controlSnap)
		{
			float num2 = ((!Settings.Instance.InvertPrecisionScroll) ? ((num > 0f) ? 2f : 0.5f) : ((num > 0f) ? 0.5f : 2f));
			if (!preciselyControlSnap)
			{
				GridMeasureSnapping = Mathf.Clamp(Mathf.RoundToInt((float)GridMeasureSnapping * num2), 1, 64);
				return;
			}
			int num3 = ((num2 > 1f) ? 1 : (-1));
			GridMeasureSnapping = Mathf.Clamp(GridMeasureSnapping + num3, 1, 64);
			return;
		}
		if (Settings.Instance.InvertScrollTime)
		{
			num *= -1f;
		}
		float num4 = 1f / (float)GridMeasureSnapping * ((num > 0f) ? 1f : (-1f));
		bool isSnapped = IsSnapped;
		MoveToJsonTime(Mathf.Max(0f, CurrentJsonTime + num4));
		if (isSnapped)
		{
			SnapToGrid(positionValidated: true);
		}
	}

	public void OnPreciselyChangeTimeandPrecision(InputAction.CallbackContext context)
	{
		if (KeybindsController.IsMouseInWindow && !customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			float num = context.ReadValue<float>();
			if (context.performed)
			{
				float num2 = ((!Settings.Instance.InvertPrecisionScroll) ? ((num > 0f) ? 2f : 0.5f) : ((num > 0f) ? 0.5f : 2f));
				int num3 = ((num2 > 1f) ? 1 : (-1));
				GridMeasureSnapping = Mathf.Clamp(GridMeasureSnapping + num3, 1, 64);
			}
		}
	}

	public void OnChangePrecisionModifier(InputAction.CallbackContext context)
	{
		controlSnap = context.performed;
	}

	public void OnPreciseSnapModification(InputAction.CallbackContext context)
	{
		preciselyControlSnap = context.performed;
	}

	public void OnGoToBeat(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PersistentUI.Instance.ShowInputBox("Mapper", "gotobeat.dialog", GoToBeat);
		}
	}

	internal void GoToBeat(string beatInput)
	{
		if (!string.IsNullOrEmpty(beatInput) && !string.IsNullOrWhiteSpace(beatInput))
		{
			if (float.TryParse(beatInput, out var result))
			{
				CurrentJsonTime = Mathf.Max(0f, result);
			}
			else
			{
				PersistentUI.Instance.ShowInputBox("Mapper", "gotobeat.dialog.invalid", GoToBeat);
			}
		}
	}

	public void OnMoveCursorForward(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			bool isSnapped = IsSnapped;
			CurrentJsonTime += 1f / (float)gridMeasureSnapping;
			if (isSnapped)
			{
				SnapToGrid(positionValidated: true);
			}
		}
	}

	public void OnMoveCursorBackward(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			bool isSnapped = IsSnapped;
			CurrentJsonTime -= 1f / (float)gridMeasureSnapping;
			if (isSnapped)
			{
				SnapToGrid(positionValidated: true);
			}
		}
	}

	private void UpdateSongVolume(object obj)
	{
		SongAudioSource.volume = (float)obj;
	}

	private void UpdateSongSpeed(object obj)
	{
		songSpeed = (float)obj;
	}

	private void UpdateTrackLength(object _)
	{
		UpdateMovables();
	}

	private void OnLevelLoaded()
	{
		levelLoaded = true;
	}

	private void UpdateMovables()
	{
		Shader.SetGlobalFloat(songTime, currentSongBpmTime);
		Shader.SetGlobalFloat(songTimeSeconds, currentSeconds);
		Shader.SetGlobalFloat(viewStart, GetSecondsFromBeat(currentSongBpmTime - (float)Settings.Instance.TrackLength / 4f));
		Shader.SetGlobalFloat(viewEnd, GetSecondsFromBeat(currentSongBpmTime + (float)Settings.Instance.TrackLength));
		float num = currentSongBpmTime * EditorScaleController.EditorScale;
		gridRenderingController.UpdateOffset(num);
		tracksManager.UpdatePosition(num * -1f);
		Track[] array = otherTracks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdatePosition(num * -1f);
		}
		TimeChanged?.Invoke();
	}

	private void ResetTime()
	{
		CurrentSeconds = 0f;
	}

	public IEnumerator StopPlayingDelayed(float delaySeconds)
	{
		StopScheduled = true;
		yield return new WaitForSeconds(delaySeconds);
		StopScheduled = false;
		if (IsPlaying)
		{
			TogglePlaying();
		}
	}

	public void TogglePlaying()
	{
		if (StopScheduled)
		{
			StopCoroutine("StopPlayingDelayed");
			StopScheduled = false;
		}
		IsPlaying = !IsPlaying;
		if (IsPlaying)
		{
			if (CurrentSeconds >= SongAudioSource.clip.length - 0.1f)
			{
				ResetTime();
			}
			playStartTime = CurrentSeconds;
			SongAudioSource.time = CurrentSeconds;
			SongAudioSource.Play();
			audioLatencyCompensationSeconds = (float)Settings.Instance.AudioLatencyCompensation / 1000f;
			CurrentSeconds -= audioLatencyCompensationSeconds * (songSpeed / 10f);
		}
		else
		{
			SongAudioSource.Stop();
			SnapToGrid();
		}
		PlayToggle?.Invoke(IsPlaying);
	}

	public void CancelPlaying()
	{
		if (IsPlaying)
		{
			TogglePlaying();
			CurrentSeconds = playStartTime;
		}
	}

	public void SnapToGrid(float seconds)
	{
		if (!IsPlaying)
		{
			float beatFromSeconds = GetBeatFromSeconds(seconds);
			currentJsonTime = BeatSaberSongContainer.Instance.Map.SongBpmTimeToJsonTime(beatFromSeconds).Value;
			SnapToGrid();
			SongAudioSource.time = CurrentSeconds;
		}
	}

	public void SnapToGrid(bool positionValidated = false)
	{
		float jsonTime = (currentJsonTime = (float)Math.Round(CurrentJsonTime * (float)GridMeasureSnapping, MidpointRounding.AwayFromZero) / (float)GridMeasureSnapping);
		currentSongBpmTime = BeatSaberSongContainer.Instance.Map.JsonTimeToSongBpmTime(jsonTime).Value;
		currentSeconds = GetSecondsFromBeat(currentSongBpmTime);
		if (!positionValidated)
		{
			ValidatePosition();
		}
		UpdateMovables();
	}

	public void RefreshGridSnapping()
	{
		GridMeasureSnappingChanged?.Invoke(GridMeasureSnapping);
	}

	public void MoveToTimeInSeconds(float seconds)
	{
		if (!IsPlaying)
		{
			CurrentSeconds = seconds;
			SongAudioSource.time = CurrentSeconds;
		}
	}

	[Obsolete("This is for existing dev plugin compatibility. Use MoveToSongBpmTime or MoveToJsonTime.", true)]
	public void MoveToTimeInBeats(float beats)
	{
		MoveToSongBpmTime(beats);
	}

	public void MoveToSongBpmTime(float songBpmTime)
	{
		if (!IsPlaying)
		{
			CurrentSongBpmTime = songBpmTime;
			SongAudioSource.time = CurrentSeconds;
		}
	}

	public void MoveToJsonTime(float jsonTime)
	{
		if (!IsPlaying)
		{
			CurrentJsonTime = jsonTime;
			SongAudioSource.time = CurrentSeconds;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetBeatFromSeconds(float seconds)
	{
		return MapInfo.BeatsPerMinute / 60f * seconds;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetSecondsFromBeat(float beat)
	{
		return 60f / MapInfo.BeatsPerMinute * beat;
	}

	private void ValidatePosition()
	{
		if (!IsPlaying)
		{
			if (currentSeconds < 0f)
			{
				currentSeconds = 0f;
			}
			if (currentSongBpmTime < 0f)
			{
				currentSongBpmTime = 0f;
			}
			if (currentJsonTime < 0f)
			{
				currentJsonTime = 0f;
			}
			if (currentSeconds > BeatSaberSongContainer.Instance.LoadedSong.length)
			{
				CurrentSeconds = BeatSaberSongContainer.Instance.LoadedSong.length;
				SnapToGrid(positionValidated: true);
			}
		}
	}
}
