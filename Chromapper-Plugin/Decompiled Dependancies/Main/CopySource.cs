using Beatmap.Info;

public class CopySource
{
	public DifficultySettings DifficultySettings { get; }

	public InfoDifficultySet CharacteristicSet { get; }

	public DifficultyRow Obj { get; }

	public CopySource(DifficultySettings difficultySettings, InfoDifficultySet characteristicSet, DifficultyRow obj)
	{
		DifficultySettings = difficultySettings;
		CharacteristicSet = characteristicSet;
		Obj = obj;
	}
}
