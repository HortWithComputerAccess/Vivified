using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3ColorNote
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

	public const string CustomKeyDirection = "cutDirection";

	public static BaseNote GetFromJson(JSONNode node, bool? customFake = false)
	{
		BaseNote baseNote = new BaseNote();
		baseNote.JsonTime = node["b"].AsFloat;
		baseNote.PosX = node["x"].AsInt;
		baseNote.PosY = node["y"].AsInt;
		baseNote.AngleOffset = node["a"].AsInt;
		baseNote.Color = node["c"].AsInt;
		baseNote.CutDirection = node["d"].AsInt;
		baseNote.CustomData = node["customData"];
		if (customFake.HasValue)
		{
			baseNote.CustomFake = customFake.Value;
		}
		return baseNote;
	}

	public static JSONNode ToJson(BaseNote note)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = note.JsonTime;
		jSONNode["x"] = note.PosX;
		jSONNode["y"] = note.PosY;
		jSONNode["a"] = note.AngleOffset;
		jSONNode["c"] = note.Color;
		jSONNode["d"] = note.CutDirection;
		note.CustomData = note.SaveCustom();
		if (!note.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = note.CustomData;
		return jSONNode;
	}
}
