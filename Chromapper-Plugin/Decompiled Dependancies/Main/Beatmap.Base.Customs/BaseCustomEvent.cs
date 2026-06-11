using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using LiteNetLib.Utils;
using SimpleJSON;

namespace Beatmap.Base.Customs;

public class BaseCustomEvent : BaseObject
{
	private JSONNode data;

	public override ObjectType ObjectType { get; set; } = ObjectType.CustomEvent;

	public string Type { get; set; }

	public JSONNode Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
			ParseCustom();
		}
	}

	public float? DataDuration { get; set; }

	public string? DataEasing { get; set; }

	public int? DataRepeat { get; set; }

	public JSONNode DataChildrenTracks { get; set; }

	public JSONNode DataParentTrack { get; set; }

	public bool? DataWorldPositionStays { get; set; }

	public string KeyTime
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_time";
			case 3:
			case 4:
				return "b";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyType
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_type";
			case 3:
			case 4:
				return "t";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyData
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_data";
			case 3:
			case 4:
				return "d";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyDuration
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_duration";
			case 3:
			case 4:
				return "duration";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyEasing
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_easing";
			case 3:
			case 4:
				return "easing";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyRepeat
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_repeat";
			case 3:
			case 4:
				return "repeat";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyChildrenTracks
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_childrenTracks";
			case 3:
			case 4:
				return "childrenTracks";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyParentTrack
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_parentTrack";
			case 3:
			case 4:
				return "parentTrack";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string DataKeyWorldPositionStays
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_worldPositionStays";
			case 3:
			case 4:
				return "worldPositionStays";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyColor
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_color";
			case 3:
			case 4:
				return "color";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyTrack
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_track";
			case 3:
			case 4:
				return "track";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Type);
		writer.Put(Data.ToString());
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Type = reader.GetString();
		Data = JSON.Parse(reader.GetString());
		base.Deserialize(reader);
	}

	public BaseCustomEvent()
	{
	}

	protected BaseCustomEvent(BaseCustomEvent other)
	{
		base.JsonTime = other.JsonTime;
		Type = other.Type;
		Data = other.SaveCustom().Clone();
	}

	public BaseCustomEvent(JSONNode node)
	{
		base.JsonTime = RetrieveRequiredNode(node, KeyTime).AsFloat;
		Type = RetrieveRequiredNode(node, KeyType).Value;
		Data = RetrieveRequiredNode(node, KeyData);
	}

	protected BaseCustomEvent(float time, string type, JSONNode node = null)
		: base(time)
	{
		Type = type;
		Data = ((node is JSONObject) ? node : new JSONObject());
	}

	public void SetData(JSONNode node)
	{
		Data = node;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		return false;
	}

	public override bool HasMatchingTrack(string filter)
	{
		bool flag = base.HasMatchingTrack(filter);
		if (!flag)
		{
			JSONNode dataChildrenTracks = DataChildrenTracks;
			bool flag2 = ((dataChildrenTracks is JSONString jSONString) ? (filter == jSONString) : (dataChildrenTracks is JSONArray jSONArray && jSONArray.Children.Any((JSONNode it) => filter == it)));
			flag = flag2;
		}
		bool flag3 = flag;
		if (!flag3)
		{
			bool flag2 = DataParentTrack is JSONString jSONString2 && filter == jSONString2;
			flag3 = flag2;
		}
		return flag3;
	}

	protected override void ParseCustom()
	{
		base.CustomTrack = (Data.HasKey(CustomKeyTrack) ? Data[CustomKeyTrack] : null);
		if (Data.HasKey(DataKeyDuration))
		{
			DataDuration = Data[DataKeyDuration].AsFloat;
		}
		else
		{
			DataDuration = null;
		}
		if (Data.HasKey(DataKeyRepeat))
		{
			DataRepeat = Data[DataKeyRepeat].AsInt;
		}
		else
		{
			DataRepeat = null;
		}
		if (Data.HasKey(DataKeyWorldPositionStays))
		{
			DataWorldPositionStays = Data[DataKeyWorldPositionStays].AsBool;
		}
		else
		{
			DataWorldPositionStays = null;
		}
		DataEasing = (Data.HasKey(DataKeyEasing) ? Data[DataKeyEasing] : null);
		DataChildrenTracks = (Data.HasKey(DataKeyChildrenTracks) ? Data[DataKeyChildrenTracks] : null);
		DataParentTrack = (Data.HasKey(DataKeyParentTrack) ? Data[DataKeyParentTrack] : null);
	}

	protected internal override JSONNode SaveCustom()
	{
		if (base.CustomTrack != null)
		{
			Data[CustomKeyTrack] = base.CustomTrack;
		}
		else
		{
			Data.Remove(CustomKeyTrack);
		}
		if (DataDuration.HasValue)
		{
			JSONNode jSONNode = Data;
			string dataKeyDuration = DataKeyDuration;
			float? dataDuration = DataDuration;
			jSONNode[dataKeyDuration] = (dataDuration.HasValue ? ((JSONNode)dataDuration.GetValueOrDefault()) : null);
		}
		else
		{
			Data.Remove(DataKeyDuration);
		}
		if (DataEasing != null)
		{
			Data[DataKeyEasing] = DataEasing;
		}
		else
		{
			Data.Remove(DataKeyEasing);
		}
		if (DataRepeat.HasValue)
		{
			JSONNode jSONNode2 = Data;
			string dataKeyRepeat = DataKeyRepeat;
			int? dataRepeat = DataRepeat;
			jSONNode2[dataKeyRepeat] = (dataRepeat.HasValue ? ((JSONNode)dataRepeat.GetValueOrDefault()) : null);
		}
		else
		{
			Data.Remove(DataKeyRepeat);
		}
		if (DataChildrenTracks != null)
		{
			Data[DataKeyChildrenTracks] = DataChildrenTracks;
		}
		else
		{
			Data.Remove(DataKeyChildrenTracks);
		}
		if (DataParentTrack != null)
		{
			Data[DataKeyParentTrack] = DataParentTrack;
		}
		else
		{
			Data.Remove(DataKeyParentTrack);
		}
		if (DataWorldPositionStays.HasValue)
		{
			JSONNode jSONNode3 = Data;
			string dataKeyWorldPositionStays = DataKeyWorldPositionStays;
			bool? dataWorldPositionStays = DataWorldPositionStays;
			jSONNode3[dataKeyWorldPositionStays] = (dataWorldPositionStays.HasValue ? ((JSONNode)(dataWorldPositionStays == true)) : null);
		}
		else
		{
			JSONNode jSONNode4 = Data;
			bool? dataWorldPositionStays = DataWorldPositionStays;
			jSONNode4.Remove(dataWorldPositionStays.HasValue ? ((JSONNode)(dataWorldPositionStays == true)) : null);
		}
		return Data;
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseCustomEvent baseCustomEvent))
		{
			return num;
		}
		if (num != 0)
		{
			return num;
		}
		return string.Compare(Type, baseCustomEvent.Type, StringComparison.Ordinal);
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2CustomEvent.ToJson(this);
		case 3:
		case 4:
			return V3CustomEvent.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseCustomEvent baseCustomEvent = new BaseCustomEvent(this);
		baseCustomEvent.ParseCustom();
		return baseCustomEvent;
	}
}
