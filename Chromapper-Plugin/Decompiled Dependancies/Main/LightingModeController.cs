using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class LightingModeController : MonoBehaviour
{
	public enum LightingMode
	{
		[PickerChoice("Mapper", "bar.events.on")]
		On,
		[PickerChoice("Mapper", "bar.events.off")]
		Off,
		[PickerChoice("Mapper", "bar.events.flash")]
		Flash,
		[PickerChoice("Mapper", "bar.events.fade")]
		Fade,
		[PickerChoice("Mapper", "bar.events.transition")]
		Transition
	}

	[SerializeField]
	private EnumPicker lightingPicker;

	[SerializeField]
	private EventPlacement eventPlacement;

	[SerializeField]
	private NotePlacement notePlacement;

	[SerializeField]
	private MaskableGraphic modeLock;

	[SerializeField]
	private Sprite lockedSprite;

	[SerializeField]
	private Sprite unlockedSprite;

	private LightingMode currentMode;

	private bool modeLocked;

	private void Start()
	{
		lightingPicker.Initialize(typeof(LightingMode));
		SetLocked(locked: false);
		lightingPicker.OnClick += UpdateMode;
	}

	public void SetMode(Enum lightingMode)
	{
		if (!modeLocked)
		{
			lightingPicker.Select(lightingMode);
			UpdateMode(lightingMode);
		}
	}

	public void SetLocked(bool locked)
	{
		modeLocked = locked;
		lightingPicker.Locked = modeLocked;
		if (modeLock is Image image)
		{
			image.sprite = (modeLocked ? lockedSprite : unlockedSprite);
		}
		else if (modeLock is SVGImage sVGImage)
		{
			sVGImage.sprite = (modeLocked ? lockedSprite : unlockedSprite);
		}
	}

	public void ToggleLock()
	{
		SetLocked(!modeLocked);
	}

	public void UpdateValue()
	{
		bool flag = notePlacement.queuedData.Type == 0;
		bool flag2 = notePlacement.queuedData.Type == 3;
		switch (currentMode)
		{
		case LightingMode.Off:
			eventPlacement.UpdateValue(0);
			break;
		case LightingMode.On:
			eventPlacement.UpdateValue(flag ? 5 : ((!flag2) ? 1 : 9));
			break;
		case LightingMode.Flash:
			eventPlacement.UpdateValue(flag ? 6 : (flag2 ? 10 : 2));
			break;
		case LightingMode.Fade:
			eventPlacement.UpdateValue(flag ? 7 : (flag2 ? 11 : 3));
			break;
		case LightingMode.Transition:
			eventPlacement.UpdateValue(flag ? 8 : (flag2 ? 12 : 4));
			break;
		}
	}

	private void UpdateMode(Enum lightingMode)
	{
		currentMode = (LightingMode)(object)lightingMode;
		UpdateValue();
	}
}
