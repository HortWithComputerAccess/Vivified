using System.Runtime.CompilerServices;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using SimpleJSON;

namespace Beatmap.Base.Customs;

public class BaseBpmChange : BaseBpmEvent
{
	public float BeatsPerBar { get; set; }

	public float MetronomeOffset { get; set; }

	public string KeyTime
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_time", 
				3 => "b", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyBeatsPerBar
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_beatsPerBar", 
				3 => "p", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyBpm
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_BPM", 
				3 => "m", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyMetronomeOffset
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_metronomeOffset", 
				3 => "o", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public BaseBpmChange()
	{
	}

	protected BaseBpmChange(BaseBpmChange other)
	{
		base.JsonTime = other.JsonTime;
		base.Bpm = other.Bpm;
		BeatsPerBar = other.BeatsPerBar;
		MetronomeOffset = other.MetronomeOffset;
	}

	protected BaseBpmChange(BaseBpmEvent other)
	{
		base.JsonTime = other.JsonTime;
		base.Bpm = other.Bpm;
		BeatsPerBar = 4f;
		MetronomeOffset = 4f;
	}

	public BaseBpmChange(JSONNode node)
	{
		base.JsonTime = RetrieveRequiredNode(node, KeyTime).AsFloat;
		base.Bpm = RetrieveRequiredNode(node, KeyBpm).AsFloat;
		BeatsPerBar = (node.HasKey(KeyBeatsPerBar) ? node[KeyBeatsPerBar].AsFloat : 4f);
		MetronomeOffset = (node.HasKey(KeyMetronomeOffset) ? node[KeyMetronomeOffset].AsFloat : 4f);
	}

	protected BaseBpmChange(float time, float bpm)
		: base(time, bpm)
	{
		base.Bpm = bpm;
		BeatsPerBar = 4f;
		MetronomeOffset = 4f;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		return true;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseBpmChange baseBpmChange)
		{
			base.Bpm = baseBpmChange.Bpm;
			BeatsPerBar = baseBpmChange.BeatsPerBar;
			MetronomeOffset = baseBpmChange.MetronomeOffset;
		}
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		return mapVersion switch
		{
			2 => V2BpmChange.ToJson(this), 
			3 => V3BpmChange.ToJson(this), 
			_ => throw new SwitchExpressionException(mapVersion), 
		};
	}
}
