using System;
using System.Collections.Generic;
using Beatmap.Base.Customs;
using UnityEngine;

namespace Beatmap.Animations;

public class AnimateProperty<T> : IAnimateProperty where T : struct
{
	public List<PointDefinition<T>> PointDefinitions;

	public Action<T> Setter;

	public T Default;

	private int count;

	public float StartTime { get; private set; } = float.PositiveInfinity;

	public AnimateProperty(List<PointDefinition<T>> points, Action<T> setter, T _default)
	{
		PointDefinitions = points;
		Setter = setter;
		Default = _default;
		count = 0;
	}

	public bool IsEmpty()
	{
		return PointDefinitions.Count == 0;
	}

	public void AddPointDef(PointDefinition<T>.Parser parser, IPointDefinition.UntypedParams p, BaseCustomEvent source)
	{
		for (int i = 0; i <= p.Repeat; i++)
		{
			IPointDefinition.UntypedParams p2 = p;
			p2.TimeBegin = p.TimeBegin + (float)i * p.Duration;
			p2.TimeEnd = p.TimeEnd + (float)i * p.Duration;
			if (i > 0)
			{
				p2.Time = p2.TimeBegin;
			}
			PointDefinitions.Add(new PointDefinition<T>(parser, p2, source));
		}
	}

	public T GetLerpedValue(float time)
	{
		GetIndexes(time, out var prev, out var _);
		if (prev < 0)
		{
			return Default;
		}
		PointDefinition<T> pointDefinition = PointDefinitions[prev];
		if (pointDefinition.StartTime < time && time < pointDefinition.StartTime + pointDefinition.Duration)
		{
			float num = time - pointDefinition.StartTime;
			float num2 = pointDefinition.Easing(Mathf.Min(num / pointDefinition.Duration, 1f));
			float time2 = pointDefinition.StartTime + num2 * pointDefinition.Duration;
			return pointDefinition.Interpolate(time2);
		}
		if (time > pointDefinition.StartTime + pointDefinition.Transition)
		{
			return pointDefinition.Interpolate(time);
		}
		float num3 = time - pointDefinition.StartTime;
		float interpolation = pointDefinition.Easing(Mathf.Min(num3 / pointDefinition.Transition, 1f));
		return PointDefinitionInterpolation.Lerp((prev == 0) ? null : PointDefinitions[prev - 1], PointDefinitions[prev], interpolation, time, Default);
	}

	public void UpdateProperty(float time)
	{
		Setter(GetLerpedValue(time));
	}

	public void Sort()
	{
		PointDefinitions.Sort();
		StartTime = PointDefinitions[0].StartTime;
		count = PointDefinitions.Count;
	}

	public void RemoveEvent(BaseCustomEvent ev)
	{
		PointDefinitions.RemoveAll((PointDefinition<T> pd) => pd.Source == ev);
	}

	private void GetIndexes(float time, out int prev, out int next)
	{
		prev = 0;
		next = count;
		while (prev < next - 1)
		{
			int num = (prev + next) / 2;
			if (PointDefinitions[num].StartTime < time)
			{
				prev = num;
			}
			else
			{
				next = num;
			}
		}
	}
}
