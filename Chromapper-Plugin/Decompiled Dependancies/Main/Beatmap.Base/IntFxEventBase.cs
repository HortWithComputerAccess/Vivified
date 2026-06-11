using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class IntFxEventBase : FxEventBase<int>
{
	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3IntFxEvent.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		return new IntFxEventBase
		{
			JsonTime = JsonTime,
			UsePreviousEventValue = UsePreviousEventValue,
			Value = Value
		};
	}
}
