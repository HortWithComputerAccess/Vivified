using System;
using UnityEngine;

public class ClampedIntegerSettingsBinder : SettingsBinder
{
	[SerializeField]
	private int lowestValue;

	[SerializeField]
	private int highestValue;

	protected override object SettingsToUIValue(object input)
	{
		return Convert.ToInt32(input);
	}

	protected override object UIValueToSettings(object input)
	{
		return Mathf.Clamp(Convert.ToInt32(input), lowestValue, highestValue);
	}
}
