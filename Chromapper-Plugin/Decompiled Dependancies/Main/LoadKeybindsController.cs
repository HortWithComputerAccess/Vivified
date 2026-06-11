using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadKeybindsController : MonoBehaviour
{
	public class KeybindOverride
	{
		public string CompositeKeybindName;

		public string InputActionName;

		public bool IsAxisComposite;

		public List<string> OverrideKeybindPaths = new List<string>();

		public KeybindOverride(JSONNode obj)
		{
			if (!obj.HasKey("_actionName"))
			{
				throw new ArgumentException("Keybind Override must have node \"_name\"");
			}
			if (!obj.HasKey("_overridePaths"))
			{
				throw new ArgumentException("Keybind Override must have node \"_overridePaths\"");
			}
			if (obj["_overridePaths"].Count < 1)
			{
				throw new ArgumentException("\"_overridePaths\" must not be empty.");
			}
			InputActionName = obj["_actionName"];
			if (obj.HasKey("_compositeName"))
			{
				CompositeKeybindName = obj["_compositeName"];
			}
			if (obj.HasKey("_axisComposite"))
			{
				IsAxisComposite = obj["_axisComposite"];
			}
			JSONNode.Enumerator enumerator = obj["_overridePaths"].AsArray.GetEnumerator();
			while (enumerator.MoveNext())
			{
				JSONNode jSONNode = enumerator.Current;
				OverrideKeybindPaths.Add(jSONNode);
			}
		}

		public KeybindOverride(string actionName, string compositeName, List<string> keybindPaths)
		{
			InputActionName = actionName;
			CompositeKeybindName = compositeName;
			OverrideKeybindPaths = keybindPaths;
		}

		public JSONNode ToJsonNode()
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject["_actionName"] = InputActionName;
			jSONObject["_axisComposite"] = IsAxisComposite;
			if (CompositeKeybindName != null)
			{
				jSONObject["_compositeName"] = CompositeKeybindName;
			}
			JSONArray jSONArray = new JSONArray();
			foreach (string overrideKeybindPath in OverrideKeybindPaths)
			{
				jSONArray.Add(overrideKeybindPath);
			}
			jSONObject["_overridePaths"] = jSONArray;
			return jSONObject;
		}
	}

	private static readonly string version = "1.0.0";

	public static List<KeybindOverride> AllOverrides = new List<KeybindOverride>();

	private string path;

	private void Start()
	{
		Application.wantsToQuit += WantsToQuit;
	}

	private void OnDestroy()
	{
		Application.wantsToQuit -= WantsToQuit;
	}

	public void InputObjectCreated(object obj)
	{
		path = Application.persistentDataPath + "/ChroMapperOverrideKeybinds.json";
		if (File.Exists(path))
		{
			JSONNode jSONNode = JSON.Parse(File.ReadAllText(path));
			if (!jSONNode.HasKey("_version") || !jSONNode.HasKey("_overrides") || jSONNode["_version"] != (object)version)
			{
				Debug.LogWarning("New Keybind Override file does not exist, skipping...");
				return;
			}
			JSONNode.Enumerator enumerator = jSONNode["_overrides"].AsArray.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeybindOverride keybindOverride = new KeybindOverride(enumerator.Current);
				Debug.Log("Adding override for " + keybindOverride.InputActionName);
				AddKeybindOverride(keybindOverride);
			}
		}
		if (!Settings.Instance.InvertNoteControls)
		{
			MigrateNoteControls();
			Settings.Instance.InvertNoteControls = true;
		}
	}

	private void MigrateNoteControls()
	{
		if (AllOverrides.Find((KeybindOverride x) => x.InputActionName == "Up Note") == null)
		{
			AddKeybindOverride(new KeybindOverride("Up Note", "", new List<string> { "/Keyboard/s" }));
		}
		if (AllOverrides.Find((KeybindOverride x) => x.InputActionName == "Right Note") == null)
		{
			AddKeybindOverride(new KeybindOverride("Right Note", "", new List<string> { "/Keyboard/a" }));
		}
		if (AllOverrides.Find((KeybindOverride x) => x.InputActionName == "Down Note") == null)
		{
			AddKeybindOverride(new KeybindOverride("Down Note", "", new List<string> { "/Keyboard/w" }));
		}
		if (AllOverrides.Find((KeybindOverride x) => x.InputActionName == "Left Note") == null)
		{
			AddKeybindOverride(new KeybindOverride("Left Note", "", new List<string> { "/Keyboard/d" }));
		}
	}

	public static void AddKeybindOverride(KeybindOverride keybindOverride)
	{
		if (keybindOverride.OverrideKeybindPaths.Count <= 0 || keybindOverride.OverrideKeybindPaths.Count > 4)
		{
			return;
		}
		AllOverrides.RemoveAll((KeybindOverride x) => x.InputActionName == keybindOverride.InputActionName && x.CompositeKeybindName == keybindOverride.CompositeKeybindName);
		InputActionMap inputActionMap = CMInputCallbackInstaller.InputInstance.asset.actionMaps.Where((InputActionMap x) => x.actions.Any((InputAction y) => y.name == keybindOverride.InputActionName)).FirstOrDefault();
		if (inputActionMap == null)
		{
			return;
		}
		InputAction inputAction = inputActionMap.FindAction(keybindOverride.InputActionName);
		List<InputBinding> list = new List<InputBinding>();
		InputBinding inputBinding = inputAction.bindings.Where((InputBinding x) => x.name == keybindOverride.CompositeKeybindName).FirstOrDefault();
		list.Add(inputBinding);
		for (int num = inputAction.GetBindingIndex(inputBinding) + 1; num < inputAction.bindings.Count && inputAction.bindings[num].isPartOfComposite; num++)
		{
			list.Add(inputAction.bindings[num]);
		}
		list.Reverse();
		foreach (InputBinding item in list)
		{
			Debug.Log("Deleting " + item.name + " from " + inputAction.name);
			inputAction.ChangeBinding(inputAction.GetBindingIndex(item)).Erase();
		}
		switch (keybindOverride.OverrideKeybindPaths.Count)
		{
		case 1:
			inputAction.AddBinding(keybindOverride.OverrideKeybindPaths[0]);
			break;
		case 2:
			if (keybindOverride.IsAxisComposite)
			{
				inputAction.AddCompositeBinding("1DAxis").With("positive", keybindOverride.OverrideKeybindPaths[0]).With("negative", keybindOverride.OverrideKeybindPaths[1]);
				RenameCompositeBinding(inputAction, keybindOverride);
			}
			else if (!keybindOverride.IsAxisComposite)
			{
				inputAction.AddCompositeBinding("ButtonWithOneModifier").With("modifier", keybindOverride.OverrideKeybindPaths[0]).With("button", keybindOverride.OverrideKeybindPaths[1]);
				RenameCompositeBinding(inputAction, keybindOverride);
			}
			break;
		case 3:
			inputAction.AddCompositeBinding("ButtonWithTwoModifiers").With("modifier2", keybindOverride.OverrideKeybindPaths[0]).With("modifier1", keybindOverride.OverrideKeybindPaths[1])
				.With("button", keybindOverride.OverrideKeybindPaths[2]);
			RenameCompositeBinding(inputAction, keybindOverride);
			break;
		case 4:
			if (keybindOverride.IsAxisComposite)
			{
				inputAction.AddCompositeBinding("2DVector(mode=2)").With("up", keybindOverride.OverrideKeybindPaths[0]).With("left", keybindOverride.OverrideKeybindPaths[1])
					.With("down", keybindOverride.OverrideKeybindPaths[2])
					.With("right", keybindOverride.OverrideKeybindPaths[3]);
				RenameCompositeBinding(inputAction, keybindOverride);
			}
			break;
		}
		Debug.Log("Added keybind override for " + keybindOverride.InputActionName + ".");
		AllOverrides.Add(keybindOverride);
	}

	private static void RenameCompositeBinding(InputAction action, KeybindOverride keybindOverride)
	{
		InputBinding match = action.bindings.Last((InputBinding x) => x.isComposite);
		action.ChangeBinding(match).WithName(keybindOverride.CompositeKeybindName ?? "Override");
	}

	private bool WantsToQuit()
	{
		JSONNode jSONNode = new JSONObject();
		jSONNode["_version"] = version;
		JSONArray jSONArray = new JSONArray();
		foreach (KeybindOverride allOverride in AllOverrides)
		{
			jSONArray.Add(allOverride.ToJsonNode());
		}
		jSONNode["_overrides"] = jSONArray;
		File.WriteAllText(path, jSONNode.ToString(2));
		return true;
	}
}
