using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ButtonComponent : CMUIComponentBase, INavigable
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI label;

	private Action onClick;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public ButtonComponent OnClick(Action onClick)
	{
		this.onClick = onClick;
		return this;
	}

	public ButtonComponent WithLabel(string table, string key, params object[] args)
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		label.text = localizedString;
		return this;
	}

	public ButtonComponent WithLabel(string text)
	{
		Debug.LogWarning("ButtonComponent using unlocalized text.");
		label.text = text;
		return this;
	}

	public ButtonComponent WithBackgroundColor(Color color)
	{
		button.targetGraphic.color = color;
		return this;
	}

	private void Start()
	{
		button.onClick.AddListener(delegate
		{
			onClick?.Invoke();
		});
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}
}
