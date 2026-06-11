using System;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class RotatingLights : RotatingLightsBase
{
	[FormerlySerializedAs("multiplier")]
	public float Multiplier = 20f;

	[SerializeField]
	private float rotationSpeed;

	[SerializeField]
	private float zPositionModifier;

	public bool OverrideLightGroup;

	public int OverrideLightGroupID;

	public bool UseZPositionForAngleOffset;

	private readonly Vector3 rotationVector = Vector3.up;

	private float songSpeed = 1f;

	private float speed;

	private Quaternion startRotation;

	private float zPositionOffset;

	private void Start()
	{
		startRotation = base.transform.localRotation;
		if (OverrideLightGroup)
		{
			PlatformDescriptor componentInParent = GetComponentInParent<PlatformDescriptor>();
			if (componentInParent != null)
			{
				componentInParent.LightingManagers[OverrideLightGroupID].RotatingLights.Add(this);
			}
		}
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
	}

	private void Update()
	{
		base.transform.Rotate(rotationVector, Time.deltaTime * rotationSpeed * songSpeed, Space.Self);
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

	public override void UpdateOffset(bool isLeftEvent, BaseEvent evt)
	{
		float angle = UnityEngine.Random.Range(0f, 180f);
		bool flag = UnityEngine.Random.Range(0, 1) == 1;
		speed = evt.Value;
		bool flag2 = false;
		if (evt.CustomData != null)
		{
			if (evt.CustomLockRotation.HasValue)
			{
				flag2 = evt.CustomLockRotation.Value;
			}
			if (speed > 0f)
			{
				if (evt.CustomPreciseSpeed.HasValue)
				{
					speed = evt.CustomPreciseSpeed.Value;
				}
				else if (evt.CustomSpeed.HasValue)
				{
					speed = evt.CustomSpeed.Value;
				}
			}
			if (evt.CustomDirection.HasValue)
			{
				flag = evt.CustomDirection.Value.Equals(0) ^ isLeftEvent;
			}
		}
		if (!flag2)
		{
			base.transform.localRotation = startRotation;
		}
		if (UseZPositionForAngleOffset && !flag2)
		{
			angle = (float)Time.frameCount + base.transform.position.z * zPositionModifier;
		}
		if (!flag2 && (speed > 0f || (evt.CustomPreciseSpeed.HasValue && evt.CustomPreciseSpeed.Value >= 0f)))
		{
			base.transform.Rotate(rotationVector, angle, Space.Self);
		}
		rotationSpeed = speed * Multiplier * (float)((!flag) ? 1 : (-1)) * Mathf.Sign(Multiplier);
	}

	public override bool IsOverrideLightGroup()
	{
		return OverrideLightGroup;
	}
}
