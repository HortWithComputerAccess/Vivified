using UnityEngine;
using UnityEngine.InputSystem;

public class RemotePlayerInputController : MonoBehaviour, CMInput.IUnitedMappingActions
{
	public void OnKickPlayer(InputAction.CallbackContext context)
	{
		Ray ray = Camera.main.ScreenPointToRay(KeybindsController.MousePosition);
		if (context.performed && Intersections.Raycast(ray, 13, out var hit))
		{
			RemotePlayerContainer componentInParent = hit.GameObject.GetComponentInParent<RemotePlayerContainer>();
			if (componentInParent != null)
			{
				componentInParent.Kick();
			}
		}
	}

	public void OnBanPlayer(InputAction.CallbackContext context)
	{
		Ray ray = Camera.main.ScreenPointToRay(KeybindsController.MousePosition);
		if (context.performed && Intersections.Raycast(ray, 13, out var hit))
		{
			RemotePlayerContainer componentInParent = hit.GameObject.GetComponentInParent<RemotePlayerContainer>();
			if (componentInParent != null)
			{
				componentInParent.Ban();
			}
		}
	}
}
