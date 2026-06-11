using UnityEngine;

internal static class ColorExtensions
{
	public static Color WithAlpha(this Color color, float alpha)
	{
		color.a = alpha;
		return color;
	}

	public static Color Set(this Color color, float r, float g, float b, float a)
	{
		color.r = r;
		color.g = g;
		color.b = b;
		color.a = a;
		return color;
	}

	public static Color Multiply(this Color color, float x)
	{
		color.r *= x;
		color.g *= x;
		color.b *= x;
		color.a *= x;
		return color;
	}

	public static Color WithSatuation(this Color color, float saturation)
	{
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(color);
		hsvColor.S = saturation;
		return HSVUtil.ConvertHsvToRgb(hsvColor.H, hsvColor.S, hsvColor.V, color.a);
	}
}
