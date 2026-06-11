using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public static class V2BpmEvent
{
	public const string CustomKeyTrack = "_track";

	public const string CustomKeyColor = "_color";

	public static BaseBpmEvent GetFromJson(JSONNode node)
	{
		BaseBpmEvent baseBpmEvent = new BaseBpmEvent();
		float asFloat = BaseItem.GetRequiredNode(node, "_floatValue").AsFloat;
		baseBpmEvent.JsonTime = BaseItem.GetRequiredNode(node, "_time").AsFloat;
		baseBpmEvent.Bpm = asFloat;
		baseBpmEvent.Type = 100;
		baseBpmEvent.FloatValue = asFloat;
		baseBpmEvent.CustomData = node["_customData"];
		return baseBpmEvent;
	}

	public static JSONNode ToJson(BaseBpmEvent bpmEvent)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_time"] = new JSONNumberWithOverridenRounding(bpmEvent.JsonTime, Settings.Instance.BpmTimeValueDecimalPrecision);
		jSONNode["_type"] = bpmEvent.Type;
		jSONNode["_value"] = 0;
		jSONNode["_floatValue"] = bpmEvent.Bpm;
		bpmEvent.CustomData = bpmEvent.SaveCustom();
		if (bpmEvent.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = bpmEvent.CustomData;
		return jSONNode;
	}
}
