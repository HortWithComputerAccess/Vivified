using System.Linq;
using System.Runtime.CompilerServices;
using Beatmap.V2;
using Beatmap.V3;
using Beatmap.V4;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseEventTypesWithKeywords : BaseItem
{
	public BaseEventTypesForKeywords[] Keywords { get; set; } = new BaseEventTypesForKeywords[0];

	public BaseEventTypesWithKeywords()
	{
	}

	protected BaseEventTypesWithKeywords(BaseEventTypesWithKeywords other)
	{
		if (other != null)
		{
			Keywords = other.Keywords.Select((BaseEventTypesForKeywords x) => x.Clone() as BaseEventTypesForKeywords).ToArray();
		}
	}

	protected BaseEventTypesWithKeywords(BaseEventTypesForKeywords[] keywords)
	{
		Keywords = keywords;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		return mapVersion switch
		{
			2 => V2SpecialEventsKeywordFilters.ToJson(this), 
			3 => V3BasicEventTypesWithKeywords.ToJson(this), 
			4 => V4BasicEventTypesWithKeywords.ToJson(this), 
			_ => throw new SwitchExpressionException(mapVersion), 
		};
	}

	public override BaseItem Clone()
	{
		return new BaseEventTypesWithKeywords(this);
	}
}
