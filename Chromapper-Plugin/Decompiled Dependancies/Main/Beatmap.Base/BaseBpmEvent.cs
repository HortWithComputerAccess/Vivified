using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;
using Beatmap.V3;
using LiteNetLib.Utils;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseBpmEvent : BaseEvent
{
	public override int Type
	{
		get
		{
			return 100;
		}
		set
		{
		}
	}

	public override ObjectType ObjectType { get; set; } = ObjectType.BpmChange;

	public float Bpm { get; set; }

	public int Beat { get; set; }

	public override string CustomKeyColor { get; } = "unusedColor";

	public override string CustomKeyTrack { get; } = "unusedTrack";

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Bpm);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Bpm = reader.GetFloat();
		base.Deserialize(reader);
	}

	public BaseBpmEvent()
	{
	}

	public BaseBpmEvent(BaseBpmEvent other)
	{
		base.JsonTime = other.JsonTime;
		Bpm = other.Bpm;
		base.CustomData = other.CustomData.Clone();
	}

	public BaseBpmEvent(float jsonTime, float bpm)
	{
		base.JsonTime = jsonTime;
		Bpm = bpm;
	}

	public BaseBpmEvent(JSONNode node)
		: this(BeatmapFactory.BpmEvent(node))
	{
	}

	public override bool IsBpmEvent()
	{
		return true;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		return other is BaseBpmEvent;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseBpmEvent baseBpmEvent)
		{
			Bpm = baseBpmEvent.Bpm;
		}
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseBpmEvent baseBpmEvent))
		{
			return num;
		}
		if (num == 0)
		{
			num = Bpm.CompareTo(baseBpmEvent.Bpm);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2BpmEvent.ToJson(this);
		case 3:
		case 4:
			return V3BpmEvent.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseBpmEvent baseBpmEvent = new BaseBpmEvent(this);
		baseBpmEvent.ParseCustom();
		return baseBpmEvent;
	}
}
