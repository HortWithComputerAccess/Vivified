using System;
using System.Linq;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public abstract class BaseObject : BaseItem, ICustomData, IHeckObject, IChromaObject, INetSerializable, IComparable<BaseObject>
{
	protected BaseDifficulty Map;

	private float jsonTime;

	internal float? songBpmTime;

	private JSONNode customData = new JSONObject();

	public abstract ObjectType ObjectType { get; set; }

	public bool HasAttachedContainer { get; set; }

	public float JsonTime
	{
		get
		{
			return jsonTime;
		}
		set
		{
			jsonTime = value;
			RecomputeSongBpmTime();
		}
	}

	public float SongBpmTime => songBpmTime.Value;

	public virtual Color? CustomColor { get; set; }

	public abstract string CustomKeyColor { get; }

	public JSONNode CustomData
	{
		get
		{
			return customData;
		}
		set
		{
			customData = value ?? new JSONObject();
			ParseCustom();
		}
	}

	public JSONNode CustomTrack { get; set; }

	public abstract string CustomKeyTrack { get; }

	public virtual void Serialize(NetDataWriter writer)
	{
		writer.Put(jsonTime);
		writer.Put(songBpmTime.Value);
		writer.Put(CustomData?.ToString());
	}

	public virtual void Deserialize(NetDataReader reader)
	{
		jsonTime = reader.GetFloat();
		songBpmTime = reader.GetFloat();
		CustomData = JSON.Parse(reader.GetString());
	}

	protected BaseObject()
	{
		SetMap();
		JsonTime = 0f;
	}

	protected BaseObject(float time, JSONNode customData = null)
	{
		SetMap();
		JsonTime = time;
		CustomData = customData;
	}

	protected BaseObject(float jsonTime, float songBpmTime, JSONNode customData = null)
	{
		SetMap();
		this.jsonTime = jsonTime;
		this.songBpmTime = songBpmTime;
		CustomData = customData;
	}

	public void SetMap(BaseDifficulty map = null)
	{
		if (map != null)
		{
			Map = map;
		}
		else
		{
			Map = ((BeatSaberSongContainer.Instance != null) ? BeatSaberSongContainer.Instance.Map : null);
		}
	}

	public void SetCustomData(JSONNode node)
	{
		customData = node ?? new JSONObject();
	}

	public virtual bool IsChroma()
	{
		return false;
	}

	public virtual bool IsNoodleExtensions()
	{
		return false;
	}

	public virtual bool IsMappingExtensions()
	{
		return false;
	}

	public virtual void RecomputeSongBpmTime()
	{
		songBpmTime = Map?.JsonTimeToSongBpmTime(JsonTime);
	}

	public virtual bool IsConflictingWith(BaseObject other, bool deletion = false)
	{
		if (Mathf.Abs(JsonTime - other.JsonTime) < BeatmapObjectContainerCollection.Epsilon)
		{
			return IsConflictingWithObjectAtSameTime(other, deletion);
		}
		return false;
	}

	protected abstract bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false);

	public virtual bool HasMatchingTrack(string filter)
	{
		bool flag = filter == null;
		if (!flag)
		{
			JSONNode customTrack = CustomTrack;
			bool flag2 = ((customTrack is JSONString jSONString) ? (filter == jSONString) : (customTrack is JSONArray jSONArray && jSONArray.Children.Any((JSONNode it) => filter == it)));
			flag = flag2;
		}
		return flag;
	}

	public virtual void Apply(BaseObject originalData)
	{
		JsonTime = originalData.JsonTime;
		CustomData = originalData.CustomData?.Clone();
	}

	protected virtual void ParseCustom()
	{
		JSONNode jSONNode = CustomData;
		CustomTrack = (((object)jSONNode == null || !jSONNode.HasKey(CustomKeyTrack)) ? null : CustomData?[CustomKeyTrack]);
		JSONNode jSONNode2 = CustomData;
		CustomColor = (((object)jSONNode2 == null || !jSONNode2.HasKey(CustomKeyColor)) ? ((Color?)null) : CustomData?[CustomKeyColor].ReadColor());
	}

	public void RefreshCustom()
	{
		ParseCustom();
	}

	protected internal virtual JSONNode SaveCustom()
	{
		JSONNode jSONNode = ((CustomData is JSONObject) ? CustomData : new JSONObject());
		if (CustomTrack != null)
		{
			jSONNode[CustomKeyTrack] = CustomTrack;
		}
		else
		{
			jSONNode.Remove(CustomKeyTrack);
		}
		if (CustomColor.HasValue)
		{
			string customKeyColor = CustomKeyColor;
			Color? customColor = CustomColor;
			jSONNode[customKeyColor] = (customColor.HasValue ? ((JSONNode)customColor.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyColor);
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}

	public void WriteCustom()
	{
		SaveCustom();
	}

	public JSONNode GetOrCreateCustom()
	{
		if (CustomData == null)
		{
			CustomData = new JSONObject();
		}
		return CustomData;
	}

	public virtual int CompareTo(BaseObject other)
	{
		return JsonTime.CompareTo(other.JsonTime);
	}
}
