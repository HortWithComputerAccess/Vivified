using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4NJSEvent
{
	public static BaseNJSEvent GetFromJson(JSONNode node, IList<V4CommonData.NJSEvent> njsEventsCommonData)
	{
		BaseNJSEvent obj = new BaseNJSEvent
		{
			JsonTime = node["b"].AsFloat
		};
		int asInt = node["i"].AsInt;
		V4CommonData.NJSEvent nJSEvent = njsEventsCommonData[asInt];
		obj.UsePrevious = nJSEvent.UsePrevious;
		obj.Easing = nJSEvent.Easing;
		obj.RelativeNJS = nJSEvent.RelativeNJS;
		return obj;
	}

	public static JSONNode ToJson(BaseNJSEvent njsEvent, IList<V4CommonData.NJSEvent> njsEventsCommonData)
	{
		JSONObject obj = new JSONObject { ["b"] = njsEvent.JsonTime };
		V4CommonData.NJSEvent item = V4CommonData.NJSEvent.FromBaseNJSEvent(njsEvent);
		obj["i"] = njsEventsCommonData.IndexOf(item);
		return obj;
	}
}
