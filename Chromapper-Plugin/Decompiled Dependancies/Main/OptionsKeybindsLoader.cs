using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsKeybindsLoader : MonoBehaviour
{
	[SerializeField]
	private OptionsActionMapController prefab;

	[SerializeField]
	private RectTransform parent;

	[SerializeField]
	private GameObject warning;

	[SerializeField]
	private RectTransform parentLayoutGroup;

	[SerializeField]
	private SearchableTab searchableTab;

	[SerializeField]
	private SearchInputField searchInputField;

	private readonly List<OptionsActionMapController> allActionMaps = new List<OptionsActionMapController>();

	private bool loadInProgress;

	private bool isInit;

	internal void OnTabSelected()
	{
		if (!isInit)
		{
			StartCoroutine(LoadKeybindsAsync());
			prefab.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator LoadKeybindsAsync()
	{
		if (loadInProgress)
		{
			yield break;
		}
		loadInProgress = true;
		CMInput inputInstance = CMInputCallbackInstaller.InputInstance;
		foreach (InputActionMap actionMap in inputInstance.asset.actionMaps)
		{
			if (!actionMap.name.StartsWith(KeybindsController.InternalKeybindIdentifier))
			{
				OptionsActionMapController component = Object.Instantiate(prefab.gameObject, parent).GetComponent<OptionsActionMapController>();
				component.Init(actionMap.name, actionMap);
				component.gameObject.name = actionMap.name + " Action Map";
				searchableTab.RegisterSection(component.SearchableSection);
				allActionMaps.Add(component);
				yield return null;
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayoutGroup);
		loadInProgress = false;
		isInit = true;
		searchableTab.UpdateSearch(searchInputField.InputField.text);
	}

	public void AskForHardReload()
	{
		PersistentUI.Instance.ShowDialogBox("Options", "keybinds.reset.confirm", HandleHardReload, PersistentUI.DialogBoxPresetType.YesNo);
	}

	private void HandleHardReload(int res)
	{
		if (res != 0)
		{
			return;
		}
		foreach (InputAction item in CMInputCallbackInstaller.InputInstance)
		{
			item.RemoveAllBindingOverrides();
		}
		isInit = false;
		foreach (OptionsActionMapController allActionMap in allActionMaps)
		{
			Object.Destroy(allActionMap.gameObject);
			searchableTab.RemoveSection(allActionMap.SearchableSection);
		}
		prefab.gameObject.SetActive(value: true);
		allActionMaps.Clear();
		LoadKeybindsController.AllOverrides.Clear();
		CMInputCallbackInstaller.InputInstance = new CMInput();
		OnTabSelected();
	}
}
