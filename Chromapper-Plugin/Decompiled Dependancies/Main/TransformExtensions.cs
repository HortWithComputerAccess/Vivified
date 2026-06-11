using UnityEngine;

internal static class TransformExtensions
{
	public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
	{
		Vector3 center = transform.TransformPoint(localBounds.center);
		Vector3 extents = localBounds.extents;
		Vector3 vector = transform.TransformVector(extents.x, 0f, 0f);
		Vector3 vector2 = transform.TransformVector(0f, extents.y, 0f);
		Vector3 vector3 = transform.TransformVector(0f, 0f, extents.z);
		extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
		extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
		extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
		return new Bounds
		{
			center = center,
			extents = extents
		};
	}

	public static Bounds InverseTransformBounds(this Transform transform, Bounds localBounds)
	{
		Vector3 center = transform.InverseTransformPoint(localBounds.center);
		Vector3 extents = localBounds.extents;
		Vector3 vector = transform.InverseTransformVector(extents.x, 0f, 0f);
		Vector3 vector2 = transform.InverseTransformVector(0f, extents.y, 0f);
		Vector3 vector3 = transform.InverseTransformVector(0f, 0f, extents.z);
		extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
		extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
		extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
		return new Bounds
		{
			center = center,
			extents = extents
		};
	}
}
