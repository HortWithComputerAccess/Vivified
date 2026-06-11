using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace Beatmap.Info;

public static class V2Info
{
	public const string Version = "2.1.0";

	public static BaseInfo GetFromJson(JSONNode node)
	{
		BaseInfo baseInfo = new BaseInfo();
		baseInfo.Version = node["_version"].Value;
		baseInfo.SongName = node["_songName"].Value;
		baseInfo.SongSubName = node["_songSubName"].Value;
		baseInfo.SongAuthorName = node["_songAuthorName"].Value;
		baseInfo.LevelAuthorName = node["_levelAuthorName"].Value;
		baseInfo.BeatsPerMinute = node["_beatsPerMinute"].AsFloat;
		baseInfo.SongTimeOffset = node["_songTimeOffset"].AsFloat;
		baseInfo.Shuffle = node["_shuffle"].AsFloat;
		baseInfo.ShufflePeriod = node["_shufflePeriod"].AsFloat;
		baseInfo.PreviewStartTime = node["_previewStartTime"].AsFloat;
		baseInfo.PreviewDuration = node["_previewDuration"].AsFloat;
		baseInfo.SongFilename = node["_songFilename"].Value;
		baseInfo.CoverImageFilename = node["_coverImageFilename"].Value;
		string value = node["_environmentName"].Value;
		baseInfo.EnvironmentNames = node["_environmentNames"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
		if (baseInfo.EnvironmentNames.Count == 0)
		{
			baseInfo.EnvironmentNames.Add(value);
		}
		List<InfoColorScheme> list = new List<InfoColorScheme>();
		foreach (JSONObject item in node["_colorSchemes"].AsArray.Children.Select((JSONNode x) => x.AsObject))
		{
			InfoColorScheme infoColorScheme = new InfoColorScheme();
			infoColorScheme.UseOverride = item["useOverride"].AsBool;
			infoColorScheme.OverrideNotes = item["useOverride"].AsBool;
			infoColorScheme.OverrideLights = item["useOverride"].AsBool;
			infoColorScheme.ColorSchemeName = item["colorScheme"]["colorSchemeId"].Value;
			infoColorScheme.SaberAColor = item["colorScheme"]["saberAColor"].ReadColor();
			infoColorScheme.SaberBColor = item["colorScheme"]["saberBColor"].ReadColor();
			infoColorScheme.ObstaclesColor = item["colorScheme"]["obstaclesColor"].ReadColor();
			infoColorScheme.EnvironmentColor0 = item["colorScheme"]["environmentColor0"].ReadColor();
			infoColorScheme.EnvironmentColor1 = item["colorScheme"]["environmentColor1"].ReadColor();
			infoColorScheme.EnvironmentColor0Boost = item["colorScheme"]["environmentColor0Boost"].ReadColor();
			infoColorScheme.EnvironmentColor1Boost = item["colorScheme"]["environmentColor1Boost"].ReadColor();
			list.Add(infoColorScheme);
		}
		baseInfo.ColorSchemes = list;
		List<InfoDifficultySet> list2 = new List<InfoDifficultySet>();
		foreach (JSONNode child in node["_difficultyBeatmapSets"].AsArray.Children)
		{
			InfoDifficultySet infoDifficultySet = new InfoDifficultySet();
			infoDifficultySet.Characteristic = child["_beatmapCharacteristicName"].Value;
			List<InfoDifficulty> list3 = new List<InfoDifficulty>();
			foreach (JSONNode child2 in child["_difficultyBeatmaps"].AsArray.Children)
			{
				InfoDifficulty infoDifficulty = new InfoDifficulty(infoDifficultySet);
				infoDifficulty.BeatmapFileName = child2["_beatmapFilename"].Value;
				infoDifficulty.Difficulty = child2["_difficulty"].Value;
				infoDifficulty.EnvironmentNameIndex = child2["_environmentNameIdx"].AsInt;
				infoDifficulty.ColorSchemeIndex = child2["_beatmapColorSchemeIdx"].AsInt;
				infoDifficulty.NoteJumpSpeed = child2["_noteJumpMovementSpeed"].AsFloat;
				infoDifficulty.NoteStartBeatOffset = child2["_noteJumpStartBeatOffset"].AsFloat;
				JSONObject asObject = child2["_customData"].AsObject;
				ParseDifficultyCustomData(asObject, infoDifficulty);
				infoDifficulty.CustomData = asObject;
				list3.Add(infoDifficulty);
			}
			infoDifficultySet.Difficulties = list3;
			JSONObject asObject2 = child["_customData"].AsObject;
			ParseDifficultySetCustomData(asObject2, infoDifficultySet);
			infoDifficultySet.CustomData = asObject2;
			list2.Add(infoDifficultySet);
		}
		baseInfo.DifficultySets = list2;
		if (node["_customData"].IsObject)
		{
			JSONNode jSONNode = node["_customData"];
			if (jSONNode["_contributors"].IsArray)
			{
				baseInfo.CustomContributors = jSONNode["_contributors"].AsArray.Children.Select(V2Contributor.GetFromJson).ToList();
				jSONNode.Remove("_contributors");
			}
			if (jSONNode["_editors"].IsObject)
			{
				baseInfo.CustomEditorsData = new BaseInfo.CustomEditorsMetadata(jSONNode["_editors"]);
				jSONNode.Remove("_editors");
			}
			if (jSONNode["_customEnvironment"].IsString)
			{
				baseInfo.CustomEnvironmentMetadata.Name = jSONNode["_customEnvironment"].Value;
				baseInfo.CustomEnvironmentMetadata.Hash = jSONNode["_customEnvironmentHash"].Value;
				jSONNode.Remove("_customEnvironment");
				jSONNode.Remove("_customEnvironmentHash");
			}
			baseInfo.CustomData = jSONNode;
		}
		return baseInfo;
	}

	public static JSONNode GetOutputJson(BaseInfo info)
	{
		JSONObject jSONObject = new JSONObject();
		jSONObject["_version"] = "2.1.0";
		jSONObject["_songName"] = info.SongName;
		jSONObject["_songSubName"] = info.SongSubName;
		jSONObject["_songSubName"] = info.SongSubName;
		jSONObject["_songAuthorName"] = info.SongAuthorName;
		jSONObject["_levelAuthorName"] = info.LevelAuthorName;
		jSONObject["_beatsPerMinute"] = info.BeatsPerMinute;
		jSONObject["_songTimeOffset"] = info.SongTimeOffset;
		jSONObject["_shuffle"] = info.Shuffle;
		jSONObject["_shufflePeriod"] = info.ShufflePeriod;
		jSONObject["_previewStartTime"] = info.PreviewStartTime;
		jSONObject["_previewDuration"] = info.PreviewDuration;
		jSONObject["_songFilename"] = info.SongFilename;
		jSONObject["_coverImageFilename"] = info.CoverImageFilename;
		jSONObject["_environmentName"] = info.EnvironmentName;
		jSONObject["_allDirectionsEnvironmentName"] = info.AllDirectionsEnvironmentName;
		JSONArray jSONArray = new JSONArray();
		foreach (string environmentName in info.EnvironmentNames)
		{
			jSONArray.Add(environmentName);
		}
		jSONObject["_environmentNames"] = jSONArray;
		JSONContainerType colorContainerType = JSONNode.ColorContainerType;
		JSONNode.ColorContainerType = JSONContainerType.Object;
		JSONArray jSONArray2 = new JSONArray();
		foreach (InfoColorScheme colorScheme in info.ColorSchemes)
		{
			JSONObject jSONObject2 = new JSONObject();
			jSONObject2["useOverride"] = colorScheme.UseOverride;
			jSONObject2["colorScheme"]["colorSchemeId"] = colorScheme.ColorSchemeName;
			jSONObject2["colorScheme"]["saberAColor"] = colorScheme.SaberAColor;
			jSONObject2["colorScheme"]["saberBColor"] = colorScheme.SaberBColor;
			jSONObject2["colorScheme"]["obstaclesColor"] = colorScheme.ObstaclesColor;
			jSONObject2["colorScheme"]["environmentColor0"] = colorScheme.EnvironmentColor0;
			jSONObject2["colorScheme"]["environmentColor1"] = colorScheme.EnvironmentColor1;
			jSONObject2["colorScheme"]["environmentColor0Boost"] = colorScheme.EnvironmentColor0Boost;
			jSONObject2["colorScheme"]["environmentColor1Boost"] = colorScheme.EnvironmentColor1Boost;
			jSONArray2.Add(jSONObject2);
		}
		jSONObject["_colorSchemes"] = jSONArray2;
		JSONNode.ColorContainerType = colorContainerType;
		JSONArray jSONArray3 = new JSONArray();
		foreach (InfoDifficultySet difficultySet in info.DifficultySets)
		{
			JSONObject jSONObject3 = new JSONObject { ["_beatmapCharacteristicName"] = difficultySet.Characteristic };
			JSONArray jSONArray4 = new JSONArray();
			foreach (InfoDifficulty item in difficultySet.Difficulties.OrderBy((InfoDifficulty x) => x.DifficultyRank))
			{
				JSONObject jSONObject4 = new JSONObject();
				jSONObject4["_difficulty"] = item.Difficulty;
				jSONObject4["_difficultyRank"] = item.DifficultyRank;
				jSONObject4["_beatmapFilename"] = item.BeatmapFileName;
				jSONObject4["_noteJumpMovementSpeed"] = item.NoteJumpSpeed;
				jSONObject4["_noteJumpStartBeatOffset"] = item.NoteStartBeatOffset;
				jSONObject4["_beatmapColorSchemeIdx"] = item.ColorSchemeIndex;
				jSONObject4["_environmentNameIdx"] = item.EnvironmentNameIndex;
				JSONNode outputDifficultyCustomData = GetOutputDifficultyCustomData(item);
				if (outputDifficultyCustomData.Count > 0)
				{
					jSONObject4["_customData"] = outputDifficultyCustomData;
				}
				jSONArray4.Add(jSONObject4);
			}
			jSONObject3["_difficultyBeatmaps"] = jSONArray4;
			JSONNode outputDifficultySetCustomData = GetOutputDifficultySetCustomData(difficultySet);
			if (outputDifficultySetCustomData.Count > 0)
			{
				jSONObject3["_customData"] = outputDifficultySetCustomData;
			}
			if (difficultySet.Difficulties.Count > 0 || outputDifficultySetCustomData.Count > 0)
			{
				jSONArray3.Add(jSONObject3);
			}
		}
		jSONObject["_difficultyBeatmapSets"] = jSONArray3;
		JSONNode jSONNode = info.CustomData.Clone();
		if (info.CustomContributors.Any())
		{
			JSONArray jSONArray5 = new JSONArray();
			foreach (BaseContributor customContributor in info.CustomContributors)
			{
				jSONArray5.Add(V2Contributor.ToJson(customContributor));
			}
			jSONNode["_contributors"] = jSONArray5;
		}
		if (!string.IsNullOrEmpty(info.CustomEnvironmentMetadata.Name))
		{
			jSONNode["_customEnvironment"] = info.CustomEnvironmentMetadata.Name;
			jSONNode["_customEnvironmentHash"] = info.CustomEnvironmentMetadata.Hash;
		}
		jSONNode["_editors"] = info.CustomEditorsData.ToJson();
		jSONObject["_customData"] = jSONNode;
		return jSONObject;
	}

	private static void ParseDifficultySetCustomData(JSONNode customData, InfoDifficultySet difficultySet)
	{
		if (customData["_characteristicLabel"].IsString)
		{
			difficultySet.CustomCharacteristicLabel = customData["_characteristicLabel"].Value;
			customData.Remove("_characteristicLabel");
		}
		if (customData["_characteristicIconImageFilename"].IsString)
		{
			difficultySet.CustomCharacteristicIconImageFileName = customData["_characteristicIconImageFilename"].Value;
			customData.Remove("_characteristicIconImageFilename");
		}
	}

	private static void ParseDifficultyCustomData(JSONNode customData, InfoDifficulty difficulty)
	{
		if (customData["_oneSaber"].IsBoolean)
		{
			difficulty.CustomOneSaberFlag = customData["_oneSaber"].AsBool;
			customData.Remove("_oneSaber");
		}
		if (customData["_showRotationNoteSpawnLines"].IsBoolean)
		{
			difficulty.CustomShowRotationNoteSpawnLinesFlag = customData["_showRotationNoteSpawnLines"].AsBool;
			customData.Remove("_showRotationNoteSpawnLines");
		}
		if (customData["_difficultyLabel"].IsString)
		{
			difficulty.CustomLabel = customData["_difficultyLabel"].Value;
			customData.Remove("_difficultyLabel");
		}
		if (customData["_information"].IsArray)
		{
			difficulty.CustomInformation = customData["_information"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("_information");
		}
		if (customData["_warnings"].IsArray)
		{
			difficulty.CustomWarnings = customData["_warnings"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("_warnings");
		}
		if (customData["_suggestions"].IsArray)
		{
			difficulty.CustomSuggestions = customData["_suggestions"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("_suggestions");
		}
		if (customData["_requirements"].IsArray)
		{
			difficulty.CustomRequirements = customData["_requirements"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("_requirements");
		}
		if (customData["_colorLeft"].IsObject)
		{
			difficulty.CustomColorLeft = customData["_colorLeft"].ReadColor();
			customData.Remove("_colorLeft");
		}
		if (customData["_colorRight"].IsObject)
		{
			difficulty.CustomColorRight = customData["_colorRight"].ReadColor();
			customData.Remove("_colorRight");
		}
		if (customData["_obstacleColor"].IsObject)
		{
			difficulty.CustomColorObstacle = customData["_obstacleColor"].ReadColor();
			customData.Remove("_obstacleColor");
		}
		if (customData["_envColorLeft"].IsObject)
		{
			difficulty.CustomEnvColorLeft = customData["_envColorLeft"].ReadColor();
			customData.Remove("_envColorLeft");
		}
		if (customData["_envColorRight"].IsObject)
		{
			difficulty.CustomEnvColorRight = customData["_envColorRight"].ReadColor();
			customData.Remove("_envColorRight");
		}
		if (customData["_envColorWhite"].IsObject)
		{
			difficulty.CustomEnvColorWhite = customData["_envColorWhite"].ReadColor();
			customData.Remove("_envColorWhite");
		}
		if (customData["_envColorLeftBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostLeft = customData["_envColorLeftBoost"].ReadColor();
			customData.Remove("_envColorLeftBoost");
		}
		if (customData["_envColorRightBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostRight = customData["_envColorRightBoost"].ReadColor();
			customData.Remove("_envColorRightBoost");
		}
		if (customData["_envColorWhiteBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostWhite = customData["_envColorWhiteBoost"].ReadColor();
			customData.Remove("_envColorWhiteBoost");
		}
	}

	private static JSONNode GetOutputDifficultySetCustomData(InfoDifficultySet difficultySet)
	{
		JSONNode jSONNode = difficultySet.CustomData.Clone();
		if (!string.IsNullOrWhiteSpace(difficultySet.CustomCharacteristicLabel))
		{
			jSONNode["_characteristicLabel"] = difficultySet.CustomCharacteristicLabel;
		}
		if (!string.IsNullOrWhiteSpace(difficultySet.CustomCharacteristicIconImageFileName))
		{
			jSONNode["_characteristicIconImageFilename"] = difficultySet.CustomCharacteristicIconImageFileName;
		}
		return jSONNode;
	}

	private static JSONNode GetOutputDifficultyCustomData(InfoDifficulty difficulty)
	{
		JSONNode jSONNode = difficulty.CustomData.Clone();
		if (difficulty.CustomOneSaberFlag.HasValue)
		{
			jSONNode["_oneSaber"] = difficulty.CustomOneSaberFlag.Value;
		}
		if (difficulty.CustomShowRotationNoteSpawnLinesFlag.HasValue)
		{
			jSONNode["_showRotationNoteSpawnLines"] = difficulty.CustomShowRotationNoteSpawnLinesFlag.Value;
		}
		if (!string.IsNullOrWhiteSpace(difficulty.CustomLabel))
		{
			jSONNode["_difficultyLabel"] = difficulty.CustomLabel;
		}
		if (difficulty.CustomInformation.Any())
		{
			jSONNode["_information"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomInformation, (string s) => s);
		}
		if (difficulty.CustomWarnings.Any())
		{
			jSONNode["_warnings"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomWarnings, (string s) => s);
		}
		if (difficulty.CustomSuggestions.Any())
		{
			jSONNode["_suggestions"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomSuggestions, (string s) => s);
		}
		if (difficulty.CustomRequirements.Any())
		{
			jSONNode["_requirements"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomRequirements, (string s) => s);
		}
		JSONNode.ColorContainerType = JSONContainerType.Object;
		if (difficulty.CustomColorLeft.HasValue)
		{
			jSONNode["_colorLeft"] = difficulty.CustomColorLeft.Value;
		}
		if (difficulty.CustomColorRight.HasValue)
		{
			jSONNode["_colorRight"] = difficulty.CustomColorRight.Value;
		}
		if (difficulty.CustomColorObstacle.HasValue)
		{
			jSONNode["_obstacleColor"] = difficulty.CustomColorObstacle.Value;
		}
		if (difficulty.CustomEnvColorLeft.HasValue)
		{
			jSONNode["_envColorLeft"] = difficulty.CustomEnvColorLeft.Value;
		}
		if (difficulty.CustomEnvColorRight.HasValue)
		{
			jSONNode["_envColorRight"] = difficulty.CustomEnvColorRight.Value;
		}
		if (difficulty.CustomEnvColorWhite.HasValue)
		{
			jSONNode["_envColorWhite"] = difficulty.CustomEnvColorWhite.Value;
		}
		if (difficulty.CustomEnvColorBoostLeft.HasValue)
		{
			jSONNode["_envColorLeftBoost"] = difficulty.CustomEnvColorBoostLeft.Value;
		}
		if (difficulty.CustomEnvColorBoostRight.HasValue)
		{
			jSONNode["_envColorRightBoost"] = difficulty.CustomEnvColorBoostRight.Value;
		}
		if (difficulty.CustomEnvColorBoostWhite.HasValue)
		{
			jSONNode["_envColorWhiteBoost"] = difficulty.CustomEnvColorBoostWhite.Value;
		}
		JSONNode.ColorContainerType = JSONContainerType.Array;
		return jSONNode;
	}
}
