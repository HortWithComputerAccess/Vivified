using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProcessingController : MonoBehaviour
{
	public Volume PostProcess;

	[SerializeField]
	private Slider intensitySlider;

	[SerializeField]
	private TextMeshProUGUI intensityLabel;

	[SerializeField]
	private Toggle chromaticAberration;

	private void Start()
	{
		Settings.NotifyBySettingName("PostProcessingIntensity", UpdatePostProcessIntensity);
		Settings.NotifyBySettingName("ChromaticAberration", UpdateChromaticAberration);
		Settings.NotifyBySettingName("HighQualityBloom", UpdateHighQualityBloom);
		UpdatePostProcessIntensity(Settings.Instance.PostProcessingIntensity);
		UpdateChromaticAberration(Settings.Instance.ChromaticAberration);
		UpdateHighQualityBloom(Settings.Instance.HighQualityBloom);
	}

	public void UpdatePostProcessIntensity(object o)
	{
		float value = Convert.ToSingle(o);
		PostProcess.profile.TryGet<Bloom>(out var component);
		component.intensity.value = value;
	}

	public void UpdateChromaticAberration(object o)
	{
		bool active = Convert.ToBoolean(o);
		PostProcess.profile.TryGet<ChromaticAberration>(out var component);
		component.active = active;
	}

	public void UpdateHighQualityBloom(object obj)
	{
		bool value = Convert.ToBoolean(obj);
		PostProcess.profile.TryGet<Bloom>(out var component);
		component.highQualityFiltering.value = value;
	}
}
