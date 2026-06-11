using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour, CMInput.IDialogBoxActions
{
	private const float roundness = 4f;

	private static readonly IEnumerable<Type> disabledActionMaps = from t in typeof(CMInput).GetNestedTypes()
		where t.IsInterface && t != typeof(CMInput.IUtilsActions) && t != typeof(CMInput.IDialogBoxActions)
		select t;

	[SerializeField]
	private GameObject raycastBlocker;

	[SerializeField]
	private GameObject titleGameObject;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private ImageWithIndependentRoundedCorners bodyRoundedCorners;

	[SerializeField]
	private Transform bodyTransform;

	[SerializeField]
	private GameObject footerGameObject;

	[SerializeField]
	private Transform footerTransform;

	private LinkedList<INavigable> navigableComponents = new LinkedList<INavigable>();

	private LinkedList<INavigable> navigableFooterButtons = new LinkedList<INavigable>();

	private Selectable currentSelectable;

	private bool destroyOnClose = true;

	private bool callbacksInstalled;

	private DialogBox parent;

	private Action quickSubmitCallback;

	private bool closeOnQuickSubmit = true;

	public DialogBox WithTitle(string title)
	{
		titleGameObject.SetActive(value: true);
		UpdateRoundedCorners();
		titleText.text = title;
		return this;
	}

	public DialogBox WithTitle(string table, string key, params object[] args)
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		return WithTitle(localizedString);
	}

	public DialogBox WithNoTitle()
	{
		titleGameObject.SetActive(value: false);
		UpdateRoundedCorners();
		return this;
	}

	public DialogBox DisableRaycastBlocker()
	{
		raycastBlocker.SetActive(value: false);
		return this;
	}

	public DialogBox DontDestroyOnClose()
	{
		destroyOnClose = false;
		return this;
	}

	public DialogBox OnQuickSubmit(Action onQuickSubmit, bool closeOnQuickSubmit = true)
	{
		quickSubmitCallback = onQuickSubmit;
		this.closeOnQuickSubmit = closeOnQuickSubmit;
		return this;
	}

	public T AddComponent<T>() where T : CMUIComponentBase
	{
		return AddComponent(typeof(T)) as T;
	}

	public CMUIComponentBase AddComponent(Type componentType)
	{
		CMUIComponentBase cMUIComponentBase = ComponentStoreSO.Instance.InstantiateCMUIComponentForComponentType(bodyTransform, componentType);
		if (cMUIComponentBase is INavigable value)
		{
			navigableComponents.AddLast(value);
		}
		if (base.gameObject.activeSelf)
		{
			ReconstructDialogBoxNavigation();
		}
		return cMUIComponentBase;
	}

	public ButtonComponent AddFooterButton(Action onClick, string label)
	{
		footerGameObject.SetActive(value: true);
		ButtonComponent buttonComponent = ComponentStoreSO.Instance.InstantiateCMUIComponentForComponentType<ButtonComponent>(footerTransform).WithLabel(label).OnClick(delegate
		{
			CloseAndInvokeCallback(onClick);
		});
		navigableFooterButtons.AddLast(buttonComponent);
		if (base.gameObject.activeSelf)
		{
			ReconstructDialogBoxNavigation();
		}
		return buttonComponent;
	}

	public ButtonComponent AddFooterButton(Action onClick, string table, string key, params object[] args)
	{
		footerGameObject.SetActive(value: true);
		ButtonComponent buttonComponent = ComponentStoreSO.Instance.InstantiateCMUIComponentForComponentType<ButtonComponent>(footerTransform).WithLabel(table, key, args).OnClick(delegate
		{
			CloseAndInvokeCallback(onClick);
		});
		navigableFooterButtons.AddLast(buttonComponent);
		if (base.gameObject.activeSelf)
		{
			ReconstructDialogBoxNavigation();
		}
		return buttonComponent;
	}

	public void Open(DialogBox parent = null)
	{
		this.parent = parent;
		if (!callbacksInstalled)
		{
			callbacksInstalled = true;
			if (parent != null)
			{
				CMInputCallbackInstaller.FindAndRemoveCallbacksRecursive(parent.transform);
			}
			CMInputCallbackInstaller.FindAndInstallCallbacksRecursive(base.transform);
		}
		if (parent == null)
		{
			CMInputCallbackInstaller.DisableActionMaps(typeof(DialogBox), disabledActionMaps);
		}
		CameraController.ClearCameraMovement();
		base.gameObject.SetActive(value: true);
		base.transform.SetSiblingIndex(base.transform.parent.childCount);
		ReconstructDialogBoxNavigation();
	}

	public void Close()
	{
		if (callbacksInstalled)
		{
			callbacksInstalled = false;
			CMInputCallbackInstaller.FindAndRemoveCallbacksRecursive(base.transform);
			if (parent != null)
			{
				CMInputCallbackInstaller.FindAndInstallCallbacksRecursive(parent.transform);
			}
		}
		if (parent == null)
		{
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(DialogBox), disabledActionMaps);
		}
		base.gameObject.SetActive(value: false);
		if (destroyOnClose)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Clear()
	{
		RemoveAllChildren(bodyTransform);
		RemoveAllChildren(footerTransform);
		navigableComponents.Clear();
		navigableFooterButtons.Clear();
	}

	public void OnCloseDialogBox(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Close();
		}
	}

	public void OnNavigateDown(InputAction.CallbackContext context)
	{
		if (context.performed && currentSelectable != null)
		{
			Selectable selectable = currentSelectable;
			currentSelectable = currentSelectable.FindSelectableOnDown();
			if (currentSelectable == null)
			{
				currentSelectable = selectable;
			}
			currentSelectable.Select();
		}
	}

	public void OnNavigateUp(InputAction.CallbackContext context)
	{
		if (context.performed && currentSelectable != null)
		{
			Selectable selectable = currentSelectable;
			currentSelectable = currentSelectable.FindSelectableOnUp();
			if (currentSelectable == null)
			{
				currentSelectable = selectable;
			}
			currentSelectable.Select();
		}
	}

	public void OnAttemptQuickSubmit(InputAction.CallbackContext context)
	{
		AttemptQuickSubmit(context);
	}

	public void OnAttemptQuickSubmitAlt(InputAction.CallbackContext context)
	{
		AttemptQuickSubmit(context);
	}

	private void AttemptQuickSubmit(InputAction.CallbackContext context)
	{
		if (context.performed && currentSelectable != null && currentSelectable.GetComponentInParent<CMUIComponentBase>() is IQuickSubmitComponent && quickSubmitCallback != null)
		{
			quickSubmitCallback();
			if (closeOnQuickSubmit)
			{
				Close();
			}
		}
	}

	private void UpdateRoundedCorners()
	{
		float num = (titleGameObject.activeSelf ? 0f : 4f);
		float num2 = (footerGameObject.activeSelf ? 0f : 4f);
		bodyRoundedCorners.r.Set(num, num, num2, num2);
		bodyRoundedCorners.Refresh();
	}

	private void CloseAndInvokeCallback(Action callback)
	{
		callback?.Invoke();
		Close();
	}

	private void RemoveAllChildren(Transform parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		while (parent.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
		}
	}

	public void ReconstructDialogBoxNavigation()
	{
		LinkedListNode<INavigable> navigableNode = navigableComponents.First;
		if (navigableNode != null)
		{
			if (currentSelectable == null)
			{
				currentSelectable = navigableNode.Value.Selectable;
			}
			IterateNavigableList(ref navigableNode);
			SetNavigation(navigableNode.Value, navigableNode.Previous?.Value, navigableFooterButtons.First?.Value);
		}
		navigableNode = navigableFooterButtons.First;
		if (navigableNode != null)
		{
			if (currentSelectable == null)
			{
				currentSelectable = navigableNode.Value.Selectable;
			}
			SetNavigation(navigableNode.Value, navigableComponents.Last?.Value, navigableNode.Next?.Value);
			navigableNode = navigableNode.Next;
			if (navigableNode != null)
			{
				IterateNavigableList(ref navigableNode);
				SetNavigation(navigableNode.Value, navigableNode.Previous?.Value, null);
			}
		}
		if (currentSelectable != null)
		{
			currentSelectable.Select();
		}
	}

	private void IterateNavigableList(ref LinkedListNode<INavigable> navigableNode)
	{
		while (navigableNode.Next != null)
		{
			SetNavigation(navigableNode.Value, navigableNode.Previous?.Value, navigableNode.Next?.Value);
			navigableNode = navigableNode.Next;
		}
	}

	private void SetNavigation(INavigable navigable, INavigable up, INavigable down)
	{
		navigable.Selectable.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = up?.Selectable,
			selectOnDown = down?.Selectable,
			selectOnLeft = null,
			selectOnRight = null
		};
	}

	private void Start()
	{
		UpdateRoundedCorners();
	}
}
