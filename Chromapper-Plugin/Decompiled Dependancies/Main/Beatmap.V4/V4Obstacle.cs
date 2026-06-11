using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4Obstacle
{
	public static BaseObstacle GetFromJson(JSONNode node, IList<V4CommonData.Obstacle> obstaclesCommonData)
	{
		BaseObstacle obj = new BaseObstacle
		{
			JsonTime = node["b"].AsFloat,
			Rotation = node["r"].AsInt
		};
		int asInt = node["i"].AsInt;
		V4CommonData.Obstacle obstacle = obstaclesCommonData[asInt];
		obj.PosX = obstacle.PosX;
		obj.PosY = obstacle.PosY;
		obj.Duration = obstacle.Duration;
		obj.Width = obstacle.Width;
		obj.Height = obstacle.Height;
		return obj;
	}

	public static JSONNode ToJson(BaseObstacle obstacle, IList<V4CommonData.Obstacle> obstaclesCommonData)
	{
		JSONObject obj = new JSONObject
		{
			["b"] = obstacle.JsonTime,
			["r"] = obstacle.Rotation
		};
		V4CommonData.Obstacle item = V4CommonData.Obstacle.FromBaseObstacle(obstacle);
		obj["i"] = obstaclesCommonData.IndexOf(item);
		return obj;
	}
}
