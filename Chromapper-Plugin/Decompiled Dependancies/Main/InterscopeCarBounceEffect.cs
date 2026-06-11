using Beatmap.Base;
using UnityEngine;

public class InterscopeCarBounceEffect : InterscopeCarEventHandler
{
	[SerializeField]
	private Rigidbody wheelRigidbody;

	[SerializeField]
	private Vector3 impulse;

	[SerializeField]
	private float forceRandomness = 0.5f;

	[SerializeField]
	private float eventDelay = 0.5f;

	private float timeSinceLastEvent;

	public override int[] ListeningEventTypes => new int[1] { 8 };

	protected override void OnCarGroupTriggered(BaseEvent @event)
	{
		float timeSinceLevelLoad = Time.timeSinceLevelLoad;
		if (!(timeSinceLevelLoad - timeSinceLastEvent < eventDelay))
		{
			timeSinceLastEvent = timeSinceLevelLoad;
			Transform transform = base.transform;
			wheelRigidbody.AddForceAtPosition(impulse * (1f + Random.Range((0f - forceRandomness) / 2f, forceRandomness / 2f)), transform.position + transform.forward * 0.2f);
			CarRigidbody.WakeUp();
		}
	}
}
