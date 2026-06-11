using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Info;

public static class V4Info
{
	public const string Version = "4.0.1";

	public static BaseInfo GetFromJson(JSONNode node)
	{
		BaseInfo baseInfo = new BaseInfo();
		baseInfo.Version = node["version"].Value;
		JSONObject asObject = node["song"].AsObject;
		baseInfo.SongName = asObject["title"].Value;
		baseInfo.SongSubName = asObject["subTitle"].Value;
		baseInfo.SongAuthorName = asObject["author"].Value;
		JSONObject asObject2 = node["audio"].AsObject;
		baseInfo.SongFilename = asObject2["songFilename"].Value;
		baseInfo.SongDurationMetadata = asObject2["songDuration"].AsFloat;
		baseInfo.AudioDataFilename = asObject2["audioDataFilename"].Value;
		baseInfo.BeatsPerMinute = asObject2["bpm"].AsFloat;
		baseInfo.PreviewStartTime = asObject2["previewStartTime"].AsFloat;
		baseInfo.PreviewDuration = asObject2["previewDuration"].AsFloat;
		baseInfo.Lufs = asObject2["lufs"].AsFloat;
		baseInfo.SongPreviewFilename = node["songPreviewFilename"].Value;
		baseInfo.CoverImageFilename = node["coverImageFilename"].Value;
		baseInfo.EnvironmentNames = node["environmentNames"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
		List<InfoColorScheme> list = new List<InfoColorScheme>();
		foreach (JSONObject item in node["colorSchemes"].AsArray.Children.Select((JSONNode x) => x.AsObject))
		{
			InfoColorScheme infoColorScheme = new InfoColorScheme();
			infoColorScheme.ColorSchemeName = item["colorSchemeName"].Value;
			if (item.HasKey("useOverride"))
			{
				infoColorScheme.UseOverride = item["useOverride"].AsBool;
				infoColorScheme.OverrideNotes = infoColorScheme.UseOverride;
				infoColorScheme.OverrideLights = infoColorScheme.UseOverride;
			}
			else
			{
				infoColorScheme.OverrideNotes = item["overrideNotes"].AsBool;
				infoColorScheme.OverrideLights = item["overrideLights"].AsBool;
				infoColorScheme.UseOverride = infoColorScheme.OverrideNotes || infoColorScheme.OverrideLights;
			}
			infoColorScheme.SaberAColor = item["saberAColor"].ReadHtmlStringColor();
			infoColorScheme.SaberBColor = item["saberBColor"].ReadHtmlStringColor();
			infoColorScheme.ObstaclesColor = item["obstaclesColor"].ReadHtmlStringColor();
			infoColorScheme.EnvironmentColor0 = item["environmentColor0"].ReadHtmlStringColor();
			infoColorScheme.EnvironmentColor1 = item["environmentColor1"].ReadHtmlStringColor();
			infoColorScheme.EnvironmentColor0Boost = item["environmentColor0Boost"].ReadHtmlStringColor();
			infoColorScheme.EnvironmentColor1Boost = item["environmentColor1Boost"].ReadHtmlStringColor();
			list.Add(infoColorScheme);
		}
		baseInfo.ColorSchemes = list;
		JSONArray asArray = node["difficultyBeatmaps"].AsArray;
		List<InfoDifficultySet> list2 = new List<InfoDifficultySet>();
		foreach (IGrouping<string, JSONNode> item2 in from x in asArray.Children
			group x by x["characteristic"].Value)
		{
			InfoDifficultySet infoDifficultySet = new InfoDifficultySet
			{
				Characteristic = item2.Key
			};
			List<InfoDifficulty> list3 = new List<InfoDifficulty>();
			foreach (JSONNode item3 in item2)
			{
				InfoDifficulty infoDifficulty = new InfoDifficulty(infoDifficultySet);
				infoDifficulty.Difficulty = item3["difficulty"].Value;
				infoDifficulty.EnvironmentNameIndex = item3["environmentNameIdx"].AsInt;
				infoDifficulty.ColorSchemeIndex = item3["beatmapColorSchemeIdx"].AsInt;
				infoDifficulty.NoteJumpSpeed = item3["noteJumpMovementSpeed"].AsFloat;
				infoDifficulty.NoteStartBeatOffset = item3["noteJumpStartBeatOffset"].AsFloat;
				infoDifficulty.BeatmapFileName = item3["beatmapDataFilename"].Value;
				infoDifficulty.LightshowFileName = item3["lightshowDataFilename"].Value;
				JSONObject asObject3 = item3["beatmapAuthors"].AsObject;
				infoDifficulty.Mappers = asObject3["mappers"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
				infoDifficulty.Lighters = asObject3["lighters"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
				JSONObject asObject4 = item3["customData"].AsObject;
				ParseDifficultyCustomData(asObject4, infoDifficulty);
				infoDifficulty.CustomData = asObject4;
				list3.Add(infoDifficulty);
			}
			infoDifficultySet.Difficulties = list3;
			list2.Add(infoDifficultySet);
		}
		baseInfo.DifficultySets = list2;
		if (node["customData"].IsObject)
		{
			JSONNode jSONNode = node["customData"];
			if (jSONNode["contributors"].IsArray)
			{
				baseInfo.CustomContributors = jSONNode["contributors"].AsArray.Children.Select(V4Contributor.GetFromJson).ToList();
				jSONNode.Remove("contributors");
			}
			if (jSONNode["characteristicData"].IsArray)
			{
				foreach (JSONNode child in jSONNode["characteristicData"].AsArray.Children)
				{
					string characteristic = child["characteristic"].Value;
					InfoDifficultySet infoDifficultySet2 = baseInfo.DifficultySets.FirstOrDefault((InfoDifficultySet x) => x.Characteristic == characteristic);
					if (infoDifficultySet2 == null)
					{
						infoDifficultySet2 = new InfoDifficultySet
						{
							Characteristic = characteristic
						};
						baseInfo.DifficultySets.Add(infoDifficultySet2);
					}
					ParseDifficultySetCustomData(child, infoDifficultySet2);
				}
				jSONNode.Remove("characteristicData");
			}
			if (jSONNode["editors"].IsObject)
			{
				baseInfo.CustomEditorsData = new BaseInfo.CustomEditorsMetadata(jSONNode["editors"]);
				jSONNode.Remove("editors");
			}
			if (jSONNode["customEnvironment"].IsString)
			{
				baseInfo.CustomEnvironmentMetadata.Name = jSONNode["customEnvironment"].Value;
				baseInfo.CustomEnvironmentMetadata.Hash = jSONNode["customEnvironmentHash"].Value;
				jSONNode.Remove("customEnvironment");
				jSONNode.Remove("customEnvironmentHash");
			}
			baseInfo.CustomData = jSONNode;
		}
		return baseInfo;
	}

	public static JSONNode GetOutputJson(BaseInfo info)
	{
		JSONObject jSONObject = new JSONObject();
		jSONObject["version"] = "4.0.1";
		JSONObject jSONObject2 = new JSONObject();
		jSONObject2["title"] = info.SongName;
		jSONObject2["subTitle"] = info.SongSubName;
		jSONObject2["author"] = info.SongAuthorName;
		jSONObject["song"] = jSONObject2;
		JSONObject jSONObject3 = new JSONObject();
		jSONObject3["songFilename"] = info.SongFilename;
		float? num = BeatSaberSongContainer.Instance?.LoadedSongLength;
		jSONObject3["songDuration"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
		jSONObject3["audioDataFilename"] = info.AudioDataFilename;
		jSONObject3["bpm"] = info.BeatsPerMinute;
		jSONObject3["lufs"] = info.Lufs;
		jSONObject3["previewStartTime"] = info.PreviewStartTime;
		jSONObject3["previewDuration"] = info.PreviewDuration;
		jSONObject["audio"] = jSONObject3;
		jSONObject["songPreviewFilename"] = info.SongPreviewFilename;
		jSONObject["coverImageFilename"] = info.CoverImageFilename;
		JSONArray jSONArray = new JSONArray();
		foreach (string environmentName in info.EnvironmentNames)
		{
			jSONArray.Add(environmentName);
		}
		jSONObject["environmentNames"] = jSONArray;
		JSONArray jSONArray2 = new JSONArray();
		foreach (InfoColorScheme colorScheme in info.ColorSchemes)
		{
			JSONObject jSONObject4 = new JSONObject();
			jSONObject4["colorSchemeName"] = colorScheme.ColorSchemeName;
			jSONObject4["overrideNotes"] = colorScheme.OverrideNotes;
			jSONObject4["saberAColor"] = ColorUtility.ToHtmlStringRGBA(colorScheme.SaberAColor);
			jSONObject4["saberBColor"] = ColorUtility.ToHtmlStringRGBA(colorScheme.SaberBColor);
			jSONObject4["obstaclesColor"] = ColorUtility.ToHtmlStringRGBA(colorScheme.ObstaclesColor);
			jSONObject4["overrideLights"] = colorScheme.OverrideLights;
			jSONObject4["environmentColor0"] = ColorUtility.ToHtmlStringRGBA(colorScheme.EnvironmentColor0);
			jSONObject4["environmentColor1"] = ColorUtility.ToHtmlStringRGBA(colorScheme.EnvironmentColor1);
			jSONObject4["environmentColor0Boost"] = ColorUtility.ToHtmlStringRGBA(colorScheme.EnvironmentColor0Boost);
			jSONObject4["environmentColor1Boost"] = ColorUtility.ToHtmlStringRGBA(colorScheme.EnvironmentColor1Boost);
			jSONArray2.Add(jSONObject4);
		}
		jSONObject["colorSchemes"] = jSONArray2;
		JSONArray jSONArray3 = new JSONArray();
		foreach (InfoDifficulty item in from x in info.DifficultySets.SelectMany((InfoDifficultySet x) => x.Difficulties)
			orderby x.Characteristic, x.DifficultyRank
			select x)
		{
			JSONObject jSONObject5 = new JSONObject();
			jSONObject5["characteristic"] = item.Characteristic;
			jSONObject5["difficulty"] = item.Difficulty;
			JSONObject jSONObject6 = new JSONObject();
			JSONArray jSONArray4 = new JSONArray();
			foreach (string item2 in item.Mappers.Where((string mapper) => !string.IsNullOrEmpty(mapper)))
			{
				jSONArray4.Add(item2);
			}
			jSONObject6["mappers"] = jSONArray4;
			JSONArray jSONArray5 = new JSONArray();
			foreach (string item3 in item.Lighters.Where((string lighter) => !string.IsNullOrEmpty(lighter)))
			{
				jSONArray5.Add(item3);
			}
			jSONObject6["lighters"] = jSONArray5;
			jSONObject5["beatmapAuthors"] = jSONObject6;
			jSONObject5["environmentNameIdx"] = item.EnvironmentNameIndex;
			jSONObject5["beatmapColorSchemeIdx"] = item.ColorSchemeIndex;
			jSONObject5["noteJumpMovementSpeed"] = item.NoteJumpSpeed;
			jSONObject5["noteJumpStartBeatOffset"] = item.NoteStartBeatOffset;
			jSONObject5["beatmapDataFilename"] = item.BeatmapFileName;
			jSONObject5["lightshowDataFilename"] = item.LightshowFileName;
			JSONNode outputDifficultyCustomData = GetOutputDifficultyCustomData(item);
			if (outputDifficultyCustomData.Count > 0)
			{
				jSONObject5["customData"] = outputDifficultyCustomData;
			}
			jSONArray3.Add(jSONObject5);
		}
		jSONObject["difficultyBeatmaps"] = jSONArray3;
		JSONNode jSONNode = info.CustomData.Clone();
		if (info.CustomContributors.Any())
		{
			JSONArray jSONArray6 = new JSONArray();
			foreach (BaseContributor customContributor in info.CustomContributors)
			{
				jSONArray6.Add(V4Contributor.ToJson(customContributor));
			}
			jSONNode["contributors"] = jSONArray6;
		}
		JSONArray jSONArray7 = new JSONArray();
		foreach (InfoDifficultySet difficultySet in info.DifficultySets)
		{
			JSONObject jSONObject7 = new JSONObject { ["characteristic"] = difficultySet.Characteristic };
			if (!string.IsNullOrWhiteSpace(difficultySet.CustomCharacteristicLabel))
			{
				jSONObject7["label"] = difficultySet.CustomCharacteristicLabel;
			}
			if (!string.IsNullOrWhiteSpace(difficultySet.CustomCharacteristicIconImageFileName))
			{
				jSONObject7["iconPath"] = difficultySet.CustomCharacteristicIconImageFileName;
			}
			if (jSONObject7.Count > 1)
			{
				jSONArray7.Add(jSONObject7);
			}
		}
		if (jSONArray7.Count > 0)
		{
			jSONNode["characteristicData"] = jSONArray7;
		}
		if (!string.IsNullOrEmpty(info.CustomEnvironmentMetadata.Name))
		{
			jSONNode["customEnvironment"] = info.CustomEnvironmentMetadata.Name;
			jSONNode["customEnvironmentHash"] = info.CustomEnvironmentMetadata.Hash;
		}
		jSONNode["editors"] = info.CustomEditorsData.ToJson();
		jSONObject["customData"] = jSONNode;
		return jSONObject;
	}

	private static void ParseDifficultySetCustomData(JSONNode customData, InfoDifficultySet difficultySet)
	{
		if (customData["label"].IsString)
		{
			difficultySet.CustomCharacteristicLabel = customData["label"].Value;
		}
		if (customData["iconPath"].IsString)
		{
			difficultySet.CustomCharacteristicIconImageFileName = customData["iconPath"].Value;
		}
	}

	private static void ParseDifficultyCustomData(JSONNode customData, InfoDifficulty difficulty)
	{
		if (customData["oneSaber"].IsBoolean)
		{
			difficulty.CustomOneSaberFlag = customData["oneSaber"].AsBool;
			customData.Remove("oneSaber");
		}
		if (customData["showRotationNoteSpawnLines"].IsBoolean)
		{
			difficulty.CustomShowRotationNoteSpawnLinesFlag = customData["showRotationNoteSpawnLines"].AsBool;
			customData.Remove("showRotationNoteSpawnLines");
		}
		if (customData["difficultyLabel"].IsString)
		{
			difficulty.CustomLabel = customData["difficultyLabel"].Value;
			customData.Remove("difficultyLabel");
		}
		if (customData["information"].IsArray)
		{
			difficulty.CustomInformation = customData["information"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("information");
		}
		if (customData["warnings"].IsArray)
		{
			difficulty.CustomWarnings = customData["warnings"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("warnings");
		}
		if (customData["suggestions"].IsArray)
		{
			difficulty.CustomSuggestions = customData["suggestions"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("suggestions");
		}
		if (customData["requirements"].IsArray)
		{
			difficulty.CustomRequirements = customData["requirements"].AsArray.Children.Select((JSONNode x) => x.Value).ToList();
			customData.Remove("requirements");
		}
		if (customData["colorLeft"].IsObject)
		{
			difficulty.CustomColorLeft = customData["colorLeft"].ReadColor();
			customData.Remove("colorLeft");
		}
		if (customData["colorRight"].IsObject)
		{
			difficulty.CustomColorRight = customData["colorRight"].ReadColor();
			customData.Remove("colorRight");
		}
		if (customData["obstacleColor"].IsObject)
		{
			difficulty.CustomColorObstacle = customData["obstacleColor"].ReadColor();
			customData.Remove("obstacleColor");
		}
		if (customData["envColorLeft"].IsObject)
		{
			difficulty.CustomEnvColorLeft = customData["envColorLeft"].ReadColor();
			customData.Remove("envColorLeft");
		}
		if (customData["envColorRight"].IsObject)
		{
			difficulty.CustomEnvColorRight = customData["envColorRight"].ReadColor();
			customData.Remove("envColorRight");
		}
		if (customData["envColorWhite"].IsObject)
		{
			difficulty.CustomEnvColorWhite = customData["envColorWhite"].ReadColor();
			customData.Remove("envColorWhite");
		}
		if (customData["envColorLeftBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostLeft = customData["envColorLeftBoost"].ReadColor();
			customData.Remove("envColorLeftBoost");
		}
		if (customData["envColorRightBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostRight = customData["envColorRightBoost"].ReadColor();
			customData.Remove("envColorRightBoost");
		}
		if (customData["envColorWhiteBoost"].IsObject)
		{
			difficulty.CustomEnvColorBoostWhite = customData["envColorWhiteBoost"].ReadColor();
			customData.Remove("envColorWhiteBoost");
		}
	}

	private static JSONNode GetOutputDifficultyCustomData(InfoDifficulty difficulty)
	{
		JSONNode jSONNode = difficulty.CustomData.Clone();
		if (difficulty.CustomOneSaberFlag.HasValue)
		{
			jSONNode["oneSaber"] = difficulty.CustomOneSaberFlag.Value;
		}
		if (difficulty.CustomShowRotationNoteSpawnLinesFlag.HasValue)
		{
			jSONNode["showRotationNoteSpawnLines"] = difficulty.CustomShowRotationNoteSpawnLinesFlag.Value;
		}
		if (!string.IsNullOrWhiteSpace(difficulty.CustomLabel))
		{
			jSONNode["difficultyLabel"] = difficulty.CustomLabel;
		}
		if (difficulty.CustomInformation.Any())
		{
			jSONNode["information"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomInformation, (string s) => s);
		}
		if (difficulty.CustomWarnings.Any())
		{
			jSONNode["warnings"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomWarnings, (string s) => s);
		}
		if (difficulty.CustomSuggestions.Any())
		{
			jSONNode["suggestions"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomSuggestions, (string s) => s);
		}
		if (difficulty.CustomRequirements.Any())
		{
			jSONNode["requirements"] = SimpleJSONHelper.MapSequenceToJSONArray(difficulty.CustomRequirements, (string s) => s);
		}
		JSONNode.ColorContainerType = JSONContainerType.Object;
		if (difficulty.CustomColorLeft.HasValue)
		{
			jSONNode["colorLeft"] = difficulty.CustomColorLeft.Value;
		}
		if (difficulty.CustomColorRight.HasValue)
		{
			jSONNode["colorRight"] = difficulty.CustomColorRight.Value;
		}
		if (difficulty.CustomColorObstacle.HasValue)
		{
			jSONNode["obstacleColor"] = difficulty.CustomColorObstacle.Value;
		}
		if (difficulty.CustomEnvColorLeft.HasValue)
		{
			jSONNode["envColorLeft"] = difficulty.CustomEnvColorLeft.Value;
		}
		if (difficulty.CustomEnvColorRight.HasValue)
		{
			jSONNode["envColorRight"] = difficulty.CustomEnvColorRight.Value;
		}
		if (difficulty.CustomEnvColorWhite.HasValue)
		{
			jSONNode["envColorWhite"] = difficulty.CustomEnvColorWhite.Value;
		}
		if (difficulty.CustomEnvColorBoostLeft.HasValue)
		{
			jSONNode["envColorLeftBoost"] = difficulty.CustomEnvColorBoostLeft.Value;
		}
		if (difficulty.CustomEnvColorBoostRight.HasValue)
		{
			jSONNode["envColorRightBoost"] = difficulty.CustomEnvColorBoostRight.Value;
		}
		if (difficulty.CustomEnvColorBoostWhite.HasValue)
		{
			jSONNode["envColorWhiteBoost"] = difficulty.CustomEnvColorBoostWhite.Value;
		}
		JSONNode.ColorContainerType = JSONContainerType.Array;
		return jSONNode;
	}
}
