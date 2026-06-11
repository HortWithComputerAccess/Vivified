public static class EnvironmentInfoHelper
{
	public static string GetName()
	{
		if (!(BeatSaberSongContainer.Instance.MapDifficultyInfo.Characteristic == "90Degree") && !(BeatSaberSongContainer.Instance.MapDifficultyInfo.Characteristic == "360Degree"))
		{
			return BeatSaberSongContainer.Instance.Info.EnvironmentName;
		}
		return BeatSaberSongContainer.Instance.Info.AllDirectionsEnvironmentName;
	}
}
