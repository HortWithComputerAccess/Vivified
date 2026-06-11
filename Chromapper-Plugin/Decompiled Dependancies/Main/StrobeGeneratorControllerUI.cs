using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StrobeGeneratorControllerUI : MonoBehaviour, CMInput.IStrobeGeneratorActions
{
	[SerializeField]
	private VerticalLayoutGroup settingsPanelList;

	[SerializeField]
	private StrobeGenerator strobeGen;

	private StrobeGeneratorPassUIController[] allPassUIControllers;

	private void Start()
	{
		allPassUIControllers = GetComponentsInChildren<StrobeGeneratorPassUIController>();
	}

	public void OnQuickStrobeGen(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			GenerateStrobeWithUISettings();
		}
	}

	public void GenerateStrobeWithUISettings()
	{
		if (!SelectionController.HasSelectedObjects())
		{
			PersistentUI.Instance.ShowDialogBox("Mapper", "gradient.error", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		List<StrobeGeneratorPass> list = new List<StrobeGeneratorPass>();
		foreach (StrobeGeneratorPassUIController item in allPassUIControllers.Where((StrobeGeneratorPassUIController x) => x.WillGenerate))
		{
			list.Add(item.GetPassForGeneration());
		}
		strobeGen.GenerateStrobe(list);
	}

	private IEnumerator DirtySettingsList()
	{
		settingsPanelList.enabled = false;
		yield return new WaitForEndOfFrame();
		settingsPanelList.enabled = true;
	}
}
