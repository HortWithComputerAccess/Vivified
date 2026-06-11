using System;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base.Customs;

public class BaseBookmark : BaseObject
{
	private static readonly System.Random rand = new System.Random();

	public override ObjectType ObjectType { get; set; } = ObjectType.Bookmark;

	public string Name { get; set; } = "Invalid Bookmark";

	public Color Color { get; set; }

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

	public string KeyName
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_name", 
				3 => "n", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyColor
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_color", 
				3 => "c", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public override string CustomKeyTrack { get; } = "unusedTrack";

	public override string CustomKeyColor { get; } = "unusedColor";

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Name);
		writer.Put(Color.r);
		writer.Put(Color.g);
		writer.Put(Color.b);
		writer.Put(Color.a);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Name = reader.GetString();
		float r = reader.GetFloat();
		float g = reader.GetFloat();
		float b = reader.GetFloat();
		float a = reader.GetFloat();
		Color = new Color(r, g, b, a);
		base.Deserialize(reader);
	}

	private static Color NextRandomColor()
	{
		return Color.HSVToRGB((float)rand.NextDouble(), 0.75f, 1f);
	}

	public BaseBookmark()
	{
	}

	protected BaseBookmark(BaseBookmark other)
	{
		base.JsonTime = other.JsonTime;
		Name = other.Name;
		Color = other.Color;
	}

	public BaseBookmark(JSONNode node)
	{
		base.JsonTime = (node.HasKey(KeyTime) ? node[KeyTime].AsFloat : 0f);
		Name = (node.HasKey(KeyName) ? node[KeyName].Value : "Missing Name");
		Color = (node.HasKey(KeyColor) ? node[KeyColor].ReadColor() : NextRandomColor());
	}

	public BaseBookmark(float time, string name)
		: base(time)
	{
		Name = name;
		Color = NextRandomColor();
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		return true;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2Bookmark.ToJson(this);
		case 3:
		case 4:
			return V3Bookmark.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		return new BaseBookmark(this);
	}
}
