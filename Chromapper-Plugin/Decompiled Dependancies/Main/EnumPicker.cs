using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class EnumPicker : MonoBehaviour
{
	[SerializeField]
	internal Color normalColor = Color.white;

	[SerializeField]
	internal Color selectedColor = Color.white;

	public bool Locked { get; set; }

	public event Action<Enum> OnClick;

	public void Initialize(Type type)
	{
		Array values = Enum.GetValues(type);
		foreach (Enum item in values)
		{
			CreateOptionForEnumValue(item);
		}
		Select(values.GetValue(0) as Enum);
	}

	public abstract void CreateOptionForEnumValue(Enum enumValue);

	public abstract void Select(Enum enumValue);

	protected void OnEnumValueSelected(Enum enumValue)
	{
		this.OnClick?.Invoke(enumValue);
	}

	protected static PickerChoiceAttribute GetPickerChoice(Enum GenericEnum)
	{
		MemberInfo[] member = GenericEnum.GetType().GetMember(GenericEnum.ToString());
		if (member != null && member.Length != 0)
		{
			object[] customAttributes = member[0].GetCustomAttributes(typeof(PickerChoiceAttribute), inherit: false);
			if (customAttributes != null && customAttributes.Count() > 0)
			{
				return (PickerChoiceAttribute)customAttributes.ElementAt(0);
			}
		}
		return null;
	}
}
public abstract class EnumPicker<TGraphic> : EnumPicker where TGraphic : UIBehaviour
{
	internal Dictionary<Enum, TGraphic> items = new Dictionary<Enum, TGraphic>();

	public override void Select(Enum enumValue)
	{
		Select(items[enumValue]);
	}

	protected abstract void Select(TGraphic selectedGraphic);
}
