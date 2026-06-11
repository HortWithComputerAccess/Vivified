using System;
using UnityEngine;

public class CameraPositionToChunk : MonoBehaviour
{
	private static readonly Func<int, int> alternatingChunkFunc = delegate(int x)
	{
		int num = x - Intersections.CurrentGroup;
		return Intersections.CurrentGroup - ((num > 0) ? num : (num - 1));
	};

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private Transform trackTransform;

	private Transform t;

	private void Start()
	{
		t = base.transform;
		Intersections.NextGroupSearchFunction = alternatingChunkFunc;
	}

	private void Update()
	{
		Intersections.CurrentGroup = (int)(trackTransform.InverseTransformPoint(t.position).z / EditorScaleController.EditorScale / 1f);
	}
}
