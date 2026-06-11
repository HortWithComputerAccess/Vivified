using SimpleJSON;
using UnityEngine;

public class MultiSettings : IJsonSetting
{
	public string DisplayName = "Mapper";

	public Color GridColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);

	public string ChroMapTogetherServerUrl = "http://chromapper.caeden.dev";

	public string LastHostedPort = "6969";

	public string LastJoinedIP = "127.0.0.1";

	public string LastJoinedPort = "6969";

	public MapperIdentityPacket LocalIdentity => new MapperIdentityPacket(DisplayName, 0, GridColor);

	public void FromJson(JSONNode obj)
	{
		DisplayName = obj["DisplayName"];
		GridColor = obj["GridColor"];
		ChroMapTogetherServerUrl = obj["ChroMapTogetherServerUrl"];
		LastHostedPort = obj["LastHostedPort"];
		LastJoinedIP = obj["LastJoinedIP"];
		LastJoinedPort = obj["LastJoinedPort"];
	}

	public JSONObject ToJson()
	{
		return new JSONObject
		{
			["DisplayName"] = DisplayName,
			["GridColor"] = GridColor,
			["ChroMapTogetherServerUrl"] = ChroMapTogetherServerUrl,
			["LastHostedPort"] = LastHostedPort,
			["LastJoinedIP"] = LastJoinedIP,
			["LastJoinedPort"] = LastJoinedPort
		};
	}
}
