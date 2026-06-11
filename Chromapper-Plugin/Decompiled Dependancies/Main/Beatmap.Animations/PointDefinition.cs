using System;
using System.Collections.Generic;
using Beatmap.Base.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Animations;

public class PointDefinition<T> : IPointDefinition, IComparable<PointDefinition<T>> where T : struct
{
	public delegate T Parser(JSONArray data, ref int i);

	public delegate T InterpolationHandler(PointData[] points, int prev, int next, float time);

	public class PointData : IComparable<PointData>
	{
		public T Value { get; }

		public float Time { get; }

		public Func<float, float> Easing { get; }

		public InterpolationHandler Lerp { get; }

		public PointData(Parser parser, JSONArray data, float tbegin = 0f, float tend = 0f)
		{
			int i = 0;
			Value = parser(data, ref i);
			int count = data.Count;
			if (count > i)
			{
				Time = ((tend == 0f) ? data[i++].AsFloat : Mathf.LerpUnclamped(tbegin, tend, data[i++]));
			}
			else
			{
				Time = 0f;
			}
			Easing = global::Easing.Linear;
			Lerp = PointDataInterpolators.LinearLerp<T>();
			for (; i < count; i++)
			{
				string text = data[i];
				if (text[0] == 'e')
				{
					Easing = global::Easing.Named(text);
				}
				if (text == "splineCatmullRom")
				{
					Lerp = PointDataInterpolators.CatmullRomLerp<T>();
				}
				if (text == "lerpHSV")
				{
					Lerp = PointDataInterpolators.HSVLerp<T>();
				}
			}
		}

		public int CompareTo(PointData other)
		{
			return Time.CompareTo(other.Time);
		}
	}

	public BaseCustomEvent Source;

	public PointData[] Points;

	public float Duration;

	public float Transition;

	public Func<float, float> Easing;

	public float StartTime { get; private set; }

	public PointDefinition(float start)
	{
		StartTime = start;
	}

	public PointDefinition(Parser parser, IPointDefinition.UntypedParams p, BaseCustomEvent source)
	{
		Source = source;
		StartTime = p.Time;
		Transition = p.Transition;
		Duration = p.Duration;
		Easing = global::Easing.Named(p.Easing ?? "easeLinear");
		List<PointData> list = new List<PointData>();
		JSONNode points = p.Points;
		JSONArray jSONArray2;
		if (!(points is JSONArray jSONArray))
		{
			if (points is JSONString jSONString)
			{
				if (!BeatSaberSongContainer.Instance.Map.PointDefinitions.ContainsKey(jSONString))
				{
					throw new Exception($"Missing point definition {jSONString}");
				}
				jSONArray2 = BeatSaberSongContainer.Instance.Map.PointDefinitions[jSONString];
			}
			else
			{
				jSONArray2 = new JSONArray();
			}
		}
		else
		{
			jSONArray2 = jSONArray;
		}
		JSONArray jSONArray3 = jSONArray2;
		JSONNode.Enumerator enumerator = jSONArray3.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> current = enumerator.Current;
			if (current.Value.AsArray == null)
			{
				list.Add(new PointData(parser, jSONArray3, p.TimeBegin, p.TimeEnd));
				break;
			}
			list.Add(new PointData(parser, current.Value.AsArray, p.TimeBegin, p.TimeEnd));
		}
		Points = list.ToArray();
	}

	public T Interpolate(float time)
	{
		int num = Points.Length;
		if (num == 0)
		{
			return default(T);
		}
		if (Points[num - 1].Time <= time)
		{
			return Points[num - 1].Value;
		}
		if (Points[0].Time >= time)
		{
			return Points[0].Value;
		}
		GetIndexes(time, out var prev, out var next);
		float num2 = Points[next].Time - Points[prev].Time;
		float arg = ((num2 == 0f) ? 0f : ((time - Points[prev].Time) / num2));
		arg = Points[next].Easing(arg);
		return Points[next].Lerp(Points, prev, next, arg);
	}

	private void GetIndexes(float time, out int prev, out int next)
	{
		prev = 0;
		next = Points.Length;
		while (prev < next - 1)
		{
			int num = (prev + next) / 2;
			if (Points[num].Time < time)
			{
				prev = num;
			}
			else
			{
				next = num;
			}
		}
	}

	public int CompareTo(PointDefinition<T> other)
	{
		return StartTime.CompareTo(other.StartTime);
	}
}
