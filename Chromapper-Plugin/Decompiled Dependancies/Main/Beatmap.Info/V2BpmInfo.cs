using System.Collections.Generic;
using SimpleJSON;

namespace Beatmap.Info;

public static class V2BpmInfo
{
	public const string FileName = "BPMInfo.dat";

	public static BaseBpmInfo GetFromJson(JSONNode node)
	{
		BaseBpmInfo baseBpmInfo = new BaseBpmInfo();
		baseBpmInfo.Version = node["_version"];
		baseBpmInfo.AudioSamples = node["_songSampleCount"];
		baseBpmInfo.AudioFrequency = node["_songFrequency"];
		List<BpmInfoBpmRegion> list = new List<BpmInfoBpmRegion>();
		JSONNode.Enumerator enumerator = node["_regions"].GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode = enumerator.Current;
			BpmInfoBpmRegion item = new BpmInfoBpmRegion
			{
				StartSampleIndex = jSONNode["_startSampleIndex"],
				EndSampleIndex = jSONNode["_endSampleIndex"],
				StartBeat = jSONNode["_startBeat"],
				EndBeat = jSONNode["_endBeat"]
			};
			list.Add(item);
		}
		baseBpmInfo.BpmRegions = list;
		baseBpmInfo.LufsRegions = new List<BpmInfoLufsRegion>();
		return baseBpmInfo;
	}

	public static JSONNode GetOutputJson(BaseBpmInfo bpmInfo)
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_version"] = bpmInfo.Version;
		jSONNode["_songSampleCount"] = bpmInfo.AudioSamples;
		jSONNode["_songFrequency"] = bpmInfo.AudioFrequency;
		float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
		int bpmTimeValueDecimalPrecision = Settings.Instance.BpmTimeValueDecimalPrecision;
		JSONArray jSONArray = new JSONArray();
		if (bpmInfo.BpmRegions.Count == 0)
		{
			float num = (float)bpmInfo.AudioSamples / (float)bpmInfo.AudioFrequency;
			jSONArray.Add(new JSONObject
			{
				["_startSampleIndex"] = 0,
				["_endSampleIndex"] = bpmInfo.AudioSamples,
				["_startBeat"] = 0f,
				["_endBeat"] = new JSONNumberWithOverridenRounding(beatsPerMinute / 60f * num, bpmTimeValueDecimalPrecision)
			});
		}
		else
		{
			foreach (BpmInfoBpmRegion bpmRegion in bpmInfo.BpmRegions)
			{
				jSONArray.Add(new JSONObject
				{
					["_startSampleIndex"] = bpmRegion.StartSampleIndex,
					["_endSampleIndex"] = bpmRegion.EndSampleIndex,
					["_startBeat"] = new JSONNumberWithOverridenRounding(bpmRegion.StartBeat, bpmTimeValueDecimalPrecision),
					["_endBeat"] = new JSONNumberWithOverridenRounding(bpmRegion.EndBeat, bpmTimeValueDecimalPrecision)
				});
			}
		}
		jSONNode["_regions"] = jSONArray;
		return jSONNode;
	}
}
