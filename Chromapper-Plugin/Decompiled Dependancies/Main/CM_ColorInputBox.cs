using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CM_ColorInputBox : MenuBase
{
	[SerializeField]
	[FormerlySerializedAs("ColorInputField")]
	private ColorPicker colorInputField;

	[SerializeField]
	[FormerlySerializedAs("UIMessage")]
	private TextMeshProUGUI uiMessage;

	[SerializeField]
	private CanvasGroup group;

	private Action<Color?> resultAction;

	private readonly IEnumerable<Type> disabledActionMaps = from t in typeof(CMInput).GetNestedTypes()
		where t.IsInterface && t != typeof(CMInput.IUtilsActions) && t != typeof(CMInput.IMenusExtendedActions)
		select t;

	public bool IsEnabled => group.alpha == 1f;

	public void SetParams(string message, Action<Color?> result, Color selectedColor, string defaultText = "")
	{
		if (IsEnabled)
		{
			throw new Exception("Input box is already enabled! Please wait until this Input Box has been disabled.");
		}
		CMInputCallbackInstaller.DisableActionMaps(typeof(CM_ColorInputBox), disabledActionMaps);
		UpdateGroup(visible: true);
		CameraController.ClearCameraMovement();
		colorInputField.CurrentColor = selectedColor;
		uiMessage.text = message;
		resultAction = result;
	}

	public void EndEdit()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			SendResult(0);
		}
	}

	public void SendResult(int buttonID)
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(CM_ColorInputBox), disabledActionMaps);
		UpdateGroup(visible: false);
		Color currentColor = colorInputField.CurrentColor;
		if (buttonID == 0)
		{
			resultAction?.Invoke(currentColor);
		}
		else
		{
			resultAction?.Invoke(null);
		}
		resultAction = null;
	}

	private void UpdateGroup(bool visible)
	{
		group.alpha = (visible ? 1 : 0);
		StartCoroutine(WaitAndChangeInteractive(visible));
		group.blocksRaycasts = visible;
	}

	private IEnumerator WaitAndChangeInteractive(bool visible)
	{
		yield return new WaitForSeconds(0.25f);
		group.interactable = visible;
		EventSystem.current.SetSelectedGameObject(colorInputField.gameObject, new BaseEventData(EventSystem.current));
	}

	public override void OnTab(InputAction.CallbackContext context)
	{
		if (IsEnabled)
		{
			base.OnTab(context);
		}
	}

	protected override GameObject GetDefault()
	{
		return colorInputField.gameObject;
	}

	public override void OnLeaveMenu(InputAction.CallbackContext context)
	{
		SendResult(1);
	}
}
