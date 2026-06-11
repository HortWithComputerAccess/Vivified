using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V2.Customs;

public static class V2Bookmark
{
	public const string KeyTime = "_time";

	public const string KeyName = "_name";

	public const string KeyColor = "_color";

	public static BaseBookmark GetFromJson(JSONNode node)
	{
		return new BaseBookmark(node);
	}

	public static JSONNode ToJson(BaseBookmark bookmark)
	{
		return new JSONObject
		{
			["_time"] = bookmark.JsonTime,
			["_name"] = bookmark.Name,
			["_color"] = bookmark.Color
		};
	}
}
