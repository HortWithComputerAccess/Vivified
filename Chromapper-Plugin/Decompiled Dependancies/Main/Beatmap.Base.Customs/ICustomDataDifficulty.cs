using System.Collections.Generic;

namespace Beatmap.Base.Customs;

public interface ICustomDataDifficulty : ICustomData
{
	float Time { get; set; }

	List<BaseBpmChange> BpmChanges { get; set; }

	List<BaseBookmark> Bookmarks { get; set; }

	List<BaseCustomEvent> CustomEvents { get; set; }

	List<BaseEnvironmentEnhancement> EnvironmentEnhancements { get; set; }
}
