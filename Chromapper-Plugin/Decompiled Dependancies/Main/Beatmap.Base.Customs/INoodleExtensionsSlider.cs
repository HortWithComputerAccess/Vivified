using SimpleJSON;

namespace Beatmap.Base.Customs;

public interface INoodleExtensionsSlider : INoodleExtensionsGrid
{
	JSONNode CustomTailCoordinate { get; set; }

	string CustomKeyTailCoordinate { get; }
}
