using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3VfxEventEventBox
{
	public static BaseVfxEventEventBox GetFromJson(JSONNode node, IList<FloatFxEventBase> floatFxEvents)
	{
		BaseVfxEventEventBox baseVfxEventEventBox = new BaseVfxEventEventBox();
		baseVfxEventEventBox.IndexFilter = V3IndexFilter.GetFromJson(BaseItem.GetRequiredNode(node, "f"));
		baseVfxEventEventBox.BeatDistribution = node["w"].AsFloat;
		baseVfxEventEventBox.BeatDistributionType = node["d"].AsInt;
		baseVfxEventEventBox.VfxDistribution = node["s"].AsFloat;
		baseVfxEventEventBox.VfxDistributionType = node["t"].AsInt;
		baseVfxEventEventBox.VfxAffectFirst = node["b"].AsInt;
		baseVfxEventEventBox.Easing = node["i"].AsInt;
		if (node.HasKey("l"))
		{
			baseVfxEventEventBox.FloatFxEvents = node["l"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> x)
			{
				int asInt = x.Value.AsInt;
				return (FloatFxEventBase)floatFxEvents[asInt].Clone();
			}).ToList();
		}
		return baseVfxEventEventBox;
	}

	public static JSONNode ToJson(BaseVfxEventEventBox vfxBox, IList<FloatFxEventBase> floatFxEvents)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["f"] = vfxBox.IndexFilter.ToJson();
		jSONNode["w"] = vfxBox.BeatDistribution;
		jSONNode["d"] = vfxBox.BeatDistributionType;
		jSONNode["s"] = vfxBox.VfxDistribution;
		jSONNode["t"] = vfxBox.VfxDistributionType;
		jSONNode["b"] = vfxBox.VfxAffectFirst;
		jSONNode["i"] = vfxBox.Easing;
		jSONNode["l"] = new JSONArray();
		foreach (FloatFxEventBase floatFxEvent in vfxBox.FloatFxEvents)
		{
			jSONNode["l"].Add(floatFxEvents.IndexOf(floatFxEvent));
		}
		return jSONNode;
	}
}
