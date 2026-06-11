using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V2.Customs;

public class V2BpmChange
{
	public const string KeyTime = "_time";

	public const string KeyBeatsPerBar = "_beatsPerBar";

	public const string KeyBpm = "_BPM";

	public const string KeyMetronomeOffset = "_metronomeOffset";

	public static BaseBpmChange GetFromJson(JSONNode node)
	{
		return new BaseBpmChange(node);
	}

	public static JSONNode ToJson(BaseBpmChange bpmChange)
	{
		return new JSONObject
		{
			["_time"] = bpmChange.JsonTime,
			["_BPM"] = bpmChange.Bpm,
			["_beatsPerBar"] = bpmChange.BeatsPerBar,
			["_metronomeOffset"] = bpmChange.MetronomeOffset
		};
	}
}
