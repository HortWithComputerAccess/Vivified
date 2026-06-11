using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightColorBase
{
	public static BaseLightColorBase GetFromJson(JSONNode node)
	{
		return new BaseLightColorBase
		{
			JsonTime = node["b"].AsFloat,
			Color = node["c"].AsInt,
			Brightness = node["s"].AsFloat,
			TransitionType = node["i"].AsInt,
			Frequency = node["f"].AsInt,
			StrobeBrightness = node["sb"].AsFloat,
			StrobeFade = node["sf"].AsInt,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseLightColorBase lightColorBase)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = lightColorBase.JsonTime;
		jSONNode["c"] = lightColorBase.Color;
		jSONNode["s"] = lightColorBase.Brightness;
		jSONNode["i"] = lightColorBase.TransitionType;
		jSONNode["f"] = lightColorBase.Frequency;
		jSONNode["sb"] = lightColorBase.StrobeBrightness;
		jSONNode["sf"] = lightColorBase.StrobeFade;
		lightColorBase.CustomData = lightColorBase.SaveCustom();
		if (!lightColorBase.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = lightColorBase.CustomData;
		return jSONNode;
	}
}
