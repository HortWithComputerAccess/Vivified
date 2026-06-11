using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4LightColorEventBoxGroup
{
	public static BaseLightColorEventBoxGroup<BaseLightColorEventBox> GetFromJson(JSONNode node, IList<BaseIndexFilter> indexFilters, IList<V4CommonData.LightColorEventBox> lightColorEventBoxesCommonData, IList<V4CommonData.LightColorEvent> lightColorEventsCommonData)
	{
		BaseLightColorEventBoxGroup<BaseLightColorEventBox> obj = new BaseLightColorEventBoxGroup<BaseLightColorEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt
		};
		JSONArray asArray = node["e"].AsArray;
		obj.Events = asArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> x)
		{
			JSONNode value = x.Value;
			BaseLightColorEventBox baseLightColorEventBox = new BaseLightColorEventBox();
			int asInt = value["f"].AsInt;
			baseLightColorEventBox.IndexFilter = (BaseIndexFilter)indexFilters[asInt].Clone();
			int asInt2 = value["e"].AsInt;
			V4CommonData.LightColorEventBox lightColorEventBox = lightColorEventBoxesCommonData[asInt2];
			baseLightColorEventBox.BeatDistribution = lightColorEventBox.BeatDistribution;
			baseLightColorEventBox.BeatDistributionType = lightColorEventBox.BeatDistributionType;
			baseLightColorEventBox.BrightnessDistribution = lightColorEventBox.BrightnessDistribution;
			baseLightColorEventBox.BrightnessDistributionType = lightColorEventBox.BrightnessDistributionType;
			baseLightColorEventBox.BrightnessAffectFirst = lightColorEventBox.BrightnessAffectFirst;
			baseLightColorEventBox.Easing = lightColorEventBox.Easing;
			baseLightColorEventBox.Events = value["l"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> keyValuePair)
			{
				JSONNode value2 = keyValuePair.Value;
				BaseLightColorBase obj2 = new BaseLightColorBase
				{
					JsonTime = value2["b"].AsFloat
				};
				int asInt3 = value2["i"].AsInt;
				V4CommonData.LightColorEvent lightColorEvent = lightColorEventsCommonData[asInt3];
				obj2.Color = lightColorEvent.Color;
				obj2.Brightness = lightColorEvent.Brightness;
				obj2.TransitionType = lightColorEvent.TransitionType;
				obj2.Frequency = lightColorEvent.Frequency;
				obj2.StrobeBrightness = lightColorEvent.StrobeBrightness;
				obj2.StrobeFade = lightColorEvent.StrobeFade;
				obj2.Easing = lightColorEvent.Easing;
				return obj2;
			}).ToArray();
			return baseLightColorEventBox;
		}).ToList();
		return obj;
	}

	public static JSONNode ToJson(BaseLightColorEventBoxGroup<BaseLightColorEventBox> group, IList<V4CommonData.IndexFilter> indexFiltersCommonData, IList<V4CommonData.LightColorEventBox> lightColorEventBoxesCommonData, IList<V4CommonData.LightColorEvent> lightColorEventsCommonData)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		jSONNode["t"] = 1;
		JSONArray jSONArray = new JSONArray();
		foreach (BaseLightColorEventBox @event in group.Events)
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject["f"] = indexFiltersCommonData.IndexOf(V4CommonData.IndexFilter.FromBaseIndexFilter(@event.IndexFilter));
			jSONObject["e"] = lightColorEventBoxesCommonData.IndexOf(V4CommonData.LightColorEventBox.FromBaseLightColorEventBox(@event));
			JSONArray jSONArray2 = new JSONArray();
			BaseLightColorBase[] events = @event.Events;
			foreach (BaseLightColorBase baseLightColorBase in events)
			{
				JSONObject jSONObject2 = new JSONObject();
				jSONObject2["b"] = baseLightColorBase.JsonTime;
				jSONObject2["i"] = lightColorEventsCommonData.IndexOf(V4CommonData.LightColorEvent.FromBaseLightColorEvent(baseLightColorBase));
				jSONArray2.Add(jSONObject2);
			}
			jSONObject["l"] = jSONArray2;
			jSONArray.Add(jSONObject);
		}
		jSONNode["e"] = jSONArray;
		return jSONNode;
	}
}
