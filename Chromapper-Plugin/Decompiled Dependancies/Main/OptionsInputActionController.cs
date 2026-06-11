using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class OptionsInputActionController : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI keybindName;

	[SerializeField]
	private TMP_InputField[] keybindInputFields;

	[FormerlySerializedAs("SearchableOption")]
	[SerializeField]
	internal SearchableOption searchableOption;

	private readonly Dictionary<TMP_InputField, InputBinding> binds = new Dictionary<TMP_InputField, InputBinding>();

	private readonly Dictionary<string, TMP_InputField> keybindNameToInputField = new Dictionary<string, TMP_InputField>();

	private readonly List<string> overrideKeybindPaths = new List<string>();

	private InputAction action;

	private string compositeName;

	private bool isAxisComposite;

	private int maxKeys = 3;

	private int minKeys = 1;

	private Color unselectedTextColor = new Color(0.792f, 0.792f, 0.792f);

	private Color selectedTextColor = new Color(0.162f, 0.629f, 0.802f);

	private Color unselectedImageColor = new Color(0.31f, 0.31f, 0.31f);

	private Color selectedImageColor = new Color(0.162f, 0.629f, 0.802f).Multiply(0.5f);

	private bool rebinding;

	private string sectionName;

	public void Init(string sName, InputAction inputAction, List<InputBinding> bindings, string compositeName = null, bool useCompositeName = false)
	{
		sectionName = sName;
		action = inputAction;
		this.compositeName = compositeName;
		string text = (useCompositeName ? (inputAction.name + " (" + compositeName + ")") : inputAction.name);
		keybindName.text = (text.StartsWith(KeybindsController.PersistentKeybindIdentifier) ? text[1..] : text);
		UnselectKeybindUIs();
		searchableOption.Keywords = (keybindName.text + " " + sectionName).Split(' ');
		char[] separator = new char[1] { '/' };
		for (int i = 0; i < bindings.Count; i++)
		{
			binds.Add(keybindInputFields[i], bindings[i]);
			string text2 = ((bindings[i].path.FirstOrDefault() == '<') ? PrettifyName(bindings[i].path.Split(separator, 2).Last()) : PrettifyName(bindings[i].path.Split(separator, 3).Last()));
			keybindNameToInputField.Add(text2, keybindInputFields[i]);
			keybindInputFields[i].text = text2;
			keybindInputFields[i].onSelect.AddListener(OnKeybindSelected);
			keybindInputFields[i].onDeselect.AddListener(CancelKeybindRebind);
		}
		TMP_InputField[] array = keybindInputFields;
		foreach (TMP_InputField tMP_InputField in array)
		{
			tMP_InputField.gameObject.SetActive(binds.ContainsKey(tMP_InputField));
		}
		InputBinding inputBinding = action.bindings.Where((InputBinding x) => x.name == compositeName).FirstOrDefault();
		if (!string.IsNullOrEmpty(inputBinding.path))
		{
			if (inputBinding.path.Contains("2DVector"))
			{
				minKeys = (maxKeys = 4);
				isAxisComposite = true;
			}
			else if (inputBinding.path.Contains("1DAxis"))
			{
				minKeys = (maxKeys = 2);
				isAxisComposite = true;
			}
		}
	}

	public void OnKeybindSelected(string text)
	{
		if (keybindNameToInputField.ContainsKey(text))
		{
			SelectKeybindUIs();
			keybindNameToInputField[text].text = "";
			Debug.Log("Performing rebind for " + action.name + " (" + compositeName + ")");
			keybindNameToInputField.Clear();
			for (int i = 1; i < keybindInputFields.Length; i++)
			{
				keybindInputFields[i].gameObject.SetActive(value: false);
			}
			GetComponentInParent<OptionsKeybindsLoader>().BroadcastMessage("CancelKeybindRebind", "LULW", SendMessageOptions.DontRequireReceiver);
			StartCoroutine(PerformRebinding(minKeys, maxKeys, isAxisComposite));
		}
	}

	private IEnumerator PerformRebinding(int minKeys, int maxKeys, bool isAxisComposite = false)
	{
		rebinding = true;
		List<ButtonControl> allControls = new List<ButtonControl>();
		foreach (InputDevice device in InputSystem.devices)
		{
			allControls.AddRange(device.allControls.Where((InputControl x) => x is ButtonControl).Cast<ButtonControl>());
		}
		overrideKeybindPaths.Clear();
		int keys = 0;
		while (keys < maxKeys)
		{
			yield return new WaitUntil(() => allControls.Find((ButtonControl x) => x.wasPressedThisFrame) != null);
			InputControl inputControl = allControls.Find((ButtonControl x) => x.wasPressedThisFrame && x != Keyboard.current.anyKey);
			if (inputControl == null || inputControl.path.ToUpper().Contains("POSITION"))
			{
				continue;
			}
			if (inputControl.path == Keyboard.current.enterKey.path)
			{
				if (keys >= minKeys)
				{
					break;
				}
				continue;
			}
			if (inputControl.path.ToUpper().Contains("SHIFT"))
			{
				inputControl = Keyboard.current.shiftKey;
			}
			else if (inputControl.path.ToUpper().Contains("CTRL"))
			{
				inputControl = Keyboard.current.ctrlKey;
			}
			else if (inputControl.path.ToUpper().Contains("ALT"))
			{
				inputControl = Keyboard.current.altKey;
			}
			else if (inputControl.path.ToUpper().Contains("PRESS"))
			{
				inputControl = Mouse.current.leftButton;
			}
			Debug.Log("Detected key " + inputControl.path);
			string text = PrettifyName(inputControl.path.Split('/').Last());
			if (!keybindNameToInputField.ContainsKey(text))
			{
				overrideKeybindPaths.Add(inputControl.path);
				TMP_InputField tMP_InputField = keybindInputFields[keys];
				tMP_InputField.gameObject.SetActive(value: true);
				tMP_InputField.text = text;
				keybindNameToInputField.Add(text, tMP_InputField);
				keys++;
			}
		}
		CompleteRebind();
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void CompleteRebind()
	{
		Debug.Log("Completed rebinding.");
		LoadKeybindsController.KeybindOverride keybindOverride = new LoadKeybindsController.KeybindOverride(action.name, compositeName, overrideKeybindPaths)
		{
			IsAxisComposite = isAxisComposite
		};
		action.Disable();
		LoadKeybindsController.AddKeybindOverride(keybindOverride);
		action.Enable();
		Reinitialize();
	}

	public void CancelKeybindRebind(string _)
	{
		if (rebinding)
		{
			Debug.Log("Cancelling rebinding for " + action.name);
			StopAllCoroutines();
			Reinitialize();
		}
	}

	public string PrettifyName(string name)
	{
		char[] array = name.ToCharArray();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == 0)
			{
				stringBuilder.Append(char.ToUpper(array[i]));
			}
			else if (char.IsUpper(array[i]))
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(array[i]);
			}
			else if (char.IsLetterOrDigit(array[i]))
			{
				stringBuilder.Append(array[i]);
			}
			else
			{
				stringBuilder.Append(" ");
			}
		}
		return stringBuilder.ToString();
	}

	private void Reinitialize()
	{
		rebinding = false;
		binds.Clear();
		keybindNameToInputField.Clear();
		if (action.bindings.Any((InputBinding x) => x.isComposite))
		{
			string text = action.bindings.First((InputBinding x) => x.isComposite).name;
			bool useCompositeName = action.bindings.Count((InputBinding x) => x.isComposite) > 1;
			List<InputBinding> list = new List<InputBinding>();
			for (int num = 0; num < action.bindings.Count; num++)
			{
				if (action.bindings[num].isComposite && list.Any())
				{
					Init(sectionName, action, list, text, useCompositeName);
					break;
				}
				if (action.bindings[num].isPartOfComposite)
				{
					list.Add(action.bindings[num]);
				}
			}
			Init(sectionName, action, list, text, useCompositeName);
		}
		else
		{
			Init(sectionName, action, action.bindings.ToList());
		}
	}

	private void UnselectKeybindUIs()
	{
		keybindName.color = unselectedTextColor;
		keybindName.fontStyle = FontStyles.Normal;
		TMP_InputField[] array = keybindInputFields;
		foreach (TMP_InputField obj in array)
		{
			obj.image.color = unselectedImageColor;
			obj.textComponent.fontStyle = FontStyles.Normal;
		}
	}

	private void SelectKeybindUIs()
	{
		keybindName.color = selectedTextColor;
		keybindName.fontStyle = FontStyles.Italic;
		TMP_InputField[] array = keybindInputFields;
		foreach (TMP_InputField obj in array)
		{
			obj.image.color = selectedImageColor;
			obj.textComponent.fontStyle = FontStyles.Italic;
		}
	}
}
