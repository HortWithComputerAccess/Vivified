using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightRotationEventBoxGroup<T> : BaseEventBoxGroup<T> where T : BaseLightRotationEventBox
{
	public override string CustomKeyColor { get; } = "unusedKeyColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightRotationEventBoxGroup()
	{
	}

	protected BaseLightRotationEventBoxGroup(float time, int id, List<T> events, JSONNode customData = null)
		: base(time, id, events, customData)
	{
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightRotationEventBoxGroup.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
