using System;
using Unity.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	private static readonly int sampleSize = Shader.PropertyToID("SampleSize");

	private static readonly int processingOffset = Shader.PropertyToID("ProcessingOffset");

	private static readonly int fftSize = Shader.PropertyToID("FFTSize");

	private static readonly int fftCount = Shader.PropertyToID("FFTCount");

	private static readonly int fftFrequency = Shader.PropertyToID("FFTFrequency");

	private static readonly int fftScaleFactor = Shader.PropertyToID("FFTScaleFactor");

	private static readonly int fftInitialized = Shader.PropertyToID("FFTInitialized");

	private static readonly int fftQuality = Shader.PropertyToID("FFTQuality");

	private static readonly int multiplyA = Shader.PropertyToID("A");

	private static readonly int multiplyB = Shader.PropertyToID("B");

	private static readonly int initializeBuffer = Shader.PropertyToID("BufferToInitialize");

	private static readonly int fftReal = Shader.PropertyToID("Real");

	private static readonly int fftImaginary = Shader.PropertyToID("Imaginary");

	private static readonly int fftResults = Shader.PropertyToID("FFTResults");

	[SerializeField]
	private ComputeShader multiplyShader;

	[SerializeField]
	private ComputeShader fftShader;

	[SerializeField]
	private ComputeShader initializeShader;

	private ComputeBuffer cachedFFTBuffer;

	private ComputeBuffer dummyBuffer;

	public void GenerateFFT(AudioClip clip, int sampleSize, int quality)
	{
		if (SampleBufferManager.MonoSamples == null)
		{
			throw new InvalidOperationException("remember to call SampleBufferManager first, thanks.");
		}
		ClearFFTCache();
		int monoSampleCount = SampleBufferManager.MonoSampleCount;
		while ((long)monoSampleCount * (long)quality * 4 > SystemInfo.maxGraphicsBufferSize)
		{
			quality /= 2;
			Debug.Log($"FFT buffer exceeded. Reduced spectrogram quality to: {quality}");
		}
		if (quality < 1)
		{
			Debug.LogWarning("Refusing to render spectrogram: Exceeds maximum Compute Buffer size.");
			PersistentUI.Instance.ShowDialogBox("PersistentUI", "spectrofailed.computebuffer", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		long num = (long)SystemInfo.graphicsMemorySize * 1024L * 1024;
		while ((long)monoSampleCount * (long)quality * 4 * 3 > num / 2)
		{
			quality /= 2;
			Debug.Log($"Video Memory exceeded. Reduced spectrogram quality to: {quality}");
		}
		if (quality < 1)
		{
			Debug.LogWarning("Refusing to render spectrogram: Exceeds half of available video memory.");
			PersistentUI.Instance.ShowDialogBox("PersistentUI", "spectrofailed.vram", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		int value = sampleSize / 2;
		int num2 = monoSampleCount * quality;
		float[] windowForSize = WindowCoefficients.GetWindowForSize(sampleSize);
		float value2 = WindowCoefficients.Signal(windowForSize);
		Shader.SetGlobalInt(AudioManager.sampleSize, sampleSize);
		Shader.SetGlobalInt(fftSize, value);
		Shader.SetGlobalInt(fftCount, num2);
		Shader.SetGlobalFloat(fftScaleFactor, value2);
		Shader.SetGlobalFloat(fftFrequency, clip.frequency * quality);
		Shader.SetGlobalFloat(fftQuality, quality);
		cachedFFTBuffer = new ComputeBuffer(num2, 4);
		Shader.SetGlobalBuffer(fftResults, cachedFFTBuffer);
		using ComputeBuffer computeBuffer = new ComputeBuffer(num2, 4);
		using (NativeArray<float> nativeArray = new NativeArray<float>(num2, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
		{
			using ComputeBuffer computeBuffer2 = new ComputeBuffer(sampleSize, 4);
			for (int i = 0; i < monoSampleCount; i += sampleSize / quality)
			{
				int length = Mathf.Clamp(monoSampleCount - i, 0, sampleSize);
				NativeArray<float>.Copy(SampleBufferManager.MonoSamples, i, nativeArray, i * quality, length);
			}
			computeBuffer.SetData(nativeArray);
			computeBuffer2.SetData(windowForSize);
			multiplyShader.SetBuffer(0, multiplyA, computeBuffer);
			multiplyShader.SetBuffer(0, multiplyB, computeBuffer2);
			ExecuteOverLargeArray(multiplyShader, num2);
		}
		using ComputeBuffer buffer = new ComputeBuffer(num2, 4);
		initializeShader.SetBuffer(0, initializeBuffer, buffer);
		ExecuteOverLargeArray(initializeShader, num2);
		fftShader.SetBuffer(0, fftReal, computeBuffer);
		fftShader.SetBuffer(0, fftImaginary, buffer);
		ExecuteOverLargeArray(fftShader, num2 / sampleSize);
		Shader.SetGlobalInt(fftInitialized, 1);
	}

	private static void ExecuteOverLargeArray(ComputeShader shader, int length)
	{
		shader.GetKernelThreadGroupSizes(0, out var x, out var y, out var z);
		int num = (int)(x * y * z);
		int num2;
		for (int i = 0; i < length; i += num2)
		{
			num2 = Mathf.Clamp(length - i, 0, 65535);
			shader.SetInt(processingOffset, i);
			shader.Dispatch(0, num2 / num, 1, 1);
		}
	}

	private void ClearFFTCache()
	{
		if (cachedFFTBuffer != null)
		{
			cachedFFTBuffer.Dispose();
			cachedFFTBuffer = null;
			Shader.SetGlobalInt(fftCount, 0);
			Shader.SetGlobalInt(fftInitialized, 0);
			Shader.SetGlobalBuffer(fftReal, dummyBuffer);
			Shader.SetGlobalBuffer(fftImaginary, dummyBuffer);
			Shader.SetGlobalBuffer(fftResults, dummyBuffer);
		}
	}

	private void Awake()
	{
		dummyBuffer = new ComputeBuffer(1, 4);
		Shader.SetGlobalBuffer(fftReal, dummyBuffer);
		Shader.SetGlobalBuffer(fftImaginary, dummyBuffer);
		Shader.SetGlobalBuffer(fftResults, dummyBuffer);
	}

	private void OnDestroy()
	{
		ClearFFTCache();
		dummyBuffer.Dispose();
		dummyBuffer = null;
	}
}
