using System.Collections.Generic;
using SimpleJSON;

namespace Beatmap.Info;

public static class V4AudioData
{
	public const string FileName = "AudioData.dat";

	public static BaseBpmInfo GetFromJson(JSONNode node)
	{
		BaseBpmInfo baseBpmInfo = new BaseBpmInfo();
		baseBpmInfo.Version = node["version"];
		baseBpmInfo.AudioSamples = node["songSampleCount"];
		baseBpmInfo.AudioFrequency = node["songFrequency"];
		List<BpmInfoBpmRegion> list = new List<BpmInfoBpmRegion>();
		JSONNode.Enumerator enumerator = node["bpmData"].GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode = enumerator.Current;
			BpmInfoBpmRegion item = new BpmInfoBpmRegion
			{
				StartSampleIndex = jSONNode["si"],
				EndSampleIndex = jSONNode["ei"],
				StartBeat = jSONNode["sb"],
				EndBeat = jSONNode["eb"]
			};
			list.Add(item);
		}
		baseBpmInfo.BpmRegions = list;
		List<BpmInfoLufsRegion> list2 = new List<BpmInfoLufsRegion>();
		enumerator = node["lufsData"].GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode2 = enumerator.Current;
			BpmInfoLufsRegion item2 = new BpmInfoLufsRegion
			{
				StartSampleIndex = jSONNode2["si"],
				EndSampleIndex = jSONNode2["ei"],
				Loudness = jSONNode2["l"]
			};
			list2.Add(item2);
		}
		baseBpmInfo.LufsRegions = list2;
		return baseBpmInfo;
	}

	public static JSONNode GetOutputJson(BaseBpmInfo bpmInfo)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["version"] = bpmInfo.Version;
		jSONNode["songChecksum"] = "";
		jSONNode["songSampleCount"] = bpmInfo.AudioSamples;
		jSONNode["songFrequency"] = bpmInfo.AudioFrequency;
		float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
		int bpmTimeValueDecimalPrecision = Settings.Instance.BpmTimeValueDecimalPrecision;
		JSONArray jSONArray = new JSONArray();
		if (bpmInfo.BpmRegions.Count == 0)
		{
			float num = (float)bpmInfo.AudioSamples / (float)bpmInfo.AudioFrequency;
			jSONArray.Add(new JSONObject
			{
				["si"] = 0,
				["ei"] = bpmInfo.AudioSamples,
				["sb"] = 0f,
				["eb"] = new JSONNumberWithOverridenRounding(beatsPerMinute / 60f * num, bpmTimeValueDecimalPrecision)
			});
		}
		else
		{
			foreach (BpmInfoBpmRegion bpmRegion in bpmInfo.BpmRegions)
			{
				jSONArray.Add(new JSONObject
				{
					["si"] = bpmRegion.StartSampleIndex,
					["ei"] = bpmRegion.EndSampleIndex,
					["sb"] = new JSONNumberWithOverridenRounding(bpmRegion.StartBeat, bpmTimeValueDecimalPrecision),
					["eb"] = new JSONNumberWithOverridenRounding(bpmRegion.EndBeat, bpmTimeValueDecimalPrecision)
				});
			}
		}
		jSONNode["bpmData"] = jSONArray;
		JSONArray jSONArray2 = new JSONArray();
		foreach (BpmInfoLufsRegion lufsRegion in bpmInfo.LufsRegions)
		{
			jSONArray2.Add(new JSONObject
			{
				["si"] = lufsRegion.StartSampleIndex,
				["ei"] = lufsRegion.EndSampleIndex,
				["l"] = lufsRegion.Loudness
			});
		}
		jSONNode["lufsData"] = jSONArray2;
		return jSONNode;
	}
}
