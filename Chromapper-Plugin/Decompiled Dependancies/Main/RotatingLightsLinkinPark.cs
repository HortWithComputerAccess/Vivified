using System;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class RotatingLightsLinkinPark : RotatingLightsBase
{
	[FormerlySerializedAs("_rotationVector")]
	[SerializeField]
	protected Vector3 RotationVector = Vector3.up;

	[FormerlySerializedAs("multiplier")]
	public float Multiplier = 20f;

	public bool Left;

	[FormerlySerializedAs("rotatingLightsRandom")]
	[SerializeField]
	protected RotatingLightsRandom RotatingLightsRandom;

	protected bool UseZPositionForAngleOffset;

	protected float ZPositionAngleOffsetScale = 1f;

	private float rotationAngle;

	private bool rotationEnabled;

	private float rotationSpeed;

	private float songSpeed = 1f;

	private Quaternion startRotation;

	private float startRotationAngle;

	private void Start()
	{
		startRotation = base.gameObject.transform.localRotation;
		startRotationAngle = (Left ? (0f - RotatingLightsRandom.StartRotationAngle) : RotatingLightsRandom.StartRotationAngle);
		startRotationAngle *= 4f;
		rotationAngle = startRotationAngle;
		base.transform.localRotation = startRotation * Quaternion.Euler(RotationVector * startRotationAngle);
		RotatingLightsRandom rotatingLightsRandom = RotatingLightsRandom;
		rotatingLightsRandom.ONSwitchStyle = (Action)Delegate.Combine(rotatingLightsRandom.ONSwitchStyle, new Action(SwitchStyle));
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
	}

	private void Update()
	{
		if (rotationEnabled)
		{
			rotationAngle += Time.deltaTime * rotationSpeed * songSpeed;
			base.transform.localRotation = startRotation * Quaternion.Euler(RotationVector * rotationAngle);
		}
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("SongSpeed");
	}

	private void UpdateSongSpeed(object value)
	{
		float num = (float)Convert.ChangeType(value, typeof(float));
		songSpeed = num / 10f;
	}

	public void SwitchStyle()
	{
		rotationAngle = RotatingLightsRandom.randomStartRotation;
		rotationSpeed = Mathf.Abs(RotatingLightsRandom.rotationSpeed);
		if (!Left)
		{
			rotationAngle = 0f - rotationAngle;
			rotationSpeed = 0f - rotationSpeed;
		}
		rotationAngle += startRotationAngle;
	}

	public override void UpdateOffset(bool isLeftEvent, BaseEvent evt)
	{
		RotatingLightsRandom.RandomUpdate(Left);
		if (Left)
		{
			UpdateRotationData(evt.Value, RotatingLightsRandom.randomStartRotation, RotatingLightsRandom.randomDirection);
		}
		else
		{
			UpdateRotationData(evt.Value, 0f - RotatingLightsRandom.randomStartRotation, 0f - RotatingLightsRandom.randomDirection);
		}
	}

	public void UpdateRotationData(int beatmapEventDataValue, float startRotationOffset, float direction)
	{
		if (beatmapEventDataValue == 0)
		{
			rotationEnabled = false;
			base.transform.localRotation = startRotation * Quaternion.Euler(RotationVector * startRotationAngle);
		}
		else if (beatmapEventDataValue > 0)
		{
			rotationEnabled = true;
			rotationAngle = startRotationOffset + startRotationAngle;
			base.transform.localRotation = startRotation * Quaternion.Euler(RotationVector * rotationAngle);
			rotationSpeed = (float)beatmapEventDataValue * 20f * direction;
			if (Left)
			{
				RotatingLightsRandom.rotationSpeed = rotationSpeed;
			}
		}
	}

	public override bool IsOverrideLightGroup()
	{
		return false;
	}
}
