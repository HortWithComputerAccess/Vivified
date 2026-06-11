namespace Beatmap.Animations;

public class PointDataInterpolators
{
	public static PointDefinition<T>.InterpolationHandler LinearLerp<T>() where T : struct
	{
		return LinearPDI<T>.Instance.Lerp;
	}

	public static PointDefinition<T>.InterpolationHandler CatmullRomLerp<T>() where T : struct
	{
		return CatmullRomPDI<T>.Instance.Lerp;
	}

	public static PointDefinition<T>.InterpolationHandler HSVLerp<T>() where T : struct
	{
		return HSVPDI<T>.Instance.Lerp;
	}
}
