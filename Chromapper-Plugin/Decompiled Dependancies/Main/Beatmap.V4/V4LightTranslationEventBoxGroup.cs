using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4LightTranslationEventBoxGroup
{
	public static BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> GetFromJson(JSONNode node, IList<BaseIndexFilter> indexFilters, IList<V4CommonData.LightTranslationEventBox> lightTranslationEventBoxesCommonData, IList<V4CommonData.LightTranslationEvent> lightTranslationEventsCommonData)
	{
		BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> obj = new BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt
		};
		JSONArray asArray = node["e"].AsArray;
		obj.Events = asArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> x)
		{
			JSONNode value = x.Value;
			BaseLightTranslationEventBox baseLightTranslationEventBox = new BaseLightTranslationEventBox();
			int asInt = value["f"].AsInt;
			baseLightTranslationEventBox.IndexFilter = (BaseIndexFilter)indexFilters[asInt].Clone();
			int asInt2 = value["e"].AsInt;
			V4CommonData.LightTranslationEventBox lightTranslationEventBox = lightTranslationEventBoxesCommonData[asInt2];
			baseLightTranslationEventBox.BeatDistribution = lightTranslationEventBox.BeatDistribution;
			baseLightTranslationEventBox.BeatDistributionType = lightTranslationEventBox.BeatDistributionType;
			baseLightTranslationEventBox.TranslationDistribution = lightTranslationEventBox.TranslationDistribution;
			baseLightTranslationEventBox.TranslationDistributionType = lightTranslationEventBox.TranslationDistributionType;
			baseLightTranslationEventBox.TranslationAffectFirst = lightTranslationEventBox.TranslationAffectFirst;
			baseLightTranslationEventBox.Easing = lightTranslationEventBox.Easing;
			baseLightTranslationEventBox.Axis = lightTranslationEventBox.Axis;
			baseLightTranslationEventBox.Flip = lightTranslationEventBox.Flip;
			baseLightTranslationEventBox.Events = value["l"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> y)
			{
				JSONNode value2 = y.Value;
				BaseLightTranslationBase obj2 = new BaseLightTranslationBase
				{
					JsonTime = value2["b"].AsFloat
				};
				int asInt3 = value2["i"].AsInt;
				V4CommonData.LightTranslationEvent lightTranslationEvent = lightTranslationEventsCommonData[asInt3];
				obj2.Translation = lightTranslationEvent.Translation;
				obj2.UsePrevious = lightTranslationEvent.TransitionType;
				obj2.EaseType = lightTranslationEvent.Easing;
				return obj2;
			}).ToArray();
			return baseLightTranslationEventBox;
		}).ToList();
		return obj;
	}

	public static JSONNode ToJson(BaseLightTranslationEventBoxGroup<BaseLightTranslationEventBox> group, IList<V4CommonData.IndexFilter> indexFiltersCommonData, IList<V4CommonData.LightTranslationEventBox> lightTranslationEventBoxesCommonData, IList<V4CommonData.LightTranslationEvent> lightTranslationEventsCommonData)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		jSONNode["t"] = 3;
		JSONArray jSONArray = new JSONArray();
		foreach (BaseLightTranslationEventBox @event in group.Events)
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject["f"] = indexFiltersCommonData.IndexOf(V4CommonData.IndexFilter.FromBaseIndexFilter(@event.IndexFilter));
			jSONObject["e"] = lightTranslationEventBoxesCommonData.IndexOf(V4CommonData.LightTranslationEventBox.FromBaseLightTranslationEventBox(@event));
			JSONArray jSONArray2 = new JSONArray();
			BaseLightTranslationBase[] events = @event.Events;
			foreach (BaseLightTranslationBase baseLightTranslationBase in events)
			{
				JSONObject jSONObject2 = new JSONObject();
				jSONObject2["b"] = baseLightTranslationBase.JsonTime;
				jSONObject2["i"] = lightTranslationEventsCommonData.IndexOf(V4CommonData.LightTranslationEvent.FromBaseLightTranslationEvent(baseLightTranslationBase));
				jSONArray2.Add(jSONObject2);
			}
			jSONObject["l"] = jSONArray2;
			jSONArray.Add(jSONObject);
		}
		jSONNode["e"] = jSONArray;
		return jSONNode;
	}
}
