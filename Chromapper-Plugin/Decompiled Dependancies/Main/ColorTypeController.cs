using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorTypeController : MonoBehaviour
{
	[SerializeField]
	private NotePlacement notePlacement;

	[SerializeField]
	private LightingModeController lightMode;

	[SerializeField]
	private CustomColorsUIController customColors;

	[SerializeField]
	private Image leftSelected;

	[SerializeField]
	private Image rightSelected;

	[SerializeField]
	private Image leftNote;

	[SerializeField]
	private Image leftLight;

	[SerializeField]
	private Image rightNote;

	[SerializeField]
	private Image rightLight;

	private PlatformDescriptor platform;

	private void Start()
	{
		leftSelected.enabled = true;
		rightSelected.enabled = false;
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(SetupColors));
		customColors.CustomColorsUpdatedEvent += UpdateColors;
	}

	private void OnDestroy()
	{
		customColors.CustomColorsUpdatedEvent -= UpdateColors;
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(SetupColors));
	}

	private void SetupColors(PlatformDescriptor descriptor)
	{
		platform = descriptor;
		UpdateColors();
	}

	private void UpdateColors()
	{
		leftNote.color = platform.Colors.RedNoteColor;
		leftLight.color = platform.Colors.RedColor;
		rightNote.color = platform.Colors.BlueNoteColor;
		rightLight.color = platform.Colors.BlueColor;
	}

	public void RedNote(bool active)
	{
		if (active)
		{
			UpdateValue(0);
		}
	}

	public void BlueNote(bool active)
	{
		if (active)
		{
			UpdateValue(1);
		}
	}

	public void BombNote(bool active)
	{
		if (active)
		{
			UpdateValue(3);
		}
	}

	public void UpdateValue(int type)
	{
		notePlacement.UpdateType(type);
		lightMode.UpdateValue();
		UpdateUI();
	}

	public void UpdateUI()
	{
		leftSelected.enabled = notePlacement.queuedData.Type == 0;
		rightSelected.enabled = notePlacement.queuedData.Type == 1;
	}

	public bool LeftSelectedEnabled()
	{
		return leftSelected.enabled;
	}

	public bool RightSelectEnalbed()
	{
		return rightSelected.enabled;
	}
}
