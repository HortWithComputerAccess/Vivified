using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIScaleSlider : BetterSlider
{
	[FormerlySerializedAs("canvasScaler")]
	public CanvasScaler CanvasScaler;

	private Vector2 referenceResolution;

	protected override void Start()
	{
		referenceResolution = CanvasScaler.referenceResolution;
		base.Start();
	}

	public void OnPointerDown()
	{
		CanvasScaler.gameObject.SetActive(value: true);
	}

	public void OnPointerUp()
	{
		CanvasScaler.gameObject.SetActive(value: false);
		SendMessage("SendValueToSettings", base.Value);
	}

	protected override void UpdateDisplay(bool _)
	{
		CanvasScaler.referenceResolution = referenceResolution * base.Value;
		ValueString.StringReference.RefreshString();
		if (DecimalsMustMatchForDefault)
		{
			ValueText.color = ((DefaultSliderValue.ToString($"F{DecimalPlaces}") == base.Value.ToString($"F{DecimalPlaces}")) ? new Color(1f, 0.75f, 0.23f) : Color.white);
		}
		else
		{
			ValueText.color = ((DefaultSliderValue.ToString("F0") == base.Value.ToString("F0")) ? new Color(1f, 0.75f, 0.23f) : Color.white);
		}
	}
}
