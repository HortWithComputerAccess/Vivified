using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class StrobeLaserSpeedInterpolationPass : StrobeGeneratorPass
{
	private readonly int decimalPrecision;

	private readonly Func<float, float> easingFunc;

	private readonly float interval;

	private readonly bool leftRotatesClockwise;

	private readonly bool lockLaserRotation;

	private readonly bool overrideDirection;

	private readonly System.Random random;

	private readonly bool rightRotatesClockwise;

	public StrobeLaserSpeedInterpolationPass(float interval, string easingID, int spinDirection, bool uniqueLaserDirection, bool lockRotation, int decimalPrecision)
	{
		this.interval = interval;
		lockLaserRotation = lockRotation;
		this.decimalPrecision = decimalPrecision;
		easingFunc = Easing.Named(easingID);
		random = new System.Random();
		overrideDirection = lockLaserRotation;
		if (spinDirection != 2)
		{
			leftRotatesClockwise = spinDirection == 0;
			rightRotatesClockwise = spinDirection == 0;
		}
		else
		{
			leftRotatesClockwise = random.Next() == 1;
			rightRotatesClockwise = random.Next() == 1;
		}
		if (uniqueLaserDirection)
		{
			rightRotatesClockwise = !leftRotatesClockwise;
		}
	}

	public override bool IsEventValidForPass(BaseEvent @event)
	{
		return @event.IsLaserRotationEvent();
	}

	public override IEnumerable<BaseEvent> StrobePassForLane(IEnumerable<BaseEvent> original, int type, EventGridContainer.PropMode propMode, int[] propID)
	{
		List<BaseEvent> list = new List<BaseEvent>();
		float jsonTime = original.First().JsonTime;
		float endTime = original.Last().JsonTime;
		float distanceInBeats = endTime - jsonTime;
		float num = distanceInBeats;
		BaseEvent lastPassed = original.First();
		BaseEvent baseEvent = original.ElementAt(1);
		float laserSpeedFromEvent = GetLaserSpeedFromEvent(lastPassed);
		float laserSpeedFromEvent2 = GetLaserSpeedFromEvent(baseEvent);
		for (; distanceInBeats >= 0f; distanceInBeats -= 1f / interval)
		{
			BaseEvent baseEvent2 = original.LastOrDefault((BaseEvent x) => x.JsonTime <= endTime - distanceInBeats);
			if (lastPassed != baseEvent2)
			{
				lastPassed = baseEvent2;
				baseEvent = original.FirstOrDefault((BaseEvent x) => x.JsonTime > lastPassed.JsonTime);
				laserSpeedFromEvent = GetLaserSpeedFromEvent(lastPassed);
				if (baseEvent == null)
				{
					baseEvent = lastPassed;
				}
				laserSpeedFromEvent2 = GetLaserSpeedFromEvent(baseEvent);
			}
			float num2 = num - distanceInBeats + jsonTime;
			float arg = Mathf.InverseLerp(lastPassed.JsonTime, baseEvent.JsonTime, num2);
			float num3 = (float)Math.Round(Mathf.Lerp(laserSpeedFromEvent, laserSpeedFromEvent2, easingFunc(arg)), decimalPrecision);
			int num4 = (int)Math.Max(1.0, Math.Round(num3, MidpointRounding.AwayFromZero));
			BaseEvent baseEvent3 = new BaseEvent
			{
				JsonTime = num2,
				Type = type,
				Value = num4,
				FloatValue = 0f
			};
			if (Math.Abs(num3 - (float)num4) > 0.01f)
			{
				baseEvent3.CustomSpeed = num3;
			}
			if (overrideDirection)
			{
				BaseEvent baseEvent4 = baseEvent3;
				baseEvent4.CustomDirection = type switch
				{
					12 => Convert.ToInt32(leftRotatesClockwise), 
					13 => Convert.ToInt32(rightRotatesClockwise), 
					_ => baseEvent3.CustomDirection, 
				};
			}
			if (lockLaserRotation)
			{
				baseEvent3.CustomLockRotation = true;
			}
			list.Add(baseEvent3);
		}
		return list;
	}

	private float GetLaserSpeedFromEvent(BaseEvent @event)
	{
		if (!@event.CustomPreciseSpeed.HasValue && !@event.CustomSpeed.HasValue)
		{
			return @event.Value;
		}
		return (@event.CustomPreciseSpeed ?? @event.CustomSpeed).Value;
	}
}
