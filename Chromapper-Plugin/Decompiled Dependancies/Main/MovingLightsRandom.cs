using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MovingLightsRandom : MonoBehaviour
{
	[FormerlySerializedAs("startOffset")]
	public float StartOffset;

	internal float movementSpeed;

	protected bool OverrideRandomValues;

	protected int RandomGenerationFrameNum = -1;

	internal float randomStartOffset;

	protected bool UseZPositionForAngleOffset;

	protected float ZPositionAngleOffsetScale = 1f;

	public Action ONSwitchStyle;

	public void SwitchStyle()
	{
		OverrideRandomValues = !OverrideRandomValues;
		RandomUpdate(leftEvent: false);
		ONSwitchStyle();
	}

	public void RandomUpdate(bool leftEvent)
	{
		int frameCount = Time.frameCount;
		if (RandomGenerationFrameNum != frameCount)
		{
			if (OverrideRandomValues)
			{
				randomStartOffset = 0f;
			}
			else
			{
				randomStartOffset = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			}
			RandomGenerationFrameNum = Time.frameCount;
		}
	}
}
