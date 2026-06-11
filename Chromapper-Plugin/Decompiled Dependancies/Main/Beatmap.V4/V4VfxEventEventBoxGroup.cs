using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4VfxEventEventBoxGroup
{
	public static BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> GetFromJson(JSONNode node, IList<BaseIndexFilter> indexFilters, IList<V4CommonData.FxEventBox> fxEventBoxesCommonData, IList<V4CommonData.FloatFxEvent> floatFxEventsCommonData)
	{
		return new BaseVfxEventEventBoxGroup<BaseVfxEventEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt,
			Events = node["e"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> x)
			{
				JSONNode value = x.Value;
				BaseVfxEventEventBox baseVfxEventEventBox = new BaseVfxEventEventBox();
				int asInt = value["f"].AsInt;
				baseVfxEventEventBox.IndexFilter = (BaseIndexFilter)indexFilters[asInt].Clone();
				int asInt2 = value["e"].AsInt;
				V4CommonData.FxEventBox fxEventBox = fxEventBoxesCommonData[asInt2];
				baseVfxEventEventBox.BeatDistribution = fxEventBox.BeatDistribution;
				baseVfxEventEventBox.BeatDistributionType = fxEventBox.BeatDistributionType;
				baseVfxEventEventBox.VfxDistribution = fxEventBox.FxDistribution;
				baseVfxEventEventBox.VfxDistributionType = fxEventBox.FxDistributionType;
				baseVfxEventEventBox.VfxAffectFirst = fxEventBox.FxAffectFirst;
				baseVfxEventEventBox.Easing = fxEventBox.Easing;
				baseVfxEventEventBox.FloatFxEvents = value["l"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> y)
				{
					JSONNode value2 = y.Value;
					FloatFxEventBase obj = new FloatFxEventBase
					{
						JsonTime = value2["b"].AsFloat
					};
					int asInt3 = value2["i"].AsInt;
					V4CommonData.FloatFxEvent floatFxEvent = floatFxEventsCommonData[asInt3];
					obj.Value = floatFxEvent.Value;
					obj.UsePreviousEventValue = floatFxEvent.TransitionType;
					obj.Easing = floatFxEvent.Easing;
					return obj;
				}).ToList();
				return baseVfxEventEventBox;
			}).ToList(),
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseVfxEventEventBoxGroup<BaseVfxEventEventBox> group, IList<V4CommonData.IndexFilter> indexFiltersCommonData, IList<V4CommonData.FxEventBox> fxEventBoxesCommonData, IList<V4CommonData.FloatFxEvent> floatFxEventsCommonData)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		jSONNode["t"] = 4;
		JSONArray jSONArray = new JSONArray();
		foreach (BaseVfxEventEventBox @event in group.Events)
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject["f"] = indexFiltersCommonData.IndexOf(V4CommonData.IndexFilter.FromBaseIndexFilter(@event.IndexFilter));
			jSONObject["e"] = fxEventBoxesCommonData.IndexOf(V4CommonData.FxEventBox.FromBaseFxEventBox(@event));
			JSONArray jSONArray2 = new JSONArray();
			foreach (FloatFxEventBase floatFxEvent in @event.FloatFxEvents)
			{
				JSONObject jSONObject2 = new JSONObject();
				jSONObject2["b"] = floatFxEvent.JsonTime;
				jSONObject2["i"] = floatFxEventsCommonData.IndexOf(V4CommonData.FloatFxEvent.FromFloatFxEventBase(floatFxEvent));
				jSONArray2.Add(jSONObject2);
			}
			jSONObject["l"] = jSONArray2;
			jSONArray.Add(jSONObject);
		}
		jSONNode["e"] = jSONArray;
		return jSONNode;
	}
}
