using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V3.Customs;

public class V3BpmChange
{
	public const string KeyTime = "b";

	public const string KeyBeatsPerBar = "p";

	public const string KeyBpm = "m";

	public const string KeyMetronomeOffset = "o";

	public static BaseBpmChange GetFromJson(JSONNode node)
	{
		return new BaseBpmChange(node);
	}

	public static JSONNode ToJson(BaseBpmChange bpmChange)
	{
		return new JSONObject
		{
			["b"] = bpmChange.JsonTime,
			["m"] = bpmChange.Bpm,
			["p"] = bpmChange.BeatsPerBar,
			["o"] = bpmChange.MetronomeOffset
		};
	}
}
