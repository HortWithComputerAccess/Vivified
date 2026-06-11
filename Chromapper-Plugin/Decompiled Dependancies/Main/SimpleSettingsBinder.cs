using System;

public class SimpleSettingsBinder : SettingsBinder
{
	protected override object SettingsToUIValue(object input)
	{
		return Convert.ChangeType(input, Settings.AllFieldInfos[BindedSetting].FieldType);
	}

	protected override object UIValueToSettings(object input)
	{
		return Convert.ChangeType(input, Settings.AllFieldInfos[BindedSetting].FieldType);
	}
}
