using System.Linq;
using System.Runtime.CompilerServices;
using Beatmap.V2;
using Beatmap.V3;
using Beatmap.V4;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseEventTypesForKeywords : BaseItem
{
	public string Keyword { get; set; }

	public int[] Events { get; set; } = new int[0];

	public BaseEventTypesForKeywords()
	{
	}

	protected BaseEventTypesForKeywords(BaseEventTypesForKeywords other)
	{
		Keyword = other.Keyword;
		Events = other.Events.Select((int x) => x).ToArray();
	}

	protected BaseEventTypesForKeywords(string keyword, int[] events)
	{
		Keyword = keyword;
		Events = events;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		return mapVersion switch
		{
			2 => V2SpecialEventsKeywordFiltersKeywords.ToJson(this), 
			3 => V3BasicEventTypesForKeywords.ToJson(this), 
			4 => V4BasicEventTypesForKeywords.ToJson(this), 
			_ => throw new SwitchExpressionException(mapVersion), 
		};
	}

	public override BaseItem Clone()
	{
		return new BaseEventTypesForKeywords(this);
	}
}
