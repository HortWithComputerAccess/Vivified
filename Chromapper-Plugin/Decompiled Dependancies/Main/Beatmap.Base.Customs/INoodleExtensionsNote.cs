namespace Beatmap.Base.Customs;

public interface INoodleExtensionsNote : INoodleExtensionsGrid
{
	float? CustomDirection { get; set; }

	string CustomKeyDirection { get; }
}
