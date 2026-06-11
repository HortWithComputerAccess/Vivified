using System;
using UnityEngine;

public class GagaArc : MonoBehaviour
{
	public GameObject TargetObject;

	public Material ArcMaterial;

	private const float thickness = 1f;

	private GameObject lightningMesh;

	private void Start()
	{
		lightningMesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
		lightningMesh.GetComponent<Renderer>().material = ArcMaterial;
		lightningMesh.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		lightningMesh.transform.position = Vector3.zero;
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = TargetObject.transform.position;
		float num = -Math.Sign((double)position2.x - (double)Math.Sign(position2.x) * 14.08);
		Vector3 eulerAngles = Quaternion.LookRotation(new Vector3(position2.x - position.x, 0f, position2.z - position.z)).eulerAngles;
		float num2 = Mathf.Atan((position2.y - position.y) / (position2.z - position.z)) * 57.29578f;
		if ((double)Mathf.Abs(position2.x) < 14.08)
		{
			num2 = 0f - num2;
		}
		lightningMesh.transform.SetPositionAndRotation((position + position2) / 2f, Quaternion.Euler(-90f * num - num2, eulerAngles.y, 90f));
		lightningMesh.transform.localScale = new Vector3(Vector3.Distance(position, position2) / 10f, 1f, 1f);
	}
}
