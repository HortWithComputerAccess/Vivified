using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;

internal class LinkinParkRingLaserManager : TrackLaneRingsManagerBase
{
	[SerializeField]
	private List<RotatingLightsRandom> lpLasers;

	[SerializeField]
	private bool SwitchOnZoom;

	public override Object[] GetToDestroy()
	{
		return new Object[1] { this };
	}

	public override void HandlePositionEvent(BaseEvent evt)
	{
		if (SwitchOnZoom)
		{
			TriggerSwitch();
		}
	}

	public override void HandleRotationEvent(BaseEvent evt)
	{
		if (!SwitchOnZoom)
		{
			TriggerSwitch();
		}
	}

	private void TriggerSwitch()
	{
		lpLasers.ForEach(delegate(RotatingLightsRandom it)
		{
			it.SwitchStyle();
		});
	}
}
