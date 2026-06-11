using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class InterscopeCarEventHandler : PlatformEventHandler
{
	[FormerlySerializedAs("carRigidbody")]
	[SerializeField]
	protected Rigidbody CarRigidbody;

	[FormerlySerializedAs("carFlags")]
	[Tooltip("All Cars = 0 & 1\nLeft Side = 2\nRight Side = 3\nFirst Row = 4\nSecond Row = 5\nThird Row = 6\nFourth Row = 7")]
	[SerializeField]
	protected int[] CarFlags;

	private HashSet<int> eventValuesHash;

	protected virtual void Start()
	{
		eventValuesHash = new HashSet<int>(CarFlags);
	}

	public override void OnEventTrigger(int type, BaseEvent @event)
	{
		if (@event.Value == 0 || @event.Value == 1 || eventValuesHash.Contains(@event.Value))
		{
			OnCarGroupTriggered(@event);
		}
	}

	protected abstract void OnCarGroupTriggered(BaseEvent @event);
}
