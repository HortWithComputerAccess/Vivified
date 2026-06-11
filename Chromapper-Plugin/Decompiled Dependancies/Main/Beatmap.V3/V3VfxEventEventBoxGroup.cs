using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3VfxEventEventBoxGroup
{
	public static BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> GetFromJson(JSONNode node, IList<FloatFxEventBase> floatFxEvents)
	{
		return new BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt,
			Type = node["t"].AsInt,
			Events = new List<BaseVfxEventEventBox>(BaseItem.GetRequiredNode(node, "e").AsArray.Linq.Select((KeyValuePair<string, JSONNode> x) => V3VfxEventEventBox.GetFromJson(x.Value, floatFxEvents)).ToList()),
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> vfxGroup, IList<FloatFxEventBase> floatFxEvents)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = vfxGroup.JsonTime;
		jSONNode["g"] = vfxGroup.ID;
		jSONNode["t"] = vfxGroup.Type;
		JSONArray jSONArray = new JSONArray();
		foreach (BaseVfxEventEventBox @event in vfxGroup.Events)
		{
			jSONArray.Add(V3VfxEventEventBox.ToJson(@event, floatFxEvents));
		}
		jSONNode["e"] = jSONArray;
		vfxGroup.CustomData = vfxGroup.SaveCustom();
		if (!vfxGroup.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = vfxGroup.CustomData;
		return jSONNode;
	}
}
