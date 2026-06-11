using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(BoxSlider), typeof(RawImage))]
[ExecuteInEditMode]
public class SVBoxSlider : MonoBehaviour
{
	[FormerlySerializedAs("picker")]
	public ColorPicker Picker;

	[SerializeField]
	private bool overrideComputeShader;

	private readonly int textureHeight = 100;

	private readonly int textureWidth = 100;

	private ComputeShader compute;

	private RawImage image;

	private int kernelID;

	private float lastH = -1f;

	private bool listen = true;

	private RenderTexture renderTexture;

	private BoxSlider slider;

	private bool supportsComputeShaders;

	public RectTransform RectTransform => base.transform as RectTransform;

	private void Awake()
	{
		slider = GetComponent<BoxSlider>();
		image = GetComponent<RawImage>();
		if (Application.isPlaying)
		{
			supportsComputeShaders = SystemInfo.supportsComputeShaders;
			if (overrideComputeShader)
			{
				supportsComputeShaders = false;
			}
			if (supportsComputeShaders)
			{
				InitializeCompute();
			}
			RegenerateSvTexture();
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying && Picker != null)
		{
			slider.ONValueChanged.AddListener(SliderChanged);
			Picker.OnhsvChanged.AddListener(HSVChanged);
		}
	}

	private void OnDisable()
	{
		if (Picker != null)
		{
			slider.ONValueChanged.RemoveListener(SliderChanged);
			Picker.OnhsvChanged.RemoveListener(HSVChanged);
		}
	}

	private void OnDestroy()
	{
		if (image.texture != null)
		{
			if (supportsComputeShaders)
			{
				renderTexture.Release();
			}
			else
			{
				Object.DestroyImmediate(image.texture);
			}
		}
	}

	private void InitializeCompute()
	{
		if (renderTexture == null)
		{
			renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.RGB111110Float)
			{
				enableRandomWrite = true
			};
			renderTexture.Create();
		}
		compute = Resources.Load<ComputeShader>("Shaders/Compute/GenerateSVTexture");
		kernelID = compute.FindKernel("CSMain");
		image.texture = renderTexture;
	}

	private void SliderChanged(float saturation, float value)
	{
		if (listen)
		{
			Picker.AssignColor(ColorValues.Saturation, saturation);
			Picker.AssignColor(ColorValues.Value, value);
		}
		listen = true;
	}

	private void HSVChanged(float h, float s, float v)
	{
		if (!lastH.Equals(h))
		{
			lastH = h;
			RegenerateSvTexture();
		}
		if (!s.Equals(slider.NormalizedValue))
		{
			listen = false;
			slider.NormalizedValue = s;
		}
		if (!v.Equals(slider.NormalizedValueY))
		{
			listen = false;
			slider.NormalizedValueY = v;
		}
	}

	private void RegenerateSvTexture()
	{
		if (supportsComputeShaders)
		{
			float val = ((Picker != null) ? Picker.H : 0f);
			compute.SetTexture(kernelID, "Texture", renderTexture);
			compute.SetFloat("Hue", val);
			int threadGroupsX = Mathf.CeilToInt((float)textureWidth / 32f);
			int threadGroupsY = Mathf.CeilToInt((float)textureHeight / 32f);
			compute.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1);
			return;
		}
		double h = ((Picker != null) ? (Picker.H * 360f) : 0f);
		if (image.texture != null)
		{
			Object.DestroyImmediate(image.texture);
		}
		Texture2D texture2D = new Texture2D(textureWidth, textureHeight)
		{
			hideFlags = HideFlags.DontSave
		};
		for (int i = 0; i < textureWidth; i++)
		{
			Color32[] array = new Color32[textureHeight];
			for (int j = 0; j < textureHeight; j++)
			{
				array[j] = HSVUtil.ConvertHsvToRgb(h, (float)i / 100f, (float)j / 100f, 1f);
			}
			texture2D.SetPixels32(i, 0, 1, textureHeight, array);
		}
		texture2D.Apply();
		image.texture = texture2D;
	}
}
