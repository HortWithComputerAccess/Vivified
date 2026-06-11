using System.Linq;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V3;

public class V3BombNote
{
	public static BaseNote GetFromJson(JSONNode node, bool customFake = false)
	{
		return new BaseNote
		{
			JsonTime = node["b"].AsFloat,
			PosX = node["x"].AsInt,
			PosY = node["y"].AsInt,
			Type = 3,
			CustomData = node["customData"],
			CustomFake = customFake
		};
	}

	public static JSONNode ToJson(BaseNote note)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["b"] = note.JsonTime;
		jSONNode["x"] = note.PosX;
		jSONNode["y"] = note.PosY;
		note.CustomData = note.SaveCustom();
		if (!note.CustomData.Children.Any())
		{
			return jSONNode;
		}
		jSONNode["customData"] = note.CustomData;
		return jSONNode;
	}
}
