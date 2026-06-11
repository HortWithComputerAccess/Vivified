using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4ColorBoostEvent
{
	public static BaseEvent GetFromJson(JSONNode node, IList<V4CommonData.ColorBoostEvent> colorBoostEventsCommonData)
	{
		BaseEvent obj = new BaseEvent
		{
			JsonTime = node["b"].AsFloat,
			Type = 5,
			FloatValue = 0f
		};
		int asInt = node["i"].AsInt;
		obj.Value = colorBoostEventsCommonData[asInt].Boost;
		return obj;
	}

	public static JSONNode ToJson(BaseEvent evt, IList<V4CommonData.ColorBoostEvent> colorBoostEventsCommonData)
	{
		JSONObject obj = new JSONObject { ["b"] = evt.JsonTime };
		V4CommonData.ColorBoostEvent item = V4CommonData.ColorBoostEvent.FromBaseEvent(evt);
		obj["i"] = colorBoostEventsCommonData.IndexOf(item);
		return obj;
	}
}
