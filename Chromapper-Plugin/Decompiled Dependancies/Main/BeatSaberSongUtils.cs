using System;
using System.IO;
using Beatmap.Base;
using Beatmap.Helper;
using Beatmap.Info;
using SimpleJSON;
using UnityEngine;

public static class BeatSaberSongUtils
{
	public static BaseInfo GetInfoFromFolder(string directory)
	{
		try
		{
			JSONNode nodeFromFile = GetNodeFromFile(directory + "/Info.dat");
			if (nodeFromFile == null)
			{
				nodeFromFile = GetNodeFromFile(directory + "/info.dat");
				if (nodeFromFile == null)
				{
					return null;
				}
				File.Move(directory + "/info.dat", directory + "/Info.dat");
			}
			int num = -1;
			if (nodeFromFile.HasKey("_version"))
			{
				num = 2;
			}
			else if (nodeFromFile.HasKey("version"))
			{
				num = ((nodeFromFile["version"].Value[0] == '4') ? 4 : (-1));
			}
			BaseInfo baseInfo = num switch
			{
				2 => V2Info.GetFromJson(nodeFromFile), 
				4 => V4Info.GetFromJson(nodeFromFile), 
				_ => null, 
			};
			if (baseInfo != null)
			{
				baseInfo.Directory = directory;
			}
			else
			{
				Debug.LogWarning("Could not parse Info.dat in " + directory);
			}
			return baseInfo;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return null;
		}
	}

	public static BaseDifficulty GetMapFromInfoFiles(BaseInfo info, InfoDifficulty difficultyData)
	{
		if (!Directory.Exists(info.Directory))
		{
			Debug.LogWarning("Failed to get difficulty json file.");
			return null;
		}
		string text = Path.Combine(info.Directory, difficultyData.BeatmapFileName);
		JSONNode nodeFromFile = GetNodeFromFile(text);
		if (nodeFromFile == null)
		{
			Debug.LogWarning("Failed to get difficulty json file " + text);
			return null;
		}
		return BeatmapFactory.GetDifficultyFromJson(nodeFromFile, text, info, difficultyData);
	}

	public static JSONNode GetNodeFromFile(string file)
	{
		if (!File.Exists(file))
		{
			return null;
		}
		try
		{
			using StreamReader streamReader = new StreamReader(file);
			return JSON.Parse(streamReader.ReadToEnd());
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error trying to read from file {file}\n{arg}");
		}
		return null;
	}
}
