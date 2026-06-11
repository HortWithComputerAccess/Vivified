using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsActionMapController : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private OptionsInputActionController keybindPrefab;

	[SerializeField]
	private VerticalLayoutGroup layoutGroup;

	public SearchableSection SearchableSection;

	private InputActionMap actionMap;

	private bool hasInit;

	public void Init(string name, InputActionMap map)
	{
		if (hasInit)
		{
			return;
		}
		title.text = name;
		actionMap = map;
		foreach (InputAction action in actionMap.actions)
		{
			if (action.name.StartsWith(KeybindsController.InternalKeybindIdentifier))
			{
				continue;
			}
			if (action.bindings.Any((InputBinding x) => x.isComposite))
			{
				string compositeName = action.bindings.First((InputBinding x) => x.isComposite).name;
				bool useCompositeName = action.bindings.Count((InputBinding x) => x.isComposite) > 1;
				List<InputBinding> list = new List<InputBinding>();
				for (int num = 0; num < action.bindings.Count; num++)
				{
					if (action.bindings[num].isComposite && list.Any())
					{
						OptionsInputActionController component = Object.Instantiate(keybindPrefab.gameObject, base.transform).GetComponent<OptionsInputActionController>();
						component.Init(name, action, list, compositeName, useCompositeName);
						SearchableSection.RegisterOption(component.searchableOption);
						list.Clear();
						compositeName = action.bindings[num].name;
					}
					else if (action.bindings[num].isPartOfComposite)
					{
						list.Add(action.bindings[num]);
					}
				}
				OptionsInputActionController component2 = Object.Instantiate(keybindPrefab.gameObject, base.transform).GetComponent<OptionsInputActionController>();
				component2.Init(name, action, list, compositeName, useCompositeName);
				SearchableSection.RegisterOption(component2.searchableOption);
			}
			else
			{
				OptionsInputActionController component3 = Object.Instantiate(keybindPrefab.gameObject, base.transform).GetComponent<OptionsInputActionController>();
				component3.Init(name, action, action.bindings.ToList());
				SearchableSection.RegisterOption(component3.searchableOption);
			}
		}
		keybindPrefab.gameObject.SetActive(value: false);
		layoutGroup.spacing = layoutGroup.spacing;
		hasInit = true;
	}
}
