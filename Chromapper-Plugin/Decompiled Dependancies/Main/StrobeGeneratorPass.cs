using System.Collections.Generic;
using Beatmap.Base;

public abstract class StrobeGeneratorPass
{
	public abstract bool IsEventValidForPass(BaseEvent @event);

	public abstract IEnumerable<BaseEvent> StrobePassForLane(IEnumerable<BaseEvent> original, int type, EventGridContainer.PropMode propMode, int[] propID);
}
