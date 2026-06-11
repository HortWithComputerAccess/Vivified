using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.HSVPicker;

public class ColorPresetList
{
	public string ListId { get; }

	public List<Color> Colors { get; }

	public event UnityAction<List<Color>> ColorsUpdated;

	public ColorPresetList(string listId, List<Color> colors = null)
	{
		if (colors == null)
		{
			colors = new List<Color>();
		}
		Colors = colors;
		ListId = listId;
	}

	public void AddColor(Color color)
	{
		Colors.Add(color);
		if (this.ColorsUpdated != null)
		{
			this.ColorsUpdated(Colors);
		}
	}

	public void UpdateList(IEnumerable<Color> colors)
	{
		Colors.Clear();
		Colors.AddRange(colors);
		if (this.ColorsUpdated != null)
		{
			this.ColorsUpdated(Colors);
		}
	}
}
