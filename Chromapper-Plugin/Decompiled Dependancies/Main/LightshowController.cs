using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class LightshowController : MonoBehaviour, CMInput.ILightshowActions
{
	[FormerlySerializedAs("ThingsToToggle")]
	[SerializeField]
	private GameObject[] thingsToToggle;

	[SerializeField]
	private CameraController cameraController;

	private bool previouslyLocked;

	private bool showObjects = true;

	public void OnToggleLightshowMode(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			if (showObjects)
			{
				previouslyLocked = cameraController.LockedOntoNoteGrid;
				cameraController.LockedOntoNoteGrid = false;
				UpdateLightshow(enable: false);
			}
			else
			{
				UpdateLightshow(enable: true);
				cameraController.LockedOntoNoteGrid = previouslyLocked;
			}
		}
	}

	public void UpdateLightshow(bool enable)
	{
		showObjects = enable;
		GameObject[] array = thingsToToggle;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(enable);
		}
	}
}
