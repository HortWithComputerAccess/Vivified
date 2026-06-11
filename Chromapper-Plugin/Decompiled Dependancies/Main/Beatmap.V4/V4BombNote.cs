using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4BombNote
{
	public static BaseNote GetFromJson(JSONNode node, IList<V4CommonData.Bomb> bombsCommonData)
	{
		BaseNote obj = new BaseNote
		{
			JsonTime = node["b"].AsFloat,
			Rotation = node["r"].AsInt
		};
		int asInt = node["i"].AsInt;
		V4CommonData.Bomb bomb = bombsCommonData[asInt];
		obj.PosX = bomb.PosX;
		obj.PosY = bomb.PosY;
		obj.Type = 3;
		return obj;
	}

	public static JSONNode ToJson(BaseNote note, IList<V4CommonData.Bomb> bombsCommonData)
	{
		JSONObject obj = new JSONObject
		{
			["b"] = note.JsonTime,
			["r"] = note.Rotation
		};
		V4CommonData.Bomb item = V4CommonData.Bomb.FromBaseNote(note);
		obj["i"] = bombsCommonData.IndexOf(item);
		return obj;
	}
}
