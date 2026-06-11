using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4ColorNote
{
	public static BaseNote GetFromJson(JSONNode node, IList<V4CommonData.Note> notesCommonData)
	{
		BaseNote obj = new BaseNote
		{
			JsonTime = node["b"].AsFloat,
			Rotation = node["r"].AsInt
		};
		int asInt = node["i"].AsInt;
		V4CommonData.Note note = notesCommonData[asInt];
		obj.PosX = note.PosX;
		obj.PosY = note.PosY;
		obj.Color = note.Color;
		obj.CutDirection = note.CutDirection;
		obj.AngleOffset = note.AngleOffset;
		return obj;
	}

	public static JSONNode ToJson(BaseNote note, IList<V4CommonData.Note> notesCommonData)
	{
		JSONObject obj = new JSONObject
		{
			["b"] = note.JsonTime,
			["r"] = note.Rotation
		};
		V4CommonData.Note item = V4CommonData.Note.FromBaseNote(note);
		obj["i"] = notesCommonData.IndexOf(item);
		return obj;
	}
}
