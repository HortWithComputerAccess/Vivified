using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;

public class StrobeLightingPass : StrobeGeneratorPass
{
	private readonly bool alternateColors;

	private readonly bool dynamic;

	private readonly Func<float, float> easingFunc;

	private readonly float precision;

	private readonly IEnumerable<int> values;

	private readonly bool easeTime;

	private readonly bool easeValue;

	public StrobeLightingPass(IEnumerable<int> alternatingValues, bool switchColors, bool dynamicStrobe, float strobePrecision, string strobeEasing, bool easingTimeSwitch, bool easingValueSwitch)
	{
		values = alternatingValues;
		alternateColors = switchColors;
		dynamic = dynamicStrobe;
		precision = strobePrecision;
		easingFunc = Easing.Named(strobeEasing);
		easeTime = easingTimeSwitch;
		easeValue = easingValueSwitch;
	}

	public override bool IsEventValidForPass(BaseEvent @event)
	{
		if (!@event.IsUtilityEvent())
		{
			return !@event.IsLegacyChroma;
		}
		return false;
	}

	public override IEnumerable<BaseEvent> StrobePassForLane(IEnumerable<BaseEvent> original, int type, EventGridContainer.PropMode propMode, int[] propID)
	{
		List<BaseEvent> list = new List<BaseEvent>();
		float jsonTime = original.First().JsonTime;
		float endTime = original.Last().JsonTime;
		float floatValue = original.First().FloatValue;
		float num = original.Last().FloatValue - floatValue;
		List<int> list2 = new List<int>(values);
		int num2 = 0;
		if (alternateColors)
		{
			for (int i = 0; i < values.Count(); i++)
			{
				list2.Add(InvertColors(list2[i]));
			}
		}
		float distanceInBeats = endTime - jsonTime;
		float num3 = distanceInBeats;
		BaseEvent baseEvent = null;
		while (distanceInBeats >= 0f)
		{
			if (num2 >= list2.Count)
			{
				num2 = 0;
			}
			BaseEvent baseEvent2 = original.Where((BaseEvent x) => x.JsonTime <= endTime - distanceInBeats).LastOrDefault();
			if (baseEvent2 != baseEvent && dynamic && LightEventHelper.IsBlueFromValue(baseEvent2.Value) != LightEventHelper.IsBlueFromValue(list2[num2]))
			{
				baseEvent = baseEvent2;
				for (int num4 = 0; num4 < list2.Count; num4++)
				{
					list2[num4] = InvertColors(list2[num4]);
				}
			}
			int value = list2[num2];
			float num5 = (num3 - distanceInBeats) / num3;
			float jsonTime2 = (easeTime ? (easingFunc(num5) * num3 + jsonTime) : (num5 * num3 + jsonTime));
			float floatValue2 = (easeValue ? (easingFunc(num5) * num + floatValue) : (num5 * num + floatValue));
			BaseEvent baseEvent3 = new BaseEvent
			{
				JsonTime = jsonTime2,
				Type = type,
				Value = value,
				FloatValue = floatValue2
			};
			if (propMode != EventGridContainer.PropMode.Off)
			{
				baseEvent3.CustomLightID = propID;
			}
			list.Add(baseEvent3);
			num2++;
			if (distanceInBeats > 0f && (distanceInBeats -= 1f / precision) < -0.001f)
			{
				distanceInBeats = 0f;
			}
			else if (distanceInBeats <= 0f)
			{
				break;
			}
		}
		return list;
	}

	private int InvertColors(int colorValue)
	{
		return colorValue switch
		{
			1 => 5, 
			2 => 6, 
			3 => 7, 
			4 => 8, 
			5 => 1, 
			6 => 2, 
			7 => 3, 
			8 => 4, 
			9 => 9, 
			10 => 10, 
			11 => 11, 
			12 => 12, 
			_ => 0, 
		};
	}
}
