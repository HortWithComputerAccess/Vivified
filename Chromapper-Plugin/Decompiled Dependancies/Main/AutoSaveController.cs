using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Beatmap.Base;
using Beatmap.Enums;
using QuestDumper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class AutoSaveController : MonoBehaviour, CMInput.ISavingActions
{
	public enum SaveType
	{
		None,
		Menu,
		Quit
	}

	private const int maximumAutosaveCount = 15;

	[SerializeField]
	private Toggle autoSaveToggle;

	[SerializeField]
	private PauseManager pauseManager;

	[SerializeField]
	private BeatmapActionContainer beatmapActionContainer;

	private List<DirectoryInfo> currentAutoSaves = new List<DirectoryInfo>();

	private Task savingThread;

	private float t;

	private float maxSongBpmTime;

	private const int FALSE = 0;

	private const int TRUE = 1;

	private Task objectCheckingThread;

	private int objectsOutsideMap;

	private int objectsCheckIsComplete;

	private int saveFlag;

	private string saveWarningMessage;

	private string localizedSaveWarningHeader;

	private string localizedSaveWarningSubHeader;

	public bool IsSaving
	{
		get
		{
			if (savingThread != null)
			{
				return !savingThread.IsCompleted;
			}
			return false;
		}
	}

	private static MapExporter Exporter => new MapExporter(BeatSaberSongContainer.Instance.Info);

	private void Start()
	{
		localizedSaveWarningHeader = LocalizationSettings.StringDatabase.GetLocalizedString("Mapper", "unsupported.properties.warning.header", null, FallbackBehavior.UseProjectSettings);
		localizedSaveWarningSubHeader = LocalizationSettings.StringDatabase.GetLocalizedString("Mapper", "unsupported.properties.warning.subheader", null, FallbackBehavior.UseProjectSettings);
		autoSaveToggle.isOn = Settings.Instance.AutoSave;
		t = 0f;
		string path = Path.Combine(BeatSaberSongContainer.Instance.Info.Directory, "autosaves");
		if (Directory.Exists(path))
		{
			foreach (string item in Directory.EnumerateDirectories(path))
			{
				currentAutoSaves.Add(new DirectoryInfo(item));
			}
		}
		maxSongBpmTime = BeatSaberSongContainer.Instance.LoadedSong.length * BeatSaberSongContainer.Instance.Info.BeatsPerMinute / 60f;
		CleanAutosaves();
	}

	private void Update()
	{
		if (Interlocked.Exchange(ref objectsCheckIsComplete, 0) == 1)
		{
			if (Interlocked.Exchange(ref objectsOutsideMap, 0) == 1)
			{
				if (saveFlag == 0)
				{
					PersistentUI.Instance.ShowDialogBox("Mapper", "save.objects.outside", CleanAndSave, PersistentUI.DialogBoxPresetType.YesNo);
				}
				if (saveFlag == 1)
				{
					PersistentUI.Instance.ShowDialogBox("Mapper", "save.objects.outside", CleanAndMenu, PersistentUI.DialogBoxPresetType.YesNo);
				}
				if (saveFlag == 2)
				{
					PersistentUI.Instance.ShowDialogBox("Mapper", "save.objects.outside", CleanAndQuit, PersistentUI.DialogBoxPresetType.YesNo);
				}
			}
			else
			{
				if (saveFlag == 0)
				{
					Save();
				}
				if (saveFlag == 1)
				{
					pauseManager.SaveAndExitToMenu();
				}
				if (saveFlag == 2)
				{
					pauseManager.SaveAndQuitCM();
				}
			}
		}
		if (!string.IsNullOrEmpty(saveWarningMessage))
		{
			SceneTransitionManager.Instance.CancelLoading(string.Empty);
			PersistentUI.Instance.ShowDialogBox(saveWarningMessage, null, PersistentUI.DialogBoxPresetType.Ok);
			saveWarningMessage = null;
		}
		if (Settings.Instance.AutoSave && Application.isFocused)
		{
			t += Time.deltaTime;
			if (t > (float)(Settings.Instance.AutoSaveInterval * 60))
			{
				t = 0f;
				Save(auto: true);
			}
		}
	}

	public void OnSave(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			CheckAndSave();
		}
	}

	public void OnSaveQuest(InputAction.CallbackContext context)
	{
		if (context.performed && Adb.IsAdbInstalled(out var _))
		{
			CheckAndSaveQuest();
		}
	}

	public void CheckAndSaveQuest(int _)
	{
		CheckAndSaveQuest();
	}

	public async void CheckAndSaveQuest(SaveType saveType = SaveType.None)
	{
		CheckAndSave(saveType);
		await objectCheckingThread;
		await savingThread;
		await Exporter.ExportToQuest();
	}

	public void CheckAndSave(int _)
	{
		CheckAndSave();
	}

	public void CheckAndSave(SaveType saveType = SaveType.None)
	{
		if (objectCheckingThread != null && !objectCheckingThread.IsCompleted)
		{
			Debug.LogError(":hyperPepega: :mega: PLEASE BE PATIENT THANKS");
			return;
		}
		objectCheckingThread = Task.Run(delegate
		{
			if (ObjectIsOutsideMap())
			{
				Debug.Log("Found object outside of the map.");
				Interlocked.Exchange(ref objectsOutsideMap, 1);
			}
			Interlocked.Exchange(ref saveFlag, (int)saveType);
			Interlocked.Exchange(ref objectsCheckIsComplete, 1);
		});
	}

	private void CleanAndSave(int res)
	{
		if (res == 0)
		{
			CleanObjectsOutsideMap();
		}
		Save();
	}

	private void CleanAndMenu(int res)
	{
		if (res == 0)
		{
			CleanObjectsOutsideMap();
		}
		pauseManager.SaveAndExitToMenu();
	}

	private void CleanAndQuit(int res)
	{
		if (res == 0)
		{
			CleanObjectsOutsideMap();
		}
		pauseManager.SaveAndQuitCM();
	}

	private void CleanObjectsOutsideMap()
	{
		List<BaseObject> list = new List<BaseObject>();
		if (Settings.Instance.RemoveNotesOutsideMap)
		{
			BeatmapObjectContainerCollection collectionForType = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note);
			List<BaseNote> list2 = BeatSaberSongContainer.Instance.Map.Notes.Where((BaseNote note) => note.SongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseNote item in list2)
			{
				collectionForType.DeleteObject(item, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType.DoPostObjectsDeleteWorkflow();
			list.AddRange(list2);
		}
		if (Settings.Instance.RemoveEventsOutsideMap)
		{
			BeatmapObjectContainerCollection collectionForType2 = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event);
			List<BaseEvent> list3 = BeatSaberSongContainer.Instance.Map.Events.Where((BaseEvent evt) => evt.SongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseEvent item2 in list3)
			{
				collectionForType2.DeleteObject(item2, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType2.DoPostObjectsDeleteWorkflow();
			list.AddRange(list3);
			BeatmapObjectContainerCollection collectionForType3 = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.BpmChange);
			List<BaseBpmEvent> list4 = BeatSaberSongContainer.Instance.Map.BpmEvents.Where((BaseBpmEvent bpmEvt) => bpmEvt.SongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseBpmEvent item3 in list4)
			{
				collectionForType3.DeleteObject(item3, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType3.DoPostObjectsDeleteWorkflow();
			list.AddRange(list4);
		}
		if (Settings.Instance.RemoveObstaclesOutsideMap)
		{
			BeatmapObjectContainerCollection collectionForType4 = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Obstacle);
			List<BaseObstacle> list5 = BeatSaberSongContainer.Instance.Map.Obstacles.Where((BaseObstacle obst) => obst.SongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseObstacle item4 in list5)
			{
				collectionForType4.DeleteObject(item4, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType4.DoPostObjectsDeleteWorkflow();
			list.AddRange(list5);
		}
		if (Settings.Instance.RemoveArcsOutsideMap)
		{
			BeatmapObjectContainerCollection collectionForType5 = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Arc);
			List<BaseArc> list6 = BeatSaberSongContainer.Instance.Map.Arcs.Where((BaseArc arc) => arc.SongBpmTime >= maxSongBpmTime || arc.TailSongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseArc item5 in list6)
			{
				collectionForType5.DeleteObject(item5, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType5.DoPostObjectsDeleteWorkflow();
			list.AddRange(list6);
		}
		if (Settings.Instance.RemoveChainsOutsideMap)
		{
			BeatmapObjectContainerCollection collectionForType6 = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Chain);
			List<BaseChain> list7 = BeatSaberSongContainer.Instance.Map.Chains.Where((BaseChain chain) => chain.SongBpmTime >= maxSongBpmTime || chain.TailSongBpmTime >= maxSongBpmTime).ToList();
			foreach (BaseChain item6 in list7)
			{
				collectionForType6.DeleteObject(item6, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
			}
			collectionForType6.DoPostObjectsDeleteWorkflow();
			list.AddRange(list7);
		}
		if (list.Count > 0)
		{
			BeatmapActionContainer.AddAction(new SelectionDeletedAction(list));
		}
	}

	private bool ObjectIsOutsideMap()
	{
		if (Settings.Instance.RemoveNotesOutsideMap && BeatSaberSongContainer.Instance.Map.Notes.Any((BaseNote note) => note.SongBpmTime >= maxSongBpmTime))
		{
			return true;
		}
		if (Settings.Instance.RemoveEventsOutsideMap)
		{
			if (BeatSaberSongContainer.Instance.Map.Events.Any((BaseEvent evt) => evt.SongBpmTime >= maxSongBpmTime))
			{
				return true;
			}
			if (BeatSaberSongContainer.Instance.Map.BpmEvents.Any((BaseBpmEvent bpmEvt) => bpmEvt.SongBpmTime >= maxSongBpmTime))
			{
				return true;
			}
		}
		if (Settings.Instance.RemoveObstaclesOutsideMap && BeatSaberSongContainer.Instance.Map.Obstacles.Any((BaseObstacle obst) => obst.SongBpmTime >= maxSongBpmTime))
		{
			return true;
		}
		if (Settings.Instance.RemoveArcsOutsideMap && BeatSaberSongContainer.Instance.Map.Arcs.Any((BaseArc arc) => arc.SongBpmTime >= maxSongBpmTime || arc.TailSongBpmTime >= maxSongBpmTime))
		{
			return true;
		}
		if (Settings.Instance.RemoveChainsOutsideMap && BeatSaberSongContainer.Instance.Map.Chains.Any((BaseChain chain) => chain.SongBpmTime >= maxSongBpmTime || chain.TailSongBpmTime >= maxSongBpmTime))
		{
			return true;
		}
		return false;
	}

	public void ToggleAutoSave(bool enabled)
	{
		Settings.Instance.AutoSave = enabled;
	}

	private void ScheduleWarningIfIncompatibleDataIsPresent()
	{
		BaseDifficulty map = BeatSaberSongContainer.Instance.Map;
		StringBuilder stringBuilder = new StringBuilder();
		switch (Settings.Instance.MapVersion)
		{
		case 2:
			if (map.Notes.Any((BaseNote note) => note.AngleOffset != 0))
			{
				stringBuilder.AppendLine("* Note Angle Offset (v3/v4)");
			}
			if (map.Obstacles.Any((BaseObstacle obs) => obs.PosY != 0 && obs.PosY != 2))
			{
				stringBuilder.AppendLine("* Obstacle Y position (v3/v4)");
			}
			if (map.Obstacles.Any((BaseObstacle obs) => (obs.PosY == 0 && obs.Height < 5) || (obs.PosY == 2 && obs.Height < 3)))
			{
				stringBuilder.AppendLine("* Obstacle Height (v3/v4)");
			}
			if (map.Chains.Any())
			{
				stringBuilder.AppendLine("* Chains (v3/v4)");
			}
			if (map.Arcs.Any())
			{
				stringBuilder.AppendLine("* Arcs (v3/v4)");
			}
			if (map.LightColorEventBoxGroups.Any() || map.LightRotationEventBoxGroups.Any() || map.LightTranslationEventBoxGroups.Any())
			{
				stringBuilder.AppendLine("* Group Lighting (v3/v4)");
			}
			if (map.Notes.Any((BaseNote obj) => obj.Rotation != 0) || map.Obstacles.Any((BaseObstacle obj) => obj.Rotation != 0) || map.Arcs.Any((BaseArc obj) => obj.Rotation != 0 || obj.TailRotation != 0) || map.Chains.Any((BaseChain obj) => obj.Rotation != 0 || obj.TailRotation != 0))
			{
				stringBuilder.AppendLine("* Rotation Properties (v4)");
			}
			if (map.NJSEvents.Any())
			{
				stringBuilder.AppendLine("* NJS Events (v4)");
			}
			break;
		case 3:
			if (map.Notes.Any((BaseNote obj) => obj.Rotation != 0) || map.Obstacles.Any((BaseObstacle obj) => obj.Rotation != 0) || map.Arcs.Any((BaseArc obj) => obj.Rotation != 0 || obj.TailRotation != 0) || map.Chains.Any((BaseChain obj) => obj.Rotation != 0 || obj.TailRotation != 0))
			{
				stringBuilder.AppendLine("* Rotation Properties (v4)");
			}
			if (map.NJSEvents.Any())
			{
				stringBuilder.AppendLine("* NJS Events (v4)");
			}
			break;
		case 4:
			if (map.Events.Any((BaseEvent e) => e.IsLaneRotationEvent()))
			{
				stringBuilder.AppendLine("* Lane Rotation Event (v2/v3)");
			}
			if (map.Notes.Any((BaseNote n) => n.CustomData.Count > 0) || map.Obstacles.Any((BaseObstacle o) => o.CustomData.Count > 0) || map.Events.Any((BaseEvent e) => e.CustomData.Count > 0) || map.Arcs.Any((BaseArc c) => c.CustomData.Count > 0) || map.Chains.Any((BaseChain c) => c.CustomData.Count > 0))
			{
				stringBuilder.AppendLine("* Object CustomData (v2/v3)");
			}
			if (map.EnvironmentEnhancements.Any())
			{
				stringBuilder.AppendLine("* Environment Enhancements (v2/v3)");
			}
			if (map.CustomEvents.Any())
			{
				stringBuilder.AppendLine("* Custom Events (v2/v3)");
			}
			break;
		}
		if (stringBuilder.Length != 0)
		{
			stringBuilder.Insert(0, string.Format(localizedSaveWarningHeader, Settings.Instance.MapVersion));
			stringBuilder.AppendLine(localizedSaveWarningSubHeader);
		}
		saveWarningMessage = stringBuilder.ToString();
	}

	public void Save(bool auto = false)
	{
		if (IsSaving)
		{
			Debug.LogError(":hyperPepega: :mega: STOP TRYING TO SAVE THE SONG WHILE ITS ALREADY SAVING TO DISK");
			return;
		}
		PersistentUI.MessageDisplayer.NotificationMessage notification = PersistentUI.Instance.DisplayMessage("Mapper", (auto ? "auto" : "") + "save.message", PersistentUI.DisplayMessageType.Bottom);
		notification.SkipFade = true;
		notification.WaitTime = 5f;
		if (!auto)
		{
			beatmapActionContainer.UpdateActiveActionsAfterSave();
		}
		savingThread = Task.Run(delegate
		{
			Thread.CurrentThread.IsBackground = true;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			string directoryAndFile = BeatSaberSongContainer.Instance.Map.DirectoryAndFile;
			string directory = BeatSaberSongContainer.Instance.Info.Directory;
			try
			{
				if (auto)
				{
					string text = Path.Combine(directory, "autosaves", $"{DateTime.Now:dd-MM-yyyy_HH-mm-ss}");
					Debug.Log("Auto saved to: " + text);
					Directory.CreateDirectory(text);
					BeatSaberSongContainer.Instance.Map.DirectoryAndFile = Path.Combine(text, BeatSaberSongContainer.Instance.MapDifficultyInfo.BeatmapFileName);
					BeatSaberSongContainer.Instance.Info.Directory = text;
					DirectoryInfo item = new DirectoryInfo(text);
					currentAutoSaves.Add(item);
					CleanAutosaves();
				}
				if (!auto)
				{
					ScheduleWarningIfIncompatibleDataIsPresent();
				}
				BeatSaberSongContainer.Instance.Map.Save();
				BeatSaberSongContainer.Instance.MapDifficultyInfo.RefreshRequirementsAndWarnings(BeatSaberSongContainer.Instance.Map);
				BeatSaberSongContainer.Instance.Info.Save();
			}
			catch (Exception exception)
			{
				Debug.LogError("Failed to autosave (don't worry, progress wasn't lost)");
				Debug.LogException(exception);
			}
			BeatSaberSongContainer.Instance.Info.Directory = directory;
			BeatSaberSongContainer.Instance.Map.DirectoryAndFile = directoryAndFile;
			notification.SkipDisplay = true;
		});
	}

	private void CleanAutosaves()
	{
		if (currentAutoSaves.Count <= 15)
		{
			return;
		}
		Debug.Log($"Too many autosaves; removing excess... ({currentAutoSaves.Count} > {15})");
		DirectoryInfo[] source = currentAutoSaves.OrderByDescending((DirectoryInfo d) => d.LastWriteTime).ToArray();
		currentAutoSaves = source.Take(15).ToList();
		foreach (DirectoryInfo item in source.Skip(15))
		{
			try
			{
				Directory.Delete(item.FullName, recursive: true);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to delete an old autosave (" + item.Name + "): " + ex.Message + ".");
			}
		}
	}
}
