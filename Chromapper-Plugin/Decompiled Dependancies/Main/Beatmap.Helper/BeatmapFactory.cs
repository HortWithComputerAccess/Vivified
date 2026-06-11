using System;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Info;
using Beatmap.V2;
using Beatmap.V2.Customs;
using Beatmap.V3;
using Beatmap.V3.Customs;
using Beatmap.V4;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Helper;

public static class BeatmapFactory
{
	public static BaseDifficulty GetDifficultyFromJson(JSONNode mainNode, string directoryAndFile, BaseInfo info, InfoDifficulty infoDifficulty)
	{
		BaseDifficulty fromJson;
		switch (PeekMapVersionFromJson(mainNode)[0])
		{
		case '4':
			Settings.Instance.MapVersion = 4;
			fromJson = V4Difficulty.GetFromJson(mainNode, directoryAndFile);
			V4Difficulty.LoadBpmFromAudioData(fromJson, info);
			V4Difficulty.LoadLightsFromLightshowFile(fromJson, info, infoDifficulty);
			V4Difficulty.LoadBookmarksFromOfficialEditor(fromJson, info, infoDifficulty);
			break;
		case '3':
			Settings.Instance.MapVersion = 3;
			fromJson = V3Difficulty.GetFromJson(mainNode, directoryAndFile);
			break;
		case '2':
			Settings.Instance.MapVersion = 2;
			fromJson = V2Difficulty.GetFromJson(mainNode, directoryAndFile);
			break;
		default:
			return null;
		}
		fromJson.BootstrapBpmEvents(info.BeatsPerMinute);
		fromJson.RecomputeAllObjectSongBpmTimes();
		return fromJson;
	}

	private static string PeekMapVersionFromJson(JSONNode mainNode)
	{
		JSONNode.Enumerator enumerator = mainNode.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			JSONNode value = enumerator.Current.Value;
			if (key == "version" || key == "_version")
			{
				return value.Value;
			}
		}
		Debug.LogError("no version detected, return default version 2.0.0.");
		return "2.0.0";
	}

	public static TConcrete Clone<TConcrete>(TConcrete cloneable) where TConcrete : BaseItem
	{
		if (cloneable == null)
		{
			throw new ArgumentException("cloneable is null.");
		}
		return cloneable.Clone() as TConcrete;
	}

	public static BaseBpmEvent BpmEvent(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2BpmEvent.GetFromJson(node);
		}
		return V3BpmEvent.GetFromJson(node);
	}

	public static BaseNote Note(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Note.GetFromJson(node);
		}
		if (!node.HasKey("c"))
		{
			return V3BombNote.GetFromJson(node);
		}
		return V3ColorNote.GetFromJson(node, false);
	}

	public static BaseNote Bomb(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Note.GetFromJson(node);
		}
		return V3BombNote.GetFromJson(node);
	}

	public static BaseObstacle Obstacle(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Obstacle.GetFromJson(node);
		}
		return V3Obstacle.GetFromJson(node);
	}

	public static BaseArc Arc(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Arc.GetFromJson(node);
		}
		return V3Arc.GetFromJson(node);
	}

	public static BaseChain Chain(JSONNode node)
	{
		return V3Chain.GetFromJson(node);
	}

	public static BaseWaypoint Waypoint(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Waypoint.GetFromJson(node);
		}
		return V3Waypoint.GetFromJson(node);
	}

	public static BaseEvent Event(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3 || mapVersion == 4)
		{
			if (node.HasKey("e") || node.HasKey("r"))
			{
				return V3RotationEvent.GetFromJson(node);
			}
			if (node.HasKey("o"))
			{
				return V3ColorBoostEvent.GetFromJson(node);
			}
			return V3BasicEvent.GetFromJson(node);
		}
		return V2Event.GetFromJson(node);
	}

	public static BaseLightColorEventBoxGroup<BaseLightColorEventBox> LightColorEventBoxGroups(JSONNode node)
	{
		return V3LightColorEventBoxGroup.GetFromJson(node);
	}

	public static BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> LightRotationEventBoxGroups(JSONNode node)
	{
		return V3LightRotationEventBoxGroup.GetFromJson(node);
	}

	public static BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> LightTranslationEventBoxGroups(JSONNode node)
	{
		return V3LightTranslationEventBoxGroup.GetFromJson(node);
	}

	public static BaseEventTypesWithKeywords EventTypesWithKeywords(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2SpecialEventsKeywordFilters.GetFromJson(node);
		}
		return V3BasicEventTypesWithKeywords.GetFromJson(node);
	}

	public static BaseBpmChange BpmChange(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2BpmChange.GetFromJson(node);
		}
		return V3BpmChange.GetFromJson(node);
	}

	public static BaseBookmark Bookmark(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2Bookmark.GetFromJson(node);
		}
		return V3Bookmark.GetFromJson(node);
	}

	public static BaseCustomEvent CustomEvent(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2CustomEvent.GetFromJson(node);
		}
		return V3CustomEvent.GetFromJson(node);
	}

	public static BaseEnvironmentEnhancement EnvironmentEnhancement(JSONNode node)
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion != 3 && mapVersion != 4)
		{
			return V2EnvironmentEnhancement.GetFromJson(node);
		}
		return V3EnvironmentEnhancement.GetFromJson(node);
	}
}
