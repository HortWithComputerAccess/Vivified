using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class MenuBase : MonoBehaviour, CMInput.IMenusExtendedActions
{
	public abstract void OnLeaveMenu(InputAction.CallbackContext context);

	public virtual void OnTab(InputAction.CallbackContext context)
	{
		if (!context.performed || this == null)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		try
		{
			Selectable component = current.currentSelectedGameObject.GetComponent<Selectable>();
			Selectable selectable = ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? component.FindSelectableOnUp() : component.FindSelectableOnDown());
			if (selectable != null)
			{
				TMP_InputField component2 = selectable.GetComponent<TMP_InputField>();
				if (component2 != null)
				{
					component2.MoveToEndOfLine(shift: false, ctrl: false);
				}
				current.SetSelectedGameObject(selectable.gameObject, new BaseEventData(current));
			}
		}
		catch (Exception)
		{
			current.SetSelectedGameObject(GetDefault(), new BaseEventData(current));
		}
	}

	protected abstract GameObject GetDefault();
}
