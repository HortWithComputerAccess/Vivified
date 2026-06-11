using UnityEngine;

namespace Beatmap.Animations;

public class CatmullRomPDI<T> : IPointDataInterpolator<T> where T : struct
{
	public static readonly IPointDataInterpolator<T> Instance = (CatmullRomPDI.Instance as IPointDataInterpolator<T>) ?? new CatmullRomPDI<T>();

	T IPointDataInterpolator<T>.Lerp(PointDefinition<T>.PointData[] points, int prev, int next, float time)
	{
		return LinearPDI<T>.Instance.Lerp(points, prev, next, time);
	}
}
public class CatmullRomPDI : IPointDataInterpolator<Vector3>
{
	public static CatmullRomPDI Instance = new CatmullRomPDI();

	Vector3 IPointDataInterpolator<Vector3>.Lerp(PointDefinition<Vector3>.PointData[] points, int a, int b, float time)
	{
		Vector3 vector = ((a - 1 < 0) ? points[a].Value : points[a - 1].Value);
		Vector3 value = points[a].Value;
		Vector3 value2 = points[b].Value;
		Vector3 vector2 = ((b + 1 > points.Length - 1) ? points[b].Value : points[b + 1].Value);
		float num = time * time;
		float num2 = num * time;
		float num3 = 0f - num2 + 2f * num - time;
		float num4 = 3f * num2 - 5f * num + 2f;
		float num5 = -3f * num2 + 4f * num + time;
		float num6 = num2 - num;
		return 0.5f * (vector * num3 + value * num4 + value2 * num5 + vector2 * num6);
	}
}
