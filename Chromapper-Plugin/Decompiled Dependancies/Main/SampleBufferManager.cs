using UnityEngine;

public static class SampleBufferManager
{
	private static readonly int frequency = Shader.PropertyToID("SongFrequency");

	private static readonly int sampleCount = Shader.PropertyToID("SampleCount");

	private static readonly int monoSamples = Shader.PropertyToID("MonoSamples");

	private static ComputeBuffer sampleBuffer;

	public static int MonoSampleCount;

	public static float[] MonoSamples;

	public static void GenerateSamplesBuffer(AudioClip clip)
	{
		ClearSamplesBuffer();
		int channels = clip.channels;
		int num = (MonoSampleCount = clip.samples);
		float[] array = new float[num * channels];
		clip.GetData(array, 0);
		MonoSamples = new float[num];
		for (int i = 0; i < array.Length; i++)
		{
			MonoSamples[i / channels] += array[i] / (float)channels / 1.5f;
		}
		sampleBuffer = new ComputeBuffer(MonoSampleCount, 4);
		sampleBuffer.SetData(MonoSamples);
		Shader.SetGlobalInt(frequency, clip.frequency);
		Shader.SetGlobalInt(sampleCount, MonoSampleCount);
		Shader.SetGlobalBuffer(monoSamples, sampleBuffer);
	}

	public static void ClearSamplesBuffer()
	{
		if (sampleBuffer != null)
		{
			sampleBuffer.Dispose();
			sampleBuffer = null;
		}
	}
}
