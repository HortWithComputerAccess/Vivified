using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrecisionStepDisplayController : DisableActionsField
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private TMP_InputField display;

	[SerializeField]
	private TMP_InputField secondDisplay;

	[SerializeField]
	private Outline firstOutline;

	[SerializeField]
	private Outline secondOutline;

	[SerializeField]
	private Color defaultOutlineColor;

	[SerializeField]
	private Color selectedOutlineColor;

	private bool firstActive;

	private void Start()
	{
		display.text = Settings.Instance.CursorPrecisionA.ToString();
		secondDisplay.text = Settings.Instance.CursorPrecisionB.ToString();
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.GridMeasureSnappingChanged = (Action<int>)Delegate.Combine(audioTimeSyncController.GridMeasureSnappingChanged, new Action<int>(UpdateText));
		SelectSnap(first: true);
	}

	private void OnDestroy()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.GridMeasureSnappingChanged = (Action<int>)Delegate.Remove(audioTimeSyncController.GridMeasureSnappingChanged, new Action<int>(UpdateText));
	}

	private void UpdateText(int newSnapping)
	{
		if (firstActive)
		{
			Settings.Instance.CursorPrecisionA = newSnapping;
			display.text = newSnapping.ToString();
		}
		else
		{
			Settings.Instance.CursorPrecisionB = newSnapping;
			secondDisplay.text = newSnapping.ToString();
		}
	}

	public void SelectSnap(bool first)
	{
		firstActive = first;
		firstOutline.effectColor = (first ? selectedOutlineColor : defaultOutlineColor);
		secondOutline.effectColor = ((!first) ? selectedOutlineColor : defaultOutlineColor);
		UpdateManualPrecisionStep(first ? display.text : secondDisplay.text);
	}

	public void SwapSelectedInterval()
	{
		SelectSnap(!firstActive);
	}

	public void UpdateManualPrecisionStep(string result)
	{
		if (int.TryParse(result, out var result2))
		{
			if (result2 < 0)
			{
				Debug.LogError(":hyperPepega: :mega: WHY ARE YOU USING NEGATIVE PRECISION");
				result2 = Mathf.Abs(result2);
			}
			if (result2 == 0)
			{
				Debug.LogError(":hyperPepega: :mega: WHY ARE YOU USING 1/0 PRECISION");
				result2 = 1;
			}
			atsc.GridMeasureSnapping = result2;
		}
	}
}
