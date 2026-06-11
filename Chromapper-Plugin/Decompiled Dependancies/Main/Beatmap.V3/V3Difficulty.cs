using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.V3.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V3;

public class V3Difficulty
{
	public const string Version = "3.3.0";

	public static JSONNode GetOutputJson(BaseDifficulty difficulty)
	{
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			JSONObject jSONObject = new JSONObject { ["version"] = "3.3.0" };
			JSONArray jSONArray = new JSONArray();
			if (difficulty.BpmEvents.Count > 0 && difficulty.BpmEvents.First().JsonTime != 0f)
			{
				float bpm = ((BeatSaberSongContainer.Instance != null) ? BeatSaberSongContainer.Instance.Info.BeatsPerMinute : 100f);
				difficulty.BpmEvents.Insert(0, new BaseBpmEvent
				{
					JsonTime = 0f,
					Bpm = bpm
				});
			}
			foreach (BaseBpmEvent bpmEvent in difficulty.BpmEvents)
			{
				jSONArray.Add(bpmEvent.ToJson());
			}
			jSONObject["bpmEvents"] = jSONArray;
			JSONArray jSONArray2 = new JSONArray();
			JSONArray jSONArray3 = new JSONArray();
			JSONArray jSONArray4 = new JSONArray();
			foreach (BaseEvent @event in difficulty.Events)
			{
				switch (@event.Type)
				{
				case 5:
					jSONArray3.Add(@event.ToJson());
					break;
				case 14:
				case 15:
					jSONArray4.Add(@event.ToJson());
					break;
				default:
					jSONArray2.Add(@event.ToJson());
					break;
				}
			}
			jSONObject["rotationEvents"] = jSONArray4;
			jSONObject["basicBeatmapEvents"] = jSONArray2;
			jSONObject["colorBoostBeatmapEvents"] = jSONArray3;
			JSONArray jSONArray5 = new JSONArray();
			JSONArray jSONArray6 = new JSONArray();
			foreach (BaseNote note in difficulty.Notes)
			{
				switch (note.Type)
				{
				case 0:
				case 1:
					if (!note.CustomFake)
					{
						jSONArray5.Add(note.ToJson());
					}
					break;
				case 3:
					if (!note.CustomFake)
					{
						jSONArray6.Add(note.ToJson());
					}
					break;
				}
			}
			jSONObject["colorNotes"] = jSONArray5;
			jSONObject["bombNotes"] = jSONArray6;
			JSONArray jSONArray7 = new JSONArray();
			foreach (BaseObstacle item in difficulty.Obstacles.Where((BaseObstacle o) => !o.CustomFake))
			{
				jSONArray7.Add(item.ToJson());
			}
			jSONObject["obstacles"] = jSONArray7;
			JSONArray jSONArray8 = new JSONArray();
			foreach (BaseArc arc in difficulty.Arcs)
			{
				jSONArray8.Add(arc.ToJson());
			}
			jSONObject["sliders"] = jSONArray8;
			JSONArray jSONArray9 = new JSONArray();
			foreach (BaseChain item2 in difficulty.Chains.Where((BaseChain c) => !c.CustomFake))
			{
				jSONArray9.Add(item2.ToJson());
			}
			jSONObject["burstSliders"] = jSONArray9;
			JSONArray jSONArray10 = new JSONArray();
			foreach (BaseWaypoint waypoint in difficulty.Waypoints)
			{
				jSONArray10.Add(waypoint.ToJson());
			}
			jSONObject["waypoints"] = jSONArray10;
			JSONArray jSONArray11 = new JSONArray();
			foreach (BaseLightColorEventBoxGroup<BaseLightColorEventBox> lightColorEventBoxGroup in difficulty.LightColorEventBoxGroups)
			{
				jSONArray11.Add(lightColorEventBoxGroup.ToJson());
			}
			jSONObject["lightColorEventBoxGroups"] = jSONArray11;
			JSONArray jSONArray12 = new JSONArray();
			foreach (BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> lightRotationEventBoxGroup in difficulty.LightRotationEventBoxGroups)
			{
				jSONArray12.Add(lightRotationEventBoxGroup.ToJson());
			}
			jSONObject["lightRotationEventBoxGroups"] = jSONArray12;
			JSONArray jSONArray13 = new JSONArray();
			foreach (BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> lightTranslationEventBoxGroup in difficulty.LightTranslationEventBoxGroups)
			{
				jSONArray13.Add(lightTranslationEventBoxGroup.ToJson());
			}
			jSONObject["lightTranslationEventBoxGroups"] = jSONArray13;
			List<FloatFxEventBase> list = difficulty.VfxEventBoxGroups.SelectMany((BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> group) => group.Events).SelectMany((BaseVfxEventEventBox box) => box.FloatFxEvents).Distinct()
				.ToList();
			JSONArray jSONArray14 = new JSONArray();
			foreach (BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> vfxEventBoxGroup in difficulty.VfxEventBoxGroups)
			{
				jSONArray14.Add(V3VfxEventEventBoxGroup.ToJson(vfxEventBoxGroup, list));
			}
			jSONObject["vfxEventBoxGroups"] = jSONArray14;
			difficulty.FxEventsCollection.FloatFxEvents = list.ToArray();
			jSONObject["_fxEventsCollection"] = difficulty.FxEventsCollection?.ToJson() ?? new BaseFxEventsCollection().ToJson();
			jSONObject["basicEventTypesWithKeywords"] = difficulty.EventTypesWithKeywords?.ToJson() ?? new BaseEventTypesWithKeywords().ToJson();
			jSONObject["useNormalEventsAsCompatibleEvents"] = difficulty.UseNormalEventsAsCompatibleEvents;
			if (Settings.Instance.SaveWithoutDefaultValues)
			{
				SimpleJSONHelper.RemovePropertiesWithDefaultValues(jSONObject);
			}
			JSONNode outputCustomJsonData = GetOutputCustomJsonData(difficulty);
			if (outputCustomJsonData.Children.Any())
			{
				jSONObject["customData"] = outputCustomJsonData;
			}
			return jSONObject;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("This is bad. You are recommended to restart ChroMapper; progress made after this point is not guaranteed to be saved.");
			return null;
		}
	}

	private static JSONNode GetOutputCustomJsonData(BaseDifficulty difficulty)
	{
		JSONNode jSONNode = difficulty.CustomData?.Clone() ?? new JSONObject();
		if (difficulty.Bookmarks.Any())
		{
			JSONArray jSONArray = new JSONArray();
			foreach (BaseBookmark bookmark in difficulty.Bookmarks)
			{
				jSONArray.Add(bookmark.ToJson());
			}
			jSONNode["bookmarks"] = jSONArray;
			jSONNode[difficulty.BookmarksUseOfficialBpmEventsKey] = true;
		}
		if (difficulty.CustomEvents.Any())
		{
			JSONArray jSONArray2 = new JSONArray();
			foreach (BaseCustomEvent customEvent in difficulty.CustomEvents)
			{
				jSONArray2.Add(customEvent.ToJson());
			}
			jSONNode["customEvents"] = jSONArray2;
		}
		if (difficulty.EnvironmentEnhancements.Any())
		{
			JSONArray jSONArray3 = new JSONArray();
			foreach (BaseEnvironmentEnhancement environmentEnhancement in difficulty.EnvironmentEnhancements)
			{
				jSONArray3.Add(environmentEnhancement.ToJson());
			}
			jSONNode["environment"] = jSONArray3;
		}
		if (difficulty.PointDefinitions.Any())
		{
			jSONNode["pointDefinitions"] = new JSONObject();
			foreach (KeyValuePair<string, JSONArray> pointDefinition in difficulty.PointDefinitions)
			{
				jSONNode["pointDefinitions"][pointDefinition.Key] = pointDefinition.Value;
			}
		}
		if (difficulty.Materials.Any())
		{
			jSONNode["materials"] = new JSONObject();
			foreach (KeyValuePair<string, BaseMaterial> material in difficulty.Materials)
			{
				jSONNode["materials"][material.Key] = material.Value.ToJson();
			}
		}
		if (difficulty.Time > 0f)
		{
			jSONNode["time"] = Math.Round(difficulty.Time, 3);
		}
		JSONArray jSONArray4 = new JSONArray();
		JSONArray jSONArray5 = new JSONArray();
		foreach (BaseNote note in difficulty.Notes)
		{
			switch (note.Type)
			{
			case 0:
			case 1:
				if (note.CustomFake)
				{
					jSONArray4.Add(note.ToJson());
				}
				break;
			case 3:
				if (note.CustomFake)
				{
					jSONArray5.Add(note.ToJson());
				}
				break;
			}
		}
		jSONNode["fakeColorNotes"] = jSONArray4;
		jSONNode["fakeBombNotes"] = jSONArray5;
		JSONArray jSONArray6 = new JSONArray();
		foreach (BaseObstacle item in difficulty.Obstacles.Where((BaseObstacle o) => o.CustomFake))
		{
			jSONArray6.Add(item.ToJson());
		}
		jSONNode["fakeObstacles"] = jSONArray6;
		JSONArray jSONArray7 = new JSONArray();
		foreach (BaseChain item2 in difficulty.Chains.Where((BaseChain c) => c.CustomFake))
		{
			jSONArray7.Add(item2.ToJson());
		}
		jSONNode["fakeBurstSliders"] = jSONArray7;
		SimpleJSONHelper.CleanObject(jSONNode);
		return jSONNode;
	}

	public static BaseDifficulty GetFromJson(JSONNode mainNode, string path)
	{
		try
		{
			BaseDifficulty baseDifficulty = new BaseDifficulty
			{
				DirectoryAndFile = path,
				Version = "3.3.0"
			};
			JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				JSONNode value = enumerator.Current.Value;
				if (key == "_fxEventsCollection")
				{
					baseDifficulty.FxEventsCollection = V3FxEventsCollection.GetFromJson(value);
					break;
				}
			}
			enumerator = mainNode.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string key2 = enumerator.Current.Key;
				JSONNode value2 = enumerator.Current.Value;
				switch (key2)
				{
				case "colorNotes":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node12 = enumerator2.Current;
						baseDifficulty.Notes.Add(V3ColorNote.GetFromJson(node12, false));
					}
					break;
				}
				case "bombNotes":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node4 = enumerator2.Current;
						baseDifficulty.Notes.Add(V3BombNote.GetFromJson(node4));
					}
					break;
				}
				case "basicBeatmapEvents":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node8 = enumerator2.Current;
						baseDifficulty.Events.Add(V3BasicEvent.GetFromJson(node8));
					}
					break;
				}
				case "colorBoostBeatmapEvents":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node14 = enumerator2.Current;
						baseDifficulty.Events.Add(V3ColorBoostEvent.GetFromJson(node14));
					}
					break;
				}
				case "rotationEvents":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node10 = enumerator2.Current;
						baseDifficulty.Events.Add(V3RotationEvent.GetFromJson(node10));
					}
					break;
				}
				case "bpmEvents":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node6 = enumerator2.Current;
						baseDifficulty.BpmEvents.Add(V3BpmEvent.GetFromJson(node6));
					}
					break;
				}
				case "obstacles":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node2 = enumerator2.Current;
						baseDifficulty.Obstacles.Add(V3Obstacle.GetFromJson(node2));
					}
					break;
				}
				case "sliders":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node13 = enumerator2.Current;
						baseDifficulty.Arcs.Add(V3Arc.GetFromJson(node13));
					}
					break;
				}
				case "burstSliders":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node11 = enumerator2.Current;
						baseDifficulty.Chains.Add(V3Chain.GetFromJson(node11));
					}
					break;
				}
				case "waypoints":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node9 = enumerator2.Current;
						baseDifficulty.Waypoints.Add(V3Waypoint.GetFromJson(node9));
					}
					break;
				}
				case "lightColorEventBoxGroups":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node7 = enumerator2.Current;
						baseDifficulty.LightColorEventBoxGroups.Add(V3LightColorEventBoxGroup.GetFromJson(node7));
					}
					break;
				}
				case "lightRotationEventBoxGroups":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node5 = enumerator2.Current;
						baseDifficulty.LightRotationEventBoxGroups.Add(V3LightRotationEventBoxGroup.GetFromJson(node5));
					}
					break;
				}
				case "lightTranslationEventBoxGroups":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node3 = enumerator2.Current;
						baseDifficulty.LightTranslationEventBoxGroups.Add(V3LightTranslationEventBoxGroup.GetFromJson(node3));
					}
					break;
				}
				case "vfxEventBoxGroups":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node = enumerator2.Current;
						baseDifficulty.VfxEventBoxGroups.Add(V3VfxEventEventBoxGroup.GetFromJson(node, baseDifficulty.FxEventsCollection.FloatFxEvents));
					}
					break;
				}
				case "basicEventTypesWithKeywords":
					baseDifficulty.EventTypesWithKeywords = V3BasicEventTypesWithKeywords.GetFromJson(value2);
					break;
				case "useNormalEventsAsCompatibleEvents":
					baseDifficulty.UseNormalEventsAsCompatibleEvents = value2.AsBool;
					break;
				}
			}
			LoadCustom(baseDifficulty, mainNode);
			baseDifficulty.Notes.Sort();
			baseDifficulty.Events.Sort();
			baseDifficulty.BpmEvents.Sort();
			baseDifficulty.Obstacles.Sort();
			baseDifficulty.Waypoints.Sort();
			baseDifficulty.Chains.Sort();
			baseDifficulty.Arcs.Sort();
			return baseDifficulty;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	private static void LoadCustom(BaseDifficulty map, JSONNode mainNode)
	{
		if (mainNode["customData"] == null)
		{
			return;
		}
		map.CustomData = mainNode["customData"];
		List<BaseBpmChange> list = new List<BaseBpmChange>();
		List<BaseBookmark> list2 = new List<BaseBookmark>();
		List<BaseCustomEvent> list3 = new List<BaseCustomEvent>();
		Dictionary<string, JSONArray> dictionary = new Dictionary<string, JSONArray>();
		List<BaseEnvironmentEnhancement> list4 = new List<BaseEnvironmentEnhancement>();
		Dictionary<string, BaseMaterial> dictionary2 = new Dictionary<string, BaseMaterial>();
		JSONNode.Enumerator enumerator = mainNode["customData"].Clone().GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			JSONNode value = enumerator.Current.Value;
			switch (key)
			{
			case "BPMChanges":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node3 = enumerator2.Current;
					list.Add(V3BpmChange.GetFromJson(node3));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "bookmarks":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node2 = enumerator2.Current;
					list2.Add(V3Bookmark.GetFromJson(node2));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "customEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node7 = enumerator2.Current;
					list3.Add(V3CustomEvent.GetFromJson(node7));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "fakeColorNotes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node = enumerator2.Current;
					map.Notes.Add(V3ColorNote.GetFromJson(node, true));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "fakeBombNotes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node8 = enumerator2.Current;
					map.Notes.Add(V3BombNote.GetFromJson(node8, customFake: true));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "fakeObstacles":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node5 = enumerator2.Current;
					map.Obstacles.Add(V3Obstacle.GetFromJson(node5, customFake: true));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "fakeBurstSliders":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node6 = enumerator2.Current;
					map.Chains.Add(V3Chain.GetFromJson(node6, customFake: true));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "pointDefinitions":
				if (value is JSONArray jSONArray)
				{
					JSONNode.Enumerator enumerator2 = jSONArray.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, JSONNode> current2 = enumerator2.Current;
						if (current2.Value is JSONObject jSONObject2)
						{
							if (!dictionary.ContainsKey(current2.Key))
							{
								dictionary.Add(jSONObject2["name"], jSONObject2["points"].AsArray);
							}
							else
							{
								Debug.LogWarning("Duplicate key " + current2.Key + " found in point definitions");
							}
						}
					}
				}
				else if (value is JSONObject jSONObject3)
				{
					JSONNode.Enumerator enumerator2 = jSONObject3.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, JSONNode> current3 = enumerator2.Current;
						if (!dictionary.ContainsKey(current3.Key))
						{
							dictionary.Add(current3.Key, current3.Value.AsArray);
						}
						else
						{
							Debug.LogWarning("Duplicate key " + current3.Key + " found in point definitions");
						}
					}
				}
				else
				{
					Debug.LogWarning("Could not read point definitions");
					map.CustomData.Remove(key);
				}
				break;
			case "environment":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node4 = enumerator2.Current;
					list4.Add(V3EnvironmentEnhancement.GetFromJson(node4));
				}
				map.CustomData.Remove(key);
				break;
			}
			case "materials":
				if (value is JSONObject jSONObject)
				{
					JSONNode.Enumerator enumerator2 = jSONObject.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, JSONNode> current = enumerator2.Current;
						if (!dictionary2.ContainsKey(current.Key))
						{
							dictionary2.Add(current.Key, V3Material.GetFromJson(current.Value.AsObject));
						}
						else
						{
							Debug.LogWarning("Duplicate key " + current.Key + " found in materials");
						}
					}
				}
				else
				{
					Debug.LogWarning("Could not read materials");
					map.CustomData.Remove(key);
				}
				break;
			case "time":
				map.Time = value.AsFloat;
				map.CustomData.Remove(key);
				break;
			}
		}
		map.BpmChanges = list.DistinctBy((BaseBpmChange x) => x.JsonTime).ToList();
		map.Bookmarks = list2;
		map.CustomEvents = list3.DistinctBy((BaseCustomEvent x) => x.ToString()).ToList();
		map.PointDefinitions = dictionary;
		map.EnvironmentEnhancements = list4;
		map.Materials = dictionary2;
	}
}
