using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3FloatFxEvent
{
	public static FloatFxEventBase GetFromJson(JSONNode node)
	{
		return new FloatFxEventBase
		{
			JsonTime = node["b"].AsFloat,
			UsePreviousEventValue = node["p"].AsInt,
			Value = node["v"].AsFloat,
			Easing = node["i"].AsInt
		};
	}

	public static JSONNode ToJson(FloatFxEventBase floatFxEventBase)
	{
		return new JSONObject
		{
			["b"] = floatFxEventBase.JsonTime,
			["p"] = floatFxEventBase.UsePreviousEventValue,
			["v"] = floatFxEventBase.Value,
			["i"] = floatFxEventBase.Easing
		};
	}
}
