using SimpleJSON;
using UnityEngine;

namespace Beatmap.Animations;

public class PointDataParsers
{
	public static float ParseFloat(JSONArray data, ref int i)
	{
		i++;
		return data[0];
	}

	public static Color ParseColor(JSONArray data, ref int i)
	{
		Color color;
		if (data[i].IsString)
		{
			i++;
			color = data[0].Value switch
			{
				"baseNote0Color" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.RedNoteColor : DefaultColors.LeftNote, 
				"baseNote1Color" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.BlueNoteColor : DefaultColors.RightNote, 
				"baseEnvironmentColor0" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.RedColor : DefaultColors.Left, 
				"baseEnvironmentColor1" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.BlueColor : DefaultColors.Right, 
				"baseEnvironmentColorW" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.WhiteColor : DefaultColors.White, 
				"baseEnvironmentColor0Boost" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.RedBoostColor : DefaultColors.Left, 
				"baseEnvironmentColor1Boost" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.BlueBoostColor : DefaultColors.Right, 
				"baseEnvironmentColorWBoost" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.WhiteBoostColor : DefaultColors.White, 
				"baseObstaclesColor" => (LoadInitialMap.Platform != null) ? LoadInitialMap.Platform.Colors.ObstacleColor : DefaultColors.White, 
				_ => DefaultColors.White, 
			};
		}
		else
		{
			i += 4;
			color = new Color(data[0], data[1], data[2], data[3]);
		}
		if (data[i] is JSONArray jSONArray)
		{
			i++;
			int i2 = 0;
			Color color2 = ParseColor(jSONArray, ref i2);
			color = jSONArray[i2].Value switch
			{
				"opAdd" => color + color2, 
				"opSub" => color - color2, 
				"opMul" => color * color2, 
				"opDiv" => new Color(color.r / color2.r, color.g / color2.g, color.b / color2.b, color.a / color2.a), 
				_ => color, 
			};
		}
		return color;
	}

	public static Vector3 ParseVector3(JSONArray data, ref int i)
	{
		i += 3;
		return new Vector3(data[0], data[1], data[2]);
	}

	public static Quaternion ParseQuaternion(JSONArray data, ref int i)
	{
		i += 3;
		return Quaternion.Euler(data[0], data[1], data[2]);
	}
}
