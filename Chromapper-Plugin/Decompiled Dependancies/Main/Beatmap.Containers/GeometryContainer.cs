using System;
using Beatmap.Animations;
using Beatmap.Base;
using Beatmap.Base.Customs;
using UnityEngine;

namespace Beatmap.Containers;

public class GeometryContainer : ObjectContainer
{
	private static Mesh triangleMesh;

	public GameObject Shape;

	public BaseEnvironmentEnhancement EnvironmentEnhancement;

	public ObjectAnimator MaterialAnimator;

	public override BaseObject ObjectData
	{
		get
		{
			return EnvironmentEnhancement;
		}
		set
		{
			EnvironmentEnhancement = (BaseEnvironmentEnhancement)value;
		}
	}

	public override void UpdateGridPosition()
	{
	}

	public static GeometryContainer SpawnGeometry(BaseEnvironmentEnhancement eh, ref GameObject prefab)
	{
		if ((string)eh.Geometry[eh.GeometryKeyType] == null)
		{
			return null;
		}
		GeometryContainer component = UnityEngine.Object.Instantiate(prefab).GetComponent<GeometryContainer>();
		PrimitiveType result;
		if (eh.Geometry[eh.GeometryKeyType] == (object)"Triangle")
		{
			result = PrimitiveType.Quad;
		}
		else if (!Enum.TryParse<PrimitiveType>(eh.Geometry[eh.GeometryKeyType], out result))
		{
			Debug.LogError(string.Concat("Invalid geometry type '", eh.Geometry[eh.GeometryKeyType], "'!"));
		}
		component.EnvironmentEnhancement = eh;
		component.Shape = GameObject.CreatePrimitive(result);
		component.Shape.layer = 9;
		Collider componentInChildren = component.Shape.GetComponentInChildren<Collider>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.DestroyImmediate(componentInChildren);
		}
		if (eh.Geometry[eh.GeometryKeyType] == (object)"Triangle")
		{
			if (triangleMesh == null)
			{
				triangleMesh = CreateTriangleMesh();
			}
			component.Shape.GetComponent<MeshFilter>().sharedMesh = triangleMesh;
			component.SelectionRenderers[0].transform.localPosition = new Vector3(0f, 0f, 0.01f);
		}
		else if (result == PrimitiveType.Quad)
		{
			component.SelectionRenderers[0].transform.localPosition = new Vector3(0f, 0f, -0.01f);
		}
		Mesh sharedMesh = component.Shape.GetComponent<MeshFilter>().sharedMesh;
		component.SelectionRenderers[0].GetComponent<MeshFilter>().sharedMesh = sharedMesh;
		IntersectionCollider intersectionCollider = component.Shape.AddComponent<IntersectionCollider>();
		MeshRenderer component2 = component.Shape.GetComponent<MeshRenderer>();
		intersectionCollider.Mesh = sharedMesh;
		intersectionCollider.BoundsRenderer = component2;
		if (component.MaterialPropertyBlock == null)
		{
			component.MaterialPropertyBlock = new MaterialPropertyBlock();
			component.modelRenderers.Add(component2);
		}
		component.Colliders.Add(intersectionCollider);
		component.Shape.transform.parent = component.Animator.AnimationThis.transform;
		component.Shape.transform.localScale = 1.667f * Vector3.one;
		component.Animator.AttachToGeometry(eh);
		component.gameObject.SetActive(value: true);
		component.UpdateCollisionGroups();
		return component;
	}

	private static Mesh CreateTriangleMesh()
	{
		Vector3[] vertices = new Vector3[3]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(0f, 0.5f, 0f)
		};
		Vector2[] uv = new Vector2[3]
		{
			new Vector3(0f, 0f),
			new Vector3(1f, 0f),
			new Vector3(0.5f, 1f)
		};
		int[] triangles = new int[3] { 0, 1, 2 };
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		return mesh;
	}
}
