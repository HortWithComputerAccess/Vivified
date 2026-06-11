using System.Collections.Generic;
using SimpleJSON;

namespace Beatmap.Info;

public class InfoDifficultySet
{
	public string Characteristic { get; set; }

	public List<InfoDifficulty> Difficulties { get; set; } = new List<InfoDifficulty>();

	public JSONObject CustomData { get; set; } = new JSONObject();

	public string CustomCharacteristicLabel { get; set; }

	public string CustomCharacteristicIconImageFileName { get; set; }
}
