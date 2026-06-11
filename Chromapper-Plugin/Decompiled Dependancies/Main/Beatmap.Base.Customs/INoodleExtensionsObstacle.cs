using SimpleJSON;

namespace Beatmap.Base.Customs;

public interface INoodleExtensionsObstacle : INoodleExtensionsGrid
{
	JSONNode CustomSize { get; set; }

	string CustomKeySize { get; }
}
