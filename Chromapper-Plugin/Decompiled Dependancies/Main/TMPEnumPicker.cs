using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TMPEnumPicker : EnumPicker<TextMeshProUGUI>
{
	[SerializeField]
	private GameObject optionPrefab;

	[SerializeField]
	private bool shouldBold = true;

	[SerializeField]
	private bool resizeSelected;

	[SerializeField]
	private float selectedSize = 16f;

	private float regularSize;

	private TextMeshProUGUI lastSelected;

	private TextMeshProUGUI beforeLastSelected;

	public override void CreateOptionForEnumValue(Enum enumValue)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(optionPrefab, optionPrefab.transform.parent);
		TextMeshProUGUI textMesh = gameObject.GetComponent<TextMeshProUGUI>();
		regularSize = textMesh.fontSize;
		textMesh.text = enumValue.ToString();
		textMesh.color = normalColor;
		if (shouldBold)
		{
			textMesh.fontStyle &= ~FontStyles.Bold;
		}
		PickerChoiceAttribute pickerChoice = EnumPicker.GetPickerChoice(enumValue);
		if (pickerChoice != null)
		{
			LocalizeStringEvent component = textMesh.GetComponent<LocalizeStringEvent>();
			if (component == null)
			{
				throw new Exception("Enum Picker prefab for type '" + enumValue.GetType().Name + "' is missing LocalizeStringEvent component");
			}
			LocalizedString localizedString = new LocalizedString();
			localizedString.SetReference(pickerChoice.Table, pickerChoice.Entry);
			component.StringReference = localizedString;
		}
		items.Add(enumValue, textMesh);
		gameObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			if (!base.Locked)
			{
				Select(textMesh);
				OnEnumValueSelected(enumValue);
			}
		});
		gameObject.SetActive(value: true);
	}

	protected override void Select(TextMeshProUGUI toSelect)
	{
		StopAllCoroutines();
		if (beforeLastSelected != null && beforeLastSelected != toSelect && resizeSelected)
		{
			beforeLastSelected.fontSize = regularSize;
		}
		if (lastSelected != null)
		{
			if (shouldBold)
			{
				lastSelected.fontStyle &= ~FontStyles.Bold;
			}
			lastSelected.color = normalColor;
			if (resizeSelected)
			{
				StartCoroutine(InterpolateToSize(lastSelected, regularSize));
			}
		}
		if (shouldBold)
		{
			toSelect.fontStyle |= FontStyles.Bold;
		}
		toSelect.color = selectedColor;
		if (resizeSelected)
		{
			StartCoroutine(InterpolateToSize(toSelect, selectedSize));
		}
		beforeLastSelected = lastSelected;
		lastSelected = toSelect;
	}

	private IEnumerator InterpolateToSize(TextMeshProUGUI textMesh, float size)
	{
		float originalSize = textMesh.fontSize;
		for (int time = 0; time <= 10; time++)
		{
			textMesh.fontSize = Mathf.Lerp(originalSize, size, Mathf.Pow((float)time / 10f, 1f / 3f));
			yield return new WaitForSeconds(1f / 60f);
		}
		textMesh.fontSize = size;
	}
}
