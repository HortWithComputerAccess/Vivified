using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RotatingLightsRandom : MonoBehaviour
{
	[FormerlySerializedAs("startRotationAngle")]
	public float StartRotationAngle;

	protected bool OverrideRandomValues;

	internal float randomDirection;

	protected int RandomGenerationFrameNum = -1;

	internal float randomStartRotation;

	internal float rotationSpeed;

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
		if (RandomGenerationFrameNum == frameCount)
		{
			return;
		}
		if (OverrideRandomValues)
		{
			randomDirection = (leftEvent ? 1f : (-1f));
			randomStartRotation = (leftEvent ? frameCount : (-frameCount));
			if (UseZPositionForAngleOffset)
			{
				randomStartRotation += base.transform.position.z * ZPositionAngleOffsetScale;
			}
		}
		else
		{
			randomDirection = ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
			randomStartRotation = UnityEngine.Random.Range(0f, 360f);
		}
		RandomGenerationFrameNum = Time.frameCount;
	}
}
