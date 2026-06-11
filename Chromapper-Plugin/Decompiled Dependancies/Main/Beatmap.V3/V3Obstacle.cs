using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3Obstacle
{
	public const string CustomKeyAnimation = "animation";

	public const string CustomKeyTrack = "track";

	public const string CustomKeyColor = "color";

	public const string CustomKeyCoordinate = "coordinates";

	public const string CustomKeyWorldRotation = "worldRotation";

	public const string CustomKeyLocalRotation = "localRotation";

	public const string CustomKeySpawnEffect = "spawnEffect";

	public const string CustomKeyNoteJumpMovementSpeed = "noteJumpMovementSpeed";

	public const string CustomKeyNoteJumpStartBeatOffset = "noteJumpStartBeatOffset";

	public const string CustomKeySize = "size";

	public static BaseObstacle GetFromJson(JSONNode node, bool customFake = false)
	{
		return new BaseObstacle
		{
			JsonTime = node["b"].AsFloat,
			PosX = node["x"].AsInt,
			PosY = node["y"].AsInt,
			Duration = node["d"].AsFloat,
			Width = node["w"].AsInt,
			Height = node["h"].AsInt,
			CustomData = node["customData"],
			CustomFake = customFake
		};
	}

	public static JSONNode ToJson(BaseObstacle obstacle)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = obstacle.JsonTime;
		jSONNode["x"] = obstacle.PosX;
		jSONNode["y"] = obstacle.PosY;
		jSONNode["d"] = obstacle.Duration;
		jSONNode["w"] = obstacle.Width;
		jSONNode["h"] = obstacle.Height;
		obstacle.CustomData = obstacle.SaveCustom();
		if (!obstacle.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = obstacle.CustomData;
		return jSONNode;
	}
}
