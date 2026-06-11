using System;
using UnityEngine;

public static class WindowCoefficients
{
	public static float[] GetWindowForSize(int sampleSize)
	{
		return SineExpansion(sampleSize, 0.35875f, -0.48829f, 0.14128f, -0.01168f);
	}

	public static float Signal(float[] window)
	{
		float num = 0f;
		for (int i = 0; i < window.Length; i++)
		{
			num += window[i];
		}
		return 1f / (num / (float)window.Length);
	}

	private static float[] SineExpansion(int points, params float[] coefficients)
	{
		float[] array = new float[points];
		for (int i = 0; i < points; i++)
		{
			float num = MathF.PI * 2f * (float)i / (float)points;
			float num2 = coefficients[0];
			for (int j = 1; j < coefficients.Length; j++)
			{
				num2 += coefficients[j] * Mathf.Cos((float)j * num);
			}
			array[i] = num2;
		}
		return array;
	}
}
