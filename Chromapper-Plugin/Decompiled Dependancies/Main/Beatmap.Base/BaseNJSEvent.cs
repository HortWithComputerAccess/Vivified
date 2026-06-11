using Beatmap.Enums;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseNJSEvent : BaseObject
{
	public override ObjectType ObjectType { get; set; } = ObjectType.NJSEvent;

	public int UsePrevious { get; set; }

	public int Easing { get; set; }

	public float RelativeNJS { get; set; }

	public override string CustomKeyColor { get; } = "unusedColor";

	public override string CustomKeyTrack { get; } = "unusedTrack";

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(UsePrevious);
		writer.Put(Easing);
		writer.Put(RelativeNJS);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		UsePrevious = reader.GetInt();
		Easing = reader.GetInt();
		RelativeNJS = reader.GetFloat();
		base.Deserialize(reader);
	}

	public BaseNJSEvent()
	{
	}

	public BaseNJSEvent(BaseNJSEvent other)
	{
		base.JsonTime = other.JsonTime;
		UsePrevious = other.UsePrevious;
		Easing = other.Easing;
		RelativeNJS = other.RelativeNJS;
		base.CustomData = other.SaveCustom().Clone();
	}

	public BaseNJSEvent(JSONNode node)
	{
		base.JsonTime = node["beat"].AsFloat;
		UsePrevious = node["usePrevious"].AsInt;
		Easing = node["easing"].AsInt;
		RelativeNJS = node["relative-njs"].AsFloat;
		base.CustomData = node["customData"].AsObject;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseNJSEvent baseNJSEvent)
		{
			return Mathf.Approximately(baseNJSEvent.JsonTime, base.JsonTime);
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseNJSEvent baseNJSEvent)
		{
			UsePrevious = baseNJSEvent.UsePrevious;
			Easing = baseNJSEvent.Easing;
			RelativeNJS = baseNJSEvent.RelativeNJS;
		}
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseNJSEvent baseNJSEvent))
		{
			return num;
		}
		if (UsePrevious == 1 && baseNJSEvent.UsePrevious == 1)
		{
			return num;
		}
		if (num == 0)
		{
			num = RelativeNJS.CompareTo(baseNJSEvent.RelativeNJS);
		}
		if (num == 0)
		{
			num = Easing.CompareTo(baseNJSEvent.Easing);
		}
		if (num == 0)
		{
			num = UsePrevious.CompareTo(baseNJSEvent.UsePrevious);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		_ = Settings.Instance.MapVersion;
		return new JSONObject
		{
			["beat"] = base.JsonTime,
			["usePrevious"] = UsePrevious,
			["easing"] = Easing,
			["relative-njs"] = RelativeNJS
		};
	}

	public override BaseItem Clone()
	{
		BaseNJSEvent baseNJSEvent = new BaseNJSEvent(this);
		baseNJSEvent.ParseCustom();
		return baseNJSEvent;
	}
}
