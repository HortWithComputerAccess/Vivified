using UnityEngine;

public abstract class SettingsBinder : MonoBehaviour
{
	public enum SettingsType
	{
		All,
		STRING,
		INT,
		SINGLE,
		BOOL
	}

	[HideInInspector]
	public SettingsType BindedSettingSearchType;

	[HideInInspector]
	public string BindedSetting = "None";

	[HideInInspector]
	public bool PopupEditorWarning;

	public virtual void SendValueToSettings(object value)
	{
		if (!string.IsNullOrEmpty(BindedSetting) && BindedSetting != "None")
		{
			if (PopupEditorWarning && PersistentUI.Instance != null)
			{
				PersistentUI.Instance.ShowDialogBox("Options", "restartwarning", null, PersistentUI.DialogBoxPresetType.Ok);
			}
			Settings.ApplyOptionByName(BindedSetting, UIValueToSettings(value));
		}
	}

	public virtual object RetrieveValueFromSettings()
	{
		if (string.IsNullOrEmpty(BindedSetting) || BindedSetting == "None")
		{
			return null;
		}
		return SettingsToUIValue(Settings.AllFieldInfos[BindedSetting].GetValue(Settings.Instance));
	}

	protected abstract object UIValueToSettings(object input);

	protected abstract object SettingsToUIValue(object input);
}
