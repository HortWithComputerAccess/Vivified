using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3LightTranslationEventBoxGroup
{
	public static BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> GetFromJson(JSONNode node)
	{
		return new BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt,
			Events = new List<BaseLightTranslationEventBox>(BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3LightTranslationEventBox.GetFromJson(x)).ToList()),
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson<T>(BaseLightTranslationEventBoxGroup<T> box) where T : BaseLightTranslationEventBox
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = box.JsonTime;
		jSONNode["g"] = box.ID;
		JSONArray jSONArray = new JSONArray();
		foreach (T @event in box.Events)
		{
			jSONArray.Add(@event.ToJson());
		}
		jSONNode["e"] = jSONArray;
		box.CustomData = box.SaveCustom();
		if (!box.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = box.CustomData;
		return jSONNode;
	}
}
