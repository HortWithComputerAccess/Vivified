using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3IntFxEvent
{
	public static IntFxEventBase GetFromJson(JSONNode node)
	{
		return new IntFxEventBase
		{
			JsonTime = node["b"].AsFloat,
			UsePreviousEventValue = node["p"].AsInt,
			Value = node["v"].AsInt
		};
	}

	public static JSONNode ToJson(IntFxEventBase intFxEventBase)
	{
		return new JSONObject
		{
			["b"] = intFxEventBase.JsonTime,
			["p"] = intFxEventBase.UsePreviousEventValue,
			["v"] = intFxEventBase.Value
		};
	}
}
