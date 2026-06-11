using SimpleJSON;

namespace Beatmap.Info;

public static class V4Contributor
{
	public static BaseContributor GetFromJson(JSONNode node)
	{
		return new BaseContributor
		{
			Name = node["name"]?.Value,
			Role = node["role"]?.Value,
			LocalImageLocation = node["iconPath"]?.Value
		};
	}

	public static JSONObject ToJson(BaseContributor contributor)
	{
		return new JSONObject
		{
			["name"] = contributor.Name,
			["role"] = contributor.Role,
			["iconPath"] = contributor.LocalImageLocation
		};
	}
}
