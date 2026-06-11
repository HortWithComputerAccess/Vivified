using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public class V4BasicEventTypesWithKeywords
{
	public static BaseEventTypesWithKeywords GetFromJson(JSONNode node)
	{
		BaseEventTypesWithKeywords baseEventTypesWithKeywords = new BaseEventTypesWithKeywords();
		if (node.HasKey("d"))
		{
			baseEventTypesWithKeywords.Keywords = node["d"].AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V4BasicEventTypesForKeywords.GetFromJson(x.Value)).ToArray();
		}
		else
		{
			baseEventTypesWithKeywords.Keywords = new BaseEventTypesForKeywords[0];
		}
		return baseEventTypesWithKeywords;
	}

	public static JSONNode ToJson(BaseEventTypesWithKeywords withKeywords)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["d"] = new JSONArray();
		BaseEventTypesForKeywords[] keywords = withKeywords.Keywords;
		foreach (BaseEventTypesForKeywords forKeywords in keywords)
		{
			jSONNode["d"].Add(V4BasicEventTypesForKeywords.ToJson(forKeywords));
		}
		return jSONNode;
	}
}
