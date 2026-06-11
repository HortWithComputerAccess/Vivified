using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3Arc
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

	public const string CustomKeyTailCoordinate = "tailCoordinates";

	public static BaseArc GetFromJson(JSONNode node)
	{
		return new BaseArc
		{
			JsonTime = node["b"].AsFloat,
			Color = node["c"].AsInt,
			PosX = node["x"].AsInt,
			PosY = node["y"].AsInt,
			CutDirection = node["d"].AsInt,
			HeadControlPointLengthMultiplier = node["mu"].AsFloat,
			TailJsonTime = node["tb"].AsFloat,
			TailPosX = node["tx"].AsInt,
			TailPosY = node["ty"].AsInt,
			TailCutDirection = node["tc"].AsInt,
			TailControlPointLengthMultiplier = node["tmu"].AsFloat,
			MidAnchorMode = node["m"].AsInt,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseArc arc)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = arc.JsonTime;
		jSONNode["c"] = arc.Color;
		jSONNode["x"] = arc.PosX;
		jSONNode["y"] = arc.PosY;
		jSONNode["d"] = arc.CutDirection;
		jSONNode["mu"] = arc.HeadControlPointLengthMultiplier;
		jSONNode["tb"] = arc.TailJsonTime;
		jSONNode["tx"] = arc.TailPosX;
		jSONNode["ty"] = arc.TailPosY;
		jSONNode["tc"] = arc.TailCutDirection;
		jSONNode["tmu"] = arc.TailControlPointLengthMultiplier;
		jSONNode["m"] = arc.MidAnchorMode;
		arc.CustomData = arc.SaveCustom();
		if (!arc.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = arc.CustomData;
		return jSONNode;
	}
}
