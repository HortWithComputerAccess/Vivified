using UnityEngine;

namespace Beatmap.Animations;

public class HSVPDI<T> : IPointDataInterpolator<T> where T : struct
{
	public static readonly IPointDataInterpolator<T> Instance = (HSVPDI.Instance as IPointDataInterpolator<T>) ?? new HSVPDI<T>();

	T IPointDataInterpolator<T>.Lerp(PointDefinition<T>.PointData[] points, int prev, int next, float time)
	{
		return LinearPDI<T>.Instance.Lerp(points, prev, next, time);
	}
}
public class HSVPDI : IPointDataInterpolator<Color>
{
	public static HSVPDI Instance = new HSVPDI();

	Color IPointDataInterpolator<Color>.Lerp(PointDefinition<Color>.PointData[] points, int a, int b, float time)
	{
		Color.RGBToHSV(points[a].Value, out var H, out var S, out var V);
		Color.RGBToHSV(points[b].Value, out var H2, out var S2, out var V2);
		Color color = Color.HSVToRGB(Mathf.LerpUnclamped(H, H2, time), Mathf.LerpUnclamped(S, S2, time), Mathf.LerpUnclamped(V, V2, time));
		return new Color(color.r, color.g, color.b, Mathf.LerpUnclamped(points[a].Value.a, points[b].Value.a, time));
	}
}
