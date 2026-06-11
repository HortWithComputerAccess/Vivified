using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseVfxEventEventBoxGroup<T> : BaseEventBoxGroup<T> where T : BaseVfxEventEventBox
{
	public int Type { get; set; }

	public override string CustomKeyColor { get; } = "unusedKeyColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseVfxEventEventBoxGroup()
	{
	}

	protected BaseVfxEventEventBoxGroup(float time, int id, int type, List<T> events, JSONNode customData = null)
		: base(time, id, events, customData)
	{
		Type = type;
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
