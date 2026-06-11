using System.Linq;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class TrackLaneRingsManager : TrackLaneRingsManagerBase
{
	[FormerlySerializedAs("ringCount")]
	public int RingCount = 10;

	[FormerlySerializedAs("prefab")]
	public TrackLaneRing Prefab;

	[FormerlySerializedAs("moveFirstRing")]
	public bool MoveFirstRing;

	[FormerlySerializedAs("minPositionStep")]
	public float MINPositionStep = 1f;

	[FormerlySerializedAs("maxPositionStep")]
	public float MAXPositionStep = 2f;

	[FormerlySerializedAs("moveSpeed")]
	public float MoveSpeed = 1f;

	[FormerlySerializedAs("rotationStep")]
	[Header("Rotation")]
	public float RotationStep = 5f;

	[FormerlySerializedAs("propagationSpeed")]
	public float PropagationSpeed = 1f;

	[FormerlySerializedAs("flexySpeed")]
	public float FlexySpeed = 1f;

	[FormerlySerializedAs("rotationEffect")]
	public TrackLaneRingsRotationEffect RotationEffect;

	private bool zoomed;

	public TrackLaneRing[] Rings { get; private set; }

	public void Awake()
	{
		Prefab.gameObject.SetActive(value: false);
		Rings = new TrackLaneRing[RingCount];
		for (int i = 0; i < Rings.Length; i++)
		{
			Rings[i] = Object.Instantiate(Prefab, base.transform);
			Rings[i].gameObject.SetActive(value: true);
			Rings[i].gameObject.name = $"Ring {i}";
			Vector3 pos = new Vector3(0f, 0f, (float)i * MAXPositionStep);
			Rings[i].Init(pos, Vector3.zero);
			if (RingCount <= 1)
			{
				continue;
			}
			foreach (IGrouping<int, LightingEvent> item in from x in Rings[i].GetComponentsInChildren<LightingEvent>()
				group x by (!x.OverrideLightGroup) ? (-1) : x.OverrideLightGroupID)
			{
				foreach (LightingEvent item2 in item)
				{
					item2.PropGroup = i;
					item2.LightID += i * item.Count();
				}
			}
		}
	}

	private void FixedUpdate()
	{
		TrackLaneRing[] rings = Rings;
		for (int i = 0; i < rings.Length; i++)
		{
			rings[i].FixedUpdateRing(TimeHelper.FixedDeltaTime);
		}
	}

	private void LateUpdate()
	{
		TrackLaneRing[] rings = Rings;
		for (int i = 0; i < rings.Length; i++)
		{
			rings[i].LateUpdateRing(TimeHelper.InterpolationFactor);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 forward = base.transform.forward;
		Vector3 position = base.transform.position;
		float num = 0.5f;
		float num2 = 45f;
		Gizmos.DrawRay(position, forward);
		Vector3 vector = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 180f + num2, 0f) * new Vector3(0f, 0f, 1f);
		Vector3 vector2 = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 180f - num2, 0f) * new Vector3(0f, 0f, 1f);
		Gizmos.DrawRay(position + forward, vector * num);
		Gizmos.DrawRay(position + forward, vector2 * num);
	}

	protected virtual bool IsAffectedByZoom()
	{
		return !Mathf.Approximately(MAXPositionStep, MINPositionStep);
	}

	public override void HandlePositionEvent(BaseEvent evt)
	{
		float num = (zoomed ? MAXPositionStep : MINPositionStep);
		if (IsAffectedByZoom() && evt.CustomStep.HasValue)
		{
			num = evt.CustomStep.Value;
		}
		float moveSpeed = evt.CustomSpeed ?? (MoveSpeed * 5f);
		zoomed = !zoomed;
		for (int i = 0; i < Rings.Length; i++)
		{
			float destinationZ = (float)(i + (MoveFirstRing ? 1 : 0)) * num;
			Rings[i].SetPosition(destinationZ, moveSpeed);
		}
	}

	public override void HandleRotationEvent(BaseEvent evt)
	{
		if (RotationEffect != null)
		{
			RotationEffect.AddRingRotationEvent(Rings[0].GetDestinationRotation(), Random.Range(0f, RotationStep), PropagationSpeed, FlexySpeed, evt);
		}
	}

	public override Object[] GetToDestroy()
	{
		return new Object[2] { this, RotationEffect };
	}
}
