using Beatmap.Base;

public class SkrillexPrimaryRingManager : TrackLaneRingsManager
{
	protected override bool IsAffectedByZoom()
	{
		return true;
	}

	public override void HandlePositionEvent(BaseEvent evt)
	{
	}

	public override void HandleRotationEvent(BaseEvent evt)
	{
		base.HandleRotationEvent(evt);
		base.HandlePositionEvent(evt);
	}
}
