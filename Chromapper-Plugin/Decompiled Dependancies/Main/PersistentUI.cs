using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Info;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PersistentUI : MonoBehaviour
{
	public enum DisplayMessageType
	{
		Bottom,
		Center
	}

	[Serializable]
	public class MessageDisplayer
	{
		public class NotificationMessage
		{
			public readonly DisplayMessageType Type;

			private AsyncOperationHandle<string>? localisable;

			public bool Cancelled;

			public string Message;

			public bool SkipDisplay;

			public bool SkipFade;

			public float WaitTime = 2f;

			public NotificationMessage(AsyncOperationHandle<string> localisable, DisplayMessageType type)
			{
				this.localisable = localisable;
				Type = type;
			}

			public NotificationMessage(string message, DisplayMessageType type)
			{
				Message = message;
				Type = type;
			}

			public IEnumerator LoadMessage()
			{
				if (localisable.HasValue)
				{
					yield return localisable.Value;
					Message = localisable.Value.Result;
				}
			}
		}

		[SerializeField]
		private TMP_Text messageText;

		[FormerlySerializedAs("host")]
		public MonoBehaviour Host;

		private readonly Queue<NotificationMessage> messagesQueue = new Queue<NotificationMessage>();

		private bool isShowingMessages;

		private IEnumerator MessageRoutine()
		{
			isShowingMessages = true;
			while (messagesQueue.Count > 0)
			{
				NotificationMessage notificationMessage = messagesQueue.Dequeue();
				if (!notificationMessage.Cancelled)
				{
					yield return Host.StartCoroutine(MessageFadingRoutine(notificationMessage));
				}
			}
			isShowingMessages = false;
		}

		private IEnumerator MessageFadingRoutine(NotificationMessage message)
		{
			float t = 0f;
			messageText.alpha = 0f;
			messageText.text = message.Message;
			while (t < 1f && !message.Cancelled && !message.SkipFade)
			{
				yield return null;
				t += Time.deltaTime;
				if (t > 1f)
				{
					t = 1f;
				}
				messageText.alpha = t;
			}
			messageText.alpha = 1f;
			yield return new WaitForFixedUpdate();
			while (t <= message.WaitTime && !message.Cancelled && !message.SkipDisplay)
			{
				t += Time.deltaTime;
				yield return null;
			}
			t = 1f;
			yield return new WaitForFixedUpdate();
			while (t > 0f && !message.Cancelled && !message.SkipFade)
			{
				yield return null;
				t -= Time.deltaTime;
				if (t < 0f)
				{
					t = 0f;
				}
				messageText.alpha = t;
			}
			messageText.alpha = 0f;
		}

		public void DisplayMessage(NotificationMessage message)
		{
			messagesQueue.Enqueue(message);
			if (!isShowingMessages)
			{
				Host.StartCoroutine(MessageRoutine());
			}
		}
	}

	public enum DialogBoxPresetType
	{
		Ok,
		OkCancel,
		YesNo,
		YesNoCancel,
		OkIgnore
	}

	public Slider LevelLoadSlider;

	public TextMeshProUGUI LevelLoadSliderLabel;

	[SerializeField]
	private Localization localization;

	[Header("Loading")]
	[SerializeField]
	private CanvasGroup loadingCanvasGroup;

	[SerializeField]
	private Canvas persistentCanvas;

	[SerializeField]
	private TMP_Text loadingTip;

	[SerializeField]
	private Image editorLoadingBackground;

	[SerializeField]
	private Image editorWaifu;

	[SerializeField]
	private TextMeshProUGUI editorWaifuCredits;

	[SerializeField]
	private ImageList editorImageList;

	[SerializeField]
	private AnimationCurve fadeInCurve;

	[SerializeField]
	private AnimationCurve fadeOutCurve;

	[SerializeField]
	private Text tooltipText;

	[SerializeField]
	private GameObject tooltipObject;

	[SerializeField]
	private RectTransform tooltipPanelRect;

	[SerializeField]
	private Vector3 tooltipOffset;

	[SerializeField]
	private HorizontalLayoutGroup tooltipLayout;

	[Header("Dialog Box")]
	[SerializeField]
	private ComponentStoreSO componentStore;

	[SerializeField]
	private DialogBox newDialogBoxPrefab;

	[SerializeField]
	private CM_DialogBox dialogBox;

	[SerializeField]
	private TMP_FontAsset greenFont;

	[SerializeField]
	private TMP_FontAsset redFont;

	[SerializeField]
	private TMP_FontAsset goldFont;

	[Header("Input Box")]
	[SerializeField]
	private CM_InputBox inputBox;

	[FormerlySerializedAs("DialogBox_Loading")]
	public bool DialogBoxLoading;

	[Header("Center Message")]
	[SerializeField]
	private MessageDisplayer centerDisplay;

	[SerializeField]
	private MessageDisplayer bottomDisplay;

	[FormerlySerializedAs("enableTransitions")]
	public bool EnableTransitions = true;

	public UIDropdown DropdownPrefab;

	public UIButton ButtonPrefab;

	public UITextInput TextInputPrefab;

	public Sprites Sprites;

	private string currentTooltipAdvancedMessage;

	private string currentTooltipMessage;

	[Header("Tooltip")]
	private bool showTooltip;

	[Header("Color Input Box")]
	[SerializeField]
	private CM_ColorInputBox colorInputBox;

	public static PersistentUI Instance { get; private set; }

	public bool DialogBoxIsEnabled
	{
		get
		{
			if (!dialogBox.IsEnabled)
			{
				return DialogBoxLoading;
			}
			return true;
		}
	}

	public bool InputBoxIsEnabled => inputBox.IsEnabled;

	public bool ColorInputBox_IsEnabled => colorInputBox.IsEnabled;

	private void Awake()
	{
		if (Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Instance = this;
	}

	private void Start()
	{
		CMInputCallbackInstaller.PersistentObject(base.transform);
		LocalizationSettings.SelectedLocale = Locale.CreateLocale(Settings.Instance.Language);
		UpdateDSPBufferSize();
		AudioListener.volume = Settings.Instance.Volume;
		RequirementCheck.Setup();
		centerDisplay.Host = this;
		bottomDisplay.Host = this;
		EnableTransitions = !Settings.Instance.InstantLoadingTransitions;
	}

	private void LateUpdate()
	{
		if (showTooltip)
		{
			UpdateTooltipPosition();
		}
	}

	private void OnApplicationQuit()
	{
		ColourHistory.Save();
		Settings.Instance.Save();
	}

	private static void UpdateDSPBufferSize()
	{
		AudioConfiguration configuration = AudioSettings.GetConfiguration();
		configuration.dspBufferSize = (int)Math.Pow(2.0, Settings.Instance.DSPBufferSize);
		AudioSettings.Reset(configuration);
	}

	public MessageDisplayer.NotificationMessage DisplayMessage(string message, DisplayMessageType type)
	{
		Debug.LogWarning("Message not localized '" + message + "'");
		MessageDisplayer.NotificationMessage notificationMessage = new MessageDisplayer.NotificationMessage(message, type);
		DoDisplayMessage(notificationMessage);
		return notificationMessage;
	}

	private void DoDisplayMessage(MessageDisplayer.NotificationMessage message)
	{
		switch (message.Type)
		{
		case DisplayMessageType.Bottom:
			bottomDisplay.DisplayMessage(message);
			break;
		case DisplayMessageType.Center:
			centerDisplay.DisplayMessage(message);
			break;
		}
	}

	public MessageDisplayer.NotificationMessage DisplayMessage(string table, string key, DisplayMessageType type)
	{
		MessageDisplayer.NotificationMessage notificationMessage = new MessageDisplayer.NotificationMessage(LocalizationSettings.StringDatabase.GetLocalizedString(table, key, null, FallbackBehavior.UseProjectSettings), type);
		StartCoroutine(DisplayMessage(notificationMessage));
		return notificationMessage;
	}

	public IEnumerator DisplayMessage(MessageDisplayer.NotificationMessage notifiation)
	{
		yield return notifiation.LoadMessage();
		DoDisplayMessage(notifiation);
	}

	public static void UpdateBackground(BaseInfo info)
	{
		if (!Instance.editorLoadingBackground.gameObject.activeSelf)
		{
			Instance.editorLoadingBackground.gameObject.SetActive(value: true);
		}
		Instance.editorLoadingBackground.sprite = Instance.editorImageList.GetBgSprite(info);
	}

	public Coroutine FadeInLoadingScreen()
	{
		loadingTip.text = localization.GetRandomLoadingMessage();
		editorWaifu.gameObject.SetActive(Settings.Instance.Waifu);
		editorWaifu.sprite = localization.GetRandomWaifuSprite();
		editorWaifuCredits.text = editorWaifu.sprite.name;
		return StartCoroutine(FadeInLoadingScreen(2f));
	}

	private IEnumerator FadeInLoadingScreen(float rate)
	{
		loadingCanvasGroup.blocksRaycasts = true;
		loadingCanvasGroup.interactable = true;
		float t = 0f;
		while (t < 1f && EnableTransitions)
		{
			loadingCanvasGroup.alpha = fadeInCurve.Evaluate(t);
			t += Time.deltaTime * rate;
			yield return null;
		}
		loadingCanvasGroup.alpha = 1f;
	}

	public Coroutine FadeOutLoadingScreen()
	{
		return StartCoroutine(FadeOutLoadingScreen(2f));
	}

	private IEnumerator FadeOutLoadingScreen(float rate)
	{
		float t = 1f;
		while (t > 0f && EnableTransitions)
		{
			loadingCanvasGroup.alpha = fadeOutCurve.Evaluate(t);
			t -= Time.deltaTime * rate;
			yield return null;
		}
		loadingCanvasGroup.alpha = 0f;
		loadingCanvasGroup.blocksRaycasts = false;
		loadingCanvasGroup.interactable = false;
		Instance.editorLoadingBackground.gameObject.SetActive(value: false);
	}

	public void SetTooltip(string message, string advancedMessage = null)
	{
		currentTooltipMessage = message;
		currentTooltipAdvancedMessage = ((!string.IsNullOrEmpty(advancedMessage)) ? advancedMessage : null);
	}

	public void ShowTooltip()
	{
		showTooltip = true;
	}

	public void HideTooltip()
	{
		showTooltip = false;
		if (tooltipObject != null)
		{
			tooltipObject.SetActive(value: false);
		}
	}

	private void UpdateTooltipPosition()
	{
		if (Input.GetKey(KeyCode.LeftControl) && currentTooltipAdvancedMessage != null)
		{
			tooltipText.text = currentTooltipAdvancedMessage;
		}
		else
		{
			tooltipText.text = currentTooltipMessage;
		}
		tooltipText.color = Color.white;
		if (!tooltipObject.activeSelf)
		{
			tooltipObject.SetActive(value: true);
		}
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = tooltipPanelRect.rect.width * persistentCanvas.scaleFactor * 0.5f;
		float num4 = tooltipPanelRect.rect.height * persistentCanvas.scaleFactor * 0.5f;
		Vector2 vector = new Vector2(Mathf.Clamp(Input.mousePosition.x, num3, num - num3), Mathf.Clamp(Input.mousePosition.y + (num4 - 4f), num4, num2 - num4));
		tooltipPanelRect.position = vector;
	}

	public DialogBox CreateNewDialogBox()
	{
		return UnityEngine.Object.Instantiate(newDialogBoxPrefab, base.transform);
	}

	public void ShowDialogBox(string message, Action<int> result, DialogBoxPresetType preset)
	{
		Debug.LogWarning("Dialog box not localized '" + message + "'");
		DialogBoxLoading = true;
		DoShowDialogBox(message, result, preset);
	}

	public void ShowDialogBox(string table, string key, Action<int> result, DialogBoxPresetType preset, object[] args = null)
	{
		DialogBoxLoading = true;
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		DoShowDialogBox(localizedString, result, preset);
	}

	private void DoShowDialogBox(string message, Action<int> result, DialogBoxPresetType preset)
	{
		switch (preset)
		{
		case DialogBoxPresetType.Ok:
			DoShowDialogBox(message, result, GetStrings("PersistentUI", "ok"), new TMP_FontAsset[1] { greenFont });
			break;
		case DialogBoxPresetType.OkCancel:
			DoShowDialogBox(message, result, GetStrings("PersistentUI", "ok", "cancel"), new TMP_FontAsset[2] { greenFont, goldFont });
			break;
		case DialogBoxPresetType.YesNo:
			DoShowDialogBox(message, result, GetStrings("PersistentUI", "yes", "no"), new TMP_FontAsset[2] { greenFont, redFont });
			break;
		case DialogBoxPresetType.YesNoCancel:
			DoShowDialogBox(message, result, GetStrings("PersistentUI", "yes", "no", "cancel"), new TMP_FontAsset[3] { greenFont, redFont, goldFont });
			break;
		case DialogBoxPresetType.OkIgnore:
			DoShowDialogBox(message, result, GetStrings("PersistentUI", "ok", "ignore"), new TMP_FontAsset[2] { greenFont, goldFont });
			break;
		}
	}

	private List<string> GetStrings(string table, params string[] keys)
	{
		return keys.Select((string key) => LocalizationSettings.StringDatabase.GetLocalizedString(table, key, null, FallbackBehavior.UseProjectSettings)).ToList();
	}

	public void ShowDialogBox(string table, string key, Action<int> result, List<string> buttonText, TMP_FontAsset[] ba)
	{
		DialogBoxLoading = true;
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, null, FallbackBehavior.UseProjectSettings);
		DoShowDialogBox(localizedString, result, buttonText, ba);
	}

	public void ShowDialogBox(string table, string key, Action<int> result, string[] buttonTexts, TMP_FontAsset[] ba = null)
	{
		ShowDialogBox(table, key, result, GetStrings(table, buttonTexts), ba);
	}

	public void ShowDialogBox(string message, Action<int> result, string b0 = null, string b1 = null, string b2 = null, TMP_FontAsset b0A = null, TMP_FontAsset b1A = null, TMP_FontAsset b2A = null)
	{
		Debug.LogWarning("Dialog box not localized '" + message + "'");
		DoShowDialogBox(message, result, new string[3] { b0, b1, b2 }, new TMP_FontAsset[3]
		{
			b0A ? b0A : greenFont,
			b1A ? b1A : goldFont,
			b2A ? b2A : redFont
		});
	}

	private void DoShowDialogBox(string message, Action<int> result, IList<string> buttonText, TMP_FontAsset[] ba)
	{
		DialogBox dialogBox = CreateNewDialogBox().WithNoTitle();
		dialogBox.AddComponent<TextComponent>().WithInitialValue(message);
		foreach (string item in buttonText)
		{
			int i = buttonText.IndexOf(item);
			ButtonComponent buttonComponent = dialogBox.AddFooterButton(delegate
			{
				result?.Invoke(i);
			}, item);
			if (i < ba.Length && Enumerable.Contains(ba[i].material.shaderKeywords, "GLOW_ON"))
			{
				Color color = ba[i].material.GetColor("_GlowColor");
				buttonComponent.WithBackgroundColor(color.Multiply(color.a).WithAlpha(1f).WithSatuation(0.5f));
			}
		}
		dialogBox.Open();
		DialogBoxLoading = false;
	}

	public void ShowInputBox(string message, Action<string> result, string defaultText = "")
	{
		Debug.LogWarning("Input box not localized '" + message + "'");
		DoShowInputBox(message, result, defaultText);
	}

	public void ShowInputBox(string table, string key, Action<string> result, string defaultTextKey = "", string defaultDefault = "")
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, null, FallbackBehavior.UseProjectSettings);
		string defaultText = defaultDefault;
		if (!string.IsNullOrEmpty(defaultTextKey))
		{
			defaultText = LocalizationSettings.StringDatabase.GetLocalizedString(table, defaultTextKey, null, FallbackBehavior.UseProjectSettings);
		}
		DoShowInputBox(localizedString, result, defaultText);
	}

	private void DoShowInputBox(string message, Action<string> result, string defaultText)
	{
		DialogBox dialogBox = CreateNewDialogBox().WithNoTitle();
		dialogBox.AddComponent<TextComponent>().WithInitialValue(message);
		TextBoxComponent textBox = dialogBox.AddComponent<TextBoxComponent>().WithInitialValue(defaultText).WithNoLabel();
		dialogBox.AddFooterButton(delegate
		{
			result?.Invoke(null);
		}, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "cancel", null, FallbackBehavior.UseProjectSettings));
		dialogBox.AddFooterButton(delegate
		{
			result?.Invoke(textBox.Value);
		}, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "submit", null, FallbackBehavior.UseProjectSettings));
		dialogBox.OnQuickSubmit(delegate
		{
			result?.Invoke(textBox.Value);
		});
		dialogBox.Open();
	}

	public void ShowColorInputBox(string table, string key, Action<Color?> result, Color selctedColor, string defaultTextKey = "", string defaultDefault = "")
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, null, FallbackBehavior.UseProjectSettings);
		if (!string.IsNullOrEmpty(defaultTextKey))
		{
			LocalizationSettings.StringDatabase.GetLocalizedString(table, defaultTextKey, null, FallbackBehavior.UseProjectSettings);
		}
		DoShowColorInputBox(localizedString, result, selctedColor);
	}

	public void ShowColorInputBox(string table, string key, Action<Color?> result, string defaultTextKey = "", string defaultDefault = "")
	{
		ShowColorInputBox(table, key, result, Color.red, defaultTextKey, defaultDefault);
	}

	private void DoShowColorInputBox(string message, Action<Color?> result, Color defaultColor)
	{
		DialogBox dialogBox = CreateNewDialogBox().WithNoTitle();
		dialogBox.AddComponent<TextComponent>().WithInitialValue(message);
		ColorPickerComponent colorPicker = dialogBox.AddComponent<ColorPickerComponent>().WithInitialValue(defaultColor);
		dialogBox.AddFooterButton(delegate
		{
			result?.Invoke(null);
		}, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "cancel", null, FallbackBehavior.UseProjectSettings));
		dialogBox.AddFooterButton(delegate
		{
			result?.Invoke(colorPicker.Value);
		}, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "submit", null, FallbackBehavior.UseProjectSettings));
		dialogBox.OnQuickSubmit(delegate
		{
			result?.Invoke(colorPicker.Value);
		});
		dialogBox.Open();
	}
}
