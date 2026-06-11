using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExtensionButtonUI : MonoBehaviour
{
	[SerializeField]
	private Tooltip tooltip;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Button button;

	public string Tooltip
	{
		get
		{
			return tooltip.TooltipOverride;
		}
		set
		{
			tooltip.TooltipOverride = value;
		}
	}

	public Sprite Icon
	{
		get
		{
			return icon.sprite;
		}
		set
		{
			icon.sprite = value;
		}
	}

	public bool Visible
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool Interactable
	{
		get
		{
			return button.interactable;
		}
		set
		{
			button.interactable = value;
		}
	}

	public void Init(ExtensionButton extenstionButton)
	{
		extenstionButton.buttonUI = this;
		Tooltip = extenstionButton.Tooltip;
		Icon = extenstionButton.Icon;
		SetClickAction(extenstionButton.Click);
		Interactable = extenstionButton.Interactable;
		Visible = extenstionButton.Visible;
	}

	public void SetClickAction(UnityAction onClick)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(onClick);
	}
}
