using Beatmap.Base;
using UnityEngine;

public class SkrillexSecondaryRingManager : TrackLaneRingsManager
{
	[SerializeField]
	private InterscopeRingLaserManager[] laserManagers;

	protected override bool IsAffectedByZoom()
	{
		return true;
	}

	public override void HandlePositionEvent(BaseEvent evt)
	{
		base.HandlePositionEvent(evt);
		base.HandleRotationEvent(evt);
		InterscopeRingLaserManager[] array = laserManagers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].HandlePositionEvent(evt);
		}
	}

	public override void HandleRotationEvent(BaseEvent evt)
	{
	}
}
