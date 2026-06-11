using Beatmap.Containers;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Arc Appearance SO", fileName = "ArcAppearanceSO")]
public class ArcAppearanceSO : ScriptableObject
{
	public Color RedColor { get; private set; } = DefaultColors.LeftNote;

	public Color BlueColor { get; private set; } = DefaultColors.RightNote;

	public void UpdateColor(Color red, Color blue)
	{
		RedColor = red;
		BlueColor = blue;
	}

	public void SetArcAppearance(ArcContainer arc)
	{
		switch (arc.ArcData.Color)
		{
		case 0:
			arc.SetColor(RedColor);
			break;
		case 1:
			arc.SetColor(BlueColor);
			break;
		}
		if (arc.ArcData.CustomColor.HasValue)
		{
			arc.SetColor(arc.ArcData.CustomColor.Value);
		}
	}
}
