using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3FxEventsCollection
{
	public static BaseFxEventsCollection GetFromJson(JSONNode node)
	{
		BaseFxEventsCollection baseFxEventsCollection = new BaseFxEventsCollection();
		if (node.HasKey("_il"))
		{
			baseFxEventsCollection.IntFxEvents = node["_il"].AsArray.Linq.Select((KeyValuePair<string, JSONNode> childNode) => V3IntFxEvent.GetFromJson(childNode.Value)).ToArray();
		}
		if (node.HasKey("_fl"))
		{
			baseFxEventsCollection.FloatFxEvents = node["_fl"].AsArray.Linq.Select((KeyValuePair<string, JSONNode> childNode) => V3FloatFxEvent.GetFromJson(childNode.Value)).ToArray();
		}
		return baseFxEventsCollection;
	}

	public static JSONNode ToJson(BaseFxEventsCollection fxEventsCollection)
	{
		JSONObject jSONObject = new JSONObject();
		jSONObject["_il"] = new JSONArray();
		IntFxEventBase[] intFxEvents = fxEventsCollection.IntFxEvents;
		foreach (IntFxEventBase intFxEventBase in intFxEvents)
		{
			jSONObject["_il"].Add(intFxEventBase.ToJson());
		}
		jSONObject["_fl"] = new JSONArray();
		FloatFxEventBase[] floatFxEvents = fxEventsCollection.FloatFxEvents;
		foreach (FloatFxEventBase floatFxEventBase in floatFxEvents)
		{
			jSONObject["_fl"].Add(floatFxEventBase.ToJson());
		}
		return jSONObject;
	}
}
