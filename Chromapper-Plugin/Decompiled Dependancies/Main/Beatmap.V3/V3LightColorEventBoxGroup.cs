using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightColorEventBoxGroup
{
	public static BaseLightColorEventBoxGroup<BaseLightColorEventBox> GetFromJson(JSONNode node)
	{
		return new BaseLightColorEventBoxGroup<BaseLightColorEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt,
			Events = new List<BaseLightColorEventBox>(BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightColorEventBox.GetFromJson(x.Value)).ToList()),
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson<T>(BaseLightColorEventBoxGroup<T> group) where T : BaseLightColorEventBox
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		JSONArray jSONArray = new JSONArray();
		foreach (T @event in group.Events)
		{
			jSONArray.Add(V3LightColorEventBox.ToJson(@event));
		}
		jSONNode["e"] = jSONArray;
		group.CustomData = group.SaveCustom();
		if (!group.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = group.CustomData;
		return jSONNode;
	}
}
