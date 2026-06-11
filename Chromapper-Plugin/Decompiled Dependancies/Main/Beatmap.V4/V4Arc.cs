using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4Arc
{
	public static BaseArc GetFromJson(JSONNode node, IList<V4CommonData.Note> notesCommonData, IList<V4CommonData.Arc> arcsCommonData)
	{
		BaseArc baseArc = new BaseArc();
		baseArc.JsonTime = node["hb"].AsFloat;
		baseArc.TailJsonTime = node["tb"].AsFloat;
		baseArc.Rotation = node["hr"].AsInt;
		baseArc.Rotation = node["tr"].AsInt;
		int asInt = node["hi"].AsInt;
		V4CommonData.Note note = notesCommonData[asInt];
		baseArc.PosX = note.PosX;
		baseArc.PosY = note.PosY;
		baseArc.Color = note.Color;
		baseArc.CutDirection = note.CutDirection;
		baseArc.AngleOffset = note.AngleOffset;
		int asInt2 = node["ti"].AsInt;
		V4CommonData.Note note2 = notesCommonData[asInt2];
		baseArc.TailPosX = note2.PosX;
		baseArc.TailPosY = note2.PosY;
		baseArc.TailCutDirection = note2.CutDirection;
		int asInt3 = node["ai"].AsInt;
		V4CommonData.Arc arc = arcsCommonData[asInt3];
		baseArc.HeadControlPointLengthMultiplier = arc.HeadControlPointLengthMultiplier;
		baseArc.TailControlPointLengthMultiplier = arc.TailControlPointLengthMultiplier;
		baseArc.MidAnchorMode = arc.MidAnchorMode;
		return baseArc;
	}

	public static JSONNode ToJson(BaseArc arc, IList<V4CommonData.Note> notesCommonData, IList<V4CommonData.Arc> arcsCommonData)
	{
		JSONObject obj = new JSONObject
		{
			["hb"] = arc.JsonTime,
			["tb"] = arc.TailJsonTime,
			["hr"] = arc.Rotation,
			["tr"] = arc.TailRotation
		};
		V4CommonData.Note item = V4CommonData.Note.FromBaseSliderHead(arc);
		obj["hi"] = notesCommonData.IndexOf(item);
		V4CommonData.Note item2 = V4CommonData.Note.FromBaseArcTail(arc);
		obj["ti"] = notesCommonData.IndexOf(item2);
		V4CommonData.Arc item3 = V4CommonData.Arc.FromBaseArc(arc);
		obj["ai"] = arcsCommonData.IndexOf(item3);
		return obj;
	}
}
