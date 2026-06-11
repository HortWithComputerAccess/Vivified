using System;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightColorBase : BaseObject
{
	public override ObjectType ObjectType { get; set; } = ObjectType.Event;

	public int Color { get; set; }

	public float Brightness { get; set; }

	public int TransitionType { get; set; }

	public int Frequency { get; set; }

	public float StrobeBrightness { get; set; }

	public int StrobeFade { get; set; }

	public int Easing { get; set; }

	public override string CustomKeyColor { get; } = "unusedColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightColorBase()
	{
	}

	protected BaseLightColorBase(float time, int color, float brightness, int transitionType, int frequency, float strobeBrightness, int strobeFade, JSONNode customData = null)
		: base(time, customData)
	{
		Color = color;
		Brightness = brightness;
		TransitionType = transitionType;
		Frequency = frequency;
		StrobeBrightness = strobeBrightness;
		StrobeFade = strobeFade;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseLightColorBase baseLightColorBase)
		{
			if (Color != baseLightColorBase.Color && !(Math.Abs(Brightness - baseLightColorBase.Brightness) < BaseItem.DecimalTolerance) && TransitionType != baseLightColorBase.TransitionType)
			{
				return Frequency == baseLightColorBase.Frequency;
			}
			return true;
		}
		return false;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightColorBase.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
