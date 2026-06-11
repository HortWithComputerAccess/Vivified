using System;
using System.Collections.Generic;
using Assets.HSVPicker;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorPresets : MonoBehaviour
{
	[FormerlySerializedAs("picker")]
	public ColorPicker Picker;

	[FormerlySerializedAs("presets")]
	public GameObject[] Presets;

	[FormerlySerializedAs("createPresetImage")]
	public Image CreatePresetImage;

	private ColorPresetList colors;

	private void Awake()
	{
		Picker.ONValueChanged.AddListener(ColorChanged);
	}

	private void Start()
	{
		colors = ColorPresetManager.Get(Picker.Setup.PresetColorsId);
		if (colors.Colors.Count < Picker.Setup.DefaultPresetColors.Length)
		{
			colors.UpdateList(Picker.Setup.DefaultPresetColors);
		}
		colors.ColorsUpdated += OnColorsUpdate;
		OnColorsUpdate(colors.Colors);
	}

	private void OnDestroy()
	{
		colors.ColorsUpdated -= OnColorsUpdate;
	}

	private void OnColorsUpdate(List<Color> colors)
	{
		for (int i = 0; i < Presets.Length; i++)
		{
			if (colors.Count <= i)
			{
				Presets[i].SetActive(value: false);
				continue;
			}
			Presets[i].SetActive(value: true);
			Presets[i].GetComponent<Image>().color = colors[i];
		}
		CreatePresetImage.gameObject.SetActive(colors.Count < Presets.Length);
	}

	public void CreatePresetButton()
	{
		colors.AddColor(Picker.CurrentColor);
	}

	public void PresetSelect(GameObject sender)
	{
		Picker.CurrentColor = sender.GetComponent<Image>().color;
	}

	public void DeletePreset(GameObject sender)
	{
		colors.Colors.RemoveAt(Array.IndexOf(Presets, sender));
		OnColorsUpdate(colors.Colors);
	}

	public void OverridePreset(GameObject sender)
	{
		colors.Colors[Array.IndexOf(Presets, sender)] = Picker.CurrentColor;
		OnColorsUpdate(colors.Colors);
	}

	private void ColorChanged(Color color)
	{
		CreatePresetImage.color = color;
	}
}
