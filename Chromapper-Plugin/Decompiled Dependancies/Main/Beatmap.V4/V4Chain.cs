using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4Chain
{
	public static BaseChain GetFromJson(JSONNode node, IList<V4CommonData.Note> notesCommonData, IList<V4CommonData.Chain> chainsCommonData)
	{
		BaseChain obj = new BaseChain
		{
			JsonTime = node["hb"].AsFloat,
			TailJsonTime = node["tb"].AsFloat,
			Rotation = node["hr"].AsInt,
			TailRotation = node["tr"].AsInt
		};
		int asInt = node["i"].AsInt;
		V4CommonData.Note note = notesCommonData[asInt];
		obj.PosX = note.PosX;
		obj.PosY = note.PosY;
		obj.Color = note.Color;
		obj.CutDirection = note.CutDirection;
		obj.AngleOffset = note.AngleOffset;
		int asInt2 = node["ci"].AsInt;
		V4CommonData.Chain chain = chainsCommonData[asInt2];
		obj.TailPosX = chain.TailPosX;
		obj.TailPosY = chain.TailPosY;
		obj.SliceCount = chain.SliceCount;
		obj.Squish = chain.Squish;
		return obj;
	}

	public static JSONNode ToJson(BaseChain chain, IList<V4CommonData.Note> notesCommonData, IList<V4CommonData.Chain> chainsCommonData)
	{
		JSONObject obj = new JSONObject
		{
			["hb"] = chain.JsonTime,
			["tb"] = chain.TailJsonTime,
			["hr"] = chain.Rotation,
			["tr"] = chain.TailRotation
		};
		V4CommonData.Note item = V4CommonData.Note.FromBaseSliderHead(chain);
		obj["i"] = notesCommonData.IndexOf(item);
		V4CommonData.Chain item2 = V4CommonData.Chain.FromBaseChain(chain);
		obj["ci"] = chainsCommonData.IndexOf(item2);
		return obj;
	}
}
