using UnityEngine;

public class ColourManager
{
	public const int RgbintOffset = 2000000000;

	public const int RGBReset = 1900000001;

	public const int RGBAlt = 1900000002;

	public const int RGBWhite = 1900000003;

	public const int RGBTechni = 1900000004;

	public const int RGBRandom = 1900000005;

	public static readonly Color DefaultLightAltA = new Color(1f, 0.032f, 1f, 1f);

	public static readonly Color DefaultLightAltB = new Color(0.016f, 1f, 0.016f, 1f);

	public static readonly Color DefaultLightWhite = new Color(1f, 1f, 1f, 1f);

	public static int ColourToInt(Color color)
	{
		int num = Mathf.FloorToInt(color.r * 255f);
		int num2 = Mathf.FloorToInt(color.g * 255f);
		int num3 = Mathf.FloorToInt(color.b * 255f);
		return 2000000000 + (((num & 0xFF) << 16) | ((num2 & 0xFF) << 8) | (num3 & 0xFF));
	}

	public static Color ColourFromInt(int rgb)
	{
		rgb -= 2000000000;
		int num = (rgb >> 16) & 0xFF;
		int num2 = (rgb >> 8) & 0xFF;
		int num3 = rgb & 0xFF;
		return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f, 1f);
	}
}
