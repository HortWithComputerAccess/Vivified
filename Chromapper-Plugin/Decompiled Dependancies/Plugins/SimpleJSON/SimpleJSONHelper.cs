using System;
using System.Collections.Generic;

namespace SimpleJSON;

public static class SimpleJSONHelper
{
	private const string v2CustomData = "_customData";

	private const string v3CustomData = "customData";

	public static JSONArray MapSequenceToJSONArray<T>(IEnumerable<T> source, Func<T, JSONNode> func)
	{
		JSONArray jSONArray = new JSONArray();
		foreach (T item in source)
		{
			jSONArray.Add(func(item));
		}
		return jSONArray;
	}

	public static void RemovePropertiesWithDefaultValues(JSONNode node)
	{
		if (node.IsArray)
		{
			JSONNode.ValueEnumerator enumerator = node.AsArray.Values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				RemovePropertiesWithDefaultValues(enumerator.Current);
			}
		}
		else
		{
			if (!node.IsObject)
			{
				return;
			}
			List<string> list = new List<string>();
			JSONNode.KeyEnumerator enumerator2 = node.Keys.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				string current = enumerator2.Current;
				if (!(current == "_customData") && !(current == "customData"))
				{
					JSONNode jSONNode = node[current];
					if (jSONNode.IsObject || jSONNode.IsArray)
					{
						RemovePropertiesWithDefaultValues(jSONNode);
					}
					else if ((jSONNode.IsBoolean && !jSONNode.AsBool) || (jSONNode.IsNumber && jSONNode.AsFloat == 0f) || jSONNode.IsNull)
					{
						list.Add(current);
					}
				}
			}
			foreach (string item in list)
			{
				node.Remove(item);
			}
			list.Clear();
		}
	}

	public static JSONNode CleanObject(JSONNode obj)
	{
		if ((object)obj == null)
		{
			return null;
		}
		JSONNode.KeyEnumerator enumerator = obj.Clone().Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (!obj.HasKey(current))
			{
				continue;
			}
			if (!obj[current].IsNull)
			{
				JSONArray asArray = obj[current].AsArray;
				if (((object)asArray == null || asArray.Count > 0) && (obj.IsArray || obj.IsObject || !string.IsNullOrEmpty(obj[current].Value)))
				{
					continue;
				}
			}
			obj.Remove(current);
		}
		return obj;
	}
}
