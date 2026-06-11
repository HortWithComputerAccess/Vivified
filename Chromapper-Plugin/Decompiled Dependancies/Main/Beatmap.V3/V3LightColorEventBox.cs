using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightColorEventBox
{
	public static BaseLightColorEventBox GetFromJson(JSONNode node)
	{
		return new BaseLightColorEventBox
		{
			IndexFilter = V3IndexFilter.GetFromJson(BaseItem.GetRequiredNode(node, "f")),
			BeatDistribution = node["w"].AsFloat,
			BeatDistributionType = node["d"].AsInt,
			BrightnessDistribution = node["r"].AsFloat,
			BrightnessDistributionType = node["t"].AsInt,
			BrightnessAffectFirst = node["b"].AsInt,
			Easing = node["i"].AsInt,
			Events = BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightColorBase.GetFromJson(x.Value)).ToArray()
		};
	}

	public static JSONNode ToJson(BaseLightColorEventBox box)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["f"] = box.IndexFilter.ToJson();
		jSONNode["w"] = box.BeatDistribution;
		jSONNode["d"] = box.BeatDistributionType;
		jSONNode["r"] = box.BrightnessDistribution;
		jSONNode["t"] = box.BrightnessDistributionType;
		jSONNode["b"] = box.BrightnessAffectFirst;
		jSONNode["i"] = box.Easing;
		JSONArray jSONArray = new JSONArray();
		BaseLightColorBase[] events = box.Events;
		foreach (BaseLightColorBase lightColorBase in events)
		{
			jSONArray.Add(V3LightColorBase.ToJson(lightColorBase));
		}
		jSONNode["e"] = jSONArray;
		return jSONNode;
	}
}
