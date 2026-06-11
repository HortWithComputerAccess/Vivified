using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2;

public class V2Event
{
	public const string CustomKeyTrack = "_track";

	public const string CustomKeyColor = "_color";

	public const string CustomKeyPropID = "_propID";

	public const string CustomKeyLightID = "_lightID";

	public const string CustomKeyLerpType = "_lerpType";

	public const string CustomKeyEasing = "_easing";

	public const string CustomKeyLightGradient = "_lightGradient";

	public const string CustomKeyStep = "_step";

	public const string CustomKeyProp = "_prop";

	public const string CustomKeySpeed = "_speed";

	public const string CustomKeyRingRotation = "_rotation";

	public const string CustomKeyStepMult = "_stepMult";

	public const string CustomKeyPropMult = "_propMult";

	public const string CustomKeySpeedMult = "_speedMult";

	public const string CustomKeyPreciseSpeed = "_preciseSpeed";

	public const string CustomKeyDirection = "_direction";

	public const string CustomKeyLockRotation = "_lockPosition";

	public const string CustomKeyLaneRotation = "_rotation";

	public const string CustomKeyNameFilter = "_nameFilter";

	public static BaseEvent GetFromJson(JSONNode node)
	{
		return new BaseEvent
		{
			JsonTime = BaseItem.GetRequiredNode(node, "_time").AsFloat,
			Type = BaseItem.GetRequiredNode(node, "_type").AsInt,
			Value = BaseItem.GetRequiredNode(node, "_value").AsInt,
			FloatValue = (node.HasKey("_floatValue") ? node["_floatValue"].AsFloat : 1f),
			CustomData = node["_customData"]
		};
	}

	public static JSONNode ToJson(BaseEvent evt)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_time"] = evt.JsonTime;
		jSONNode["_type"] = evt.Type;
		jSONNode["_value"] = evt.Value;
		jSONNode["_floatValue"] = evt.FloatValue;
		evt.CustomData = evt.SaveCustom();
		if (!evt.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["_customData"] = evt.CustomData;
		return jSONNode;
	}
}
