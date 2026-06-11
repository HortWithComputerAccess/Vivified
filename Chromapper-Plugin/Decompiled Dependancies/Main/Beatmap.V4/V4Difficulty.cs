using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Info;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V4;

public class V4Difficulty
{
	public const string BeatmapVersion = "4.1.0";

	private const string lightshowVersion = "4.0.0";

	public static JSONNode GetOutputJson(BaseDifficulty difficulty)
	{
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			JSONObject jSONObject = new JSONObject { ["version"] = "4.1.0" };
			JSONArray jSONArray = new JSONArray();
			JSONArray jSONArray2 = new JSONArray();
			List<BaseNote> list = difficulty.Notes.Where((BaseNote x) => x.Type != 3).ToList();
			List<V4CommonData.Note> list2 = new List<V4CommonData.Note>();
			list2.AddRange(list.Select(V4CommonData.Note.FromBaseNote));
			list2.AddRange(difficulty.Arcs.Select(V4CommonData.Note.FromBaseSliderHead));
			list2.AddRange(difficulty.Arcs.Select(V4CommonData.Note.FromBaseArcTail));
			list2.AddRange(difficulty.Chains.Select(V4CommonData.Note.FromBaseSliderHead));
			list2 = list2.Distinct().ToList();
			foreach (BaseNote item in list)
			{
				jSONArray.Add(V4ColorNote.ToJson(item, list2));
			}
			foreach (V4CommonData.Note item2 in list2)
			{
				jSONArray2.Add(item2.ToJson());
			}
			jSONObject["colorNotes"] = jSONArray;
			jSONObject["colorNotesData"] = jSONArray2;
			JSONArray jSONArray3 = new JSONArray();
			JSONArray jSONArray4 = new JSONArray();
			List<BaseNote> list3 = difficulty.Notes.Where((BaseNote x) => x.Type == 3).ToList();
			List<V4CommonData.Bomb> list4 = list3.Select(V4CommonData.Bomb.FromBaseNote).Distinct().ToList();
			foreach (BaseNote item3 in list3)
			{
				jSONArray3.Add(V4BombNote.ToJson(item3, list4));
			}
			foreach (V4CommonData.Bomb item4 in list4)
			{
				jSONArray4.Add(item4.ToJson());
			}
			jSONObject["bombNotes"] = jSONArray3;
			jSONObject["bombNotesData"] = jSONArray4;
			JSONArray jSONArray5 = new JSONArray();
			JSONArray jSONArray6 = new JSONArray();
			List<V4CommonData.Arc> list5 = difficulty.Arcs.Select(V4CommonData.Arc.FromBaseArc).Distinct().ToList();
			foreach (BaseArc arc in difficulty.Arcs)
			{
				jSONArray5.Add(V4Arc.ToJson(arc, list2, list5));
			}
			foreach (V4CommonData.Arc item5 in list5)
			{
				jSONArray6.Add(item5.ToJson());
			}
			jSONObject["arcs"] = jSONArray5;
			jSONObject["arcsData"] = jSONArray6;
			JSONArray jSONArray7 = new JSONArray();
			JSONArray jSONArray8 = new JSONArray();
			List<V4CommonData.Chain> list6 = difficulty.Chains.Select(V4CommonData.Chain.FromBaseChain).Distinct().ToList();
			foreach (BaseChain chain in difficulty.Chains)
			{
				jSONArray7.Add(V4Chain.ToJson(chain, list2, list6));
			}
			foreach (V4CommonData.Chain item6 in list6)
			{
				jSONArray8.Add(item6.ToJson());
			}
			jSONObject["chains"] = jSONArray7;
			jSONObject["chainsData"] = jSONArray8;
			JSONArray jSONArray9 = new JSONArray();
			JSONArray jSONArray10 = new JSONArray();
			List<V4CommonData.Obstacle> list7 = difficulty.Obstacles.Select(V4CommonData.Obstacle.FromBaseObstacle).Distinct().ToList();
			foreach (BaseObstacle obstacle in difficulty.Obstacles)
			{
				jSONArray9.Add(V4Obstacle.ToJson(obstacle, list7));
			}
			foreach (V4CommonData.Obstacle item7 in list7)
			{
				jSONArray10.Add(item7.ToJson());
			}
			jSONObject["obstacles"] = jSONArray9;
			jSONObject["obstaclesData"] = jSONArray10;
			JSONArray jSONArray11 = new JSONArray();
			JSONArray jSONArray12 = new JSONArray();
			List<V4CommonData.NJSEvent> list8 = difficulty.NJSEvents.Select(V4CommonData.NJSEvent.FromBaseNJSEvent).Distinct().ToList();
			foreach (BaseNJSEvent nJSEvent in difficulty.NJSEvents)
			{
				jSONArray11.Add(V4NJSEvent.ToJson(nJSEvent, list8));
			}
			foreach (V4CommonData.NJSEvent item8 in list8)
			{
				jSONArray12.Add(item8.ToJson());
			}
			jSONObject["njsEvents"] = jSONArray11;
			jSONObject["njsEventData"] = jSONArray12;
			if (Settings.Instance.SaveWithoutDefaultValues)
			{
				SimpleJSONHelper.RemovePropertiesWithDefaultValues(jSONObject);
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

	public static JSONNode GetLightshowOutputJson(BaseDifficulty difficulty)
	{
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			JSONObject jSONObject = new JSONObject { ["version"] = "4.0.0" };
			JSONArray jSONArray = new JSONArray();
			JSONArray jSONArray2 = new JSONArray();
			List<BaseEvent> list = difficulty.Events.Where((BaseEvent x) => !x.IsLaneRotationEvent() && !x.IsColorBoostEvent()).ToList();
			List<V4CommonData.BasicEvent> list2 = list.Select(V4CommonData.BasicEvent.FromBaseEvent).Distinct().ToList();
			foreach (BaseEvent item in list)
			{
				jSONArray.Add(V4BasicEvent.ToJson(item, list2));
			}
			foreach (V4CommonData.BasicEvent item2 in list2)
			{
				jSONArray2.Add(item2.ToJson());
			}
			jSONObject["basicEvents"] = jSONArray;
			jSONObject["basicEventsData"] = jSONArray2;
			JSONArray jSONArray3 = new JSONArray();
			JSONArray jSONArray4 = new JSONArray();
			List<BaseEvent> list3 = difficulty.Events.Where((BaseEvent x) => x.IsColorBoostEvent()).ToList();
			List<V4CommonData.ColorBoostEvent> list4 = list3.Select(V4CommonData.ColorBoostEvent.FromBaseEvent).Distinct().ToList();
			foreach (BaseEvent item3 in list3)
			{
				jSONArray3.Add(V4ColorBoostEvent.ToJson(item3, list4));
			}
			foreach (V4CommonData.ColorBoostEvent item4 in list4)
			{
				jSONArray4.Add(item4.ToJson());
			}
			jSONObject["colorBoostEvents"] = jSONArray3;
			jSONObject["colorBoostEventsData"] = jSONArray4;
			JSONArray jSONArray5 = new JSONArray();
			JSONArray jSONArray6 = new JSONArray();
			List<V4CommonData.Waypoint> list5 = difficulty.Waypoints.Select(V4CommonData.Waypoint.FromBaseWayPoint).Distinct().ToList();
			foreach (BaseWaypoint waypoint in difficulty.Waypoints)
			{
				jSONArray5.Add(V4Waypoint.ToJson(waypoint, list5));
			}
			foreach (V4CommonData.Waypoint item5 in list5)
			{
				jSONArray6.Add(item5.ToJson());
			}
			jSONObject["waypoints"] = jSONArray5;
			jSONObject["waypointsData"] = jSONArray6;
			JSONArray jSONArray7 = new JSONArray();
			List<V4CommonData.IndexFilter> list6 = new List<V4CommonData.IndexFilter>();
			list6.AddRange((from evt in difficulty.LightColorEventBoxGroups.SelectMany((BaseLightColorEventBoxGroup<BaseLightColorEventBox> box) => box.Events)
				select evt.IndexFilter).Select(V4CommonData.IndexFilter.FromBaseIndexFilter));
			list6.AddRange((from evt in difficulty.LightRotationEventBoxGroups.SelectMany((BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> box) => box.Events)
				select evt.IndexFilter).Select(V4CommonData.IndexFilter.FromBaseIndexFilter));
			list6.AddRange((from evt in difficulty.LightTranslationEventBoxGroups.SelectMany((BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> box) => box.Events)
				select evt.IndexFilter).Select(V4CommonData.IndexFilter.FromBaseIndexFilter));
			list6.AddRange((from evt in difficulty.VfxEventBoxGroups.SelectMany((BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> box) => box.Events)
				select evt.IndexFilter).Select(V4CommonData.IndexFilter.FromBaseIndexFilter));
			list6 = list6.Distinct().ToList();
			JSONArray jSONArray8 = new JSONArray();
			foreach (V4CommonData.IndexFilter item6 in list6)
			{
				jSONArray8.Add(item6.ToJson());
			}
			jSONObject["indexFilters"] = jSONArray8;
			JSONArray jSONArray9 = new JSONArray();
			JSONArray jSONArray10 = new JSONArray();
			List<V4CommonData.LightColorEventBox> list7 = difficulty.LightColorEventBoxGroups.SelectMany((BaseLightColorEventBoxGroup<BaseLightColorEventBox> group) => group.Events).Select(V4CommonData.LightColorEventBox.FromBaseLightColorEventBox).Distinct()
				.ToList();
			List<V4CommonData.LightColorEvent> list8 = difficulty.LightColorEventBoxGroups.SelectMany((BaseLightColorEventBoxGroup<BaseLightColorEventBox> group) => group.Events).SelectMany((BaseLightColorEventBox box) => box.Events).Select(V4CommonData.LightColorEvent.FromBaseLightColorEvent)
				.Distinct()
				.ToList();
			foreach (V4CommonData.LightColorEventBox item7 in list7)
			{
				jSONArray9.Add(item7.ToJson());
			}
			foreach (V4CommonData.LightColorEvent item8 in list8)
			{
				jSONArray10.Add(item8.ToJson());
			}
			foreach (BaseLightColorEventBoxGroup<BaseLightColorEventBox> lightColorEventBoxGroup in difficulty.LightColorEventBoxGroups)
			{
				jSONArray7.Add(V4LightColorEventBoxGroup.ToJson(lightColorEventBoxGroup, list6, list7, list8));
			}
			JSONArray jSONArray11 = new JSONArray();
			JSONArray jSONArray12 = new JSONArray();
			List<V4CommonData.LightRotationEventBox> list9 = difficulty.LightRotationEventBoxGroups.SelectMany((BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> group) => group.Events).Select(V4CommonData.LightRotationEventBox.FromBaseLightRotationEventBox).Distinct()
				.ToList();
			List<V4CommonData.LightRotationEvent> list10 = difficulty.LightRotationEventBoxGroups.SelectMany((BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> group) => group.Events).SelectMany((BaseLightRotationEventBox box) => box.Events).Select(V4CommonData.LightRotationEvent.FromBaseLightRotationEvent)
				.Distinct()
				.ToList();
			foreach (V4CommonData.LightRotationEventBox item9 in list9)
			{
				jSONArray11.Add(item9.ToJson());
			}
			foreach (V4CommonData.LightRotationEvent item10 in list10)
			{
				jSONArray12.Add(item10.ToJson());
			}
			foreach (BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> lightRotationEventBoxGroup in difficulty.LightRotationEventBoxGroups)
			{
				jSONArray7.Add(V4LightRotationEventBoxGroup.ToJson(lightRotationEventBoxGroup, list6, list9, list10));
			}
			JSONArray jSONArray13 = new JSONArray();
			JSONArray jSONArray14 = new JSONArray();
			List<V4CommonData.LightTranslationEventBox> list11 = difficulty.LightTranslationEventBoxGroups.SelectMany((BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> group) => group.Events).Select(V4CommonData.LightTranslationEventBox.FromBaseLightTranslationEventBox).Distinct()
				.ToList();
			List<V4CommonData.LightTranslationEvent> list12 = difficulty.LightTranslationEventBoxGroups.SelectMany((BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> group) => group.Events).SelectMany((BaseLightTranslationEventBox box) => box.Events).Select(V4CommonData.LightTranslationEvent.FromBaseLightTranslationEvent)
				.Distinct()
				.ToList();
			foreach (V4CommonData.LightTranslationEventBox item11 in list11)
			{
				jSONArray13.Add(item11.ToJson());
			}
			foreach (V4CommonData.LightTranslationEvent item12 in list12)
			{
				jSONArray14.Add(item12.ToJson());
			}
			foreach (BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> lightTranslationEventBoxGroup in difficulty.LightTranslationEventBoxGroups)
			{
				jSONArray7.Add(V4LightTranslationEventBoxGroup.ToJson(lightTranslationEventBoxGroup, list6, list11, list12));
			}
			JSONArray jSONArray15 = new JSONArray();
			JSONArray jSONArray16 = new JSONArray();
			List<V4CommonData.FxEventBox> list13 = difficulty.VfxEventBoxGroups.SelectMany((BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> group) => group.Events).Select(V4CommonData.FxEventBox.FromBaseFxEventBox).Distinct()
				.ToList();
			List<V4CommonData.FloatFxEvent> list14 = difficulty.VfxEventBoxGroups.SelectMany((BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> group) => group.Events).SelectMany((BaseVfxEventEventBox box) => box.FloatFxEvents).Select(V4CommonData.FloatFxEvent.FromFloatFxEventBase)
				.Distinct()
				.ToList();
			foreach (V4CommonData.FxEventBox item13 in list13)
			{
				jSONArray15.Add(item13.ToJson());
			}
			foreach (V4CommonData.FloatFxEvent item14 in list14)
			{
				jSONArray16.Add(item14.ToJson());
			}
			foreach (BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> vfxEventBoxGroup in difficulty.VfxEventBoxGroups)
			{
				jSONArray7.Add(V4VfxEventEventBoxGroup.ToJson(vfxEventBoxGroup, list6, list13, list14));
			}
			IEnumerable<JSONNode> enumerable = from x in jSONArray7.Linq
				orderby x.Value["b"].AsInt, x.Value["g"].AsInt, x.Value["t"].AsInt
				select x.Value;
			jSONArray7 = new JSONArray();
			foreach (JSONNode item15 in enumerable)
			{
				jSONArray7.Add(item15);
			}
			jSONObject["eventBoxGroups"] = jSONArray7;
			jSONObject["lightColorEventBoxes"] = jSONArray9;
			jSONObject["lightColorEvents"] = jSONArray10;
			jSONObject["lightRotationEventBoxes"] = jSONArray11;
			jSONObject["lightRotationEvents"] = jSONArray12;
			jSONObject["lightTranslationEventBoxes"] = jSONArray13;
			jSONObject["lightTranslationEvents"] = jSONArray14;
			jSONObject["fxEventBoxes"] = jSONArray15;
			jSONObject["floatFxEvents"] = jSONArray16;
			jSONObject["basicEventTypesWithKeywords"] = difficulty.EventTypesWithKeywords?.ToJson() ?? new BaseEventTypesWithKeywords().ToJson();
			jSONObject["useNormalEventsAsCompatibleEvents"] = difficulty.UseNormalEventsAsCompatibleEvents;
			if (Settings.Instance.SaveWithoutDefaultValues)
			{
				SimpleJSONHelper.RemovePropertiesWithDefaultValues(jSONObject);
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

	public static BaseDifficulty GetFromJson(JSONNode mainNode, string path)
	{
		try
		{
			BaseDifficulty baseDifficulty = new BaseDifficulty
			{
				DirectoryAndFile = path,
				Version = "4.1.0"
			};
			List<V4CommonData.Note> list = new List<V4CommonData.Note>();
			List<V4CommonData.Bomb> list2 = new List<V4CommonData.Bomb>();
			List<V4CommonData.Obstacle> list3 = new List<V4CommonData.Obstacle>();
			List<V4CommonData.Arc> list4 = new List<V4CommonData.Arc>();
			List<V4CommonData.Chain> list5 = new List<V4CommonData.Chain>();
			List<V4CommonData.RotationEvent> list6 = new List<V4CommonData.RotationEvent>();
			List<V4CommonData.NJSEvent> list7 = new List<V4CommonData.NJSEvent>();
			JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				JSONNode value = enumerator.Current.Value;
				switch (key)
				{
				case "colorNotesData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node6 = enumerator2.Current;
						list.Add(V4CommonData.Note.GetFromJson(node6));
					}
					break;
				}
				case "bombNotesData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node2 = enumerator2.Current;
						list2.Add(V4CommonData.Bomb.GetFromJson(node2));
					}
					break;
				}
				case "obstaclesData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node4 = enumerator2.Current;
						list3.Add(V4CommonData.Obstacle.GetFromJson(node4));
					}
					break;
				}
				case "arcsData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node7 = enumerator2.Current;
						list4.Add(V4CommonData.Arc.GetFromJson(node7));
					}
					break;
				}
				case "chainsData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node5 = enumerator2.Current;
						list5.Add(V4CommonData.Chain.GetFromJson(node5));
					}
					break;
				}
				case "njsEventData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node3 = enumerator2.Current;
						list7.Add(V4CommonData.NJSEvent.GetFromJson(node3));
					}
					break;
				}
				case "spawnRotationsData":
				{
					JSONNode.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node = enumerator2.Current;
						list6.Add(V4CommonData.RotationEvent.GetFromJson(node));
					}
					break;
				}
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
						JSONNode node13 = enumerator2.Current;
						baseDifficulty.Notes.Add(V4ColorNote.GetFromJson(node13, list));
					}
					break;
				}
				case "bombNotes":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node9 = enumerator2.Current;
						baseDifficulty.Notes.Add(V4BombNote.GetFromJson(node9, list2));
					}
					break;
				}
				case "obstacles":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node11 = enumerator2.Current;
						baseDifficulty.Obstacles.Add(V4Obstacle.GetFromJson(node11, list3));
					}
					break;
				}
				case "arcs":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node14 = enumerator2.Current;
						baseDifficulty.Arcs.Add(V4Arc.GetFromJson(node14, list, list4));
					}
					break;
				}
				case "chains":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node12 = enumerator2.Current;
						baseDifficulty.Chains.Add(V4Chain.GetFromJson(node12, list, list5));
					}
					break;
				}
				case "njsEvents":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node10 = enumerator2.Current;
						baseDifficulty.NJSEvents.Add(V4NJSEvent.GetFromJson(node10, list7));
					}
					break;
				}
				case "spawnRotations":
				{
					JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						JSONNode node8 = enumerator2.Current;
						baseDifficulty.Events.Add(V4RotationEvent.GetFromJson(node8, list6));
					}
					break;
				}
				}
			}
			baseDifficulty.Notes.Sort();
			baseDifficulty.Events.Sort();
			baseDifficulty.Obstacles.Sort();
			baseDifficulty.Chains.Sort();
			baseDifficulty.Arcs.Sort();
			baseDifficulty.NJSEvents.Sort();
			return baseDifficulty;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public static void LoadBpmFromAudioData(BaseDifficulty map, BaseInfo info)
	{
		string text = Path.Combine(info.Directory, info.AudioDataFilename);
		if (!File.Exists(text))
		{
			Debug.Log("No AudioData found at " + text);
			return;
		}
		BaseBpmInfo fromJson = V4AudioData.GetFromJson(BeatSaberSongUtils.GetNodeFromFile(text));
		List<BaseBpmEvent> bpmEvents = BaseBpmInfo.GetBpmEvents(fromJson.BpmRegions, fromJson.AudioFrequency);
		map.BpmEvents = bpmEvents;
		map.BootstrapBpmEvents(info.BeatsPerMinute);
	}

	public static void LoadBookmarksFromOfficialEditor(BaseDifficulty map, BaseInfo info, InfoDifficulty infoDifficulty)
	{
		string path = Path.Combine(info.Directory, "Bookmarks");
		string text = Path.Combine(path, infoDifficulty.BookmarkFileName);
		if (!File.Exists(text))
		{
			return;
		}
		JSONNode nodeFromFile = BeatSaberSongUtils.GetNodeFromFile(Path.Combine(path, text));
		if (nodeFromFile["bookmarks"].IsArray)
		{
			Color color = nodeFromFile["color"].ReadHtmlStringColor();
			List<BaseBookmark> bookmarks = (from jsonNode in nodeFromFile["bookmarks"].AsArray.Children
				select jsonNode.AsObject into jsonObj
				select new BaseBookmark
				{
					JsonTime = jsonObj["beat"].AsFloat,
					Name = jsonObj["text"].Value,
					Color = color
				}).ToList();
			map.Bookmarks = bookmarks;
		}
	}

	public static void LoadLightsFromLightshowFile(BaseDifficulty map, BaseInfo info, InfoDifficulty infoDifficulty)
	{
		string text = Path.Combine(info.Directory, infoDifficulty.LightshowFileName);
		if (!File.Exists(text))
		{
			Debug.Log("No lightshow file found at " + text);
			return;
		}
		JSONNode nodeFromFile = BeatSaberSongUtils.GetNodeFromFile(text);
		LoadLightsFromJson(map, nodeFromFile);
	}

	public static void LoadLightsFromJson(BaseDifficulty map, JSONNode mainNode)
	{
		List<V4CommonData.BasicEvent> list = new List<V4CommonData.BasicEvent>();
		List<V4CommonData.ColorBoostEvent> list2 = new List<V4CommonData.ColorBoostEvent>();
		List<V4CommonData.Waypoint> list3 = new List<V4CommonData.Waypoint>();
		List<BaseIndexFilter> list4 = new List<BaseIndexFilter>();
		List<V4CommonData.LightColorEventBox> list5 = new List<V4CommonData.LightColorEventBox>();
		List<V4CommonData.LightColorEvent> list6 = new List<V4CommonData.LightColorEvent>();
		List<V4CommonData.LightRotationEventBox> list7 = new List<V4CommonData.LightRotationEventBox>();
		List<V4CommonData.LightRotationEvent> list8 = new List<V4CommonData.LightRotationEvent>();
		List<V4CommonData.LightTranslationEventBox> list9 = new List<V4CommonData.LightTranslationEventBox>();
		List<V4CommonData.LightTranslationEvent> list10 = new List<V4CommonData.LightTranslationEvent>();
		List<V4CommonData.FxEventBox> list11 = new List<V4CommonData.FxEventBox>();
		List<V4CommonData.FloatFxEvent> list12 = new List<V4CommonData.FloatFxEvent>();
		JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			JSONNode value = enumerator.Current.Value;
			switch (key)
			{
			case "basicEventsData":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node8 = enumerator2.Current;
					list.Add(V4CommonData.BasicEvent.GetFromJson(node8));
				}
				break;
			}
			case "colorBoostEventsData":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node12 = enumerator2.Current;
					list2.Add(V4CommonData.ColorBoostEvent.GetFromJson(node12));
				}
				break;
			}
			case "waypointsData":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node4 = enumerator2.Current;
					list3.Add(V4CommonData.Waypoint.GetFromJson(node4));
				}
				break;
			}
			case "indexFilters":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node10 = enumerator2.Current;
					list4.Add(V4IndexFilter.GetFromJson(node10));
				}
				break;
			}
			case "lightColorEventBoxes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node6 = enumerator2.Current;
					list5.Add(V4CommonData.LightColorEventBox.GetFromJson(node6));
				}
				break;
			}
			case "lightColorEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node2 = enumerator2.Current;
					list6.Add(V4CommonData.LightColorEvent.GetFromJson(node2));
				}
				break;
			}
			case "lightRotationEventBoxes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node11 = enumerator2.Current;
					list7.Add(V4CommonData.LightRotationEventBox.GetFromJson(node11));
				}
				break;
			}
			case "lightRotationEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node9 = enumerator2.Current;
					list8.Add(V4CommonData.LightRotationEvent.GetFromJson(node9));
				}
				break;
			}
			case "lightTranslationEventBoxes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node7 = enumerator2.Current;
					list9.Add(V4CommonData.LightTranslationEventBox.GetFromJson(node7));
				}
				break;
			}
			case "lightTranslationEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node5 = enumerator2.Current;
					list10.Add(V4CommonData.LightTranslationEvent.GetFromJson(node5));
				}
				break;
			}
			case "fxEventBoxes":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node3 = enumerator2.Current;
					list11.Add(V4CommonData.FxEventBox.GetFromJson(node3));
				}
				break;
			}
			case "floatFxEvents":
			{
				JSONNode.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node = enumerator2.Current;
					list12.Add(V4CommonData.FloatFxEvent.GetFromJson(node));
				}
				break;
			}
			}
		}
		List<BaseEvent> list13 = new List<BaseEvent>();
		List<BaseLightColorEventBoxGroup<BaseLightColorEventBox>> list14 = new List<BaseLightColorEventBoxGroup<BaseLightColorEventBox>>();
		List<BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>> list15 = new List<BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>>();
		List<BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>> list16 = new List<BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>>();
		List<BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>> list17 = new List<BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>>();
		List<BaseWaypoint> list18 = new List<BaseWaypoint>();
		BaseEventTypesWithKeywords eventTypesWithKeywords = new BaseEventTypesWithKeywords();
		bool useNormalEventsAsCompatibleEvents = true;
		enumerator = mainNode.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key2 = enumerator.Current.Key;
			JSONNode value2 = enumerator.Current.Value;
			switch (key2)
			{
			case "basicEvents":
			{
				JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node15 = enumerator2.Current;
					list13.Add(V4BasicEvent.GetFromJson(node15, list));
				}
				break;
			}
			case "colorBoostEvents":
			{
				JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node14 = enumerator2.Current;
					list13.Add(V4ColorBoostEvent.GetFromJson(node14, list2));
				}
				break;
			}
			case "eventBoxGroups":
			{
				JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode jSONNode = enumerator2.Current;
					switch (jSONNode["t"].AsInt)
					{
					case 1:
						list14.Add(V4LightColorEventBoxGroup.GetFromJson(jSONNode, list4, list5, list6));
						break;
					case 2:
						list15.Add(V4LightRotationEventBoxGroup.GetFromJson(jSONNode, list4, list7, list8));
						break;
					case 3:
						list16.Add(V4LightTranslationEventBoxGroup.GetFromJson(jSONNode, list4, list9, list10));
						break;
					case 4:
						list17.Add(V4VfxEventEventBoxGroup.GetFromJson(jSONNode, list4, list11, list12));
						break;
					}
				}
				break;
			}
			case "waypoints":
			{
				JSONNode.Enumerator enumerator2 = value2.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					JSONNode node13 = enumerator2.Current;
					list18.Add(V4Waypoint.GetFromJson(node13, list3));
				}
				break;
			}
			case "basicEventTypesWithKeywords":
				eventTypesWithKeywords = V4BasicEventTypesWithKeywords.GetFromJson(value2);
				break;
			case "useNormalEventsAsCompatibleEvents":
				useNormalEventsAsCompatibleEvents = value2.AsBool;
				break;
			}
		}
		map.Events = list13;
		map.LightColorEventBoxGroups = list14;
		map.LightRotationEventBoxGroups = list15;
		map.LightTranslationEventBoxGroups = list16;
		map.VfxEventBoxGroups = list17;
		map.Waypoints = list18;
		map.EventTypesWithKeywords = eventTypesWithKeywords;
		map.UseNormalEventsAsCompatibleEvents = useNormalEventsAsCompatibleEvents;
		map.Events.Sort();
	}
}
