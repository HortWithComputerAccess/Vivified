using System.Linq;
using Beatmap.Base;
using Beatmap.Info;

public class LegacyChromaReq : RequirementCheck
{
	public override string Name => "Chroma Lighting Events";

	public override RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (infoDifficulty == null)
		{
			return RequirementType.None;
		}
		if (map == null || map.Events?.Any((BaseEvent e) => e.Value > 2000000000) != true)
		{
			return RequirementType.None;
		}
		return RequirementType.Suggestion;
	}
}
