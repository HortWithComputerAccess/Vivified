using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3BasicEvent
{
	public const string CustomKeyTrack = "track";

	public const string CustomKeyColor = "color";

	public const string CustomKeyPropID = "propID";

	public const string CustomKeyLightID = "lightID";

	public const string CustomKeyLerpType = "lerpType";

	public const string CustomKeyEasing = "easing";

	public const string CustomKeyLightGradient = "lightGradient";

	public const string CustomKeyStep = "step";

	public const string CustomKeyProp = "prop";

	public const string CustomKeySpeed = "speed";

	public const string CustomKeyRingRotation = "rotation";

	public const string CustomKeyStepMult = "stepMult";

	public const string CustomKeyPropMult = "propMult";

	public const string CustomKeySpeedMult = "speedMult";

	public const string CustomKeyPreciseSpeed = "preciseSpeed";

	public const string CustomKeyDirection = "direction";

	public const string CustomKeyLockRotation = "lockRotation";

	public const string CustomKeyLaneRotation = "rotation";

	public const string CustomKeyNameFilter = "nameFilter";

	public static BaseEvent GetFromJson(JSONNode node)
	{
		return new BaseEvent
		{
			JsonTime = node["b"].AsFloat,
			Type = node["et"].AsInt,
			Value = node["i"].AsInt,
			FloatValue = node["f"].AsFloat,
			CustomData = node["customData"]
		};
	}

	public static JSONNode ToJson(BaseEvent evt)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = evt.JsonTime;
		jSONNode["et"] = evt.Type;
		jSONNode["i"] = evt.Value;
		jSONNode["f"] = evt.FloatValue;
		evt.CustomData = evt.SaveCustom();
		if (!evt.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = evt.CustomData;
		return jSONNode;
	}
}
