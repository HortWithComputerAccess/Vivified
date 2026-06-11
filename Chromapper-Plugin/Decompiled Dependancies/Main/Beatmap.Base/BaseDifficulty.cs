using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Beatmap.Base.Customs;
using Beatmap.Converters;
using Beatmap.Info;
using Beatmap.V2;
using Beatmap.V3;
using Beatmap.V4;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseDifficulty : BaseItem
{
	public Dictionary<string, BaseMaterial> Materials = new Dictionary<string, BaseMaterial>();

	public Dictionary<string, JSONArray> PointDefinitions = new Dictionary<string, JSONArray>();

	private float? songBpm;

	public string DirectoryAndFile { get; set; }

	public string Version { get; set; }

	public int MajorVersion
	{
		get
		{
			if (string.IsNullOrEmpty(Version))
			{
				return -1;
			}
			return (int)char.GetNumericValue(Version[0]);
		}
	}

	public List<BaseBpmEvent> BpmEvents { get; set; } = new List<BaseBpmEvent>();

	public List<BaseNote> Notes { get; set; } = new List<BaseNote>();

	public List<BaseObstacle> Obstacles { get; set; } = new List<BaseObstacle>();

	public List<BaseArc> Arcs { get; set; } = new List<BaseArc>();

	public List<BaseChain> Chains { get; set; } = new List<BaseChain>();

	public List<BaseWaypoint> Waypoints { get; set; } = new List<BaseWaypoint>();

	public List<BaseEvent> Events { get; set; } = new List<BaseEvent>();

	public List<BaseNJSEvent> NJSEvents { get; set; } = new List<BaseNJSEvent>();

	public List<BaseLightColorEventBoxGroup<BaseLightColorEventBox>> LightColorEventBoxGroups { get; set; } = new List<BaseLightColorEventBoxGroup<BaseLightColorEventBox>>();

	public List<BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>> LightRotationEventBoxGroups { get; set; } = new List<BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>>();

	public List<BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>> LightTranslationEventBoxGroups { get; set; } = new List<BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>>();

	public List<BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>> VfxEventBoxGroups { get; set; } = new List<BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>>();

	public BaseFxEventsCollection FxEventsCollection { get; set; } = new BaseFxEventsCollection();

	public BaseEventTypesWithKeywords EventTypesWithKeywords { get; set; }

	public bool UseNormalEventsAsCompatibleEvents { get; set; } = true;

	public float Time { get; set; }

	public List<BaseBpmChange> BpmChanges { get; set; } = new List<BaseBpmChange>();

	public List<BaseBookmark> Bookmarks { get; set; } = new List<BaseBookmark>();

	public string BookmarksUseOfficialBpmEventsKey => Settings.Instance.MapVersion switch
	{
		2 => "_bookmarksUseOfficialBpmEvents", 
		3 => "bookmarksUseOfficialBpmEvents", 
		4 => "bookmarksUseOfficialBpmEvents", 
		_ => null, 
	};

	public List<BaseCustomEvent> CustomEvents { get; set; } = new List<BaseCustomEvent>();

	public List<BaseEnvironmentEnhancement> EnvironmentEnhancements { get; set; } = new List<BaseEnvironmentEnhancement>();

	public JSONNode CustomData { get; set; } = new JSONObject();

	private List<List<BaseObject>> AllBaseObjectProperties()
	{
		return new List<List<BaseObject>>
		{
			new List<BaseObject>(Notes),
			new List<BaseObject>(Obstacles),
			new List<BaseObject>(Arcs),
			new List<BaseObject>(Chains),
			new List<BaseObject>(Waypoints),
			new List<BaseObject>(Events),
			new List<BaseObject>(Bookmarks),
			new List<BaseObject>(CustomEvents),
			new List<BaseObject>(NJSEvents)
		};
	}

	public void ValidateBpmEventsAndObjectTimes(float songBpm)
	{
		if (!this.songBpm.HasValue || !Mathf.Approximately(this.songBpm.Value, songBpm))
		{
			BootstrapBpmEvents(songBpm);
			RecomputeAllObjectSongBpmTimes();
		}
	}

	public void BootstrapBpmEvents(float songBpm)
	{
		this.songBpm = songBpm;
		BpmEvents.RemoveAll((BaseBpmEvent x) => x.JsonTime < 0f);
		BpmEvents.RemoveAll((BaseBpmEvent x) => x.Bpm < 0f);
		if (!BpmEvents.Any())
		{
			return;
		}
		BpmEvents.Sort();
		if (BpmEvents.First().JsonTime > 0f)
		{
			BaseBpmEvent item = new BaseBpmEvent(0f, songBpm);
			BpmEvents.Insert(0, item);
		}
		BaseBpmEvent baseBpmEvent = null;
		foreach (BaseBpmEvent bpmEvent in BpmEvents)
		{
			bpmEvent.SetMap(this);
			if (baseBpmEvent == null)
			{
				bpmEvent.songBpmTime = bpmEvent.JsonTime;
			}
			else
			{
				bpmEvent.songBpmTime = baseBpmEvent.songBpmTime + (bpmEvent.JsonTime - baseBpmEvent.JsonTime) * (songBpm / baseBpmEvent.Bpm);
			}
			baseBpmEvent = bpmEvent;
		}
	}

	public float? JsonTimeToSongBpmTime(float jsonTime)
	{
		float? num = songBpm;
		if (!num.HasValue)
		{
			return null;
		}
		BaseBpmEvent baseBpmEvent = FindLastBpmEventByJsonTime(jsonTime);
		if (baseBpmEvent == null)
		{
			return jsonTime;
		}
		return baseBpmEvent.SongBpmTime + (jsonTime - baseBpmEvent.JsonTime) * (songBpm / baseBpmEvent.Bpm);
	}

	public float? SongBpmTimeToJsonTime(float songBpmTime)
	{
		float? num = songBpm;
		if (!num.HasValue)
		{
			return null;
		}
		BaseBpmEvent baseBpmEvent = FindLastBpmEventBySongBpmTime(songBpmTime);
		if (baseBpmEvent == null)
		{
			return songBpmTime;
		}
		return baseBpmEvent.JsonTime + (songBpmTime - baseBpmEvent.SongBpmTime) * (baseBpmEvent.Bpm / songBpm);
	}

	public BaseBpmEvent FindLastBpmEventByJsonTime(float jsonTime, bool inclusive = false)
	{
		return BpmEvents.LastOrDefault((BaseBpmEvent x) => (!inclusive) ? (x.JsonTime < jsonTime) : (x.JsonTime <= jsonTime));
	}

	public BaseBpmEvent FindLastBpmEventBySongBpmTime(float songBpmTime, bool inclusive = false)
	{
		float? num = songBpm;
		if (!num.HasValue)
		{
			return null;
		}
		return BpmEvents.LastOrDefault((BaseBpmEvent x) => (!inclusive) ? (x.SongBpmTime < songBpmTime) : (x.SongBpmTime <= songBpmTime));
	}

	public float? BpmAtJsonTime(float jsonTime)
	{
		BaseBpmEvent baseBpmEvent = FindLastBpmEventByJsonTime(jsonTime, inclusive: true);
		if (baseBpmEvent == null)
		{
			return songBpm;
		}
		return baseBpmEvent.Bpm;
	}

	public float? BpmAtSongBpmTime(float songBpmTime)
	{
		BaseBpmEvent baseBpmEvent = FindLastBpmEventBySongBpmTime(songBpmTime, inclusive: true);
		if (baseBpmEvent == null)
		{
			return songBpm;
		}
		return baseBpmEvent.Bpm;
	}

	public void RecomputeAllObjectSongBpmTimes()
	{
		foreach (List<BaseObject> item in AllBaseObjectProperties())
		{
			if (item == null)
			{
				continue;
			}
			foreach (BaseObject item2 in item)
			{
				item2.SetMap(this);
				item2.RecomputeSongBpmTime();
			}
		}
	}

	public void ConvertCustomBpmToOfficial()
	{
		float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
		JSONObject customData = BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData;
		if ((object)customData != null && customData.HasKey("_editorOffset") && (float)customData["_editorOffset"] > 0f)
		{
			float num = customData["_editorOffset"];
			customData.Remove("_editorOffset");
			customData.Remove("_editorOldOffset");
			BpmChanges.Insert(0, new BaseBpmChange
			{
				JsonTime = beatsPerMinute / 60f * (num / 1000f),
				Bpm = beatsPerMinute
			});
			Debug.Log($"Editor offset detected: {beatsPerMinute / 60f * (num / 1000f)}s");
		}
		if (BpmChanges.Count == 0)
		{
			return;
		}
		PersistentUI.Instance.ShowDialogBox("Mapper", "custom.bpm.convert", null, PersistentUI.DialogBoxPresetType.Ok);
		BpmEvents.Clear();
		for (int i = 0; i < BpmChanges.Count; i++)
		{
			BaseBpmChange baseBpmChange = BpmChanges[i];
			BaseBpmChange baseBpmChange2 = ((i > 0) ? BpmChanges[i - 1] : new BaseBpmChange
			{
				JsonTime = 0f,
				Bpm = beatsPerMinute
			});
			if (Mathf.Abs(baseBpmChange.JsonTime - Mathf.Round(baseBpmChange.JsonTime)) > 0.01f)
			{
				float jsonTime = baseBpmChange.JsonTime;
				float num2 = 1f - baseBpmChange.JsonTime % 1f;
				foreach (List<BaseObject> item in AllBaseObjectProperties())
				{
					if (item == null)
					{
						continue;
					}
					foreach (BaseObject item2 in item)
					{
						if (item2.JsonTime >= jsonTime)
						{
							item2.JsonTime += num2;
						}
						if (item2 is BaseSlider baseSlider && baseSlider.TailJsonTime >= jsonTime)
						{
							baseSlider.TailJsonTime += num2;
						}
					}
				}
				for (int j = i; j < BpmChanges.Count; j++)
				{
					BpmChanges[j].JsonTime += num2;
				}
				float num3 = 100000f;
				float num4 = num2 * baseBpmChange2.Bpm / (num3 - baseBpmChange2.Bpm);
				BpmEvents.Add(new BaseBpmEvent(jsonTime - num4, num3));
			}
			BpmEvents.Add(new BaseBpmEvent(baseBpmChange.JsonTime, baseBpmChange.Bpm));
			BaseBpmEvent baseBpmEvent = ((i + 1 >= BpmChanges.Count) ? null : BpmChanges[i + 1]);
			float num5 = ((baseBpmEvent != null) ? (baseBpmEvent.JsonTime - baseBpmChange.JsonTime) : 0f);
			float num6 = baseBpmChange.Bpm / beatsPerMinute - 1f;
			foreach (List<BaseObject> item3 in AllBaseObjectProperties())
			{
				if (item3 == null)
				{
					continue;
				}
				foreach (BaseObject item4 in item3)
				{
					if (baseBpmChange.JsonTime < item4.JsonTime)
					{
						float jsonTime2 = item4.JsonTime;
						if (baseBpmEvent == null || item4.JsonTime < baseBpmEvent.JsonTime)
						{
							item4.JsonTime += (item4.JsonTime - baseBpmChange.JsonTime) * num6;
						}
						else
						{
							item4.JsonTime += num5 * num6;
						}
						if (item4 is BaseObstacle baseObstacle)
						{
							float num7 = jsonTime2 + baseObstacle.Duration;
							num7 = ((baseBpmEvent != null && !(num7 < baseBpmEvent.JsonTime)) ? (num7 + num5 * num6) : (num7 + (num7 - baseBpmChange.JsonTime) * num6));
							baseObstacle.Duration = num7 - item4.JsonTime;
						}
					}
					if (item4 is BaseSlider baseSlider2 && baseBpmChange.JsonTime < baseSlider2.TailJsonTime)
					{
						if (baseBpmEvent == null || baseSlider2.TailJsonTime < baseBpmEvent.JsonTime)
						{
							baseSlider2.TailJsonTime += (baseSlider2.TailJsonTime - baseBpmChange.JsonTime) * num6;
						}
						else
						{
							baseSlider2.TailJsonTime += num5 * num6;
						}
					}
				}
			}
			for (int k = i + 1; k < BpmChanges.Count; k++)
			{
				BpmChanges[k].JsonTime += num5 * num6;
			}
		}
		CustomData[BookmarksUseOfficialBpmEventsKey] = true;
		BpmChanges.Clear();
	}

	public void ConvertCustomDataVersion(int fromVersion, int toVersion)
	{
		if (fromVersion == 2 && (toVersion == 3 || toVersion == 4))
		{
			foreach (BaseNote note in Notes)
			{
				note.SetCustomData(V2ToV3.CustomDataObject(note.SaveCustom()));
			}
			foreach (BaseObstacle obstacle in Obstacles)
			{
				obstacle.SetCustomData(V2ToV3.CustomDataObject(obstacle.SaveCustom()));
			}
			foreach (BaseArc arc in Arcs)
			{
				arc.SetCustomData(V2ToV3.CustomDataObject(arc.SaveCustom()));
			}
			foreach (BaseChain chain in Chains)
			{
				chain.SetCustomData(V2ToV3.CustomDataObject(chain.SaveCustom()));
			}
			foreach (BaseEvent @event in Events)
			{
				@event.SetCustomData(V2ToV3.CustomDataEvent(@event.SaveCustom()));
			}
			foreach (BaseEnvironmentEnhancement environmentEnhancement in EnvironmentEnhancements)
			{
				environmentEnhancement.Position = V2ToV3.RescaleVector3(environmentEnhancement.Position);
				environmentEnhancement.LocalPosition = V2ToV3.RescaleVector3(environmentEnhancement.LocalPosition);
				environmentEnhancement.Geometry = V2ToV3.Geometry(environmentEnhancement.Geometry?.AsObject);
			}
			foreach (BaseCustomEvent customEvent in CustomEvents)
			{
				customEvent.SetData(V2ToV3.CustomEventData(customEvent.SaveCustom()));
			}
			CustomData = V2ToV3.CustomDataRoot(CustomData, this);
		}
		if ((fromVersion != 3 && fromVersion != 4) || toVersion != 2)
		{
			return;
		}
		foreach (BaseNote note2 in Notes)
		{
			note2.SetCustomData(V3ToV2.CustomDataObject(note2.SaveCustom()));
		}
		foreach (BaseObstacle obstacle2 in Obstacles)
		{
			obstacle2.SetCustomData(V3ToV2.CustomDataObject(obstacle2.SaveCustom()));
		}
		foreach (BaseArc arc2 in Arcs)
		{
			arc2.SetCustomData(V3ToV2.CustomDataObject(arc2.SaveCustom()));
		}
		foreach (BaseChain chain2 in Chains)
		{
			chain2.SetCustomData(V3ToV2.CustomDataObject(chain2.SaveCustom()));
		}
		foreach (BaseEvent event2 in Events)
		{
			event2.SetCustomData(V3ToV2.CustomDataEvent(event2.SaveCustom()));
		}
		foreach (BaseEnvironmentEnhancement environmentEnhancement2 in EnvironmentEnhancements)
		{
			environmentEnhancement2.Position = V3ToV2.RescaleVector3(environmentEnhancement2.Position);
			environmentEnhancement2.LocalPosition = V3ToV2.RescaleVector3(environmentEnhancement2.LocalPosition);
			environmentEnhancement2.Geometry = V3ToV2.Geometry(environmentEnhancement2.Geometry?.AsObject);
		}
		foreach (BaseCustomEvent customEvent2 in CustomEvents)
		{
			customEvent2.SetData(V3ToV2.CustomEventData(customEvent2.SaveCustom()));
		}
		CustomData = V3ToV2.CustomDataRoot(CustomData, this);
	}

	public bool Save()
	{
		int mapVersion = Settings.Instance.MapVersion;
		JSONNode jSONNode = mapVersion switch
		{
			2 => V2Difficulty.GetOutputJson(this), 
			3 => V3Difficulty.GetOutputJson(this), 
			4 => V4Difficulty.GetOutputJson(this), 
			_ => throw new SwitchExpressionException(mapVersion), 
		};
		if (jSONNode == null)
		{
			return false;
		}
		File.WriteAllText(DirectoryAndFile, Settings.Instance.FormatJson ? jSONNode.ToString(2) : jSONNode.ToString());
		BeatSaberSongContainer instance = BeatSaberSongContainer.Instance;
		InfoDifficulty mapDifficultyInfo = instance.MapDifficultyInfo;
		if (Settings.Instance.MapVersion == 4)
		{
			if (string.IsNullOrWhiteSpace(mapDifficultyInfo.LightshowFileName) || mapDifficultyInfo.BeatmapFileName == mapDifficultyInfo.LightshowFileName)
			{
				mapDifficultyInfo.LightshowFileName = "LightsFor-" + mapDifficultyInfo.LightshowFileName;
			}
			JSONNode lightshowOutputJson = V4Difficulty.GetLightshowOutputJson(this);
			File.WriteAllText(Path.Combine(instance.Info.Directory, mapDifficultyInfo.LightshowFileName), Settings.Instance.FormatJson ? lightshowOutputJson.ToString(2) : lightshowOutputJson.ToString());
			JSONNode officialBookmarkOutputJson = GetOfficialBookmarkOutputJson(mapDifficultyInfo.Characteristic, mapDifficultyInfo.Difficulty);
			string text = Path.Combine(instance.Info.Directory, "Bookmarks");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllText(Path.Combine(text, mapDifficultyInfo.BookmarkFileName), officialBookmarkOutputJson.ToString(2));
		}
		BaseBpmInfo baseBpmInfo = new BaseBpmInfo().InitWithSongContainerInstance();
		if (baseBpmInfo.AudioSamples > 0 && !IsEmpty())
		{
			baseBpmInfo.BpmRegions = BaseBpmInfo.GetBpmInfoRegions(BpmEvents, instance.Info.BeatsPerMinute, baseBpmInfo.AudioSamples, baseBpmInfo.AudioFrequency);
			mapVersion = instance.Info.MajorVersion;
			JSONNode jSONNode2 = mapVersion switch
			{
				2 => V2BpmInfo.GetOutputJson(baseBpmInfo), 
				4 => V4AudioData.GetOutputJson(baseBpmInfo), 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
			if (jSONNode2 == null)
			{
				return true;
			}
			string outputFileName = BaseBpmInfo.GetOutputFileName(instance.Info.MajorVersion, instance.Info);
			File.WriteAllText(Path.Combine(instance.Info.Directory, outputFileName), jSONNode2.ToString(2));
		}
		return true;
	}

	private JSONNode GetOfficialBookmarkOutputJson(string characteristic, string difficulty)
	{
		JSONObject jSONObject = new JSONObject();
		jSONObject["name"] = "ChroMapper";
		jSONObject["characteristic"] = characteristic;
		jSONObject["difficulty"] = difficulty;
		jSONObject["color"] = "00FFFF";
		JSONArray jSONArray = new JSONArray();
		foreach (BaseBookmark bookmark in Bookmarks)
		{
			JSONObject jSONObject2 = new JSONObject();
			jSONObject2["beat"] = bookmark.JsonTime;
			jSONObject2["label"] = bookmark.Name;
			jSONObject2["text"] = bookmark.Name;
			jSONArray.Add(jSONObject2);
		}
		jSONObject["bookmarks"] = jSONArray;
		return jSONObject;
	}

	private bool IsEmpty()
	{
		if (BpmEvents.Count == 0 && Notes.Count == 0 && Obstacles.Count == 0 && Arcs.Count == 0 && Chains.Count == 0 && Waypoints.Count == 0 && Events.Count == 0 && NJSEvents.Count == 0 && LightColorEventBoxGroups.Count == 0 && LightRotationEventBoxGroups.Count == 0 && LightTranslationEventBoxGroups.Count == 0 && VfxEventBoxGroups.Count == 0)
		{
			BaseFxEventsCollection fxEventsCollection = FxEventsCollection;
			if (fxEventsCollection == null || fxEventsCollection.FloatFxEvents.Length == 0)
			{
				BaseEventTypesWithKeywords eventTypesWithKeywords = EventTypesWithKeywords;
				if ((eventTypesWithKeywords == null || eventTypesWithKeywords.Keywords.Length == 0) && Bookmarks.Count == 0 && CustomEvents.Count == 0 && Materials.Count == 0 && PointDefinitions.Count == 0 && EnvironmentEnhancements.Count == 0)
				{
					return CustomData.Count == 0;
				}
			}
		}
		return false;
	}

	public bool IsChroma()
	{
		if (!Notes.Any((BaseNote x) => x.IsChroma()) && !Arcs.Any((BaseArc x) => x.IsChroma()) && !Chains.Any((BaseChain x) => x.IsChroma()) && !Obstacles.Any((BaseObstacle x) => x.IsChroma()) && !Events.Any((BaseEvent x) => x.IsChroma()))
		{
			return EnvironmentEnhancements.Any();
		}
		return true;
	}

	public bool IsNoodleExtensions()
	{
		if (!Notes.Any((BaseNote x) => x.IsNoodleExtensions()) && !Arcs.Any((BaseArc x) => x.IsNoodleExtensions()) && !Chains.Any((BaseChain x) => x.IsNoodleExtensions()))
		{
			return Obstacles.Any((BaseObstacle x) => x.IsNoodleExtensions());
		}
		return true;
	}

	public bool IsMappingExtensions()
	{
		if (!Notes.Any((BaseNote x) => x.IsMappingExtensions()) && !Arcs.Any((BaseArc x) => x.IsMappingExtensions()) && !Chains.Any((BaseChain x) => x.IsMappingExtensions()))
		{
			return Obstacles.Any((BaseObstacle x) => x.IsMappingExtensions());
		}
		return true;
	}

	public override JSONNode ToJson()
	{
		throw new NotImplementedException();
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
