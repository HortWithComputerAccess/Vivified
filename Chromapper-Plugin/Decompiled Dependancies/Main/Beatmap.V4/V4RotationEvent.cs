using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4RotationEvent
{
	public static BaseEvent GetFromJson(JSONNode node, IList<V4CommonData.RotationEvent> rotationsCommonData)
	{
		BaseEvent obj = new BaseEvent
		{
			JsonTime = node["b"].AsFloat
		};
		int asInt = node["i"].AsInt;
		V4CommonData.RotationEvent rotationEvent = rotationsCommonData[asInt];
		obj.Type = ((rotationEvent.Type == 0) ? 14 : 15);
		obj.Rotation = rotationEvent.Rotation;
		return obj;
	}

	public static JSONNode ToJson(BaseEvent evt, IList<V4CommonData.RotationEvent> rotationsCommonData)
	{
		JSONObject obj = new JSONObject { ["b"] = evt.JsonTime };
		V4CommonData.RotationEvent item = V4CommonData.RotationEvent.FromBaseEvent(evt);
		obj["i"] = rotationsCommonData.IndexOf(item);
		return obj;
	}
}
