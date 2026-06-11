using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BetterSlider : MonoBehaviour
{
	private const float slideSpeed = 0.02f;

	[FormerlySerializedAs("showPercent")]
	[Header("Percent Settings:")]
	public bool ShowPercent;

	[FormerlySerializedAs("percentMatchesValues")]
	[Tooltip("Allows for percents that are negative and greater than 100%.")]
	public bool PercentMatchesValues;

	[FormerlySerializedAs("multipleOffset")]
	public float MultipleOffset = 10f;

	[FormerlySerializedAs("power")]
	public bool Power;

	[FormerlySerializedAs("showValue")]
	[Header("Value Settings:")]
	public bool ShowValue;

	[FormerlySerializedAs("decimalPlaces")]
	[Header("\n")]
	public int DecimalPlaces;

	[FormerlySerializedAs("defaultSliderValue")]
	[Header("Other Settings")]
	[Tooltip("Must be value slider shows.")]
	public float DefaultSliderValue = 12345.12f;

	[FormerlySerializedAs("_decimalsMustMatchForDefault")]
	public bool DecimalsMustMatchForDefault = true;

	[FormerlySerializedAs("slider")]
	public Slider Slider;

	[FormerlySerializedAs("description")]
	public TextMeshProUGUI Description;

	[SerializeField]
	private Image ringImage;

	[FormerlySerializedAs("valueString")]
	public LocalizeStringEvent ValueString;

	[FormerlySerializedAs("valueText")]
	public TextMeshProUGUI ValueText;

	private Coroutine moveRingCoroutine;

	public string TextValue
	{
		get
		{
			string text = "";
			if (ShowPercent && !PercentMatchesValues)
			{
				text = ((Value + Mathf.Abs(Slider.minValue)) / (Slider.maxValue + Mathf.Abs(Slider.minValue)) * 100f).ToString("F" + DecimalPlaces) + "%";
			}
			else if (PercentMatchesValues)
			{
				text = (Power ? Math.Pow(MultipleOffset, Value) : ((double)(Value * MultipleOffset))).ToString("F" + DecimalPlaces);
			}
			else if (ShowValue)
			{
				text = Value.ToString("F" + DecimalPlaces);
			}
			if (ShowPercent)
			{
				text += "%";
			}
			return text;
		}
	}

	public float Value
	{
		get
		{
			return Slider.value;
		}
		set
		{
			Slider.value = value;
		}
	}

	protected virtual void Start()
	{
		if (TryGetComponent<SettingsBinder>(out var component))
		{
			Slider.onValueChanged.AddListener(OnHandleMove);
			float valueWithoutNotify = Convert.ToSingle(component.RetrieveValueFromSettings());
			Slider.SetValueWithoutNotify(valueWithoutNotify);
			UpdateDisplay(sendToSettings: false);
		}
	}

	private void OnHandleMove(float value)
	{
		moveRingCoroutine = StartCoroutine(MoveRing());
		UpdateDisplay();
	}

	protected virtual void UpdateDisplay(bool sendToSettings = true)
	{
		ValueString.StringReference.RefreshString();
		float num = (PercentMatchesValues ? MultipleOffset : 1f);
		if (DecimalsMustMatchForDefault)
		{
			ValueText.color = (((DefaultSliderValue * num).ToString($"F{DecimalPlaces}") == (Value * num).ToString($"F{DecimalPlaces}")) ? new Color(1f, 0.75f, 0.23f) : Color.white);
		}
		else
		{
			ValueText.color = (((DefaultSliderValue * num).ToString("F0") == (Value * num).ToString("F0")) ? new Color(1f, 0.75f, 0.23f) : Color.white);
		}
		if (sendToSettings)
		{
			SendMessage("SendValueToSettings", Value);
		}
	}

	private IEnumerator MoveRing()
	{
		if (moveRingCoroutine != null)
		{
			StopCoroutine(moveRingCoroutine);
		}
		float startTime = Time.time;
		while (true)
		{
			float fillAmount = ringImage.fillAmount;
			float target = (Value - Slider.minValue) / (Slider.maxValue - Slider.minValue);
			fillAmount = Mathf.MoveTowardsAngle(fillAmount, target, Time.time / startTime * 0.02f);
			ringImage.fillAmount = fillAmount;
			yield return new WaitForFixedUpdate();
		}
	}
}
