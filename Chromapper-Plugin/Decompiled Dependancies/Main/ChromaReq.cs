using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Info;

public class ChromaReq : HeckRequirementCheck
{
	private static readonly List<string> chromaSpecificTrackTypes = new List<string> { "AnimateComponent" };

	private static readonly List<string> v3ChromaAnimationKeys = new List<string> { "color" };

	private static readonly List<string> v2ChromaAnimationKeys = new List<string> { "_color" };

	public override string Name => "Chroma";

	public override RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (infoDifficulty != null && (HasEnvironmentRemoval(infoDifficulty, map) || HasChromaEvents(map) || HasChromaTracks(map)))
		{
			if (!RequiresChroma(infoDifficulty, map))
			{
				return RequirementType.Suggestion;
			}
			return RequirementType.Requirement;
		}
		return RequirementType.None;
	}

	private bool HasChromaEvents(BaseDifficulty map)
	{
		return map.IsChroma();
	}

	private bool RequiresChroma(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		return infoDifficulty.CustomRequirements.Any((string x) => x == "Chroma");
	}

	private bool HasEnvironmentRemoval(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (!(infoDifficulty.CustomData != null) || !infoDifficulty.CustomData.HasKey("_environmentRemoval") || infoDifficulty.CustomData["_environmentRemoval"].AsArray.Count <= 0)
		{
			if (map.CustomData != null && map.CustomData.HasKey("_environment"))
			{
				return map.CustomData["_environment"].AsArray.Count > 0;
			}
			return false;
		}
		return true;
	}

	private bool HasChromaTracks(BaseDifficulty map)
	{
		List<string> modAnimationKeys = Settings.Instance.MapVersion switch
		{
			3 => v3ChromaAnimationKeys, 
			2 => v2ChromaAnimationKeys, 
			_ => new List<string>(), 
		};
		return HasAnimationsFromMod(map, chromaSpecificTrackTypes, modAnimationKeys);
	}
}
