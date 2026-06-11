using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindsController : MonoBehaviour, CMInput.IUtilsActions
{
	public static char InternalKeybindIdentifier = '+';

	public static char PersistentKeybindIdentifier = '=';

	public static Vector2 MousePosition = Vector2.zero;

	public static bool IsMouseInWindow { get; private set; } = true;

	public void OnControlModifier(InputAction.CallbackContext context)
	{
	}

	public void OnAltModifier(InputAction.CallbackContext context)
	{
	}

	public void OnShiftModifier(InputAction.CallbackContext context)
	{
	}

	public void OnMouseMovement(InputAction.CallbackContext context)
	{
		MousePosition = context.ReadValue<Vector2>();
		IsMouseInWindow = IsMouseInBounds();
	}

	private static bool IsMouseInBounds()
	{
		if (MousePosition.x <= 0f || MousePosition.y <= 0f || MousePosition.x >= (float)(Screen.width - 1) || MousePosition.y >= (float)(Screen.height - 1))
		{
			return false;
		}
		return true;
	}
}
