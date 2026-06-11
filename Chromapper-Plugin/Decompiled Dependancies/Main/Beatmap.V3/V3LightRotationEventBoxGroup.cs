using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightRotationEventBoxGroup
{
	public static BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> GetFromJson(JSONNode node)
	{
		return new BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt,
			Events = new List<BaseLightRotationEventBox>(BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightRotationEventBox.GetFromJson(x)).ToList()),
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson<T>(BaseLightRotationEventBoxGroup<T> group) where T : BaseLightRotationEventBox
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		JSONArray jSONArray = new JSONArray();
		foreach (T @event in group.Events)
		{
			jSONArray.Add(V3LightRotationEventBox.ToJson(@event));
		}
		jSONNode["e"] = jSONArray;
		group.CustomData = group.SaveCustom();
		if (!group.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = group.CustomData;
		return jSONNode;
	}
}
