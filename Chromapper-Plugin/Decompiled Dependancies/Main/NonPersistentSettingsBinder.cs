public class NonPersistentSettingsBinder : SettingsBinder
{
	public string OptionName;

	public string DefaultValue;

	protected override object SettingsToUIValue(object input)
	{
		return input;
	}

	protected override object UIValueToSettings(object input)
	{
		return input;
	}

	public override object RetrieveValueFromSettings()
	{
		if (Settings.NonPersistentSettings.TryGetValue(OptionName, out var value))
		{
			return value;
		}
		Settings.NonPersistentSettings.Add(OptionName, DefaultValue);
		return DefaultValue;
	}

	public override void SendValueToSettings(object value)
	{
		Settings.ManuallyNotifySettingUpdatedEvent(OptionName, value);
	}
}
