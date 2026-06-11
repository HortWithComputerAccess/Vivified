using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderComponent : CMUIComponentWithLabel<float>, INavigable
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI display;

	private float precision;

	private Func<float, string> sliderTextFormatter;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public SliderComponent WithSliderParams(float minValue = 0f, float maxValue = 1f, float precision = 0f)
	{
		slider.minValue = minValue / ((precision == 0f) ? 1f : precision);
		slider.maxValue = maxValue / ((precision == 0f) ? 1f : precision);
		slider.wholeNumbers = precision != 0f;
		this.precision = precision;
		return this;
	}

	public SliderComponent WithDisplay(bool visible)
	{
		display.gameObject.SetActive(visible);
		return this;
	}

	public SliderComponent WithCustomDisplayFormatter(Func<float, string> formatter)
	{
		sliderTextFormatter = formatter;
		return this;
	}

	protected override void OnValueUpdated(float updatedValue)
	{
		if (precision != 0f)
		{
			updatedValue /= precision;
		}
		slider.SetValueWithoutNotify(updatedValue);
		UpdateText();
	}

	protected override float ValidateValue(float rawValue)
	{
		if (precision != 0f)
		{
			return Mathf.Clamp(rawValue, slider.minValue * precision, slider.maxValue * precision);
		}
		return Mathf.Clamp(rawValue, slider.minValue, slider.maxValue);
	}

	private void Start()
	{
		OnValueUpdated(base.Value);
		slider.onValueChanged.AddListener(SliderValueChanged);
	}

	private void SliderValueChanged(float value)
	{
		base.Value = ((precision != 0f) ? (value * precision) : value);
	}

	private void UpdateText()
	{
		display.text = ((sliderTextFormatter == null) ? base.Value.ToString("F1") : sliderTextFormatter(base.Value));
	}

	private void OnDestroy()
	{
		slider.onValueChanged.RemoveAllListeners();
	}
}
