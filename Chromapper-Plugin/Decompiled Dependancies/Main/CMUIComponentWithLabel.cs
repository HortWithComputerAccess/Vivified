using TMPro;
using UnityEngine;

public abstract class CMUIComponentWithLabel<T> : CMUIComponent<T>
{
	[SerializeField]
	private TextMeshProUGUI labelText;

	[SerializeField]
	private GameObject labelContainer;

	internal override void SetLabelEnabled(bool enabled)
	{
		labelContainer.SetActive(enabled);
	}

	internal override void SetLabelText(string text)
	{
		labelText.text = text;
	}
}
