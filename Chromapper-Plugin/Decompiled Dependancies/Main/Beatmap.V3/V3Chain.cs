using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public static class V3Chain
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

	public static BaseChain GetFromJson(JSONNode node, bool customFake = false)
	{
		return new BaseChain
		{
			JsonTime = node["b"].AsFloat,
			Color = node["c"].AsInt,
			PosX = node["x"].AsInt,
			PosY = node["y"].AsInt,
			CutDirection = node["d"].AsInt,
			TailJsonTime = node["tb"].AsFloat,
			TailPosX = node["tx"].AsInt,
			TailPosY = node["ty"].AsInt,
			SliceCount = node["sc"].AsInt,
			Squish = node["s"].AsFloat,
			CustomFake = customFake,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseChain chain)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = chain.JsonTime;
		jSONNode["c"] = chain.Color;
		jSONNode["x"] = chain.PosX;
		jSONNode["y"] = chain.PosY;
		jSONNode["d"] = chain.CutDirection;
		jSONNode["tb"] = chain.TailJsonTime;
		jSONNode["tx"] = chain.TailPosX;
		jSONNode["ty"] = chain.TailPosY;
		jSONNode["sc"] = chain.SliceCount;
		jSONNode["s"] = chain.Squish;
		chain.CustomData = chain.SaveCustom();
		if (!chain.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = chain.CustomData;
		return jSONNode;
	}
}
