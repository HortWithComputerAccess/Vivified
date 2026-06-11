using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public class V2Note
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

	public const string CustomKeyDirection = "_cutDirection";

	public static BaseNote GetFromJson(JSONNode node)
	{
		return new BaseNote
		{
			JsonTime = BaseItem.GetRequiredNode(node, "_time").AsFloat,
			PosX = BaseItem.GetRequiredNode(node, "_lineIndex").AsInt,
			PosY = BaseItem.GetRequiredNode(node, "_lineLayer").AsInt,
			Type = BaseItem.GetRequiredNode(node, "_type").AsInt,
			CutDirection = BaseItem.GetRequiredNode(node, "_cutDirection").AsInt,
			CustomData = node["_customData"]
		};
	}

	public static JSONNode ToJson(BaseNote note)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_time"] = note.JsonTime;
		jSONNode["_lineIndex"] = note.PosX;
		jSONNode["_lineLayer"] = note.PosY;
		jSONNode["_type"] = note.Type;
		jSONNode["_cutDirection"] = note.CutDirection;
		note.CustomData = note.SaveCustom();
		if (!note.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = note.CustomData;
		return jSONNode;
	}
}
