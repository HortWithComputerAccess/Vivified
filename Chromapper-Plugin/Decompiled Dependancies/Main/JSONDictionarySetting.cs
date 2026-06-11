using System.Collections.Generic;
using SimpleJSON;

public abstract class JSONDictionarySetting : Dictionary<string, JSONNode>, IJsonSetting
{
	public void FromJson(JSONNode obj)
	{
		string[] array = new string[base.Keys.Count];
		base.Keys.CopyTo(array, 0);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (obj[text] != null)
			{
				base[text] = obj[text];
			}
		}
	}

	public JSONObject ToJson()
	{
		JSONObject jSONObject = new JSONObject();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> current = enumerator.Current;
			jSONObject[current.Key] = current.Value;
		}
		return jSONObject;
	}
}
