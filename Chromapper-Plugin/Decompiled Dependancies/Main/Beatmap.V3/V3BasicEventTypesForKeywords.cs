using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3BasicEventTypesForKeywords
{
	public static BaseEventTypesForKeywords GetFromJson(JSONNode node)
	{
		return new BaseEventTypesForKeywords
		{
			Keyword = BaseItem.GetRequiredNode(node, "k"),
			Events = BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => x.Value.AsInt).ToArray()
		};
	}

	public static JSONNode ToJson(BaseEventTypesForKeywords forKeywords)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["k"] = forKeywords.Keyword;
		jSONNode["e"] = new JSONArray();
		int[] events = forKeywords.Events;
		foreach (int num in events)
		{
			jSONNode["e"].Add(num);
		}
		return jSONNode;
	}
}
