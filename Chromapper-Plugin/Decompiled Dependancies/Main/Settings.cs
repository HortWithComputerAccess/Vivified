using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Beatmap.Appearances;
using SimpleJSON;
using UnityEngine;

public class Settings
{
	private static Settings instance;

	public string Language = "en";

	public bool DiscordRPCEnabled = true;

	public bool DarkTheme = true;

	public string BeatSaberInstallation = "";

	public bool AutoSave = true;

	public int AutoSaveInterval = 5;

	public bool FormatJson;

	public bool SaveWithoutDefaultValues;

	public bool OpenFileExplorerAfterCreatingZip = true;

	public bool InstantEscapeMenuTransitions;

	public bool InstantLoadingTransitions;

	public bool Waifu;

	public bool HelpfulLoadingMessages;

	public float UIScale = 1f;

	public bool VSync = true;

	public int MaximumFPS = 9999;

	public bool IncludePathForADB = true;

	public bool ShowNonImportantErrors = true;

	public float EditorScale = 4f;

	public bool EditorScaleBPMIndependent;

	public bool NoteJumpSpeedForEditorScale;

	public bool RotateTrack = true;

	public bool Reset360DisplayOnCompleteTurn = true;

	public bool PrecisionPlacementGrid;

	public PrecisionPlacementMode PrecisionPlacementMode;

	public int PrecisionPlacementGridPrecision = 4;

	public bool ShowMoreAccurateFastWalls;

	public bool QuickNoteEditing;

	public bool VanillaOnlyShift = true;

	public bool Animations = true;

	public float PastNotesGridScale = 0.5f;

	public float SongSpeedChangeAmount = 2f;

	public CountersPlusSettings CountersPlus = new CountersPlusSettings();

	public bool BoxSelect = true;

	public bool Load_Notes = true;

	public bool Load_Events = true;

	public bool Load_Obstacles = true;

	public bool Load_Others = true;

	public bool HideDisablableObjectsOnLoad;

	public bool DisplaySongDetailsInEditor = true;

	public bool DisplayDiffDetailsInEditor = true;

	public float Volume = 1f;

	public float SongVolume = 1f;

	public float MetronomeVolume;

	public float NoteHitVolume = 0.5f;

	public bool Ding_Red_Notes = true;

	public bool Ding_Blue_Notes = true;

	public bool Ding_Bombs;

	public int NoteHitSound;

	public int AudioLatencyCompensation;

	public bool EmulateChromaAdvanced = true;

	public bool EmulateChromaLite = true;

	public bool ColorFakeWalls = true;

	public bool VisualizeChromaGradients = true;

	public bool VisualizeChromaAlpha = true;

	public bool SimpleBlocks;

	public bool SolidChainLink;

	public bool PyramidEventModels;

	public EventModelType EventModel;

	public float PastNoteModelAlpha = 0.4f;

	public int ChunkDistance = 5;

	public int Offset_Spawning = 4;

	public int Offset_Despawning = 1;

	public bool DisplayFloatValueText = true;

	public bool Spectrogram = true;

	public int SpectrogramSampleSize = 512;

	public int SpectrogramEditorQuality = 8;

	public int SpectrogramShift = 1;

	public bool SpectrogramBilinearFiltering = true;

	public int SpectrogramSlices;

	public float SpectrogramHeight = 0.15f;

	public bool Reflections = true;

	public bool HighQualityBloom = true;

	public bool ChromaticAberration = true;

	public float PostProcessingIntensity = 0.1f;

	public float CameraFOV = 60f;

	public float PlayerCameraFOV = 60f;

	public float PlayerCameraOffsetZ = 3.6f;

	public int CameraAA;

	public int RenderScale = 100;

	public bool MeasureLinesShowOnTop;

	public bool HighContrastGrids;

	public bool DisplayHJDLine = true;

	public float GridTransparency;

	public float InterfaceOpacity = 0.1f;

	public int TrackLength = 8;

	public float OneBeatWidth = 0.1f;

	public bool AccurateNoteSize;

	public float NoteColorMultiplier = 1f;

	public float ArrowColorMultiplier = 1.72f;

	public float ArrowColorWhiteBlend = 0.75f;

	public float ObstacleOpacity = 0.25f;

	public bool AlternateLighting;

	public bool DisplayGridBookmarks = true;

	public bool GridBookmarksHasLine = true;

	public bool BookmarkTooltipTimeInfo;

	public int BookmarkTimelineWidth = 10;

	public float BookmarkTimelineBrightness = 0.66f;

	public float Camera_MouseSensitivity = 2f;

	public float Camera_MovementSpeed = 15f;

	public bool NodeEditor_UseKeybind = true;

	public int NodeEditorTextSize = 10;

	public int NodeEditorSize = 10;

	public bool InvertPrecisionScroll;

	public bool InvertNoteControls = true;

	public bool InvertScrollTime;

	public bool InvertScrollEventValue;

	public bool InvertScrollNoteAngle;

	public bool InvertScrollWallDuration;

	public bool InvertScrollWallBounds;

	public bool InvertScrollArcMultiplier;

	public bool InvertScrollChainSquish;

	public bool InvertScrollChainSegmentCount;

	public int TimeValueDecimalPrecision = 3;

	public int BpmTimeValueDecimalPrecision = 6;

	public bool AdvancedShit = true;

	public bool LightIDTransitionSupport;

	public int ReleaseChannel;

	public string ReleaseServer = "https://cm.topc.at";

	public int DSPBufferSize = 10;

	public bool AutomaticModRequirements = true;

	public bool RemoveNotesOutsideMap = true;

	public bool RemoveEventsOutsideMap = true;

	public bool RemoveObstaclesOutsideMap = true;

	public bool RemoveArcsOutsideMap = true;

	public bool RemoveChainsOutsideMap = true;

	public int Waveform = 1;

	public bool PickColorFromChromaEvents;

	public bool PlaceChromaColor;

	public bool BongoBoye;

	public int BongoCat = -1;

	public bool Reminder_Loading360Levels = true;

	public bool Reminder_SettingsFailed = true;

	public bool WaveformWorkflow = true;

	public int MapVersion = 3;

	public CameraPosition[] SavedPositions = new CameraPosition[8];

	public int CursorPrecisionA = 1;

	public int CursorPrecisionB = 1;

	public string LastLoadedMap = "";

	public string LastLoadedChar = "";

	public string LastLoadedDiff = "";

	public int LastSongSortType;

	public MultiSettings MultiSettings = new MultiSettings();

	public static Dictionary<string, FieldInfo> AllFieldInfos = new Dictionary<string, FieldInfo>();

	public static Dictionary<string, object> NonPersistentSettings = new Dictionary<string, object>();

	private static readonly Dictionary<string, Action<object>> nameToActions = new Dictionary<string, Action<object>>();

	public static Settings Instance => instance ?? (instance = Load());

	public string CustomSongsFolder => Path.Combine(BeatSaberInstallation, "Beat Saber_Data", "CustomLevels");

	public string CustomWIPSongsFolder => Path.Combine(BeatSaberInstallation, "Beat Saber_Data", "CustomWIPLevels");

	public string CustomPlatformsFolder => Path.Combine(BeatSaberInstallation, "CustomPlatforms");

	public static string AndroidPlatformTools => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "quest-utils");

	private static Settings Load()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		bool flag = false;
		Settings settings = new Settings();
		MemberInfo[] members = settings.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
		if (!File.Exists(Application.persistentDataPath + "/ChroMapperSettings.json"))
		{
			MemberInfo[] array = members;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is FieldInfo fieldInfo)
				{
					AllFieldInfos.Add(fieldInfo.Name, fieldInfo);
				}
			}
			return settings;
		}
		using (StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/ChroMapperSettings.json"))
		{
			JSONNode jSONNode = JSON.Parse(streamReader.ReadToEnd());
			MemberInfo[] array = members;
			foreach (MemberInfo memberInfo in array)
			{
				try
				{
					if (!(memberInfo is FieldInfo fieldInfo2))
					{
						continue;
					}
					AllFieldInfos.Add(fieldInfo2.Name, fieldInfo2);
					JSONNode jSONNode2 = jSONNode[fieldInfo2.Name];
					if (!(jSONNode2 != null))
					{
						continue;
					}
					if (jSONNode2 is JSONArray jSONArray)
					{
						Array array2 = Array.CreateInstance(fieldInfo2.FieldType.GetElementType(), jSONArray.Count);
						for (int j = 0; j < jSONArray.Count; j++)
						{
							if (!(jSONArray[j] == null))
							{
								Type elementType = fieldInfo2.FieldType.GetElementType();
								if (Activator.CreateInstance(elementType) is IJsonSetting jsonSetting)
								{
									jsonSetting.FromJson(jSONArray[j]);
									array2.SetValue(jsonSetting, j);
								}
								else
								{
									array2.SetValue(Convert.ChangeType(jSONArray[j], elementType), j);
								}
							}
						}
						fieldInfo2.SetValue(settings, array2);
					}
					else if (fieldInfo2.FieldType.BaseType == typeof(Enum))
					{
						object value = Enum.Parse(fieldInfo2.FieldType, jSONNode2);
						fieldInfo2.SetValue(settings, value);
					}
					else if (typeof(IJsonSetting).IsAssignableFrom(fieldInfo2.FieldType))
					{
						IJsonSetting jsonSetting2 = (IJsonSetting)Activator.CreateInstance(fieldInfo2.FieldType);
						jsonSetting2.FromJson(jSONNode2);
						fieldInfo2.SetValue(settings, jsonSetting2);
					}
					else
					{
						fieldInfo2.SetValue(settings, Convert.ChangeType(jSONNode2.Value, fieldInfo2.FieldType));
					}
				}
				catch (Exception arg)
				{
					Debug.LogWarning($"Setting {memberInfo.Name} failed to load.\n{arg}");
					flag = true;
				}
			}
		}
		if (flag)
		{
			PersistentUI.Instance.StartCoroutine(ShowFailedDialog());
		}
		JSONNumber.CapNumbersToDecimals = true;
		JSONNumber.DecimalPrecision = settings.TimeValueDecimalPrecision;
		settings.UpdateOldSettings();
		return settings;
	}

	public static IEnumerator ShowFailedDialog()
	{
		yield return new WaitForEndOfFrame();
		PersistentUI.Instance.ShowDialogBox("PersistentUI", "settings.loadfailed", Instance.HandleFailedReminder, PersistentUI.DialogBoxPresetType.OkIgnore, new object[1] { Application.persistentDataPath });
	}

	private void HandleFailedReminder(int res)
	{
		Reminder_SettingsFailed = res == 0;
	}

	private void UpdateOldSettings()
	{
		if (Waveform != -1)
		{
			Spectrogram = Waveform > 0;
			Waveform = -1;
		}
		if (PrecisionPlacementGrid)
		{
			PrecisionPlacementMode = PrecisionPlacementMode.Hold;
			PrecisionPlacementGrid = false;
		}
		if (PyramidEventModels)
		{
			EventModel = EventModelType.Pyramid;
			PyramidEventModels = false;
		}
		if (BongoBoye)
		{
			BongoCat = 0;
			BongoBoye = false;
		}
	}

	public void Save()
	{
		JSONObject jSONObject = new JSONObject();
		FieldInfo[] array = (from x in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)
			where x is FieldInfo
			orderby x.Name
			select x).Cast<FieldInfo>().ToArray();
		foreach (FieldInfo fieldInfo in array)
		{
			object value = fieldInfo.GetValue(this);
			if (fieldInfo.FieldType.IsArray)
			{
				JSONArray jSONArray = new JSONArray();
				object[] array2 = (object[])value;
				foreach (object obj in array2)
				{
					if (obj == null)
					{
						jSONArray.Add(null);
					}
					else if (obj is IJsonSetting jsonSetting)
					{
						jSONArray.Add(jsonSetting.ToJson());
					}
					else
					{
						jSONArray.Add(obj.ToString());
					}
				}
				jSONObject[fieldInfo.Name] = jSONArray;
			}
			else if (value is IJsonSetting jsonSetting2)
			{
				jSONObject[fieldInfo.Name] = jsonSetting2.ToJson();
			}
			else if (value != null)
			{
				jSONObject[fieldInfo.Name] = value.ToString();
			}
		}
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/ChroMapperSettings.json", append: false);
		streamWriter.Write(jSONObject.ToString(2));
	}

	public static Dictionary<string, Type> GetAllFieldInfos()
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
		MemberInfo[] members = typeof(Settings).GetMembers(BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < members.Length; i++)
		{
			if (members[i] is FieldInfo fieldInfo)
			{
				dictionary.Add(fieldInfo.Name, fieldInfo.FieldType);
			}
		}
		return dictionary;
	}

	public static void ApplyOptionByName(string name, object value)
	{
		if (AllFieldInfos.TryGetValue(name, out var value2))
		{
			value2.SetValue(Instance, value);
			ManuallyNotifySettingUpdatedEvent(name, value);
			return;
		}
		throw new ArgumentException("Setting " + name + " does not exist.");
	}

	public static void NotifyBySettingName(string name, Action<object> callback)
	{
		if (nameToActions.ContainsKey(name) && callback != null)
		{
			Dictionary<string, Action<object>> dictionary = nameToActions;
			dictionary[name] = (Action<object>)Delegate.Combine(dictionary[name], callback);
		}
		else if (!nameToActions.ContainsKey(name) && callback != null)
		{
			Action<object> value = callback.Invoke;
			nameToActions.Add(name, value);
		}
	}

	public static void ClearSettingNotifications(string name)
	{
		nameToActions.Remove(name);
	}

	public static void ManuallyNotifySettingUpdatedEvent(string name, object value)
	{
		if (NonPersistentSettings.ContainsKey(name))
		{
			NonPersistentSettings[name] = value;
		}
		if (nameToActions.TryGetValue(name, out var value2))
		{
			value2?.Invoke(value);
		}
	}

	public static bool ValidateDirectory(Action<string> errorFeedback = null)
	{
		if (!Directory.Exists(Instance.BeatSaberInstallation))
		{
			errorFeedback?.Invoke("validate.missing");
			return false;
		}
		if (!Directory.Exists(Instance.CustomSongsFolder))
		{
			errorFeedback?.Invoke("validate.nofolders");
			return false;
		}
		if (!Directory.Exists(Instance.CustomWIPSongsFolder))
		{
			errorFeedback?.Invoke("validate.nowip");
			return false;
		}
		return true;
	}
}
