using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightTranslationBase
{
	public static BaseLightTranslationBase GetFromJson(JSONNode node)
	{
		return new BaseLightTranslationBase
		{
			JsonTime = node["b"].AsFloat,
			UsePrevious = node["p"].AsInt,
			EaseType = node["e"].AsInt,
			Translation = node["t"].AsFloat,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseLightTranslationBase lightTranslationBase)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = lightTranslationBase.JsonTime;
		jSONNode["p"] = lightTranslationBase.UsePrevious;
		jSONNode["e"] = lightTranslationBase.EaseType;
		jSONNode["t"] = lightTranslationBase.Translation;
		lightTranslationBase.CustomData = lightTranslationBase.SaveCustom();
		if (!lightTranslationBase.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = lightTranslationBase.CustomData;
		return jSONNode;
	}
}
