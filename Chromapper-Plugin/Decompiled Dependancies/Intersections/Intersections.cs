using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Intersections
{
	public readonly struct IntersectionHit(GameObject gameObject, Bounds bounds, Ray impactRay, float distance)
	{
		public readonly GameObject GameObject = gameObject;

		public readonly Bounds Bounds = bounds;

		public readonly Vector3 Point = impactRay.GetPoint(distance);

		public readonly float Distance = distance;
	}

	private static Vector3 e1;

	private static Vector3 e2;

	private static Vector3 p;

	private static Vector3 q;

	private static Vector3 t;

	public const int ChunkSize = 1;

	public static Func<int, int> NextGroupSearchFunction;

	public static int CurrentGroup;

	private static readonly List<IntersectionCollider>[] colliders;

	private static readonly Dictionary<int, List<IntersectionCollider>>[] groupedColliders;

	private const float intersectionEpsilon = 0.0001f;

	private static bool RaycastIndividual_Internal(IntersectionCollider collider, in Vector3 rayDirection, in Vector3 rayOrigin, out float distance)
	{
		bool flag = false;
		distance = 0f;
		Matrix4x4 matrix = collider.transform.localToWorldMatrix;
		int[] meshTriangles = collider.MeshTriangles;
		Vector3[] meshVertices = collider.MeshVertices;
		for (int i = 0; i < meshTriangles.Length; i += 3)
		{
			if (RayTriangleIntersect(matrix.FastMultiplyPoint3x4(in meshVertices[meshTriangles[i]]), matrix.FastMultiplyPoint3x4(in meshVertices[meshTriangles[i + 1]]), matrix.FastMultiplyPoint3x4(in meshVertices[meshTriangles[i + 2]]), in rayDirection, in rayOrigin, out var distance2) && (!flag || distance2 < distance))
			{
				flag = true;
				distance = distance2;
			}
		}
		return flag;
	}

	private static bool RayTriangleIntersect(in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 rayDirection, in Vector3 rayOrigin, out float distance)
	{
		distance = 0f;
		VectorUtils.FastSubtraction(ref e1, in p2, in p1);
		VectorUtils.FastSubtraction(ref e2, in p3, in p1);
		VectorUtils.FastCross(ref p, in rayDirection, in e2);
		float num = VectorUtils.FastDot(in e1, in p);
		if (num > -0.0001f && num < 0.0001f)
		{
			return false;
		}
		float num2 = 1f / num;
		VectorUtils.FastSubtraction(ref t, in rayOrigin, in p1);
		float num3 = VectorUtils.FastDot(in t, in p) * num2;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		VectorUtils.FastCross(ref q, in t, in e1);
		float num4 = VectorUtils.FastDot(in rayDirection, in q) * num2;
		if (num4 < 0f || num3 + num4 > 1f)
		{
			return false;
		}
		if ((distance = VectorUtils.FastDot(in e2, in q) * num2) > 0.0001f)
		{
			return true;
		}
		return false;
	}

	public static bool Raycast(Ray ray, out IntersectionHit hit)
	{
		float distance;
		return Raycast(ray, -1, out hit, out distance);
	}

	public static bool Raycast(Ray ray, out IntersectionHit hit, out float distance)
	{
		return Raycast(ray, -1, out hit, out distance);
	}

	public static bool Raycast(Ray ray, int layer, out IntersectionHit hit)
	{
		float distance;
		return Raycast(ray, layer, out hit, out distance);
	}

	public static bool Raycast(Ray ray, int layer, out IntersectionHit hit, out float distance)
	{
		List<IntersectionHit> list = new List<IntersectionHit>();
		hit = default(IntersectionHit);
		distance = float.PositiveInfinity;
		Vector3 rayDirection = ray.direction;
		Vector3 rayOrigin = ray.origin;
		int num = ((layer != -1) ? layer : 0);
		int num2 = ((layer == -1) ? 32 : (layer + 1));
		for (int i = num; i < num2; i++)
		{
			list.Clear();
			Dictionary<int, List<IntersectionCollider>> dictionary = groupedColliders[i];
			if (dictionary.Count <= 0)
			{
				continue;
			}
			Dictionary<int, List<IntersectionCollider>>.KeyCollection keys = dictionary.Keys;
			int num3 = keys.Min();
			int num4 = keys.Max();
			int num5 = num3;
			int num6 = num4;
			int num7 = Mathf.Clamp(CurrentGroup, num5, num6);
			int num8 = Math.Max(num7 - num5, num6 - num7) * 2 + 1;
			for (int j = 0; j < num8; j++)
			{
				if (num7 < num5 || num7 > num6)
				{
					num7 = NextGroupSearchFunction(num7);
					continue;
				}
				if (dictionary.TryGetValue(num7, out var value) && value.Count > 0)
				{
					int count = value.Count;
					for (int k = 0; k < count; k++)
					{
						IntersectionCollider intersectionCollider = value[k];
						if (layer == -1 || intersectionCollider.CollisionLayer == layer)
						{
							Bounds bounds = intersectionCollider.BoundsRenderer.bounds;
							if (bounds.IntersectRay(ray) && RaycastIndividual_Internal(intersectionCollider, in rayDirection, in rayOrigin, out var distance2))
							{
								list.Add(new IntersectionHit(intersectionCollider.gameObject, bounds, ray, distance2));
							}
						}
					}
				}
				num7 = NextGroupSearchFunction(num7);
			}
			if (list.Count <= 0)
			{
				continue;
			}
			int count2 = list.Count;
			for (int l = 0; l < count2; l++)
			{
				IntersectionHit intersectionHit = list[l];
				if (intersectionHit.Distance < distance)
				{
					hit = intersectionHit;
					distance = intersectionHit.Distance;
				}
			}
			return true;
		}
		return false;
	}

	public static IEnumerable<IntersectionHit> RaycastAll(Ray ray)
	{
		return RaycastAll(ray, -1);
	}

	public static IEnumerable<IntersectionHit> RaycastAll(Ray ray, int layer)
	{
		List<IntersectionHit> list = new List<IntersectionHit>();
		Vector3 rayDirection = ray.direction;
		Vector3 rayOrigin = ray.origin;
		int num = ((layer != -1) ? layer : 0);
		int num2 = ((layer == -1) ? 32 : (layer + 1));
		for (int i = num; i < num2; i++)
		{
			Dictionary<int, List<IntersectionCollider>> dictionary = groupedColliders[i];
			if (dictionary.Count <= 0)
			{
				continue;
			}
			Dictionary<int, List<IntersectionCollider>>.KeyCollection keys = dictionary.Keys;
			int num3 = keys.Min();
			int num4 = keys.Max();
			int num5 = num3;
			int num6 = num4;
			int num7 = Mathf.Clamp(CurrentGroup, num5, num6);
			int num8 = Math.Max(num7 - num5, num6 - num7) * 2 + 1;
			for (int j = 0; j < num8; j++)
			{
				if (num7 < num5 || num7 > num6)
				{
					num7 = NextGroupSearchFunction(num7);
					continue;
				}
				if (dictionary.TryGetValue(num7, out var value) && value.Count > 0)
				{
					int count = value.Count;
					for (int k = 0; k < count; k++)
					{
						IntersectionCollider intersectionCollider = value[k];
						if (layer == -1 || intersectionCollider.CollisionLayer == layer)
						{
							Bounds bounds = intersectionCollider.BoundsRenderer.bounds;
							if (bounds.IntersectRay(ray) && RaycastIndividual_Internal(intersectionCollider, in rayDirection, in rayOrigin, out var distance))
							{
								list.Add(new IntersectionHit(intersectionCollider.gameObject, bounds, ray, distance));
							}
						}
					}
				}
				num7 = NextGroupSearchFunction(num7);
			}
		}
		return list;
	}

	static Intersections()
	{
		NextGroupSearchFunction = (int x) => ++x;
		CurrentGroup = 0;
		colliders = new List<IntersectionCollider>[32];
		groupedColliders = new Dictionary<int, List<IntersectionCollider>>[32];
		for (int num = 0; num < 32; num++)
		{
			colliders[num] = new List<IntersectionCollider>();
			groupedColliders[num] = new Dictionary<int, List<IntersectionCollider>>();
		}
	}

	public static void RegisterColliderToGroups(IntersectionCollider collider)
	{
		Dictionary<int, List<IntersectionCollider>> dictionary = groupedColliders[collider.CollisionLayer];
		foreach (int collisionGroup in collider.CollisionGroups)
		{
			if (!dictionary.TryGetValue(collisionGroup, out var value))
			{
				value = new List<IntersectionCollider>();
				dictionary.Add(collisionGroup, value);
			}
			value.Add(collider);
		}
	}

	public static bool UnregisterColliderFromGroups(IntersectionCollider collider)
	{
		Dictionary<int, List<IntersectionCollider>> dictionary = groupedColliders[collider.CollisionLayer];
		bool flag = false;
		foreach (int collisionGroup in collider.CollisionGroups)
		{
			if (dictionary.TryGetValue(collisionGroup, out var value))
			{
				flag |= value.Remove(collider);
				if (value.Count == 0)
				{
					dictionary.Remove(collisionGroup);
				}
			}
		}
		return flag;
	}

	public static void Clear()
	{
		Dictionary<int, List<IntersectionCollider>>[] array = groupedColliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clear();
		}
	}
}
