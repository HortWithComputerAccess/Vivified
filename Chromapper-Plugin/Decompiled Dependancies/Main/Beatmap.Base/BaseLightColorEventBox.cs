using System;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightColorEventBox : BaseEventBox
{
	public float BrightnessDistribution { get; set; }

	public int BrightnessDistributionType { get; set; }

	public int BrightnessAffectFirst { get; set; }

	public BaseLightColorBase[] Events { get; set; }

	public BaseLightColorEventBox()
	{
	}

	protected BaseLightColorEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float brightnessDistribution, int brightnessDistributionType, int brightnessAffectFirst, BaseLightColorBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType)
	{
		BrightnessDistribution = brightnessDistribution;
		BrightnessDistributionType = brightnessDistributionType;
		BrightnessAffectFirst = brightnessAffectFirst;
		Events = events;
	}

	protected BaseLightColorEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float brightnessDistribution, int brightnessDistributionType, int brightnessAffectFirst, int easing, BaseLightColorBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType, easing)
	{
		BrightnessDistribution = brightnessDistribution;
		BrightnessDistributionType = brightnessDistributionType;
		BrightnessAffectFirst = brightnessAffectFirst;
		Events = events;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightColorEventBox.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
