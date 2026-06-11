using SimpleJSON;

namespace Beatmap.Base.Customs;

public interface ICustomData
{
	JSONNode CustomData { get; set; }

	bool IsChroma();

	bool IsNoodleExtensions();

	bool IsMappingExtensions();
}
