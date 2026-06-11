using System;
using System.IO;
using Assets.HSVPicker;
using SimpleJSON;
using UnityEngine;

public class ColourHistory : MonoBehaviour
{
	public static void Save()
	{
		JSONObject jSONObject = new JSONObject();
		JSONArray jSONArray = new JSONArray();
		foreach (Color color in ColorPresetManager.Get().Colors)
		{
			JSONObject jSONObject2 = new JSONObject();
			jSONObject2.WriteColor(color);
			jSONArray.Add(jSONObject2);
		}
		jSONObject.Add("colors", jSONArray);
		using (StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/ChromaColors.json", append: false))
		{
			streamWriter.Write(jSONObject.ToString());
		}
		Debug.Log("Chroma Colors saved!");
	}

	public static void Load()
	{
		if (!File.Exists(Application.persistentDataPath + "/ChromaColors.json"))
		{
			Debug.Log("Chroma Colors file doesn't exist! Skipping loading...");
			return;
		}
		try
		{
			ColorPresetManager.Presets.Clear();
			ColorPresetList colorPresetList = new ColorPresetList("default");
			using (StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/ChromaColors.json"))
			{
				JSONNode.Enumerator enumerator = JSON.Parse(streamReader.ReadToEnd())["colors"].AsArray.GetEnumerator();
				while (enumerator.MoveNext())
				{
					JSONNode jSONNode = enumerator.Current;
					Color item = (jSONNode.IsObject ? jSONNode.ReadColor(Color.black) : ColourManager.ColourFromInt(jSONNode.AsInt));
					colorPresetList.Colors.Add(item);
				}
			}
			Debug.Log($"Loaded {colorPresetList.Colors.Count} colors!");
			ColorPresetManager.Presets.Add("default", colorPresetList);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}
}
