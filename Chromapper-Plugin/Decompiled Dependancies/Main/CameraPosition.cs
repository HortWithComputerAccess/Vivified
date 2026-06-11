using SimpleJSON;
using UnityEngine;

public class CameraPosition : IJsonSetting
{
	public Vector3 Position { get; private set; }

	public Quaternion Rotation { get; private set; }

	public CameraPosition(Vector3 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public CameraPosition()
	{
	}

	public void FromJson(JSONNode obj)
	{
		if (!(obj == null))
		{
			Position = new Vector3(obj["position"][0], obj["position"][1], obj["position"][2]);
			Rotation = new Quaternion(obj["rotation"][1], obj["rotation"][2], obj["rotation"][3], obj["rotation"][0]);
		}
	}

	public JSONObject ToJson()
	{
		JSONObject jSONObject = new JSONObject();
		jSONObject["position"].Add(Position.x);
		jSONObject["position"].Add(Position.y);
		jSONObject["position"].Add(Position.z);
		jSONObject["rotation"].Add(Rotation.w);
		jSONObject["rotation"].Add(Rotation.x);
		jSONObject["rotation"].Add(Rotation.y);
		jSONObject["rotation"].Add(Rotation.z);
		return jSONObject;
	}
}
