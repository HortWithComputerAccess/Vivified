using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformSoloEventTypeUIController : MonoBehaviour, CMInput.IPlatformSoloLightGroupActions
{
	[SerializeField]
	private TextMeshProUGUI soloEventTypeLabel;

	private PlatformDescriptor descriptor;

	private void Start()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	public void OnSoloEventType(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			UpdateSoloEventType();
		}
	}

	private void PlatformLoaded(PlatformDescriptor obj)
	{
		descriptor = obj;
	}

	public void UpdateSoloEventType()
	{
		PersistentUI.Instance.ShowInputBox("Please enter the Event Type or its label.", HandleUpdateSoloEventType);
	}

	private void HandleUpdateSoloEventType(string res)
	{
		int result;
		if (string.IsNullOrEmpty(res) || string.IsNullOrWhiteSpace(res))
		{
			descriptor.UpdateSoloEventType(solo: false, 0);
		}
		else if (int.TryParse(res, out result))
		{
			if (result >= 0 && result < descriptor.LightingManagers.Length)
			{
				descriptor.UpdateSoloEventType(solo: true, result);
			}
		}
		else if (descriptor.LightingManagers.Any((LightsManager x) => x.name == res))
		{
			descriptor.UpdateSoloEventType(solo: true, descriptor.LightingManagers.ToList().IndexOf(descriptor.LightingManagers.First((LightsManager x) => x.name == res)));
		}
		soloEventTypeLabel.gameObject.SetActive(descriptor.SoloAnEventType);
		soloEventTypeLabel.text = "Soloing <u>" + descriptor.LightingManagers[descriptor.SoloEventType].name + "</u>";
	}
}
