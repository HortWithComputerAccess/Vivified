using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beatmap.Info;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public static class BeatSaberSongExtensions
{
	public static IEnumerator LoadAudio(BaseInfo mapInfo, Action<AudioClip> onClipLoaded, float songTimeOffset = 0f, string overrideLocalPath = null)
	{
		if (!Directory.Exists(mapInfo.Directory))
		{
			yield break;
		}
		string text = Path.Combine(mapInfo.Directory, overrideLocalPath ?? mapInfo.SongFilename);
		if (!File.Exists(text))
		{
			yield break;
		}
		AudioType audioType = FileContentValidationHelper.GetAudioType(text);
		if (audioType == AudioType.UNKNOWN)
		{
			SceneTransitionManager.Instance.CancelLoading("load.error.audio2");
			yield break;
		}
		RuntimePlatform platform = Application.platform;
		string text2 = ((platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor) ? Uri.EscapeDataString(text) : Uri.EscapeUriString(text));
		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + text2, audioType);
		yield return www.SendWebRequest();
		Debug.Log("Song loaded!");
		AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
		if (audioClip == null)
		{
			Debug.Log("Error getting Audio data!");
			SceneTransitionManager.Instance.CancelLoading("load.error.audio");
			yield break;
		}
		audioClip.name = "Song";
		if (songTimeOffset != 0f)
		{
			int num = Mathf.CeilToInt(songTimeOffset * (float)audioClip.frequency) * audioClip.channels;
			float[] array = new float[audioClip.samples * audioClip.channels];
			audioClip.GetData(array, 0);
			if (num < 0)
			{
				Array.Resize(ref array, array.Length - num);
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					int num3 = num2 + num;
					array[num2] = ((num3 < 0) ? 0f : array[num3]);
				}
			}
			else
			{
				for (int i = 0; i < array.Length; i++)
				{
					int num4 = i + num;
					array[i] = ((num4 >= array.Length) ? 0f : array[num4]);
				}
				Array.Resize(ref array, Math.Max(array.Length - num, audioClip.channels * 4096));
			}
			audioClip = AudioClip.Create(audioClip.name, array.Length / audioClip.channels, audioClip.channels, audioClip.frequency, stream: false);
			audioClip.SetData(array, 0);
		}
		onClipLoaded?.Invoke(audioClip);
	}

	[CanBeNull]
	public static Dictionary<string, string> GetFilesForArchiving(BaseInfo info)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string text = "";
		if (Directory.Exists(info.Directory))
		{
			text = Path.Combine(info.Directory, "Info.dat");
		}
		if (!File.Exists(text))
		{
			Debug.LogError(":hyperPepega: :mega: WHY TF ARE YOU TRYING TO PACKAGE A MAP WITH NO INFO.DAT FILE");
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "zip.warning", null, PersistentUI.DialogBoxPresetType.Ok);
			return null;
		}
		dictionary.Add(text, "Info.dat");
		TryAddToFileDictionary(dictionary, info.Directory, info.CoverImageFilename);
		TryAddToFileDictionary(dictionary, info.Directory, info.SongFilename);
		TryAddToFileDictionary(dictionary, info.Directory, info.SongPreviewFilename);
		TryAddToFileDictionary(dictionary, info.Directory, "cinema-video.json");
		TryAddToFileDictionary(dictionary, info.Directory, (info.MajorVersion == 4) ? info.AudioDataFilename : "BPMInfo.dat");
		foreach (BaseContributor customContributor in info.CustomContributors)
		{
			TryAddToFileDictionary(dictionary, info.Directory, customContributor.LocalImageLocation);
		}
		foreach (InfoDifficulty item in info.DifficultySets.SelectMany((InfoDifficultySet set) => set.Difficulties))
		{
			TryAddToFileDictionary(dictionary, info.Directory, item.BeatmapFileName);
			TryAddToFileDictionary(dictionary, info.Directory, item.LightshowFileName);
		}
		foreach (InfoDifficultySet difficultySet in info.DifficultySets)
		{
			TryAddToFileDictionary(dictionary, info.Directory, difficultySet.CustomCharacteristicIconImageFileName);
		}
		string path = Path.Combine(info.Directory, "Bookmarks");
		if (info.MajorVersion == 4 && Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "*.bookmarks.dat");
			foreach (string path2 in files)
			{
				TryAddToFileDictionary(dictionary, info.Directory, Path.GetRelativePath(info.Directory, path2));
			}
		}
		if (dictionary.Any((KeyValuePair<string, string> file) => Path.IsPathRooted(file.Value)))
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "zip.path.error", null, PersistentUI.DialogBoxPresetType.Ok);
			return null;
		}
		foreach (KeyValuePair<string, string> item2 in dictionary.ToList())
		{
			dictionary[item2.Key] = item2.Value.Replace('\\', '/');
		}
		return dictionary;
	}

	private static bool TryAddToFileDictionary(IDictionary<string, string> fileMap, string directory, string fileLocation)
	{
		if (directory == null || fileLocation == null)
		{
			return false;
		}
		string text = Path.Combine(directory, fileLocation);
		if (File.Exists(text))
		{
			return fileMap.TryAdd(text, fileLocation);
		}
		return false;
	}
}
