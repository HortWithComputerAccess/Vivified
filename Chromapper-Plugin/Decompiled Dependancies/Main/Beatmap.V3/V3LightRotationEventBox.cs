using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightRotationEventBox
{
	public static BaseLightRotationEventBox GetFromJson(JSONNode node)
	{
		return new BaseLightRotationEventBox
		{
			IndexFilter = V3IndexFilter.GetFromJson(BaseItem.GetRequiredNode(node, "f")),
			BeatDistribution = node["w"].AsFloat,
			BeatDistributionType = node["d"].AsInt,
			RotationDistribution = node["s"].AsFloat,
			RotationDistributionType = node["t"].AsInt,
			RotationAffectFirst = node["b"].AsInt,
			Axis = node["a"].AsInt,
			Flip = node["r"].AsInt,
			Easing = node["i"].AsInt,
			Events = BaseItem.GetRequiredNode(node, "l").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightRotationBase.GetFromJson(x.Value)).ToArray()
		};
	}

	public static JSONNode ToJson(BaseLightRotationEventBox box)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["f"] = box.IndexFilter.ToJson();
		jSONNode["w"] = box.BeatDistribution;
		jSONNode["d"] = box.BeatDistributionType;
		jSONNode["s"] = box.RotationDistribution;
		jSONNode["t"] = box.RotationDistributionType;
		jSONNode["b"] = box.RotationAffectFirst;
		jSONNode["a"] = box.Axis;
		jSONNode["r"] = box.Flip;
		jSONNode["i"] = box.Easing;
		JSONArray jSONArray = new JSONArray();
		BaseLightRotationBase[] events = box.Events;
		foreach (BaseLightRotationBase lightRotationBase in events)
		{
			jSONArray.Add(V3LightRotationBase.ToJson(lightRotationBase));
		}
		jSONNode["l"] = jSONArray;
		return jSONNode;
	}
}
