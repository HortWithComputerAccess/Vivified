using System;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.UI;

public class ColourPicker : MonoBehaviour
{
	[SerializeField]
	private ColorPicker picker;

	[SerializeField]
	private ToggleColourDropdown dropdown;

	[SerializeField]
	private EventGridContainer eventGridContainer;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Toggle placeChromaToggle;

	private void Start()
	{
		SelectionController.ObjectWasSelectedEvent = (Action<BaseObject>)Delegate.Combine(SelectionController.ObjectWasSelectedEvent, new Action<BaseObject>(SelectedObject));
		toggle.isOn = Settings.Instance.PickColorFromChromaEvents;
		placeChromaToggle.isOn = Settings.Instance.PlaceChromaColor;
	}

	private void OnDestroy()
	{
		SelectionController.ObjectWasSelectedEvent = (Action<BaseObject>)Delegate.Remove(SelectionController.ObjectWasSelectedEvent, new Action<BaseObject>(SelectedObject));
	}

	public void UpdateColourPicker(bool enabled)
	{
		Settings.Instance.PickColorFromChromaEvents = enabled;
	}

	private void SelectedObject(BaseObject obj)
	{
		if (!Settings.Instance.PickColorFromChromaEvents || !dropdown.Visible)
		{
			return;
		}
		if (obj.CustomColor.HasValue)
		{
			picker.CurrentColor = obj.CustomColor.Value;
		}
		if (obj is BaseEvent baseEvent)
		{
			if (baseEvent.Value >= 2000000000)
			{
				picker.CurrentColor = ColourManager.ColourFromInt(baseEvent.Value);
			}
			else if (baseEvent.CustomLightGradient != null)
			{
				picker.CurrentColor = baseEvent.CustomLightGradient.StartColor;
			}
		}
	}
}
