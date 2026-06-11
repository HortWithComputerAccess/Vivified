using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindUpdateUIController : MonoBehaviour, CMInput.IWorkflowsActions, CMInput.IEventUIActions
{
	[SerializeField]
	private PlacementModeController placeMode;

	[SerializeField]
	private LightingModeController lightMode;

	[SerializeField]
	private EventPlacement eventPlacement;

	[SerializeField]
	private PrecisionStepDisplayController stepController;

	[SerializeField]
	private RightButtonPanel rightButtonPanel;

	[SerializeField]
	private MirrorSelection mirror;

	[SerializeField]
	private ColorTypeController colorType;

	[SerializeField]
	private Toggle redToggle;

	[SerializeField]
	private Toggle blueToggle;

	[SerializeField]
	private GameObject precisionRotationContainer;

	private void Awake()
	{
		UpdatePrecisionRotationGameObjectState();
	}

	public void OnTypeOn(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			lightMode.SetMode(LightingModeController.LightingMode.On);
		}
	}

	public void OnTypeFlash(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			lightMode.SetMode(LightingModeController.LightingMode.Flash);
		}
	}

	public void OnTypeOff(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			lightMode.SetMode(LightingModeController.LightingMode.Off);
		}
	}

	public void OnTypeFade(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			lightMode.SetMode(LightingModeController.LightingMode.Fade);
		}
	}

	public void OnTypeTransition(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			lightMode.SetMode(LightingModeController.LightingMode.Transition);
		}
	}

	public void OnTogglePrecisionRotation(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			eventPlacement.PlacePrecisionRotation = !eventPlacement.PlacePrecisionRotation;
			UpdatePrecisionRotationGameObjectState();
		}
	}

	public void OnSwapCursorInterval(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			stepController.SwapSelectedInterval();
		}
	}

	public void OnToggleRightButtonPanel(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			rightButtonPanel.TogglePanel();
		}
	}

	public void OnPlaceBlueNoteorEvent(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			blueToggle.onValueChanged.Invoke(arg0: true);
			placeMode.SetMode(PlacementModeController.PlacementMode.Note);
		}
	}

	public void OnPlaceRedNoteorEvent(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			redToggle.onValueChanged.Invoke(arg0: true);
			placeMode.SetMode(PlacementModeController.PlacementMode.Note);
		}
	}

	public void OnToggleNoteorEvent(InputAction.CallbackContext context)
	{
		if (context.performed && !eventPlacement.queuedData.IsWhite)
		{
			if (colorType.LeftSelectedEnabled())
			{
				blueToggle.onValueChanged.Invoke(arg0: true);
			}
			else
			{
				redToggle.onValueChanged.Invoke(arg0: true);
			}
			lightMode.UpdateValue();
		}
	}

	public void OnPlaceBomb(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			placeMode.SetMode(PlacementModeController.PlacementMode.Bomb);
			colorType.BombNote(active: true);
			lightMode.UpdateValue();
		}
	}

	public void OnPlaceObstacle(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			placeMode.SetMode(PlacementModeController.PlacementMode.Wall);
		}
	}

	public void OnToggleDeleteTool(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			placeMode.SetMode(PlacementModeController.PlacementMode.Delete);
		}
	}

	public void OnMirror(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			mirror.Mirror();
		}
	}

	public void OnMirrorinTime(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			mirror.MirrorTime();
		}
	}

	public void OnMirrorColoursOnly(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			mirror.Mirror(moveNotes: false);
		}
	}

	public void OnUpdateSwingArcVisualizer(InputAction.CallbackContext context)
	{
	}

	public void UpdatePrecisionRotation(string res)
	{
		if (int.TryParse(res, out var result))
		{
			eventPlacement.PrecisionRotationValue = result;
		}
	}

	private void UpdatePrecisionRotationGameObjectState()
	{
		precisionRotationContainer.SetActive(eventPlacement.PlacePrecisionRotation);
	}
}
