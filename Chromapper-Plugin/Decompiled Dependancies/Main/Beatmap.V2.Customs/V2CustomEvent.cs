using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V2.Customs;

public class V2CustomEvent
{
	public const string CustomKeyTrack = "_track";

	public const string CustomKeyColor = "_color";

	public const string KeyTime = "_time";

	public const string KeyType = "_type";

	public const string KeyData = "_data";

	public const string DataKeyDuration = "_duration";

	public const string DataKeyEasing = "_easing";

	public const string DataKeyRepeat = "_repeat";

	public const string DataKeyChildrenTracks = "_childrenTracks";

	public const string DataKeyParentTrack = "_parentTrack";

	public const string DataKeyWorldPositionStays = "_worldPositionStays";

	public static BaseCustomEvent GetFromJson(JSONNode node)
	{
		return new BaseCustomEvent(node);
	}

	public static JSONNode ToJson(BaseCustomEvent customEvent)
	{
		return new JSONObject
		{
			["_time"] = customEvent.JsonTime,
			["_type"] = customEvent.Type,
			["_data"] = customEvent.Data
		};
	}
}
