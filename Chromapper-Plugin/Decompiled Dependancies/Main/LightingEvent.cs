using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LightingEvent : MonoBehaviour
{
	public bool OverrideLightGroup;

	public int OverrideLightGroupID;

	public bool UseInvertedPlatformColors;

	public bool CanBeTurnedOff = true;

	[SerializeField]
	private float currentAlpha;

	[SerializeField]
	private float multiplyAlpha = 1f;

	[FormerlySerializedAs("lightID")]
	public int LightID;

	[FormerlySerializedAs("propGroup")]
	public int PropGroup;

	private float alphaTime;

	private float colorTime;

	private Color currentColor = Color.white;

	private MaterialPropertyBlock lightPropertyBlock;

	private Renderer lightRenderer;

	private float targetAlpha;

	private Color targetColor = Color.white;

	private float timeToTransitionAlpha;

	private float timeToTransitionColor;

	private BoostSprite boostSprite;

	private bool isLightEnabled = true;

	private Func<float, float> easing = Easing.ByName["easeLinear"];

	private static readonly int mainTex = Shader.PropertyToID("_MainTex");

	private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");

	private static readonly int baseColor = Shader.PropertyToID("_BaseColor");

	private void Start()
	{
		lightPropertyBlock = new MaterialPropertyBlock();
		lightRenderer = GetComponentInChildren<Renderer>();
		boostSprite = GetComponent<BoostSprite>();
		if (lightRenderer is SpriteRenderer spriteRenderer)
		{
			if (boostSprite != null)
			{
				boostSprite.Setup(spriteRenderer.sprite);
			}
			lightPropertyBlock.SetTexture(mainTex, spriteRenderer.sprite.texture);
		}
		if (!OverrideLightGroup)
		{
			return;
		}
		PlatformDescriptor platform = LoadInitialMap.Platform;
		if (!(platform != null) || OverrideLightGroupID < 0 || OverrideLightGroupID >= platform.LightingManagers.Length)
		{
			return;
		}
		LightsManager lightsManager = platform.LightingManagers[OverrideLightGroupID];
		while (true)
		{
			Dictionary<int, int> lightIDPlacementMapReverse = lightsManager.LightIDPlacementMapReverse;
			if (lightIDPlacementMapReverse == null || !lightIDPlacementMapReverse.ContainsKey(LightID))
			{
				break;
			}
			LightID++;
		}
		lightsManager.ControllingLights.Add(this);
		lightsManager.LoadOldLightOrder();
	}

	private void OnDestroy()
	{
		if (OverrideLightGroup)
		{
			PlatformDescriptor platform = LoadInitialMap.Platform;
			if (platform != null && OverrideLightGroupID >= 0 && OverrideLightGroupID < platform.LightingManagers.Length)
			{
				LightsManager obj = platform.LightingManagers[OverrideLightGroupID];
				obj.ControllingLights.Remove(this);
				obj.LightIDPlacementMapReverse?.Remove(LightID);
			}
		}
	}

	private void Update()
	{
		if (float.IsNaN(multiplyAlpha))
		{
			multiplyAlpha = 0f;
		}
		colorTime += Time.deltaTime;
		Color value = ((timeToTransitionColor == 0f) ? targetColor : Color.Lerp(currentColor, targetColor, easing(colorTime / timeToTransitionColor)));
		alphaTime += Time.deltaTime;
		float num = ((timeToTransitionAlpha == 0f) ? targetAlpha : (Mathf.Lerp(currentAlpha, targetAlpha, easing(alphaTime / timeToTransitionAlpha)) * multiplyAlpha));
		SetEmission(num > 0f);
		if (isLightEnabled)
		{
			lightPropertyBlock.SetColor(emissionColor, value);
			lightPropertyBlock.SetColor(baseColor, Color.white * num);
			lightRenderer.SetPropertyBlock(lightPropertyBlock);
		}
	}

	public void UpdateEasing(string easingName)
	{
		easing = Easing.ByName[easingName];
	}

	public void UpdateEasing(Func<float, float> _easing)
	{
		easing = _easing;
	}

	public void UpdateTargetColor(Color target, float timeToTransition)
	{
		targetColor = target;
		timeToTransitionColor = timeToTransition;
		colorTime = 0f;
		if (timeToTransition == 0f)
		{
			currentColor = target;
		}
	}

	public void UpdateTargetAlpha(float target, float timeToTransition)
	{
		targetAlpha = target;
		timeToTransitionAlpha = timeToTransition;
		alphaTime = 0f;
		if (timeToTransition == 0f)
		{
			currentAlpha = target;
		}
	}

	public void UpdateMultiplyAlpha(float target = 1f)
	{
		multiplyAlpha = Mathf.Clamp(target, 0f, 1.5f);
	}

	public void UpdateBoostState(bool boost)
	{
		if (boostSprite != null)
		{
			lightPropertyBlock.SetTexture(mainTex, boostSprite.GetSprite(boost).texture);
		}
	}

	public void UpdateCurrentColor(Color color)
	{
		currentColor = color;
	}

	public void UpdateTargetAlpha(float target)
	{
		targetAlpha = target;
	}

	private void SetEmission(bool enabled)
	{
		if (isLightEnabled != enabled)
		{
			lightRenderer.enabled = (isLightEnabled = enabled);
		}
	}
}
