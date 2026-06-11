using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4LightRotationEventBoxGroup
{
	public static BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> GetFromJson(JSONNode node, IList<BaseIndexFilter> indexFilters, IList<V4CommonData.LightRotationEventBox> lightRotationEventBoxesCommonData, IList<V4CommonData.LightRotationEvent> lightRotationEventsCommonData)
	{
		BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> obj = new BaseLightRotationEventBoxGroup<BaseLightRotationEventBox>
		{
			JsonTime = node["b"].AsFloat,
			ID = node["g"].AsInt
		};
		JSONArray asArray = node["e"].AsArray;
		obj.Events = asArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> x)
		{
			JSONNode value = x.Value;
			BaseLightRotationEventBox baseLightRotationEventBox = new BaseLightRotationEventBox();
			int asInt = value["f"].AsInt;
			baseLightRotationEventBox.IndexFilter = (BaseIndexFilter)indexFilters[asInt].Clone();
			int asInt2 = value["e"].AsInt;
			V4CommonData.LightRotationEventBox lightRotationEventBox = lightRotationEventBoxesCommonData[asInt2];
			baseLightRotationEventBox.BeatDistribution = lightRotationEventBox.BeatDistribution;
			baseLightRotationEventBox.BeatDistributionType = lightRotationEventBox.BeatDistributionType;
			baseLightRotationEventBox.RotationDistribution = lightRotationEventBox.RotationDistribution;
			baseLightRotationEventBox.RotationDistributionType = lightRotationEventBox.RotationDistributionType;
			baseLightRotationEventBox.RotationAffectFirst = lightRotationEventBox.RotationAffectFirst;
			baseLightRotationEventBox.Easing = lightRotationEventBox.Easing;
			baseLightRotationEventBox.Axis = lightRotationEventBox.Axis;
			baseLightRotationEventBox.Flip = lightRotationEventBox.Flip;
			baseLightRotationEventBox.Events = value["l"].AsArray.Linq.Select(delegate(KeyValuePair<string, JSONNode> keyValuePair)
			{
				JSONNode value2 = keyValuePair.Value;
				BaseLightRotationBase obj2 = new BaseLightRotationBase
				{
					JsonTime = value2["b"].AsFloat
				};
				int asInt3 = value2["i"].AsInt;
				V4CommonData.LightRotationEvent lightRotationEvent = lightRotationEventsCommonData[asInt3];
				obj2.Rotation = lightRotationEvent.Rotation;
				obj2.UsePrevious = lightRotationEvent.TransitionType;
				obj2.Direction = lightRotationEvent.Direction;
				obj2.Loop = lightRotationEvent.Loop;
				obj2.EaseType = lightRotationEvent.Easing;
				return obj2;
			}).ToArray();
			return baseLightRotationEventBox;
		}).ToList();
		return obj;
	}

	public static JSONNode ToJson(BaseLightRotationEventBoxGroup<BaseLightRotationEventBox> group, IList<V4CommonData.IndexFilter> indexFiltersCommonData, IList<V4CommonData.LightRotationEventBox> lightRotationEventBoxesCommonData, IList<V4CommonData.LightRotationEvent> lightRotationEventsCommonData)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = group.JsonTime;
		jSONNode["g"] = group.ID;
		jSONNode["t"] = 2;
		JSONArray jSONArray = new JSONArray();
		foreach (BaseLightRotationEventBox @event in group.Events)
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject["f"] = indexFiltersCommonData.IndexOf(V4CommonData.IndexFilter.FromBaseIndexFilter(@event.IndexFilter));
			jSONObject["e"] = lightRotationEventBoxesCommonData.IndexOf(V4CommonData.LightRotationEventBox.FromBaseLightRotationEventBox(@event));
			JSONArray jSONArray2 = new JSONArray();
			BaseLightRotationBase[] events = @event.Events;
			foreach (BaseLightRotationBase baseLightRotationBase in events)
			{
				JSONObject jSONObject2 = new JSONObject();
				jSONObject2["b"] = baseLightRotationBase.JsonTime;
				jSONObject2["i"] = lightRotationEventsCommonData.IndexOf(V4CommonData.LightRotationEvent.FromBaseLightRotationEvent(baseLightRotationBase));
				jSONArray2.Add(jSONObject2);
			}
			jSONObject["l"] = jSONArray2;
			jSONArray.Add(jSONObject);
		}
		jSONNode["e"] = jSONArray;
		return jSONNode;
	}
}
