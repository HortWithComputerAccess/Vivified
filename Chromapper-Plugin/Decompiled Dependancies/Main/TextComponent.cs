using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class TextComponent : CMUIComponent<string>
{
	[SerializeField]
	private TextMeshProUGUI textMeshProUGUI;

	private void Start()
	{
		OnValueUpdated(base.Value);
	}

	public TextComponent WithInitialValue(string table, string key, params object[] args)
	{
		base.Value = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		return this;
	}

	protected override void OnValueUpdated(string updatedValue)
	{
		textMeshProUGUI.text = updatedValue;
	}
}
