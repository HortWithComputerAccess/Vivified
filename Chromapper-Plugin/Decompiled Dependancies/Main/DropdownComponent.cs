using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownComponent : CMUIComponentWithLabel<int>, INavigable
{
	[SerializeField]
	private TMP_Dropdown dropdown;

	[field: SerializeField]
	public Selectable Selectable { get; set; }

	public DropdownComponent WithOptions<T>(IEnumerable<T> enumerable)
	{
		dropdown.ClearOptions();
		dropdown.AddOptions(enumerable.Select((T x) => x.ToString()).ToList());
		return this;
	}

	public DropdownComponent WithOptions(List<TMP_Dropdown.OptionData> optionData)
	{
		dropdown.ClearOptions();
		dropdown.AddOptions(optionData);
		return this;
	}

	public DropdownComponent WithOptions(List<Sprite> sprites)
	{
		dropdown.ClearOptions();
		dropdown.AddOptions(sprites);
		return this;
	}

	public DropdownComponent WithOptions<T>() where T : Enum
	{
		dropdown.ClearOptions();
		dropdown.AddOptions(Enum.GetNames(typeof(T)).ToList());
		return this;
	}

	protected override void OnValueUpdated(int updatedValue)
	{
		dropdown.SetValueWithoutNotify(updatedValue);
	}

	private void Start()
	{
		dropdown.onValueChanged.AddListener(DropdownValueChanged);
	}

	private void DropdownValueChanged(int value)
	{
		base.Value = value;
	}

	private void OnDestroy()
	{
		dropdown.onValueChanged.RemoveAllListeners();
	}
}
