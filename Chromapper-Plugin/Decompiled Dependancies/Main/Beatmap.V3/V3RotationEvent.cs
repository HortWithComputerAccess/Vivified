using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3RotationEvent
{
	public static BaseEvent GetFromJson(JSONNode node)
	{
		return new BaseEvent
		{
			JsonTime = node["b"].AsFloat,
			Type = ((node["e"].AsInt == 0) ? 14 : 15),
			Rotation = node["r"].AsFloat,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseEvent evt)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = evt.JsonTime;
		jSONNode["e"] = ((evt.Type != 14) ? 1 : 0);
		jSONNode["r"] = evt.Rotation;
		evt.CustomData = evt.SaveCustom();
		if (!evt.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = evt.CustomData;
		return jSONNode;
	}
}
