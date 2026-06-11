using System;
using Beatmap.Base;
using UnityEngine;

public class MovingLightsInterscope : RotatingLightsBase
{
	public bool Left;

	[SerializeField]
	protected MovingLightsRandom MovingLightsRandom;

	[SerializeField]
	protected Vector3 StartPositionOffset = new Vector3(0f, 0f, 0f);

	[SerializeField]
	protected Vector3 EndPositionOffset = new Vector3(0f, 2f, 0f);

	private bool movementEnabled;

	private float movementSpeed;

	private float movementValue;

	private float songSpeed = 1f;

	private Vector3 startPosition;

	private void Start()
	{
		startPosition = base.transform.localPosition;
		movementValue = MovingLightsRandom.StartOffset;
		Vector3 vector = Vector3.LerpUnclamped(StartPositionOffset, EndPositionOffset, (float)((double)Mathf.Sin(MovingLightsRandom.StartOffset) * 0.5 + 0.5));
		vector.x *= (Left ? 1f : (-1f));
		base.transform.localPosition = startPosition + vector;
		MovingLightsRandom movingLightsRandom = MovingLightsRandom;
		movingLightsRandom.ONSwitchStyle = (Action)Delegate.Combine(movingLightsRandom.ONSwitchStyle, new Action(SwitchStyle));
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
	}

	private void Update()
	{
		if (movementEnabled)
		{
			movementValue += Time.deltaTime * movementSpeed * songSpeed;
			Vector3 vector = Vector3.LerpUnclamped(StartPositionOffset, EndPositionOffset, (float)((double)Mathf.Sin(movementValue) * 0.5 + 0.5));
			vector.x *= (Left ? 1f : (-1f));
			base.transform.localPosition = startPosition + vector;
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
		movementValue = MovingLightsRandom.randomStartOffset;
		movementSpeed = Mathf.Abs(MovingLightsRandom.movementSpeed);
		movementValue += MovingLightsRandom.StartOffset;
	}

	public override void UpdateOffset(bool isLeft, BaseEvent evt)
	{
		MovingLightsRandom.RandomUpdate(Left);
		UpdateRotationData(evt.Value, MovingLightsRandom.randomStartOffset);
	}

	private void UpdateRotationData(int beatmapEventDataValue, float startRotationOffset)
	{
		if (beatmapEventDataValue == 0)
		{
			movementEnabled = false;
			Vector3 vector = Vector3.LerpUnclamped(StartPositionOffset, EndPositionOffset, (float)((double)Mathf.Sin(MovingLightsRandom.StartOffset) * 0.5 + 0.5));
			vector.x *= (Left ? 1f : (-1f));
			base.transform.localPosition = startPosition + vector;
		}
		else if (beatmapEventDataValue > 0)
		{
			movementEnabled = true;
			movementValue = startRotationOffset + MovingLightsRandom.StartOffset;
			Vector3 vector2 = Vector3.LerpUnclamped(StartPositionOffset, EndPositionOffset, (float)((double)Mathf.Sin(movementValue) * 0.5 + 0.5));
			vector2.x *= (Left ? 1f : (-1f));
			base.transform.localPosition = startPosition + vector2;
			movementSpeed = beatmapEventDataValue;
			if (Left)
			{
				MovingLightsRandom.movementSpeed = movementSpeed;
			}
		}
	}

	public override bool IsOverrideLightGroup()
	{
		return false;
	}
}
