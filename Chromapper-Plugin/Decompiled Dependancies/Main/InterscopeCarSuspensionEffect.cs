using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class InterscopeCarSuspensionEffect : InterscopeCarEventHandler
{
	[SerializeField]
	private float contractDistance = 0.35f;

	[SerializeField]
	private float expandDistance = 0.45f;

	private SpringJoint frontWheelSpringJoint;

	public override int[] ListeningEventTypes => new int[2] { 16, 17 };

	protected override void Start()
	{
		base.Start();
		frontWheelSpringJoint = (from x in GetComponentsInChildren<SpringJoint>()
			where x.connectedBody.name.Contains("FrontWheel")
			select x).FirstOrDefault();
	}

	protected override void OnCarGroupTriggered(BaseEvent @event)
	{
		if (@event.Type == 16)
		{
			SpringJoint springJoint = frontWheelSpringJoint;
			float minDistance = (frontWheelSpringJoint.maxDistance = expandDistance);
			springJoint.minDistance = minDistance;
			CarRigidbody.WakeUp();
		}
		else
		{
			SpringJoint springJoint2 = frontWheelSpringJoint;
			float minDistance = (frontWheelSpringJoint.maxDistance = contractDistance);
			springJoint2.minDistance = minDistance;
			CarRigidbody.WakeUp();
		}
	}
}
