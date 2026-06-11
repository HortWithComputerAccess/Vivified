using UnityEngine;

public static class ColorBufferManager
{
	private static readonly int gradientLength = Shader.PropertyToID("GradientLength");

	private static readonly int gradientKeys = Shader.PropertyToID("GradientKeys");

	private static readonly int gradientColors = Shader.PropertyToID("GradientColors");

	private static ComputeBuffer keysBuffer;

	private static ComputeBuffer colorsBuffer;

	public static void GenerateBuffersForGradient(Gradient heightmapGradient)
	{
		if (keysBuffer != null)
		{
			ClearBuffers();
		}
		GradientColorKey[] colorKeys = heightmapGradient.colorKeys;
		int num = colorKeys.Length;
		float[] array = new float[num];
		Color[] array2 = new Color[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = colorKeys[i].time;
			array2[i] = colorKeys[i].color;
		}
		keysBuffer = new ComputeBuffer(num, 4);
		keysBuffer.SetData(array);
		colorsBuffer = new ComputeBuffer(num, 16);
		colorsBuffer.SetData(array2);
		Shader.SetGlobalInt(gradientLength, num);
		Shader.SetGlobalBuffer(gradientKeys, keysBuffer);
		Shader.SetGlobalBuffer(gradientColors, colorsBuffer);
	}

	public static void ClearBuffers()
	{
		if (keysBuffer != null)
		{
			keysBuffer.Dispose();
			keysBuffer = null;
			colorsBuffer.Dispose();
			colorsBuffer = null;
		}
	}
}
