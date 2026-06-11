using System;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightTranslationEventBox : BaseEventBox
{
	public float TranslationDistribution { get; set; }

	public int TranslationDistributionType { get; set; }

	public int TranslationAffectFirst { get; set; }

	public int Axis { get; set; }

	public int Flip { get; set; }

	public BaseLightTranslationBase[] Events { get; set; }

	public BaseLightTranslationEventBox()
	{
	}

	protected BaseLightTranslationEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float translationDistribution, int translationDistributionType, int translationAffectFirst, int axis, int flip, BaseLightTranslationBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType)
	{
		TranslationDistribution = translationDistribution;
		TranslationDistributionType = translationDistributionType;
		TranslationAffectFirst = translationAffectFirst;
		Axis = axis;
		Flip = flip;
		Events = events;
	}

	protected BaseLightTranslationEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float translationDistribution, int translationDistributionType, int translationAffectFirst, int axis, int flip, int easing, BaseLightTranslationBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType, easing)
	{
		TranslationDistribution = translationDistribution;
		TranslationDistributionType = translationDistributionType;
		TranslationAffectFirst = translationAffectFirst;
		Axis = axis;
		Flip = flip;
		Events = events;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightTranslationEventBox.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
