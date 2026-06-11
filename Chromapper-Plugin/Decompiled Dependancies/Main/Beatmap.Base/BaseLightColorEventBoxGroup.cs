using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightColorEventBoxGroup<T> : BaseEventBoxGroup<T> where T : BaseLightColorEventBox
{
	public override string CustomKeyColor { get; } = "unusedKeyColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightColorEventBoxGroup()
	{
	}

	protected BaseLightColorEventBoxGroup(float time, int id, List<T> events, JSONNode customData = null)
		: base(time, id, events, customData)
	{
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightColorEventBoxGroup.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
