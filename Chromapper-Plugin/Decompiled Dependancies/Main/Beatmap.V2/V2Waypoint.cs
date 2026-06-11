using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public static class V2Waypoint
{
	public static BaseWaypoint GetFromJson(JSONNode node)
	{
		return new BaseWaypoint
		{
			JsonTime = BaseItem.GetRequiredNode(node, "_time").AsFloat,
			PosX = BaseItem.GetRequiredNode(node, "_lineIndex").AsInt,
			PosY = BaseItem.GetRequiredNode(node, "_lineLayer").AsInt,
			OffsetDirection = BaseItem.GetRequiredNode(node, "_offsetDirection").AsInt,
			CustomData = node["_customData"]
		};
	}

	public static JSONNode ToJson(BaseWaypoint waypoint)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_time"] = waypoint.JsonTime;
		jSONNode["_lineIndex"] = waypoint.PosX;
		jSONNode["_lineLayer"] = waypoint.PosY;
		jSONNode["_offsetDirection"] = waypoint.OffsetDirection;
		waypoint.CustomData = waypoint.SaveCustom();
		if (!waypoint.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = waypoint.CustomData;
		return jSONNode;
	}
}
