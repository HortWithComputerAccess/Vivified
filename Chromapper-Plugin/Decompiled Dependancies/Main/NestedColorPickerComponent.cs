using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NestedColorPickerComponent : CMUIComponentWithLabel<Color>, INavigable
{
	private static DialogBox nestedDialogBox;

	private static ColorPickerComponent nestedColorPicker;

	private static ButtonComponent submitButton;

	[SerializeField]
	private Button editButton;

	[SerializeField]
	private TextMeshProUGUI hexColorText;

	[SerializeField]
	private Image previewImage;

	private bool useAlpha = true;

	private float constantAlpha = 1f;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public NestedColorPickerComponent WithConstantAlpha(float alpha)
	{
		useAlpha = false;
		constantAlpha = alpha;
		return this;
	}

	public NestedColorPickerComponent WithAlpha()
	{
		useAlpha = true;
		return this;
	}

	private void Start()
	{
		editButton.onClick.AddListener(OnEditButtonClick);
		OnValueUpdated(base.Value);
	}

	private void OnEditButtonClick()
	{
		if (nestedDialogBox == null)
		{
			nestedDialogBox = PersistentUI.Instance.CreateNewDialogBox().DontDestroyOnClose().WithNoTitle();
			nestedColorPicker = nestedDialogBox.AddComponent<ColorPickerComponent>().WithInitialValue(base.Value);
			if (useAlpha)
			{
				nestedColorPicker.WithAlpha();
			}
			else
			{
				nestedColorPicker.WithConstantAlpha(constantAlpha);
			}
			nestedDialogBox.AddFooterButton(null, "PersistentUI", "cancel");
			submitButton = nestedDialogBox.AddFooterButton(delegate
			{
				base.Value = nestedColorPicker.Value;
			}, "PersistentUI", "ok");
			nestedDialogBox.OnQuickSubmit(delegate
			{
				OnValueUpdated(nestedColorPicker.Value);
			});
		}
		else
		{
			submitButton.OnClick(delegate
			{
				base.Value = nestedColorPicker.Value;
				nestedDialogBox.Close();
			});
			nestedDialogBox.OnQuickSubmit(delegate
			{
				OnValueUpdated(nestedColorPicker.Value);
			});
			nestedColorPicker.Value = base.Value;
		}
		nestedDialogBox.Open(GetComponentInParent<DialogBox>());
	}

	protected override void OnValueUpdated(Color updatedValue)
	{
		previewImage.color = updatedValue.WithAlpha(useAlpha ? updatedValue.a : constantAlpha);
		hexColorText.text = (useAlpha ? ("#" + ColorUtility.ToHtmlStringRGBA(updatedValue)) : ("#" + ColorUtility.ToHtmlStringRGB(updatedValue)));
		hexColorText.color = ((HSVUtil.ConvertRgbToHsv(updatedValue).NormalizedV > 0.5f) ? Color.black : Color.white);
	}

	private void OnDestroy()
	{
		editButton.onClick.RemoveAllListeners();
	}
}
