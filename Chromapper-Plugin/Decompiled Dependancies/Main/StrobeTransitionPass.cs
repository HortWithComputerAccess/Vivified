using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Helper;

public class StrobeTransitionPass : StrobeGeneratorPass
{
	private const string DefaultEasing = "easeLinear";

	private const string DefaultLerpType = "RGB";

	private readonly string easing;

	private readonly string lerpType;

	public StrobeTransitionPass(string easing, string lerpType)
	{
		this.easing = ((easing != "easeLinear") ? easing : null);
		this.lerpType = ((lerpType != "RGB") ? lerpType : null);
	}

	public override bool IsEventValidForPass(BaseEvent @event)
	{
		return @event.IsLightEvent();
	}

	public override IEnumerable<BaseEvent> StrobePassForLane(IEnumerable<BaseEvent> original, int type, EventGridContainer.PropMode propMode, int[] propID)
	{
		List<BaseEvent> list = original.Select(BeatmapFactory.Clone).ToList();
		for (int i = 1; i < list.Count; i++)
		{
			BaseEvent baseEvent = list[i - 1];
			baseEvent.CustomEasing = easing;
			baseEvent.CustomLerpType = lerpType;
			BaseEvent baseEvent2 = list[i];
			if (baseEvent2.IsBlue)
			{
				baseEvent2.Value = 4;
			}
			else if (baseEvent2.IsRed)
			{
				baseEvent2.Value = 8;
			}
			else if (baseEvent2.IsWhite)
			{
				baseEvent2.Value = 12;
			}
		}
		return list;
	}
}
