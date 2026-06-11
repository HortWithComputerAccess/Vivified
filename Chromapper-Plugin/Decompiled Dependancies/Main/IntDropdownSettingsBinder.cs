using System;
using TMPro;
using UnityEngine.Serialization;

public class IntDropdownSettingsBinder : SettingsBinder
{
	[FormerlySerializedAs("dropdown")]
	public TMP_Dropdown Dropdown;

	private void Start()
	{
		Dropdown.SetValueWithoutNotify((int)RetrieveValueFromSettings());
	}

	public void SendDropdownToSettings(int value)
	{
		SendValueToSettings(value);
	}

	protected override object SettingsToUIValue(object input)
	{
		string text = input.ToString();
		for (int i = 0; i < Dropdown.options.Count; i++)
		{
			if (Dropdown.options[i].text == text)
			{
				return i;
			}
		}
		return 0;
	}

	protected override object UIValueToSettings(object input)
	{
		return Convert.ToInt32(Dropdown.options[(int)input].text);
	}
}
