using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;

public class LanguageDropdownSettingsBinder : SettingsBinder
{
	[FormerlySerializedAs("dropdown")]
	public TMP_Dropdown Dropdown;

	private IEnumerator Start()
	{
		yield return LocalizationSettings.InitializationOperation;
		LocalesProvider available = (LocalesProvider)LocalizationSettings.AvailableLocales;
		yield return available.PreloadOperation;
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int value = 0;
		for (int i = 0; i < available.Locales.Count; i++)
		{
			Locale locale = available.Locales[i];
			if (LocalizationSettings.SelectedLocale.Identifier.Code.Equals(locale.Identifier.Code))
			{
				value = i;
			}
			list.Add(new TMP_Dropdown.OptionData(locale.name));
		}
		Dropdown.options = list;
		Dropdown.value = value;
	}

	public void SendDropdownToSettings(int value)
	{
		Locale locale = (LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value]);
		SendValueToSettings(locale.Identifier.Code);
	}

	protected override object SettingsToUIValue(object input)
	{
		return Convert.ToString(input);
	}

	protected override object UIValueToSettings(object input)
	{
		return Convert.ToString(input);
	}
}
