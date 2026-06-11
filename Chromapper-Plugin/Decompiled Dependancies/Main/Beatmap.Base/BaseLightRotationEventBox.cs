using System;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightRotationEventBox : BaseEventBox
{
	public float RotationDistribution { get; set; }

	public int RotationDistributionType { get; set; }

	public int RotationAffectFirst { get; set; }

	public int Axis { get; set; }

	public int Flip { get; set; }

	public BaseLightRotationBase[] Events { get; set; }

	public BaseLightRotationEventBox()
	{
	}

	protected BaseLightRotationEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float rotationDistribution, int rotationDistributionType, int rotationAffectFirst, int axis, int flip, BaseLightRotationBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType)
	{
		RotationDistribution = rotationDistribution;
		RotationDistributionType = rotationDistributionType;
		RotationAffectFirst = rotationAffectFirst;
		Axis = axis;
		Flip = flip;
		Events = events;
	}

	protected BaseLightRotationEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float rotationDistribution, int rotationDistributionType, int rotationAffectFirst, int axis, int flip, int easing, BaseLightRotationBase[] events)
		: base(indexFilter, beatDistribution, beatDistributionType, easing)
	{
		RotationDistribution = rotationDistribution;
		RotationDistributionType = rotationDistributionType;
		RotationAffectFirst = rotationAffectFirst;
		Axis = axis;
		Flip = flip;
		Events = events;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightRotationEventBox.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
