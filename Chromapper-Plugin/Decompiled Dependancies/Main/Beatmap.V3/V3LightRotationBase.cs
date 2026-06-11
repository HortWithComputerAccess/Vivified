using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightRotationBase
{
	public static BaseLightRotationBase GetFromJson(JSONNode node)
	{
		return new BaseLightRotationBase
		{
			JsonTime = node["b"].AsFloat,
			Rotation = node["r"].AsFloat,
			Direction = node["o"].AsInt,
			EaseType = node["e"].AsInt,
			Loop = node["l"].AsInt,
			UsePrevious = node["p"].AsInt,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseLightRotationBase lightRotationBase)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = lightRotationBase.JsonTime;
		jSONNode["r"] = lightRotationBase.Rotation;
		jSONNode["o"] = lightRotationBase.Direction;
		jSONNode["e"] = lightRotationBase.EaseType;
		jSONNode["l"] = lightRotationBase.Loop;
		jSONNode["p"] = lightRotationBase.UsePrevious;
		lightRotationBase.CustomData = lightRotationBase.SaveCustom();
		if (!lightRotationBase.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = lightRotationBase.CustomData;
		return jSONNode;
	}
}
