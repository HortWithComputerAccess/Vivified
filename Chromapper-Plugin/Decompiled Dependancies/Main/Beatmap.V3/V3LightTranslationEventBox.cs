using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightTranslationEventBox
{
	public static BaseLightTranslationEventBox GetFromJson(JSONNode node)
	{
		return new BaseLightTranslationEventBox
		{
			IndexFilter = V3IndexFilter.GetFromJson(BaseItem.GetRequiredNode(node, "f")),
			BeatDistribution = node["w"].AsFloat,
			BeatDistributionType = node["d"].AsInt,
			TranslationDistribution = node["s"].AsFloat,
			TranslationDistributionType = node["t"].AsInt,
			TranslationAffectFirst = node["b"].AsInt,
			Axis = node["a"].AsInt,
			Flip = node["r"].AsInt,
			Easing = node["i"].AsInt,
			Events = BaseItem.GetRequiredNode(node, "l").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightTranslationBase.GetFromJson(x.Value)).ToArray()
		};
	}

	public static JSONNode ToJson(BaseLightTranslationEventBox box)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["f"] = box.IndexFilter.ToJson();
		jSONNode["w"] = box.BeatDistribution;
		jSONNode["d"] = box.BeatDistributionType;
		jSONNode["s"] = box.TranslationDistribution;
		jSONNode["t"] = box.TranslationDistributionType;
		jSONNode["b"] = box.TranslationAffectFirst;
		jSONNode["a"] = box.Axis;
		jSONNode["r"] = box.Flip;
		jSONNode["i"] = box.Easing;
		JSONArray jSONArray = new JSONArray();
		BaseLightTranslationBase[] events = box.Events;
		foreach (BaseLightTranslationBase lightTranslationBase in events)
		{
			jSONArray.Add(V3LightTranslationBase.ToJson(lightTranslationBase));
		}
		jSONNode["l"] = jSONArray;
		return jSONNode;
	}
}
