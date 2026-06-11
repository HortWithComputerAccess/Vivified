using System;
using Assets.HSVPicker;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	[SerializeField]
	private Toggle placeChromaToggle;

	[Header("Setup")]
	public ColorPickerSetup Setup;

	[FormerlySerializedAs("onValueChanged")]
	[Header("Event")]
	public ColorChangedEvent ONValueChanged = new ColorChangedEvent();

	private float alpha = 1f;

	private float blue;

	private float brightness;

	private float green;

	private float hue;

	private float red = 1f;

	private float saturation;

	public HSVChangedEvent OnhsvChanged = new HSVChangedEvent();

	public Color CurrentColor
	{
		get
		{
			return new Color(red, green, blue, alpha);
		}
		set
		{
			if (!(CurrentColor == value))
			{
				red = value.r;
				green = value.g;
				blue = value.b;
				alpha = value.a;
				RGBChanged();
				SendChangedEvent();
			}
		}
	}

	public float H
	{
		get
		{
			return hue;
		}
		set
		{
			if (hue != value)
			{
				hue = value;
				HSVChanged();
				SendChangedEvent();
			}
		}
	}

	public float S
	{
		get
		{
			return saturation;
		}
		set
		{
			if (saturation != value)
			{
				saturation = value;
				HSVChanged();
				SendChangedEvent();
			}
		}
	}

	public float V
	{
		get
		{
			return brightness;
		}
		set
		{
			if (brightness != value)
			{
				brightness = value;
				HSVChanged();
				SendChangedEvent();
			}
		}
	}

	public float R
	{
		get
		{
			return red;
		}
		set
		{
			if (red != value)
			{
				red = value;
				RGBChanged();
				SendChangedEvent();
			}
		}
	}

	public float G
	{
		get
		{
			return green;
		}
		set
		{
			if (green != value)
			{
				green = value;
				RGBChanged();
				SendChangedEvent();
			}
		}
	}

	public float B
	{
		get
		{
			return blue;
		}
		set
		{
			if (blue != value)
			{
				blue = value;
				RGBChanged();
				SendChangedEvent();
			}
		}
	}

	private float A
	{
		get
		{
			return alpha;
		}
		set
		{
			if (alpha != value)
			{
				alpha = value;
				SendChangedEvent();
			}
		}
	}

	private void Start()
	{
		Setup.AlphaSlidiers.Toggle(Setup.ShowAlpha);
		Setup.ColorToggleElement.Toggle(Setup.ShowColorSliderToggle);
		Setup.RgbSliders.Toggle(Setup.ShowRgb);
		Setup.HsvSliders.Toggle(Setup.ShowHsv);
		Setup.ColorBox.Toggle(Setup.ShowColorBox);
		HandleHeaderSetting(Setup.ShowHeader);
		UpdateColorToggleText();
		RGBChanged();
		SendChangedEvent();
	}

	private void OnDestroy()
	{
		ColourHistory.Save();
	}

	private void RGBChanged()
	{
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(CurrentColor);
		hue = hsvColor.NormalizedH;
		saturation = hsvColor.NormalizedS;
		brightness = hsvColor.NormalizedV;
	}

	private void HSVChanged()
	{
		Color color = HSVUtil.ConvertHsvToRgb(hue * 360f, saturation, brightness, alpha);
		red = color.r;
		green = color.g;
		blue = color.b;
	}

	private void SendChangedEvent(bool updateChroma = true)
	{
		ONValueChanged.Invoke(CurrentColor);
		OnhsvChanged.Invoke(hue, saturation, brightness);
	}

	public void AssignColor(ColorValues type, float value)
	{
		switch (type)
		{
		case ColorValues.R:
			R = value;
			break;
		case ColorValues.G:
			G = value;
			break;
		case ColorValues.B:
			B = value;
			break;
		case ColorValues.A:
			A = value;
			break;
		case ColorValues.Hue:
			H = value;
			break;
		case ColorValues.Saturation:
			S = value;
			break;
		case ColorValues.Value:
			V = value;
			break;
		}
	}

	public float GetValue(ColorValues type)
	{
		return type switch
		{
			ColorValues.R => R, 
			ColorValues.G => G, 
			ColorValues.B => B, 
			ColorValues.A => A, 
			ColorValues.Hue => H, 
			ColorValues.Saturation => S, 
			ColorValues.Value => V, 
			_ => throw new NotImplementedException(""), 
		};
	}

	public void ToggleColorSliders()
	{
		Setup.ShowHsv = !Setup.ShowHsv;
		Setup.ShowRgb = !Setup.ShowRgb;
		Setup.HsvSliders.Toggle(Setup.ShowHsv);
		Setup.RgbSliders.Toggle(Setup.ShowRgb);
		UpdateColorToggleText();
	}

	private void UpdateColorToggleText()
	{
		if (Setup.ShowRgb && (bool)Setup.SliderToggleButtonText)
		{
			Setup.SliderToggleButtonText.text = "RGB";
		}
		if (Setup.ShowHsv && (bool)Setup.SliderToggleButtonText)
		{
			Setup.SliderToggleButtonText.text = "HSV";
		}
	}

	private void HandleHeaderSetting(ColorPickerSetup.ColorHeaderShowing setupShowHeader)
	{
		if (setupShowHeader == ColorPickerSetup.ColorHeaderShowing.Hide)
		{
			Setup.ColorHeader.Toggle(active: false);
			return;
		}
		Setup.ColorHeader.Toggle(active: true);
		Setup.ColorPreview.Toggle(setupShowHeader != ColorPickerSetup.ColorHeaderShowing.ShowColorCode);
		Setup.ColorCode.Toggle(setupShowHeader != ColorPickerSetup.ColorHeaderShowing.ShowColor);
	}
}
