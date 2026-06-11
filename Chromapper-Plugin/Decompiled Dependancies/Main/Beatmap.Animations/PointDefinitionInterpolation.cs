using System;
using UnityEngine;

namespace Beatmap.Animations;

public class PointDefinitionInterpolation
{
	public static T Lerp<T>(PointDefinition<T>? prev, PointDefinition<T> next, float interpolation, float time, T _default) where T : struct
	{
		T val = prev?.Interpolate(time) ?? _default;
		T val2 = next.Interpolate(time);
		if (val is float a)
		{
			if (val2 is float b)
			{
				return (T)(object)Mathf.LerpUnclamped(a, b, interpolation);
			}
		}
		else if (val is Color a2)
		{
			if (val2 is Color b2)
			{
				return (T)(object)Color.LerpUnclamped(a2, b2, interpolation);
			}
		}
		else if (val is Vector3 a3)
		{
			if (val2 is Vector3 b3)
			{
				return (T)(object)Vector3.LerpUnclamped(a3, b3, interpolation);
			}
		}
		else if (val is Quaternion a4 && val2 is Quaternion b4)
		{
			return (T)(object)Quaternion.SlerpUnclamped(a4, b4, interpolation);
		}
		throw new Exception("Unhandled PointDefinition Lerp for type " + typeof(T).Name);
	}
}
