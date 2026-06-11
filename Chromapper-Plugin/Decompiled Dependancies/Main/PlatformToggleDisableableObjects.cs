using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformToggleDisableableObjects : MonoBehaviour, CMInput.IPlatformDisableableObjectsActions
{
	private PlatformDescriptor descriptor;

	private void Start()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	public void OnTogglePlatformObjects(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			UpdateDisableableObjects();
		}
	}

	private void PlatformLoaded(PlatformDescriptor obj)
	{
		descriptor = obj;
	}

	public void UpdateDisableableObjects()
	{
		descriptor.ToggleDisablableObjects();
	}
}
