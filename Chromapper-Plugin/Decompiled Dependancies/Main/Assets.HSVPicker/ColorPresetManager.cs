using System.Collections.Generic;

namespace Assets.HSVPicker;

public static class ColorPresetManager
{
	public static Dictionary<string, ColorPresetList> Presets = new Dictionary<string, ColorPresetList>();

	public static ColorPresetList Get(string listId = "default")
	{
		if (!Presets.TryGetValue(listId, out var value))
		{
			value = new ColorPresetList(listId);
			Presets.Add(listId, value);
		}
		return value;
	}
}
