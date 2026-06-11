using Beatmap.Containers;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Chain Appearance SO", fileName = "ChainAppearanceSO")]
public class ChainAppearanceSO : ScriptableObject
{
	public Color RedColor { get; private set; } = DefaultColors.LeftNote;

	public Color BlueColor { get; private set; } = DefaultColors.RightNote;

	public void UpdateColor(Color red, Color blue)
	{
		RedColor = red;
		BlueColor = blue;
	}

	public void SetChainAppearance(ChainContainer chain)
	{
		switch (chain.ChainData.Color)
		{
		case 0:
			chain.SetColor(RedColor);
			break;
		case 1:
			chain.SetColor(BlueColor);
			break;
		}
		if (chain.ChainData.CustomColor.HasValue)
		{
			chain.SetColor(chain.ChainData.CustomColor.Value);
		}
		chain.Animator.AttachToObject(chain.ChainData);
	}
}
