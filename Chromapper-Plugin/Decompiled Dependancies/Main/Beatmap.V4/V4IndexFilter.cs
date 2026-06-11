using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4IndexFilter
{
	public static BaseIndexFilter GetFromJson(JSONNode node)
	{
		return new BaseIndexFilter
		{
			Type = node["f"].AsInt,
			Param0 = node["p"].AsInt,
			Param1 = node["t"].AsInt,
			Reverse = node["r"].AsInt,
			Chunks = node["c"].AsInt,
			Random = node["n"].AsInt,
			Seed = node["s"].AsInt,
			Limit = node["l"].AsFloat,
			LimitAffectsType = node["d"].AsInt
		};
	}

	public static JSONNode ToJson(BaseIndexFilter filter)
	{
		return new JSONObject
		{
			["f"] = filter.Type,
			["p"] = filter.Param0,
			["t"] = filter.Param1,
			["r"] = filter.Reverse,
			["c"] = filter.Chunks,
			["n"] = filter.Random,
			["s"] = filter.Seed,
			["l"] = filter.Limit,
			["d"] = filter.LimitAffectsType
		};
	}
}
