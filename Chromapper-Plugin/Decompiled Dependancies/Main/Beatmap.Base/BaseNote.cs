using System;
using System.Runtime.CompilerServices;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;
using Beatmap.V3;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseNote : BaseGrid, ICustomDataNote, ICustomData, IChromaObject, INoodleExtensionsNote, INoodleExtensionsGrid
{
	private int type;

	private int color;

	public override ObjectType ObjectType { get; set; }

	public int Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			color = value;
		}
	}

	public int Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			type = value;
		}
	}

	public int CutDirection { get; set; }

	public int AngleOffset { get; set; }

	public bool IsMainDirection
	{
		get
		{
			int cutDirection = CutDirection;
			return cutDirection == 0 || cutDirection == 1 || cutDirection == 2 || cutDirection == 3;
		}
	}

	public virtual float? CustomDirection { get; set; }

	public string CustomKeyDirection => "_cutDirection";

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

	public override string CustomKeyAnimation
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_animation";
			case 3:
			case 4:
				return "animation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyCoordinate
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_position";
			case 3:
			case 4:
				return "coordinates";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyWorldRotation
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_rotation";
			case 3:
			case 4:
				return "worldRotation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyLocalRotation
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_localRotation";
			case 3:
			case 4:
				return "localRotation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeySpawnEffect
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_disableSpawnEffect";
			case 3:
			case 4:
				return "spawnEffect";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyNoteJumpMovementSpeed
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_noteJumpMovementSpeed";
			case 3:
			case 4:
				return "noteJumpMovementSpeed";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyNoteJumpStartBeatOffset
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_noteJumpStartBeatOffset";
			case 3:
			case 4:
				return "noteJumpStartBeatOffset";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Color);
		writer.Put(Type);
		writer.Put(CutDirection);
		writer.Put(AngleOffset);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Color = reader.GetInt();
		Type = reader.GetInt();
		CutDirection = reader.GetInt();
		AngleOffset = reader.GetInt();
		base.Deserialize(reader);
	}

	public BaseNote()
	{
	}

	public BaseNote(BaseNote other)
	{
		base.JsonTime = other.JsonTime;
		base.PosX = other.PosX;
		PosY = other.PosY;
		Color = other.Color;
		Type = other.Type;
		CutDirection = other.CutDirection;
		AngleOffset = other.AngleOffset;
		base.CustomData = other.CustomData.Clone();
		CustomFake = other.CustomFake;
	}

	public BaseNote(JSONNode node)
		: this(BeatmapFactory.Note(node))
	{
	}

	protected override void ParseCustom()
	{
		base.ParseCustom();
		if (Settings.Instance.MapVersion == 2)
		{
			JSONNode jSONNode = base.CustomData;
			CustomDirection = (((object)jSONNode == null || !jSONNode.HasKey(CustomKeyDirection)) ? ((int?)null) : base.CustomData?[CustomKeyDirection].AsInt);
			JSONNode jSONNode2 = base.CustomData;
			CustomFake = (object)jSONNode2 != null && jSONNode2.HasKey("_fake") && base.CustomData["_fake"].AsBool;
		}
	}

	protected internal override JSONNode SaveCustom()
	{
		JSONNode jSONNode = base.SaveCustom();
		if (Settings.Instance.MapVersion == 2)
		{
			if (CustomDirection.HasValue)
			{
				string customKeyDirection = CustomKeyDirection;
				float? customDirection = CustomDirection;
				jSONNode[customKeyDirection] = (customDirection.HasValue ? ((JSONNode)customDirection.GetValueOrDefault()) : null);
			}
			else
			{
				jSONNode.Remove(CustomKeyDirection);
			}
			if (CustomFake)
			{
				jSONNode["_fake"] = true;
			}
			else
			{
				jSONNode.Remove("_fake");
			}
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}

	public override bool IsChroma()
	{
		if (base.CustomData != null)
		{
			if ((!base.CustomData.HasKey(CustomKeyColor) || !base.CustomData[CustomKeyColor].IsArray) && (!base.CustomData.HasKey(CustomKeySpawnEffect) || !base.CustomData[CustomKeySpawnEffect].IsBoolean))
			{
				if (base.CustomData.HasKey("disableDebris"))
				{
					return base.CustomData["disableDebris"].IsBoolean;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool IsNoodleExtensions()
	{
		if (base.CustomData != null)
		{
			if ((!base.CustomData.HasKey("disableNoteGravity") || !base.CustomData["disableNoteGravity"].IsBoolean) && (!base.CustomData.HasKey("disableNoteLook") || !base.CustomData["disableNoteLook"].IsBoolean) && (!base.CustomData.HasKey("flip") || !base.CustomData["flip"].IsArray) && (!base.CustomData.HasKey("uninteractable") || !base.CustomData["uninteractable"].IsBoolean) && (!base.CustomData.HasKey(CustomKeyLocalRotation) || !base.CustomData[CustomKeyLocalRotation].IsArray) && (!base.CustomData.HasKey(CustomKeyNoteJumpMovementSpeed) || !base.CustomData[CustomKeyNoteJumpMovementSpeed].IsNumber) && (!base.CustomData.HasKey(CustomKeyNoteJumpStartBeatOffset) || !base.CustomData[CustomKeyNoteJumpStartBeatOffset].IsNumber) && (!base.CustomData.HasKey(CustomKeyCoordinate) || !base.CustomData[CustomKeyCoordinate].IsArray))
			{
				if (base.CustomData.HasKey(CustomKeyWorldRotation))
				{
					if (!base.CustomData[CustomKeyWorldRotation].IsArray)
					{
						return base.CustomData[CustomKeyWorldRotation].IsNumber;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool IsMappingExtensions()
	{
		if (base.PosX >= 0 && base.PosX <= 3 && PosY >= 0 && PosY <= 2 && (CutDirection < 1000 || CutDirection > 1360))
		{
			if (CutDirection >= 2000)
			{
				return CutDirection <= 2360;
			}
			return false;
		}
		return true;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseNote baseNote)
		{
			Color = baseNote.Color;
			CutDirection = baseNote.CutDirection;
			AngleOffset = baseNote.AngleOffset;
		}
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseNote baseNote)
		{
			return (double)Vector2.Distance(baseNote.GetPosition(), GetPosition()) < 0.1;
		}
		return false;
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseNote baseNote))
		{
			return num;
		}
		if (num == 0)
		{
			num = base.PosX.CompareTo(baseNote.PosX);
		}
		if (num == 0)
		{
			num = PosY.CompareTo(baseNote.PosY);
		}
		if (num == 0)
		{
			num = Color.CompareTo(baseNote.Color);
		}
		if (num == 0)
		{
			num = CutDirection.CompareTo(baseNote.CutDirection);
		}
		if (num == 0)
		{
			num = AngleOffset.CompareTo(baseNote.AngleOffset);
		}
		if (num == 0)
		{
			num = string.Compare(base.CustomData?.ToString(), baseNote.CustomData?.ToString(), StringComparison.Ordinal);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2Note.ToJson(this);
		case 3:
		case 4:
			return (Type == 3) ? V3BombNote.ToJson(this) : V3ColorNote.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseNote baseNote = new BaseNote(this);
		baseNote.ParseCustom();
		return baseNote;
	}
}
