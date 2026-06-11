using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightTranslationEventBoxGroup<T> : BaseEventBoxGroup<T> where T : BaseLightTranslationEventBox
{
	public override string CustomKeyColor { get; } = "unusedKeyColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightTranslationEventBoxGroup()
	{
	}

	protected BaseLightTranslationEventBoxGroup(float time, int id, List<T> events, JSONNode customData = null)
		: base(time, id, events, customData)
	{
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightTranslationEventBoxGroup.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
