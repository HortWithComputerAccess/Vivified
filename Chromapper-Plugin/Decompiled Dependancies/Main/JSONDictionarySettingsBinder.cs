using System;
using SimpleJSON;
using UnityEngine.Serialization;

public class JSONDictionarySettingsBinder : SettingsBinder
{
	[FormerlySerializedAs("dictionaryKey")]
	public string DictionaryKey;

	protected override object SettingsToUIValue(object input)
	{
		JSONNode jSONNode = ((JSONDictionarySetting)input)[DictionaryKey];
		return jSONNode.Tag switch
		{
			JSONNodeType.String => (string)jSONNode, 
			JSONNodeType.Number => (double)jSONNode, 
			JSONNodeType.Boolean => (bool)jSONNode, 
			_ => null, 
		};
	}

	protected override object UIValueToSettings(object input)
	{
		JSONDictionarySetting jSONDictionarySetting = (JSONDictionarySetting)Settings.AllFieldInfos[BindedSetting].GetValue(Settings.Instance);
		JSONNode jSONNode = jSONDictionarySetting[DictionaryKey];
		JSONDictionarySetting jSONDictionarySetting2 = jSONDictionarySetting;
		string dictionaryKey = DictionaryKey;
		jSONDictionarySetting2[dictionaryKey] = jSONNode.Tag switch
		{
			JSONNodeType.String => (string)input, 
			JSONNodeType.Number => (double)input, 
			JSONNodeType.Boolean => (bool)input, 
			_ => throw new InvalidOperationException($"Unknown JSON Tag '{jSONNode.Tag}'."), 
		};
		return jSONDictionarySetting;
	}
}
