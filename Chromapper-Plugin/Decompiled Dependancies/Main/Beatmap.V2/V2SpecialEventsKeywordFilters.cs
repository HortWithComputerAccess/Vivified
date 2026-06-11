using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public static class V2SpecialEventsKeywordFilters
{
	public static BaseEventTypesWithKeywords GetFromJson(JSONNode node)
	{
		return new BaseEventTypesWithKeywords
		{
			Keywords = BaseItem.GetRequiredNode(node, "_keywords").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V2SpecialEventsKeywordFiltersKeywords.GetFromJson(x.Value)).ToArray()
		};
	}

	public static JSONNode ToJson(BaseEventTypesWithKeywords withKeywords)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_keywords"] = new JSONArray();
		BaseEventTypesForKeywords[] keywords = withKeywords.Keywords;
		foreach (BaseEventTypesForKeywords forKeywords in keywords)
		{
			jSONNode["_keywords"].Add(V2SpecialEventsKeywordFiltersKeywords.ToJson(forKeywords));
		}
		return jSONNode;
	}
}
