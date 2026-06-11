using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarComponent : CMUIComponentWithLabel<Progress<float>>
{
	[SerializeField]
	private Slider progressSlider;

	private Func<float, string> progressTextFormatter;

	public void UpdateProgressBar(float progress)
	{
		ProgressChanged(null, progress);
	}

	public ProgressBarComponent WithCustomLabelFormatter(Func<float, string> formatter)
	{
		progressTextFormatter = formatter;
		return this;
	}

	protected override void OnValueUpdated(Progress<float> updatedValue)
	{
		base.Value.ProgressChanged -= ProgressChanged;
		base.Value.ProgressChanged += ProgressChanged;
	}

	private void Start()
	{
		if (base.Value != null)
		{
			base.Value.ProgressChanged += ProgressChanged;
		}
		ProgressChanged(null, 0f);
	}

	private void ProgressChanged(object _, float progress)
	{
		if (progressTextFormatter != null)
		{
			SetLabelText(progressTextFormatter(progress));
		}
		else
		{
			SetLabelText($"{progress * 100f:F1}% complete.");
		}
		progressSlider.value = progress;
	}

	private void OnDestroy()
	{
		if (base.Value != null)
		{
			base.Value.ProgressChanged -= ProgressChanged;
		}
	}
}
