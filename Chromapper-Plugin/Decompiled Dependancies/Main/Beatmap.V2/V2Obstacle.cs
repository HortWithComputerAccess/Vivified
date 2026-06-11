using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public class V2Obstacle
{
	public const string CustomKeyAnimation = "_animation";

	public const string CustomKeyTrack = "_track";

	public const string CustomKeyColor = "_color";

	public const string CustomKeyCoordinate = "_position";

	public const string CustomKeyWorldRotation = "_rotation";

	public const string CustomKeyLocalRotation = "_localRotation";

	public const string CustomKeySpawnEffect = "_disableSpawnEffect";

	public const string CustomKeyNoteJumpMovementSpeed = "_noteJumpMovementSpeed";

	public const string CustomKeyNoteJumpStartBeatOffset = "_noteJumpStartBeatOffset";

	public const string CustomKeySize = "_scale";

	public static BaseObstacle GetFromJson(JSONNode node)
	{
		return new BaseObstacle
		{
			JsonTime = BaseItem.GetRequiredNode(node, "_time").AsFloat,
			PosX = BaseItem.GetRequiredNode(node, "_lineIndex").AsInt,
			Type = BaseItem.GetRequiredNode(node, "_type").AsInt,
			Duration = BaseItem.GetRequiredNode(node, "_duration").AsFloat,
			Width = BaseItem.GetRequiredNode(node, "_width").AsInt,
			CustomData = node["_customData"]
		};
	}

	public static JSONNode ToJson(BaseObstacle obstacle)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_time"] = obstacle.JsonTime;
		jSONNode["_lineIndex"] = obstacle.PosX;
		jSONNode["_type"] = obstacle.Type;
		jSONNode["_duration"] = obstacle.Duration;
		jSONNode["_width"] = obstacle.Width;
		obstacle.CustomData = obstacle.SaveCustom().Clone();
		if (!obstacle.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = obstacle.CustomData;
		return jSONNode;
	}
}
