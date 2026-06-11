using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3BpmEvent
{
	public static BaseBpmEvent GetFromJson(JSONNode node)
	{
		BaseBpmEvent baseBpmEvent = new BaseBpmEvent();
		float asFloat = BaseItem.GetRequiredNode(node, "m").AsFloat;
		baseBpmEvent.JsonTime = node["b"];
		baseBpmEvent.Bpm = asFloat;
		baseBpmEvent.Type = 100;
		baseBpmEvent.FloatValue = asFloat;
		baseBpmEvent.CustomData = node["customData"];
		return baseBpmEvent;
	}

	public static JSONNode ToJson(BaseBpmEvent bpmEvent)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = new JSONNumberWithOverridenRounding(bpmEvent.JsonTime, Settings.Instance.BpmTimeValueDecimalPrecision);
		jSONNode["m"] = bpmEvent.Bpm;
		bpmEvent.CustomData = bpmEvent.SaveCustom();
		if (!bpmEvent.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = bpmEvent.CustomData;
		return jSONNode;
	}
}
