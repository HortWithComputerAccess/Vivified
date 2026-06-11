using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Info;

public abstract class RequirementCheck
{
	public enum RequirementType
	{
		Requirement,
		Suggestion,
		None
	}

	internal static readonly HashSet<RequirementCheck> requirementsAndSuggestions = new HashSet<RequirementCheck>();

	public abstract string Name { get; }

	internal static void Setup()
	{
		requirementsAndSuggestions.Clear();
		RegisterRequirement(new ChromaReq());
		RegisterRequirement(new LegacyChromaReq());
		RegisterRequirement(new MappingExtensionsReq());
		RegisterRequirement(new NoodleExtensionsReq());
		RegisterRequirement(new CinemaReq());
		RegisterRequirement(new SoundExtensionsReq());
		RegisterRequirement(new VivifyReq());
	}

	public static void RegisterRequirement(RequirementCheck req)
	{
		requirementsAndSuggestions.Add(req);
	}

	public abstract RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map);
}
