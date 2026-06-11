using System;
using UnityEngine.Localization.Settings;

public static class CMUIComponentExtensions
{
	public static TComponent WithInitialValue<TComponent, TValue>(this TComponent component, Func<TValue> initialValue) where TComponent : CMUIComponent<TValue>
	{
		component.SetValueAccessor(initialValue);
		return component;
	}

	public static TComponent WithInitialValue<TComponent, TValue>(this TComponent component, TValue initialValue) where TComponent : CMUIComponent<TValue>
	{
		return component.WithInitialValue(() => initialValue);
	}

	public static TComponent OnChanged<TComponent, TValue>(this TComponent component, Action<TValue> onValueChanged) where TComponent : CMUIComponent<TValue>
	{
		component.SetOnValueChanged(onValueChanged);
		return component;
	}

	public static TComponent WithLabel<TComponent>(this TComponent component, string table, string key, params object[] args) where TComponent : CMUIComponentBase
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
		return component.WithLabel(localizedString);
	}

	public static TComponent WithLabel<TComponent>(this TComponent component, string labelText) where TComponent : CMUIComponentBase
	{
		component.SetLabelEnabled(!string.IsNullOrWhiteSpace(labelText));
		component.SetLabelText(labelText ?? "null");
		return component;
	}

	public static TComponent WithNoLabel<TComponent>(this TComponent component) where TComponent : CMUIComponentBase
	{
		component.SetLabelEnabled(enabled: false);
		return component;
	}
}
