using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class StrobeStepGradientPass : StrobeGeneratorPass
{
	private readonly Func<float, float> easing;

	private readonly bool alternateColors;

	private readonly float precision;

	private int value;

	public StrobeStepGradientPass(int value, bool switchColors, float precision, Func<float, float> easing)
	{
		this.value = value;
		alternateColors = switchColors;
		this.precision = precision;
		this.easing = easing;
	}

	public override bool IsEventValidForPass(BaseEvent @event)
	{
		return !@event.IsUtilityEvent();
	}

	public override IEnumerable<BaseEvent> StrobePassForLane(IEnumerable<BaseEvent> original, int type, EventGridContainer.PropMode propMode, int[] propID)
	{
		List<BaseEvent> list = new List<BaseEvent>();
		float jsonTime = original.First().JsonTime;
		float jsonTime2 = original.Last().JsonTime;
		Dictionary<float, Color> dictionary = new Dictionary<float, Color>();
		foreach (BaseEvent e in original)
		{
			if (e.CustomLightGradient != null)
			{
				dictionary.Add(e.JsonTime, e.CustomLightGradient.StartColor);
				dictionary.Add(e.JsonTime + e.CustomLightGradient.Duration, e.CustomLightGradient.EndColor);
			}
			else if (e.CustomColor.HasValue)
			{
				dictionary.Add(e.JsonTime, e.CustomColor.Value);
			}
			else if (e.IsOff)
			{
				KeyValuePair<float, Color> keyValuePair = dictionary.Where((KeyValuePair<float, Color> x) => x.Key < e.JsonTime).LastOrDefault();
				dictionary.Add(e.JsonTime, (!keyValuePair.Equals(default(KeyValuePair<float, Color>))) ? keyValuePair.Value.WithAlpha(0f) : new Color(0f, 0f, 0f, 0f));
			}
		}
		if (dictionary.Count < 2)
		{
			return Enumerable.Empty<BaseEvent>();
		}
		float num = jsonTime2 - jsonTime;
		KeyValuePair<float, Color> keyValuePair2 = dictionary.ElementAt(0);
		KeyValuePair<float, Color> keyValuePair3 = dictionary.ElementAt(1);
		int num2 = Mathf.CeilToInt(num * precision);
		for (int num3 = 0; num3 < num2 + 1; num3++)
		{
			float num4 = Mathf.Clamp((float)num3 / precision, 0f, num);
			float newTime = jsonTime + num4;
			KeyValuePair<float, Color> keyValuePair4 = dictionary.Where((KeyValuePair<float, Color> x) => x.Key <= newTime).LastOrDefault();
			if (keyValuePair4.Key != keyValuePair2.Key)
			{
				IEnumerable<KeyValuePair<float, Color>> source = dictionary.Where((KeyValuePair<float, Color> x) => x.Key > newTime);
				if (source.Any())
				{
					keyValuePair2 = keyValuePair4;
					keyValuePair3 = source.First();
				}
			}
			float t = easing(Mathf.InverseLerp(keyValuePair2.Key, keyValuePair3.Key, newTime));
			Color color = Color.Lerp(keyValuePair2.Value, keyValuePair3.Value, t);
			BaseEvent baseEvent = new BaseEvent
			{
				JsonTime = newTime,
				Type = type,
				Value = value
			};
			baseEvent.CustomColor = color;
			if (propMode != EventGridContainer.PropMode.Off)
			{
				baseEvent.CustomLightID = propID;
			}
			list.Add(baseEvent);
			if (alternateColors)
			{
				value = InvertColors(value);
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
