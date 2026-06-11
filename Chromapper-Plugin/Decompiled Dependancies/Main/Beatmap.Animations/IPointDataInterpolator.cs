namespace Beatmap.Animations;

public interface IPointDataInterpolator<T> where T : struct
{
	T Lerp(PointDefinition<T>.PointData[] points, int prev, int next, float time);
}
