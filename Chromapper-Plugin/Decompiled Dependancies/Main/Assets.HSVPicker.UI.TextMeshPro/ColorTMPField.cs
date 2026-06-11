using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.HSVPicker.UI.TextMeshPro;

[RequireComponent(typeof(TMP_InputField))]
public class ColorTMPField : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private ColorPicker picker;

	[SerializeField]
	private ColorValues type;

	[SerializeField]
	private float minValue;

	[SerializeField]
	private float maxValue = 1f;

	[SerializeField]
	private bool clampToValues = true;

	private TMP_InputField input;

	private void Awake()
	{
		input = GetComponent<TMP_InputField>();
	}

	private void OnEnable()
	{
		if (Application.isPlaying && picker != null)
		{
			picker.ONValueChanged.AddListener(ColorChanged);
			picker.OnhsvChanged.AddListener(HSVChanged);
			input.onValueChanged.AddListener(TextChanged);
		}
	}

	private void OnDisable()
	{
		if (picker != null)
		{
			picker.ONValueChanged.RemoveListener(ColorChanged);
			picker.OnhsvChanged.RemoveListener(HSVChanged);
			input.onValueChanged.RemoveListener(TextChanged);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(ColorTMPField), from x in typeof(CMInput).GetNestedTypes()
			where x.IsInterface
			select x);
	}

	public void OnSelect(BaseEventData eventData)
	{
		CMInputCallbackInstaller.DisableActionMaps(typeof(ColorTMPField), from x in typeof(CMInput).GetNestedTypes()
			where x.IsInterface
			select x);
	}

	private void ColorChanged(Color color)
	{
		UpdateValue();
	}

	private void HSVChanged(float hue, float saturation, float value)
	{
		UpdateValue();
	}

	private void UpdateValue()
	{
		if (input.isFocused)
		{
			return;
		}
		if (picker == null)
		{
			input.SetTextWithoutNotify("0");
			return;
		}
		float value = (float)Math.Round(picker.GetValue(type), 3);
		if (clampToValues)
		{
			value = Mathf.Clamp(value, minValue, maxValue);
		}
		input.SetTextWithoutNotify(value.ToString());
	}

	private void TextChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			result = (float)Math.Round(result, 3);
			if (clampToValues)
			{
				result = Mathf.Clamp(result, minValue, maxValue);
			}
			picker.AssignColor(type, result);
		}
	}
}
