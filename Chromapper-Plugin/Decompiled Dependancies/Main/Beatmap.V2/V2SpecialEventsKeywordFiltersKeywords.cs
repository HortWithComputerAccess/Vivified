using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public class V2SpecialEventsKeywordFiltersKeywords
{
	public static BaseEventTypesForKeywords GetFromJson(JSONNode node)
	{
		return new BaseEventTypesForKeywords
		{
			Keyword = BaseItem.GetRequiredNode(node, "_keyword"),
			Events = BaseItem.GetRequiredNode(node, "_specialEvents").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => x.Value.AsInt).ToArray()
		};
	}

	public static JSONNode ToJson(BaseEventTypesForKeywords forKeywords)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_keyword"] = forKeywords.Keyword;
		jSONNode["_specialEvents"] = new JSONArray();
		int[] events = forKeywords.Events;
		foreach (int num in events)
		{
			jSONNode["_specialEvents"].Add(num);
		}
		return jSONNode;
	}
}
