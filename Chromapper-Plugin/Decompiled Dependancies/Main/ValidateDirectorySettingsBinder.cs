using System;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ValidateDirectorySettingsBinder : SettingsBinder
{
	[SerializeField]
	private LocalizeStringEvent errorText;

	protected override object SettingsToUIValue(object input)
	{
		return input;
	}

	protected override object UIValueToSettings(object input)
	{
		string fullPath = Path.GetFullPath(Convert.ToString(input));
		string result = Settings.AllFieldInfos[BindedSetting].GetValue(Settings.Instance).ToString();
		Settings.AllFieldInfos[BindedSetting].SetValue(Settings.Instance, fullPath);
		errorText.StringReference.TableEntryReference = "validate.good";
		if (!Settings.ValidateDirectory(ErrorFeedback))
		{
			return result;
		}
		return fullPath;
	}

	private void ErrorFeedback(string feedback)
	{
		errorText.StringReference.TableEntryReference = feedback;
	}
}
