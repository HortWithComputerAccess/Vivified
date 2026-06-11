using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V3.Customs;

public static class V3Bookmark
{
	public const string KeyTime = "b";

	public const string KeyName = "n";

	public const string KeyColor = "c";

	public static BaseBookmark GetFromJson(JSONNode node)
	{
		return new BaseBookmark(node);
	}

	public static JSONNode ToJson(BaseBookmark bookmark)
	{
		return new JSONObject
		{
			["b"] = bookmark.JsonTime,
			["n"] = bookmark.Name,
			["c"] = bookmark.Color
		};
	}
}
