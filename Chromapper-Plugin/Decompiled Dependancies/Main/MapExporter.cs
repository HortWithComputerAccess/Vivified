using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Beatmap.Info;
using QuestDumper;
using UnityEngine;
using UnityEngine.Localization.Settings;

public struct MapExporter(BaseInfo info)
{
	public const string QUEST_CUSTOM_SONGS_LOCATION = "sdcard/ModData/com.beatgames.beatsaber/Mods/SongLoader/CustomLevels";

	public const string QUEST_CUSTOM_SONGS_WIP_LOCATION = "sdcard/ModData/com.beatgames.beatsaber/Mods/SongLoader/CustomWIPLevels";

	private readonly BaseInfo info = info;

	public async Task ExportToQuest()
	{
		var (list, adbOutput) = await Adb.GetDevices();
		if (list == null || !string.IsNullOrEmpty(adbOutput.ErrorOut))
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "quest.no-devices", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		List<string> list2 = (from s in await Task.WhenAll(list.Select(async (string device) => (device: device, quest: (await Adb.IsQuest(device)).Item1)))
			where s.quest
			select s.device).ToList();
		if (list2.Count == 0)
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "quest.no-quest", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		DialogBox dialog = PersistentUI.Instance.CreateNewDialogBox();
		dialog.WithTitle("SongEditMenu", "quest.exporting");
		ProgressBarComponent progressBar = dialog.AddComponent<ProgressBarComponent>();
		progressBar.WithCustomLabelFormatter((float f) => LocalizationSettings.StringDatabase.GetLocalizedString("SongEditMenu", "quest.exporting_progress", new object[1] { f * 100f }));
		dialog.Open();
		string songExportPath = Path.Combine("sdcard/ModData/com.beatgames.beatsaber/Mods/SongLoader/CustomWIPLevels", info.CleanSongName).Replace("\\", "/");
		Dictionary<string, string> exportedFiles = BeatSaberSongExtensions.GetFilesForArchiving(info);
		if (exportedFiles == null)
		{
			return;
		}
		Debug.Log("Creating folder if needed at " + songExportPath);
		int totalFiles = list2.Count * exportedFiles.Count;
		int fCount = 0;
		foreach (string questCandidate in list2)
		{
			Debug.Log($"ADB Create dir: {await Adb.Mkdir(songExportPath, questCandidate)}");
			foreach (KeyValuePair<string, string> item in exportedFiles)
			{
				string value = item.Value;
				string text = Path.Combine(songExportPath, value).Replace("\\", "/");
				Debug.Log("Pushing " + text + " from " + item.Key);
				Debug.Log((await Adb.Push(item.Key, text, questCandidate)).ToString());
				fCount++;
				progressBar.UpdateProgressBar((float)fCount / (float)totalFiles);
			}
		}
		dialog.Clear();
		Debug.Log("EXPORTED TO QUEST SUCCESSFULLY YAYAAYAYA");
		dialog.WithTitle("Options", "quest.success");
		dialog.AddFooterButton(null, "PersistentUI", "ok");
	}

	public bool PackageZip()
	{
		string path = "";
		string text = "";
		if (Directory.Exists(info.Directory))
		{
			text = Path.Combine(info.Directory, info.CleanSongName + ".zip");
			File.Delete(text);
			path = Path.Combine(info.Directory, "Info.dat");
		}
		if (!File.Exists(path))
		{
			Debug.LogError(":hyperPepega: :mega: WHY TF ARE YOU TRYING TO PACKAGE A MAP WITH NO INFO.DAT FILE");
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "zip.warning", null, PersistentUI.DialogBoxPresetType.Ok);
			return false;
		}
		Dictionary<string, string> filesForArchiving = BeatSaberSongExtensions.GetFilesForArchiving(info);
		if (filesForArchiving == null)
		{
			return false;
		}
		using (ZipArchive destination = ZipFile.Open(text, ZipArchiveMode.Create))
		{
			foreach (KeyValuePair<string, string> item in filesForArchiving)
			{
				destination.CreateEntryFromFile(item.Key, item.Value);
			}
		}
		return true;
	}

	public void OpenSelectedMapInFileBrowser()
	{
		if (!Directory.Exists(info.Directory))
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "explorer.warning", null, PersistentUI.DialogBoxPresetType.Ok);
		}
		else
		{
			OSTools.OpenFileBrowser(info.Directory);
		}
	}
}
