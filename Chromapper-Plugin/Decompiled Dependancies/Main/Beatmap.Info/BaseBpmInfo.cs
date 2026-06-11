using System;
using System.Collections.Generic;
using Beatmap.Base;

namespace Beatmap.Info;

public class BaseBpmInfo
{
	public List<BpmInfoBpmRegion> BpmRegions = new List<BpmInfoBpmRegion>();

	public List<BpmInfoLufsRegion> LufsRegions = new List<BpmInfoLufsRegion>();

	public string Version { get; set; }

	public int AudioSamples { get; set; }

	public int AudioFrequency { get; set; }

	public BaseBpmInfo InitWithSongContainerInstance()
	{
		Version = ((BeatSaberSongContainer.Instance.Info.Version[0] == '4') ? "4.0.0" : "2.0.0");
		AudioSamples = BeatSaberSongContainer.Instance.LoadedSongSamples - 1;
		AudioFrequency = BeatSaberSongContainer.Instance.LoadedSongFrequency;
		return this;
	}

	public static string GetOutputFileName(int difficultyMajorVersion, BaseInfo info)
	{
		if (difficultyMajorVersion == 4)
		{
			if (string.IsNullOrWhiteSpace(info.AudioDataFilename))
			{
				return "AudioData.dat";
			}
			return info.AudioDataFilename;
		}
		return "BPMInfo.dat";
	}

	public static List<BaseBpmEvent> GetBpmEvents(List<BpmInfoBpmRegion> bpmRegions, int audioFrequency)
	{
		List<BaseBpmEvent> list = new List<BaseBpmEvent>();
		foreach (BpmInfoBpmRegion bpmRegion in bpmRegions)
		{
			int num = bpmRegion.EndSampleIndex - bpmRegion.StartSampleIndex;
			float num2 = bpmRegion.EndBeat - bpmRegion.StartBeat;
			float num3 = num2 / (float)num * (float)audioFrequency * 60f;
			float num4 = (float)Math.Round(num2 / (float)num * (float)audioFrequency * 60f);
			bool flag = Math.Abs(num2 * 60f / num4 * (float)audioFrequency - (float)num) < 1.1f;
			list.Add(new BaseBpmEvent
			{
				Bpm = (flag ? num4 : num3),
				JsonTime = bpmRegion.StartBeat
			});
		}
		return list;
	}

	public static List<BpmInfoBpmRegion> GetBpmInfoRegions(List<BaseBpmEvent> bpmEvents, float songBpm, int audioSamples, int audioFrequency)
	{
		List<BpmInfoBpmRegion> list = new List<BpmInfoBpmRegion>();
		if (bpmEvents.Count == 0)
		{
			list.Add(new BpmInfoBpmRegion
			{
				StartSampleIndex = 0,
				EndSampleIndex = audioSamples,
				StartBeat = 0f,
				EndBeat = songBpm / 60f * ((float)audioSamples / (float)audioFrequency)
			});
		}
		else
		{
			int num = (int)Math.Round(bpmEvents[0].SongBpmTime * (60f / songBpm) * (float)audioFrequency, MidpointRounding.AwayFromZero);
			for (int i = 0; i < bpmEvents.Count - 1; i++)
			{
				BaseBpmEvent baseBpmEvent = bpmEvents[i];
				BaseBpmEvent baseBpmEvent2 = bpmEvents[i + 1];
				int num2 = (int)Math.Round(baseBpmEvent2.SongBpmTime * (60f / songBpm) * (float)audioFrequency, MidpointRounding.AwayFromZero);
				list.Add(new BpmInfoBpmRegion
				{
					StartSampleIndex = num,
					EndSampleIndex = num2,
					StartBeat = baseBpmEvent.JsonTime,
					EndBeat = baseBpmEvent2.JsonTime
				});
				num = num2;
			}
			BaseBpmEvent baseBpmEvent3 = bpmEvents[^1];
			float num3 = (float)(audioSamples - num) / (float)audioFrequency * (baseBpmEvent3.Bpm / 60f);
			list.Add(new BpmInfoBpmRegion
			{
				StartSampleIndex = num,
				EndSampleIndex = audioSamples,
				StartBeat = baseBpmEvent3.JsonTime,
				EndBeat = baseBpmEvent3.JsonTime + num3
			});
		}
		return list;
	}
}
