using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEnumPicker : EnumPicker<Toggle>
{
	[SerializeField]
	private Toggle[] toggles;

	private int i;

	public override void CreateOptionForEnumValue(Enum enumValue)
	{
		Toggle toggle = toggles[i];
		items.Add(enumValue, toggle);
		ColorBlock colors = toggle.colors;
		colors.normalColor = normalColor;
		Color color = (colors.pressedColor = selectedColor);
		Color color3 = (colors.highlightedColor = color);
		colors.selectedColor = color3;
		toggle.colors = colors;
		toggle.onValueChanged.AddListener(delegate
		{
			if (!base.Locked && toggle.isOn)
			{
				Select(toggle);
				OnEnumValueSelected(enumValue);
			}
		});
		i++;
	}

	protected override void Select(Toggle selectedGraphic)
	{
		selectedGraphic.SetIsOnWithoutNotify(value: true);
		SetNormalColor(selectedGraphic, selectedColor);
		Toggle[] array = toggles;
		foreach (Toggle toggle in array)
		{
			if (toggle != selectedGraphic)
			{
				toggle.SetIsOnWithoutNotify(value: false);
				SetNormalColor(toggle, normalColor);
			}
		}
	}

	private void SetNormalColor(Toggle toggle, Color color)
	{
		ColorBlock colors = toggle.colors;
		colors.normalColor = color;
		toggle.colors = colors;
	}
}
