using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class TextBoxComponent : CMUIComponentWithLabel<string>, INavigable, IQuickSubmitComponent
{
	[SerializeField]
	private TMP_InputField inputField;

	private Action<string> onEndEdit;

	private Action<string> onSelect;

	private Action<string> onDeselect;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public TextBoxComponent OnEndEdit(Action<string> onEndEdit)
	{
		this.onEndEdit = onEndEdit;
		return this;
	}

	public TextBoxComponent OnSelect(Action<string> onSelect)
	{
		this.onSelect = onSelect;
		return this;
	}

	public TextBoxComponent OnDeselect(Action<string> onDeselect)
	{
		this.onDeselect = onDeselect;
		return this;
	}

	public TextBoxComponent WithContentType(TMP_InputField.ContentType contentType)
	{
		inputField.contentType = contentType;
		return this;
	}

	public TextBoxComponent WithLineType(TMP_InputField.LineType lineType)
	{
		inputField.lineType = lineType;
		return this;
	}

	public TextBoxComponent WithMaximumLength(int characterLength)
	{
		inputField.characterLimit = characterLength;
		return this;
	}

	public TextBoxComponent WithInitialValue(string table, string key, params object[] args)
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		return this.WithInitialValue(localizedString);
	}

	private void Start()
	{
		OnValueUpdated(base.Value);
		inputField.onValueChanged.AddListener(InputFieldValueChanged);
		inputField.onEndEdit.AddListener(InputFieldEndEdit);
		inputField.onSelect.AddListener(InputFieldSelect);
		inputField.onDeselect.AddListener(InputFieldDeselect);
	}

	private void InputFieldValueChanged(string res)
	{
		base.Value = res;
	}

	private void InputFieldEndEdit(string res)
	{
		onEndEdit?.Invoke(res);
	}

	private void InputFieldSelect(string res)
	{
		onSelect?.Invoke(res);
	}

	private void InputFieldDeselect(string res)
	{
		onDeselect?.Invoke(res);
	}

	private void OnDestroy()
	{
		inputField.onValueChanged.RemoveAllListeners();
		inputField.onEndEdit.RemoveAllListeners();
		inputField.onSelect.RemoveAllListeners();
		inputField.onDeselect.RemoveAllListeners();
	}

	protected override void OnValueUpdated(string updatedValue)
	{
		inputField.SetTextWithoutNotify(updatedValue);
	}
}
