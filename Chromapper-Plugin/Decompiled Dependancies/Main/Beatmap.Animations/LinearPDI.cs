using System;
using UnityEngine;

namespace Beatmap.Animations;

public class LinearPDI<T> : IPointDataInterpolator<T> where T : struct
{
	public static readonly IPointDataInterpolator<T> Instance = (LinearPDI.Instance as IPointDataInterpolator<T>) ?? new LinearPDI<T>();

	T IPointDataInterpolator<T>.Lerp(PointDefinition<T>.PointData[] points, int prev, int next, float time)
	{
		throw new Exception("Unhandled LerpFunc for type " + typeof(T).Name);
	}
}
public class LinearPDI : IPointDataInterpolator<float>, IPointDataInterpolator<Color>, IPointDataInterpolator<Vector3>, IPointDataInterpolator<Quaternion>
{
	public static LinearPDI Instance = new LinearPDI();

	float IPointDataInterpolator<float>.Lerp(PointDefinition<float>.PointData[] points, int prev, int next, float time)
	{
		return Mathf.LerpUnclamped(points[prev].Value, points[next].Value, time);
	}

	Color IPointDataInterpolator<Color>.Lerp(PointDefinition<Color>.PointData[] points, int prev, int next, float time)
	{
		return Color.LerpUnclamped(points[prev].Value, points[next].Value, time);
	}

	Vector3 IPointDataInterpolator<Vector3>.Lerp(PointDefinition<Vector3>.PointData[] points, int prev, int next, float time)
	{
		return Vector3.LerpUnclamped(points[prev].Value, points[next].Value, time);
	}

	Quaternion IPointDataInterpolator<Quaternion>.Lerp(PointDefinition<Quaternion>.PointData[] points, int prev, int next, float time)
	{
		return Quaternion.SlerpUnclamped(points[prev].Value, points[next].Value, time);
	}
}
