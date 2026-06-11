using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CM_DialogBox : MonoBehaviour
{
	[FormerlySerializedAs("UIButton")]
	[SerializeField]
	private Button uiButton;

	[FormerlySerializedAs("UIMessage")]
	[SerializeField]
	private TextMeshProUGUI uiMessage;

	[SerializeField]
	private CanvasGroup group;

	[SerializeField]
	private TMP_FontAsset defaultFont;

	private readonly IEnumerable<Type> disabledActionMaps = from t in typeof(CMInput).GetNestedTypes()
		where t.IsInterface && t != typeof(CMInput.IUtilsActions) && t != typeof(CMInput.IMenusExtendedActions)
		select t;

	private readonly List<Button> tempButtons = new List<Button>();

	private Action<int> resultAction;

	public bool IsEnabled => group.alpha == 1f;

	private void Start()
	{
		uiButton.onClick.AddListener(delegate
		{
			SendResult(0);
		});
	}

	public void SetParams(string message, Action<int> result, string[] buttonText, TMP_FontAsset[] buttonAsset)
	{
		if (IsEnabled)
		{
			throw new Exception("Dialog box is already enabled! Please wait until this Dialog Box has been disabled.");
		}
		CMInputCallbackInstaller.DisableActionMaps(typeof(CM_DialogBox), disabledActionMaps);
		UpdateGroup(visible: true);
		CameraController.ClearCameraMovement();
		uiMessage.text = message;
		resultAction = result;
		for (int i = 0; i < buttonText.Length; i++)
		{
			SetupButton(i, buttonText[i], (Settings.Instance.DarkTheme || buttonAsset == null) ? defaultFont : buttonAsset[i], (buttonText.Length > 3) ? 80 : 100);
		}
		for (int j = buttonText.Length; j < tempButtons.Count + 1; j++)
		{
			SetupButton(j, null, null);
		}
	}

	private void SetupButton(int index, string text, TMP_FontAsset font, int width = 100)
	{
		Button button;
		if (index == 0)
		{
			button = uiButton;
		}
		else if (index > tempButtons.Count)
		{
			button = UnityEngine.Object.Instantiate(uiButton.gameObject, uiButton.transform.parent).GetComponent<Button>();
			button.onClick.AddListener(delegate
			{
				SendResult(index);
			});
			tempButtons.Add(button);
		}
		else
		{
			button = tempButtons[index - 1];
		}
		SetupButton(button, text, font, width);
	}

	private void SetupButton(Button button, string text, TMP_FontAsset font, int width)
	{
		button.gameObject.SetActive(text != null);
		button.GetComponent<LayoutElement>().preferredWidth = width;
		button.GetComponentInChildren<TextMeshProUGUI>().text = text ?? "";
		button.GetComponentInChildren<TextMeshProUGUI>().font = ((font != null) ? font : defaultFont);
	}

	public void SendResult(int buttonID)
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(CM_DialogBox), disabledActionMaps);
		UpdateGroup(visible: false);
		resultAction?.Invoke(buttonID);
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
	}
}
