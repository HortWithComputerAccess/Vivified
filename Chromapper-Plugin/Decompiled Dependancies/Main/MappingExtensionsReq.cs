using Beatmap.Base;
using Beatmap.Info;

public class MappingExtensionsReq : RequirementCheck
{
	public override string Name => "Mapping Extensions";

	public override RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (infoDifficulty == null)
		{
			return RequirementType.None;
		}
		if (!map.IsMappingExtensions())
		{
			return RequirementType.None;
		}
		return RequirementType.Requirement;
	}
}
