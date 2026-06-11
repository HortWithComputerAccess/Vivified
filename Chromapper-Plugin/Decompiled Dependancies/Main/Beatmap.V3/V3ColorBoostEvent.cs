using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3ColorBoostEvent
{
	public static BaseEvent GetFromJson(JSONNode node)
	{
		return new BaseEvent
		{
			JsonTime = node["b"].AsFloat,
			Type = 5,
			Value = (node["o"].AsBool ? 1 : 0),
			FloatValue = 0f,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseEvent evt)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = evt.JsonTime;
		jSONNode["o"] = evt.Value == 1;
		evt.CustomData = evt.SaveCustom();
		if (!evt.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = evt.CustomData;
		return jSONNode;
	}
}
