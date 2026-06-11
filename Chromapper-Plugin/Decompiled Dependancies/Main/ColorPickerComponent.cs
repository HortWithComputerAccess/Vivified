using UnityEngine;
using UnityEngine.UI;

public class ColorPickerComponent : CMUIComponent<Color>, INavigable, IQuickSubmitComponent
{
	[SerializeField]
	private ColorPicker picker;

	private bool firstUpdate = true;

	private bool useAlpha = true;

	private float constantAlpha = 1f;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public ColorPickerComponent WithConstantAlpha(float alpha)
	{
		picker.Setup.ShowAlpha = (useAlpha = false);
		constantAlpha = alpha;
		return this;
	}

	public ColorPickerComponent WithAlpha()
	{
		picker.Setup.ShowAlpha = (useAlpha = true);
		return this;
	}

	private void Start()
	{
		picker.ONValueChanged.AddListener(ColorChanged);
		picker.CurrentColor = (useAlpha ? base.Value : base.Value.WithAlpha(constantAlpha));
	}

	private void ColorChanged(Color newColor)
	{
		if (firstUpdate)
		{
			firstUpdate = false;
		}
		else
		{
			base.Value = (useAlpha ? newColor : newColor.WithAlpha(constantAlpha));
		}
	}

	private void OnDestroy()
	{
		picker.ONValueChanged.RemoveAllListeners();
	}
}
