using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.V2.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V2;

public class V2Difficulty
{
	private const string version = "2.6.0";

	public static JSONNode GetOutputJson(BaseDifficulty difficulty)
	{
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			JSONObject jSONObject = new JSONObject { ["_version"] = "2.6.0" };
			JSONArray jSONArray = new JSONArray();
			List<BaseObject> list = new List<BaseObject>();
			list.AddRange(difficulty.Events);
			list.AddRange(difficulty.BpmEvents);
			if (difficulty.BpmEvents.Count > 0 && difficulty.BpmEvents.First().JsonTime != 0f)
			{
				float bpm = ((BeatSaberSongContainer.Instance != null) ? BeatSaberSongContainer.Instance.Info.BeatsPerMinute : 100f);
				list.Add(new BaseBpmEvent
				{
					JsonTime = 0f,
					Bpm = bpm
				});
			}
			list.Sort((BaseObject lhs, BaseObject rhs) => lhs.JsonTime.CompareTo(rhs.JsonTime));
			foreach (BaseObject item in list)
			{
				jSONArray.Add(item.ToJson());
			}
			jSONObject["_events"] = jSONArray;
			JSONArray jSONArray2 = new JSONArray();
			foreach (BaseNote note in difficulty.Notes)
			{
				jSONArray2.Add(note.ToJson());
			}
			jSONObject["_notes"] = jSONArray2;
			JSONArray jSONArray3 = new JSONArray();
			foreach (BaseObstacle obstacle in difficulty.Obstacles)
			{
				jSONArray3.Add(obstacle.ToJson());
			}
			jSONObject["_obstacles"] = jSONArray3;
			JSONArray jSONArray4 = new JSONArray();
			foreach (BaseWaypoint waypoint in difficulty.Waypoints)
			{
				jSONArray4.Add(waypoint.ToJson());
			}
			jSONObject["_waypoints"] = jSONArray4;
			jSONObject["_sliders"] = new JSONArray();
			BaseEventTypesWithKeywords eventTypesWithKeywords = difficulty.EventTypesWithKeywords;
			jSONObject["_specialEventsKeywordFilters"] = ((eventTypesWithKeywords != null && eventTypesWithKeywords.Keywords.Length != 0) ? difficulty.EventTypesWithKeywords.ToJson() : new JSONObject());
			JSONNode outputCustomJsonData = GetOutputCustomJsonData(difficulty);
			if (outputCustomJsonData.Children.Any())
			{
				jSONObject["_customData"] = outputCustomJsonData;
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
			jSONNode["_bookmarks"] = jSONArray;
			jSONNode[difficulty.BookmarksUseOfficialBpmEventsKey] = true;
		}
		if (difficulty.CustomEvents.Any())
		{
			JSONArray jSONArray2 = new JSONArray();
			foreach (BaseCustomEvent customEvent in difficulty.CustomEvents)
			{
				jSONArray2.Add(customEvent.ToJson());
			}
			jSONNode["_customEvents"] = jSONArray2;
		}
		if (difficulty.EnvironmentEnhancements.Any())
		{
			JSONArray jSONArray3 = new JSONArray();
			foreach (BaseEnvironmentEnhancement environmentEnhancement in difficulty.EnvironmentEnhancements)
			{
				jSONArray3.Add(environmentEnhancement.ToJson());
			}
			jSONNode["_environment"] = jSONArray3;
		}
		if (difficulty.PointDefinitions.Any())
		{
			JSONArray jSONArray4 = new JSONArray();
			foreach (KeyValuePair<string, JSONArray> pointDefinition in difficulty.PointDefinitions)
			{
				JSONObject aItem = new JSONObject
				{
					["_name"] = pointDefinition.Key,
					["_points"] = pointDefinition.Value
				};
				jSONArray4.Add(aItem);
			}
			jSONNode["_pointDefinitions"] = jSONArray4;
		}
		if (difficulty.Materials.Any())
		{
			jSONNode["_materials"] = new JSONObject();
			foreach (KeyValuePair<string, BaseMaterial> material in difficulty.Materials)
			{
				jSONNode["_materials"][material.Key] = material.Value.ToJson();
			}
		}
		if (difficulty.Time > 0f)
		{
			jSONNode["_time"] = Math.Round(difficulty.Time, 3);
		}
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
				Version = "2.6.0"
			};
			JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				JSONNode value = enumerator.Current.Value;
				switch (key)
				{
				case "_events":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode jSONNode = enumerator2.Current;
						if (jSONNode["_type"] != null && jSONNode["_type"] == 100)
						{
							baseDifficulty.BpmEvents.Add(V2BpmEvent.GetFromJson(jSONNode));
						}
						else
						{
							baseDifficulty.Events.Add(V2Event.GetFromJson(jSONNode));
						}
					}
					break;
				}
				case "_notes":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node2 = enumerator2.Current;
						baseDifficulty.Notes.Add(V2Note.GetFromJson(node2));
					}
					break;
				}
				case "_obstacles":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node3 = enumerator2.Current;
						baseDifficulty.Obstacles.Add(V2Obstacle.GetFromJson(node3));
					}
					break;
				}
				case "_waypoints":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node = enumerator2.Current;
						baseDifficulty.Waypoints.Add(V2Waypoint.GetFromJson(node));
					}
					break;
				}
				case "_specialEventsKeywordFilter":
					baseDifficulty.EventTypesWithKeywords = V2SpecialEventsKeywordFilters.GetFromJson(value);
					break;
				}
			}
			baseDifficulty.BpmEvents.Sort();
			baseDifficulty.Events.Sort();
			baseDifficulty.Notes.Sort();
			baseDifficulty.Obstacles.Sort();
			baseDifficulty.Waypoints.Sort();
			LoadCustom(baseDifficulty, mainNode);
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
		List<BaseBpmChange> list = new List<BaseBpmChange>();
		List<BaseBookmark> list2 = new List<BaseBookmark>();
		List<BaseCustomEvent> list3 = new List<BaseCustomEvent>();
		Dictionary<string, JSONArray> dictionary = new Dictionary<string, JSONArray>();
		List<BaseEnvironmentEnhancement> list4 = new List<BaseEnvironmentEnhancement>();
		Dictionary<string, BaseMaterial> dictionary2 = new Dictionary<string, BaseMaterial>();
		JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			JSONNode value = enumerator.Current.Value;
			switch (key)
			{
			case "_customData":
			{
				map.CustomData = value;
				JSONNode.Enumerator enumerator3 = value.Clone().GetEnumerator();
				while (enumerator3.MoveNext())
				{
					string key2 = enumerator3.Current.Key;
					JSONNode value2 = enumerator3.Current.Value;
					switch (key2)
					{
					case "_BPMChanges":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode node2 = enumerator2.Current;
							list.Add(V2BpmChange.GetFromJson(node2));
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_bpmChanges":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode node6 = enumerator2.Current;
							list.Add(V2BpmChange.GetFromJson(node6));
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_bookmarks":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode node3 = enumerator2.Current;
							list2.Add(V2Bookmark.GetFromJson(node3));
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_customEvents":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode node5 = enumerator2.Current;
							list3.Add(V2CustomEvent.GetFromJson(node5));
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_pointDefinitions":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode jSONNode = enumerator2.Current;
							JSONArray asArray = jSONNode["_points"].AsArray;
							if (!dictionary.ContainsKey(jSONNode["_name"]))
							{
								dictionary.Add(jSONNode["_name"], asArray);
							}
							else
							{
								Debug.LogWarning(string.Format("Duplicate key {0} found in point definitions", jSONNode["_name"]));
							}
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_environment":
					{
						JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							JSONNode node4 = enumerator2.Current;
							list4.Add(V2EnvironmentEnhancement.GetFromJson(node4));
						}
						map.CustomData.Remove(key2);
						break;
					}
					case "_materials":
						if (value2 is JSONObject jSONObject)
						{
							JSONNode.Enumerator enumerator2 = jSONObject.GetEnumerator();
							while (enumerator2.MoveNext())
							{
								KeyValuePair<string, JSONNode> current = enumerator2.Current;
								if (!dictionary2.ContainsKey(current.Key))
								{
									dictionary2.Add(current.Key, V2Material.GetFromJson(current.Value.AsObject));
								}
								else
								{
									Debug.LogWarning("Duplicate key \"" + current.Key + "\" found in materials");
								}
							}
						}
						else
						{
							Debug.LogWarning("Could not read materials");
							map.CustomData.Remove(key2);
						}
						break;
					case "_time":
						map.Time = value2.AsFloat;
						map.CustomData.Remove(key2);
						break;
					}
				}
				break;
			}
			case "_BPMChanges":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node8 = enumerator2.Current;
					list.Add(V2BpmChange.GetFromJson(node8));
				}
				break;
			}
			case "_bpmChanges":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node9 = enumerator2.Current;
					list.Add(V2BpmChange.GetFromJson(node9));
				}
				break;
			}
			case "_bookmarks":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node7 = enumerator2.Current;
					list2.Add(V2Bookmark.GetFromJson(node7));
				}
				break;
			}
			case "_customEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node = enumerator2.Current;
					list3.Add(V2CustomEvent.GetFromJson(node));
				}
				break;
			}
			}
		}
		if (mainNode.HasKey("_BPMChanges"))
		{
			mainNode.Remove("_BPMChanges");
		}
		if (mainNode.HasKey("_bpmChanges"))
		{
			mainNode.Remove("_bpmChanges");
		}
		if (mainNode.HasKey("_bookmarks"))
		{
			mainNode.Remove("_bookmarks");
		}
		if (mainNode.HasKey("_customEvents"))
		{
			mainNode.Remove("_customEvents");
		}
		map.BpmChanges = list.DistinctBy((BaseBpmChange x) => x.JsonTime).ToList();
		map.Bookmarks = list2;
		map.CustomEvents = list3.DistinctBy((BaseCustomEvent x) => x.ToString()).ToList();
		map.PointDefinitions = dictionary;
		map.EnvironmentEnhancements = list4;
		map.Materials = dictionary2;
	}
}
