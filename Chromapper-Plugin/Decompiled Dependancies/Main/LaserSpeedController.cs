using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class LaserSpeedController : DisableActionsField, CMInput.ILaserSpeedActions
{
	[SerializeField]
	private TMP_InputField laserSpeed;

	private readonly float delayBeforeReset = 0.5f;

	private float timeSinceLastInput;

	public bool Activated { get; private set; }

	private void Start()
	{
		InputSystem.onEvent += new Action<InputEventPtr, InputDevice>(TryGetButtonControl);
	}

	private void OnDestroy()
	{
		InputSystem.onEvent -= new Action<InputEventPtr, InputDevice>(TryGetButtonControl);
	}

	public void OnActivateTopRowInput(InputAction.CallbackContext context)
	{
		Activated = context.performed;
	}

	private void TryGetButtonControl(InputEventPtr eventPtr, InputDevice device)
	{
		if (!Activated || (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()))
		{
			return;
		}
		ReadOnlyArray<InputControl> allControls = device.allControls;
		float defaultButtonPressPoint = InputSystem.settings.defaultButtonPressPoint;
		for (int i = 0; i < allControls.Count; i++)
		{
			if (allControls[i] is ButtonControl { synthetic: false, noisy: false } buttonControl && buttonControl.ReadValueFromEvent(eventPtr, out var value) && value >= defaultButtonPressPoint)
			{
				OnChangeLaserSpeed(buttonControl);
			}
		}
	}

	private void OnChangeLaserSpeed(ButtonControl control)
	{
		if (!laserSpeed.isFocused && int.TryParse(control.name.Split("numpad".ToCharArray()).Last(), out var result))
		{
			if (Time.time >= timeSinceLastInput + delayBeforeReset)
			{
				laserSpeed.text = "";
			}
			timeSinceLastInput = Time.time;
			laserSpeed.text += result;
		}
	}
}
