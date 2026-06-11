using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3Waypoint
{
	public static BaseWaypoint GetFromJson(JSONNode node)
	{
		return new BaseWaypoint
		{
			JsonTime = node["b"].AsFloat,
			PosX = node["x"].AsInt,
			PosY = node["y"].AsInt,
			OffsetDirection = node["d"].AsInt,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseWaypoint waypoint)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = waypoint.JsonTime;
		jSONNode["x"] = waypoint.PosX;
		jSONNode["y"] = waypoint.PosY;
		jSONNode["d"] = waypoint.OffsetDirection;
		waypoint.CustomData = waypoint.SaveCustom();
		if (!waypoint.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = waypoint.CustomData;
		return jSONNode;
	}
}
