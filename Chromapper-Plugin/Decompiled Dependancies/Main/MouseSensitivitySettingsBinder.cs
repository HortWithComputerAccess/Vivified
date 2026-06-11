using System;
using UnityEngine;

public class MouseSensitivitySettingsBinder : SettingsBinder
{
	protected override object SettingsToUIValue(object input)
	{
		return Mathf.Round((Convert.ToSingle(input) - 0.5f) * 2f);
	}

	protected override object UIValueToSettings(object input)
	{
		return Convert.ToSingle(input) / 2f + 0.5f;
	}
}
