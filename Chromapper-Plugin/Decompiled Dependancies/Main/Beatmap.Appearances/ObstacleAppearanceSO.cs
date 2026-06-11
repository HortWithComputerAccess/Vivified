using Beatmap.Containers;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Obstacle Appearance SO", fileName = "ObstacleAppearanceSO")]
public class ObstacleAppearanceSO : ScriptableObject
{
	[SerializeField]
	public Color DefaultObstacleColor = DefaultColors.Left;

	[SerializeField]
	private Color negativeWidthColor = Color.green;

	[SerializeField]
	private Color negativeDurationColor = Color.yellow;

	public void SetObstacleAppearance(ObstacleContainer obj, PlatformDescriptor platform = null)
	{
		if (platform != null)
		{
			DefaultObstacleColor = platform.Colors.ObstacleColor;
		}
		if (obj.ObstacleData.Duration < 0f && Settings.Instance.ColorFakeWalls)
		{
			obj.SetColor(negativeDurationColor);
		}
		else if (obj.ObstacleData.CustomData != null)
		{
			Vector2 vector = new Vector2(obj.ObstacleData.Width, obj.ObstacleData.Height);
			JSONNode customSize = obj.ObstacleData.CustomSize;
			if (customSize != null && customSize.IsArray)
			{
				if (customSize[0].IsNumber)
				{
					vector.x = customSize[0];
				}
				if (customSize[1].IsNumber)
				{
					vector.y = customSize[1];
				}
			}
			if ((vector.x < 0f || vector.y < 0f) && Settings.Instance.ColorFakeWalls)
			{
				obj.SetColor(negativeWidthColor);
			}
			else
			{
				obj.SetColor(DefaultObstacleColor);
			}
			if (obj.ObstacleData.CustomColor.HasValue)
			{
				obj.SetColor(obj.ObstacleData.CustomColor.Value);
			}
		}
		else if (obj.ObstacleData.Width < 0 && Settings.Instance.ColorFakeWalls)
		{
			obj.SetColor(negativeWidthColor);
		}
		else
		{
			obj.SetColor(DefaultObstacleColor);
		}
		obj.Animator.AttachToObject(obj.ObstacleData);
	}
}
