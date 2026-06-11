using SimpleJSON;

namespace Beatmap.Info;

public static class V2Contributor
{
	public static BaseContributor GetFromJson(JSONNode node)
	{
		return new BaseContributor
		{
			Name = node["_name"]?.Value,
			Role = node["_role"]?.Value,
			LocalImageLocation = node["_iconPath"]?.Value
		};
	}

	public static JSONObject ToJson(BaseContributor contributor)
	{
		return new JSONObject
		{
			["_name"] = contributor.Name,
			["_role"] = contributor.Role,
			["_iconPath"] = contributor.LocalImageLocation
		};
	}
}
