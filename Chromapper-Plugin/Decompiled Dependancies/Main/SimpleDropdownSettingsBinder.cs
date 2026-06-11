using System;
using TMPro;
using UnityEngine.Serialization;

public class SimpleDropdownSettingsBinder : SettingsBinder
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
		return Convert.ToInt32(input);
	}

	protected override object UIValueToSettings(object input)
	{
		return Convert.ToInt32(input);
	}
}
