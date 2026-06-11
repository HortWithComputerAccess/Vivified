using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4BasicEvent
{
	public static BaseEvent GetFromJson(JSONNode node, IList<V4CommonData.BasicEvent> basicEventsCommonData)
	{
		BaseEvent obj = new BaseEvent
		{
			JsonTime = node["b"].AsFloat
		};
		int asInt = node["i"].AsInt;
		V4CommonData.BasicEvent basicEvent = basicEventsCommonData[asInt];
		obj.Type = basicEvent.Type;
		obj.Value = basicEvent.Value;
		obj.FloatValue = basicEvent.FloatValue;
		return obj;
	}

	public static JSONNode ToJson(BaseEvent evt, IList<V4CommonData.BasicEvent> basicEventsCommonData)
	{
		JSONObject obj = new JSONObject { ["b"] = evt.JsonTime };
		V4CommonData.BasicEvent item = V4CommonData.BasicEvent.FromBaseEvent(evt);
		obj["i"] = basicEventsCommonData.IndexOf(item);
		return obj;
	}
}
