using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4Waypoint
{
	public static BaseWaypoint GetFromJson(JSONNode node, IList<V4CommonData.Waypoint> waypointsCommonData)
	{
		BaseWaypoint obj = new BaseWaypoint
		{
			JsonTime = node["b"].AsFloat
		};
		int asInt = node["i"].AsInt;
		V4CommonData.Waypoint waypoint = waypointsCommonData[asInt];
		obj.PosX = waypoint.PosX;
		obj.PosY = waypoint.PosY;
		obj.OffsetDirection = waypoint.OffsetDirection;
		return obj;
	}

	public static JSONNode ToJson(BaseWaypoint waypoint, IList<V4CommonData.Waypoint> waypointsCommonData)
	{
		JSONObject obj = new JSONObject { ["b"] = waypoint.JsonTime };
		V4CommonData.Waypoint item = V4CommonData.Waypoint.FromBaseWayPoint(waypoint);
		obj["i"] = waypointsCommonData.IndexOf(item);
		return obj;
	}
}
