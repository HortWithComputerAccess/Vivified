using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;

public class CountersPlusController : MonoBehaviour
{
	[FormerlySerializedAs("notes")]
	[SerializeField]
	private NoteGridContainer noteGrid;

	[FormerlySerializedAs("obstacles")]
	[SerializeField]
	private ObstacleGridContainer obstacleGrid;

	[FormerlySerializedAs("events")]
	[SerializeField]
	private EventGridContainer eventGrid;

	[SerializeField]
	private ArcGridContainer arcGrid;

	[SerializeField]
	private ChainGridContainer chainGrid;

	[SerializeField]
	private BPMChangeGridContainer bpm;

	[SerializeField]
	private NJSEventGridContainer njsEventGrid;

	[SerializeField]
	private AudioSource cameraAudioSource;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[Header("Localized Strings")]
	[SerializeField]
	private LocalizeStringEvent notesMesh;

	[SerializeField]
	private LocalizeStringEvent notesPSMesh;

	[SerializeField]
	private LocalizeStringEvent[] extraNoteStrings;

	[SerializeField]
	private LocalizeStringEvent obstacleString;

	[SerializeField]
	private LocalizeStringEvent eventString;

	[SerializeField]
	private LocalizeStringEvent arcString;

	[SerializeField]
	private LocalizeStringEvent chainString;

	[SerializeField]
	private LocalizeStringEvent bpmString;

	[FormerlySerializedAs("currentBPMString")]
	[SerializeField]
	private LocalizeStringEvent currentBpmString;

	[SerializeField]
	private LocalizeStringEvent selectionString;

	[SerializeField]
	private LocalizeStringEvent timeMappingString;

	[SerializeField]
	private LocalizeStringEvent[] njsEventStrings;

	private float lastBpm;

	private float lastNJS;

	private SwingsPerSecond swingsPerSecond;

	[FormerlySerializedAs("hours")]
	[HideInInspector]
	public int hours;

	[FormerlySerializedAs("minutes")]
	[HideInInspector]
	public int minutes;

	[FormerlySerializedAs("seconds")]
	[HideInInspector]
	public int seconds;

	private CountersPlusStatistic stringRefreshQueue;

	public int NotesCount => noteGrid.MapObjects.CountNoAlloc((BaseNote note) => note.Type != 3);

	public float NPSCount => (float)NotesCount / cameraAudioSource.clip.length;

	public int NotesSelected => SelectionController.SelectedObjects.Count((BaseObject x) => x is BaseNote baseNote && baseNote.Type != 3);

	public float NPSselected
	{
		get
		{
			List<BaseObject> source = SelectionController.SelectedObjects.OrderBy((BaseObject it) => it.JsonTime).ToList();
			float beat = source.Last().SongBpmTime - source.First().SongBpmTime;
			float secondsFromBeat = atsc.GetSecondsFromBeat(beat);
			return (float)NotesSelected / secondsFromBeat;
		}
	}

	public int BombCount => noteGrid.MapObjects.CountNoAlloc((BaseNote note) => note.Type == 3);

	public int ArcCount => arcGrid.MapObjects.Count;

	public int ChainCount => chainGrid.MapObjects.Count;

	public int ObstacleCount => obstacleGrid.MapObjects.Count;

	public int EventCount => eventGrid.MapObjects.Count;

	public int BPMCount => bpm.MapObjects.Count;

	public int SelectedCount => SelectionController.SelectedObjects.Count;

	public float OverallSPS => swingsPerSecond.Total.Overall;

	public float CurrentBPM => BeatSaberSongContainer.Instance.Map.BpmAtJsonTime(atsc.CurrentJsonTime).Value;

	public float CurrentNJS => njsEventGrid.CurrentNJS;

	public float CurrentHJD { get; private set; }

	public float CurrentJD { get; private set; }

	public float CurrentRT { get; private set; }

	public float NJSEventCount => njsEventGrid.MapObjects.Count;

	public float RedBlueRatio
	{
		get
		{
			int num = noteGrid.MapObjects.CountNoAlloc((BaseNote note) => note.Type == 0);
			int num2 = noteGrid.MapObjects.CountNoAlloc((BaseNote note) => note.Type == 1);
			if (num2 != 0)
			{
				return (float)num / (float)num2;
			}
			return 0f;
		}
	}

	private void Start()
	{
		Settings.NotifyBySettingName("CountersPlus", UpdateCountersVisibility);
		UpdateCountersVisibility(Settings.Instance.CountersPlus);
		swingsPerSecond = new SwingsPerSecond(noteGrid, obstacleGrid);
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoadedEvent));
		SelectionController.SelectionChangedEvent = (Action)Delegate.Combine(SelectionController.SelectionChangedEvent, new Action(SelectionChangedEvent));
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Combine(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(LoadedDifficultyChangedEvent));
	}

	private void Update()
	{
		if (Application.isFocused)
		{
			float time = BeatSaberSongContainer.Instance.Map.Time;
			int num = Mathf.FloorToInt(time * 60f % 60f);
			if (num != seconds)
			{
				seconds = num;
				minutes = Mathf.FloorToInt(time % 60f);
				hours = Mathf.FloorToInt(time / 60f);
				timeMappingString.StringReference.RefreshString();
			}
		}
		float currentBPM = CurrentBPM;
		if (lastBpm != currentBPM)
		{
			currentBpmString.StringReference.RefreshString();
			lastBpm = currentBPM;
		}
		if (stringRefreshQueue > CountersPlusStatistic.Invalid)
		{
			if ((stringRefreshQueue & CountersPlusStatistic.Notes) != CountersPlusStatistic.Invalid)
			{
				UpdateNoteStats();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.Obstacles) != CountersPlusStatistic.Invalid)
			{
				obstacleString.StringReference.RefreshString();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.Events) != CountersPlusStatistic.Invalid)
			{
				eventString.StringReference.RefreshString();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.BpmEvents) != CountersPlusStatistic.Invalid)
			{
				bpmString.StringReference.RefreshString();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.Selection) != CountersPlusStatistic.Invalid)
			{
				UpdateSelectionStats();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.Arcs) != CountersPlusStatistic.Invalid)
			{
				arcString.StringReference.RefreshString();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.Chains) != CountersPlusStatistic.Invalid)
			{
				chainString.StringReference.RefreshString();
			}
			if ((stringRefreshQueue & CountersPlusStatistic.NJSEvents) != CountersPlusStatistic.Invalid)
			{
				UpdateNJSEventsStats();
			}
			stringRefreshQueue = CountersPlusStatistic.Invalid;
		}
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("CountersPlus");
		SelectionController.SelectionChangedEvent = (Action)Delegate.Remove(SelectionController.SelectionChangedEvent, new Action(SelectionChangedEvent));
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoadedEvent));
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Remove(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(LoadedDifficultyChangedEvent));
	}

	public void UpdateStatistic(CountersPlusStatistic stat)
	{
		if ((bool)Settings.Instance.CountersPlus["enabled"])
		{
			stringRefreshQueue |= stat;
		}
	}

	private void UpdateNJSEventsStats()
	{
		if (lastNJS != CurrentNJS)
		{
			lastNJS = CurrentNJS;
			float noteJumpSpeed = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
			float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
			float noteStartBeatOffset = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteStartBeatOffset;
			float num = SpawnParameterHelper.CalculateHalfJumpDuration(noteJumpSpeed, noteStartBeatOffset, beatsPerMinute);
			float num2 = SpawnParameterHelper.CalculateJumpDistance(noteJumpSpeed, noteStartBeatOffset, beatsPerMinute);
			float num3 = 60000f / beatsPerMinute * num;
			if (CurrentNJS > noteJumpSpeed)
			{
				CurrentRT = num3;
				CurrentHJD = num;
				float num4 = CurrentNJS / noteJumpSpeed;
				CurrentJD = num2 * num4;
			}
			else
			{
				CurrentJD = num2;
				float num5 = noteJumpSpeed / CurrentNJS;
				CurrentHJD = num * num5;
				CurrentRT = num3 * num5;
			}
			LocalizeStringEvent[] array = njsEventStrings;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].StringReference.RefreshString();
			}
		}
	}

	private void LevelLoadedEvent()
	{
		foreach (object value in Enum.GetValues(typeof(CountersPlusStatistic)))
		{
			UpdateStatistic((CountersPlusStatistic)value);
		}
	}

	private void LoadedDifficultyChangedEvent()
	{
		foreach (object value in Enum.GetValues(typeof(CountersPlusStatistic)))
		{
			UpdateStatistic((CountersPlusStatistic)value);
		}
	}

	private void SelectionChangedEvent()
	{
		UpdateStatistic(CountersPlusStatistic.Selection);
	}

	private void UpdateNoteStats()
	{
		if (SelectionController.HasSelectedObjects() && NotesSelected > 0)
		{
			notesMesh.StringReference.TableEntryReference = "countersplus.notes.selected";
			notesPSMesh.StringReference.TableEntryReference = "countersplus.nps.selected";
		}
		else
		{
			notesMesh.StringReference.TableEntryReference = "countersplus.notes";
			notesPSMesh.StringReference.TableEntryReference = "countersplus.nps";
		}
		notesMesh.StringReference.RefreshString();
		notesPSMesh.StringReference.RefreshString();
		swingsPerSecond.Update();
		LocalizeStringEvent[] array = extraNoteStrings;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StringReference.RefreshString();
		}
	}

	private void UpdateSelectionStats()
	{
		selectionString.gameObject.SetActive(SelectionController.HasSelectedObjects());
		if (SelectionController.HasSelectedObjects() && NotesSelected > 0)
		{
			notesMesh.StringReference.TableEntryReference = "countersplus.notes.selected";
			notesPSMesh.StringReference.TableEntryReference = "countersplus.nps.selected";
		}
		else
		{
			notesMesh.StringReference.TableEntryReference = "countersplus.notes";
			notesPSMesh.StringReference.TableEntryReference = "countersplus.nps";
		}
		notesMesh.StringReference.RefreshString();
		notesPSMesh.StringReference.RefreshString();
		selectionString.StringReference.RefreshString();
	}

	public void UpdateCountersVisibility(object obj)
	{
		CountersPlusSettings countersPlusSettings = (CountersPlusSettings)obj;
		LocalizeStringEvent[] componentsInChildren = GetComponentsInChildren<LocalizeStringEvent>(includeInactive: true);
		foreach (LocalizeStringEvent localizeStringEvent in componentsInChildren)
		{
			string key = localizeStringEvent.ToString().Replace(" (UnityEngine.Localization.Components.LocalizeStringEvent)", "");
			if (countersPlusSettings.TryGetValue(key, out var value))
			{
				localizeStringEvent.gameObject.SetActive((bool)countersPlusSettings["enabled"] && (bool)value);
			}
		}
	}
}
