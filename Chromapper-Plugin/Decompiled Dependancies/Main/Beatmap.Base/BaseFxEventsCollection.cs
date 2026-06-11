using System.Linq;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseFxEventsCollection : BaseItem
{
	public IntFxEventBase[] IntFxEvents = new IntFxEventBase[0];

	public FloatFxEventBase[] FloatFxEvents = new FloatFxEventBase[0];

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3FxEventsCollection.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		return new BaseFxEventsCollection
		{
			IntFxEvents = IntFxEvents.Select((IntFxEventBase evt) => evt.Clone() as IntFxEventBase).ToArray(),
			FloatFxEvents = FloatFxEvents.Select((FloatFxEventBase evt) => evt.Clone() as FloatFxEventBase).ToArray()
		};
	}
}
