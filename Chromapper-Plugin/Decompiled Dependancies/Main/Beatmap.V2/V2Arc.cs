using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public class V2Arc
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

	public const string CustomKeyTailCoordinate = "_tailPosition";

	public static BaseArc GetFromJson(JSONNode node)
	{
		return new BaseArc
		{
			Color = BaseItem.GetRequiredNode(node, "_colorType").AsInt,
			JsonTime = BaseItem.GetRequiredNode(node, "_headTime").AsFloat,
			PosX = BaseItem.GetRequiredNode(node, "_headLineIndex").AsInt,
			PosY = BaseItem.GetRequiredNode(node, "_headLineLayer").AsInt,
			CutDirection = BaseItem.GetRequiredNode(node, "_headCutDirection").AsInt,
			HeadControlPointLengthMultiplier = BaseItem.GetRequiredNode(node, "_headControlPointLengthMultiplier").AsFloat,
			TailJsonTime = BaseItem.GetRequiredNode(node, "_tailTime").AsFloat,
			TailPosX = BaseItem.GetRequiredNode(node, "_tailLineIndex").AsInt,
			TailPosY = BaseItem.GetRequiredNode(node, "_tailLineLayer").AsInt,
			TailCutDirection = BaseItem.GetRequiredNode(node, "_tailCutDirection").AsInt,
			TailControlPointLengthMultiplier = BaseItem.GetRequiredNode(node, "_tailControlPointLengthMultiplier").AsFloat,
			MidAnchorMode = BaseItem.GetRequiredNode(node, "_sliderMidAnchorMode").AsInt,
			CustomData = node["_customData"]
		};
	}

	public static JSONNode ToJson(BaseArc arc)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_colorType"] = arc.Color;
		jSONNode["_headTime"] = arc.JsonTime;
		jSONNode["_headLineIndex"] = arc.PosX;
		jSONNode["_headLineLayer"] = arc.PosY;
		jSONNode["_headCutDirection"] = arc.CutDirection;
		jSONNode["_headControlPointLengthMultiplier"] = arc.HeadControlPointLengthMultiplier;
		jSONNode["_tailTime"] = arc.TailJsonTime;
		jSONNode["_tailLineIndex"] = arc.TailPosX;
		jSONNode["_tailLineLayer"] = arc.TailPosY;
		jSONNode["_tailCutDirection"] = arc.TailCutDirection;
		jSONNode["_tailControlPointLengthMultiplier"] = arc.TailControlPointLengthMultiplier;
		jSONNode["_sliderMidAnchorMode"] = arc.MidAnchorMode;
		arc.CustomData = arc.SaveCustom();
		if (!arc.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = arc.CustomData;
		return jSONNode;
	}
}
