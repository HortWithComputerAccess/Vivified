using System.Collections.Generic;
using UnityEngine;

public class IntersectionCollider : MonoBehaviour
{
	[Tooltip("The collider mesh. A more detailed mesh results in less performance.")]
	public Mesh Mesh;

	[Tooltip("A renderer on the object that actas as world-space bounds.")]
	public Renderer BoundsRenderer;

	public Vector3 Center = Vector3.zero;

	public Vector3 Size = Vector3.one;

	[HideInInspector]
	public int[] MeshTriangles;

	[HideInInspector]
	public Vector3[] MeshVertices;

	[HideInInspector]
	public int CollisionLayer;

	[HideInInspector]
	public List<int> CollisionGroups = new List<int> { 0 };

	private void OnEnable()
	{
		RefreshMeshData();
	}

	private void OnDisable()
	{
		Intersections.UnregisterColliderFromGroups(this);
	}

	private void OnDestroy()
	{
		Intersections.UnregisterColliderFromGroups(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (!(Mesh == null))
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireMesh(scale: new Vector3
			{
				x = base.transform.lossyScale.x * Size.x,
				y = base.transform.lossyScale.y * Size.y,
				z = base.transform.lossyScale.z * Size.z
			}, mesh: Mesh, position: base.transform.TransformPoint(Center), rotation: base.transform.rotation);
			if (!(BoundsRenderer == null))
			{
				Bounds bounds = BoundsRenderer.bounds;
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
		}
	}

	private void RefreshMeshData()
	{
		if (!(Mesh == null))
		{
			CollisionLayer = base.gameObject.layer;
			MeshTriangles = Mesh.triangles;
			MeshVertices = Mesh.vertices;
			for (int i = 0; i < MeshVertices.Length; i++)
			{
				MeshVertices[i].x = (MeshVertices[i].x + Center.x) * Size.x;
				MeshVertices[i].y = (MeshVertices[i].y + Center.y) * Size.y;
				MeshVertices[i].z = (MeshVertices[i].z + Center.z) * Size.z;
			}
			if (CollisionGroups == null || CollisionGroups.Count == 0)
			{
				CollisionGroups = new List<int> { 0 };
			}
			Intersections.RegisterColliderToGroups(this);
		}
	}
}
