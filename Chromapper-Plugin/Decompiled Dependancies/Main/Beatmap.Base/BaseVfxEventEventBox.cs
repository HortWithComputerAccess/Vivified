using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseVfxEventEventBox : BaseEventBox
{
	public float VfxDistribution { get; set; }

	public int VfxDistributionType { get; set; }

	public int VfxAffectFirst { get; set; }

	public List<FloatFxEventBase> FloatFxEvents { get; set; } = new List<FloatFxEventBase>();

	public BaseVfxEventEventBox()
	{
	}

	protected BaseVfxEventEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float vfxDistribution, int vfxDistributionType, int vfxAffectFirst, IList<FloatFxEventBase> floatFxEvents)
		: base(indexFilter, beatDistribution, beatDistributionType)
	{
		VfxDistribution = vfxDistribution;
		VfxDistributionType = vfxDistributionType;
		VfxAffectFirst = vfxAffectFirst;
		FloatFxEvents = FloatFxEvents.Select((FloatFxEventBase e) => (FloatFxEventBase)e.Clone()).ToList();
	}

	protected BaseVfxEventEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, float vfxDistribution, int vfxDistributionType, int vfxAffectFirst, int easing, IList<FloatFxEventBase> floatFxEvents)
		: base(indexFilter, beatDistribution, beatDistributionType, easing)
	{
		VfxDistribution = vfxDistribution;
		VfxDistributionType = vfxDistributionType;
		VfxAffectFirst = vfxAffectFirst;
		FloatFxEvents = FloatFxEvents.Select((FloatFxEventBase e) => (FloatFxEventBase)e.Clone()).ToList();
	}

	public override JSONNode ToJson()
	{
		throw new NotImplementedException();
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
