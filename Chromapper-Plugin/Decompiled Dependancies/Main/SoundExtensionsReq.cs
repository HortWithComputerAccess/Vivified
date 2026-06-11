using System.Linq;
using Beatmap.Base;
using Beatmap.Info;

public class SoundExtensionsReq : RequirementCheck
{
	public override string Name => "Sound Extensions";

	public override RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (!HasSounds(infoDifficulty) && !HasNotesWithCustomSounds(map))
		{
			return RequirementType.None;
		}
		return RequirementType.Suggestion;
	}

	private bool HasSounds(InfoDifficulty infoDifficulty)
	{
		if (infoDifficulty.CustomData != null)
		{
			return infoDifficulty.CustomData.HasKey("_sounds");
		}
		return false;
	}

	private bool HasNotesWithCustomSounds(BaseDifficulty map)
	{
		return map.Notes.Any((BaseNote note) => note.CustomData != null && (note.CustomData.HasKey("_soundID") || note.CustomData.HasKey("_soundHitID") || note.CustomData.HasKey("_soundHitVolume") || note.CustomData.HasKey("_soundMissVolume")));
	}
}
