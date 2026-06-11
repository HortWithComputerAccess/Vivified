using System;
using System.Runtime.CompilerServices;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;
using Beatmap.V3;
using LiteNetLib.Utils;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseArc : BaseSlider, ICustomDataArc, ICustomDataSlider, ICustomData, IChromaObject, INoodleExtensionsSlider, INoodleExtensionsGrid
{
	public override ObjectType ObjectType { get; set; } = ObjectType.Arc;

	public float HeadControlPointLengthMultiplier { get; set; }

	public int TailCutDirection { get; set; }

	public float TailControlPointLengthMultiplier { get; set; }

	public int MidAnchorMode { get; set; }

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

	public override string CustomKeyTailCoordinate
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_tailPosition";
			case 3:
			case 4:
				return "tailCoordinates";
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
		writer.Put(HeadControlPointLengthMultiplier);
		writer.Put(TailCutDirection);
		writer.Put(TailControlPointLengthMultiplier);
		writer.Put(MidAnchorMode);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		HeadControlPointLengthMultiplier = reader.GetFloat();
		TailCutDirection = reader.GetInt();
		TailControlPointLengthMultiplier = reader.GetFloat();
		MidAnchorMode = reader.GetInt();
		base.Deserialize(reader);
	}

	public BaseArc()
	{
	}

	public BaseArc(BaseArc other)
	{
		base.JsonTime = other.JsonTime;
		base.Color = other.Color;
		base.PosX = other.PosX;
		PosY = other.PosY;
		base.CutDirection = other.CutDirection;
		HeadControlPointLengthMultiplier = other.HeadControlPointLengthMultiplier;
		base.TailJsonTime = other.TailJsonTime;
		base.TailPosX = other.TailPosX;
		base.TailPosY = other.TailPosY;
		TailCutDirection = other.TailCutDirection;
		TailControlPointLengthMultiplier = other.TailControlPointLengthMultiplier;
		MidAnchorMode = other.MidAnchorMode;
		base.CustomData = other.CustomData.Clone();
	}

	public BaseArc(BaseNote start, BaseNote end)
	{
		base.JsonTime = start.JsonTime;
		base.Color = start.Color;
		base.PosX = start.PosX;
		PosY = start.PosY;
		base.CutDirection = start.CutDirection;
		HeadControlPointLengthMultiplier = 1f;
		base.TailJsonTime = end.JsonTime;
		base.TailPosX = end.PosX;
		base.TailPosY = end.PosY;
		TailCutDirection = end.CutDirection;
		TailControlPointLengthMultiplier = 1f;
		MidAnchorMode = 0;
		base.CustomData = SaveCustomFromNotes(start, end);
	}

	public BaseArc(JSONNode node)
		: this(BeatmapFactory.Arc(node))
	{
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
			if ((!base.CustomData.HasKey("disableNoteGravity") || !base.CustomData["disableNoteGravity"].IsBoolean) && (!base.CustomData.HasKey("disableNoteLook") || !base.CustomData["disableNoteLook"].IsBoolean) && (!base.CustomData.HasKey("flip") || !base.CustomData["flip"].IsArray) && (!base.CustomData.HasKey("uninteractable") || !base.CustomData["uninteractable"].IsBoolean) && (!base.CustomData.HasKey(CustomKeyLocalRotation) || !base.CustomData[CustomKeyLocalRotation].IsArray) && (!base.CustomData.HasKey(CustomKeyNoteJumpMovementSpeed) || !base.CustomData[CustomKeyNoteJumpMovementSpeed].IsNumber) && (!base.CustomData.HasKey(CustomKeyNoteJumpStartBeatOffset) || !base.CustomData[CustomKeyNoteJumpStartBeatOffset].IsNumber) && (!base.CustomData.HasKey(CustomKeyCoordinate) || !base.CustomData[CustomKeyCoordinate].IsArray) && (!base.CustomData.HasKey(CustomKeyTailCoordinate) || !base.CustomData[CustomKeyTailCoordinate].IsArray))
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
		if (base.PosX <= -1000 || base.PosX >= 1000 || PosY < 0 || PosY > 2 || base.TailPosX <= -1000 || base.TailPosX >= 1000 || base.TailPosY < 0 || base.TailPosY > 2 || (base.CutDirection >= 1000 && base.CutDirection <= 1360) || (base.CutDirection >= 2000 && base.CutDirection <= 2360) || (TailCutDirection >= 1000 && TailCutDirection <= 1360))
		{
			return !IsNoodleExtensions();
		}
		return false;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseArc baseArc)
		{
			if (base.IsConflictingWithObjectAtSameTime(other, false) && HeadControlPointLengthMultiplier == baseArc.HeadControlPointLengthMultiplier && TailCutDirection == baseArc.TailCutDirection && TailControlPointLengthMultiplier == baseArc.TailControlPointLengthMultiplier)
			{
				return MidAnchorMode == baseArc.MidAnchorMode;
			}
			return false;
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseArc baseArc)
		{
			HeadControlPointLengthMultiplier = baseArc.HeadControlPointLengthMultiplier;
			TailCutDirection = baseArc.TailCutDirection;
			TailControlPointLengthMultiplier = baseArc.TailControlPointLengthMultiplier;
			MidAnchorMode = baseArc.MidAnchorMode;
		}
	}

	public override void SwapHeadAndTail()
	{
		base.SwapHeadAndTail();
		int tailCutDirection = TailCutDirection;
		int cutDirection = base.CutDirection;
		int num = (base.CutDirection = tailCutDirection);
		num = (TailCutDirection = cutDirection);
		float tailControlPointLengthMultiplier = TailControlPointLengthMultiplier;
		float headControlPointLengthMultiplier = HeadControlPointLengthMultiplier;
		float num4 = (HeadControlPointLengthMultiplier = tailControlPointLengthMultiplier);
		num4 = (TailControlPointLengthMultiplier = headControlPointLengthMultiplier);
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseArc baseArc))
		{
			return num;
		}
		if (num == 0)
		{
			num = HeadControlPointLengthMultiplier.CompareTo(baseArc.HeadControlPointLengthMultiplier);
		}
		if (num == 0)
		{
			num = TailControlPointLengthMultiplier.CompareTo(baseArc.TailControlPointLengthMultiplier);
		}
		if (num == 0)
		{
			num = TailCutDirection.CompareTo(baseArc.TailCutDirection);
		}
		if (num == 0)
		{
			num = MidAnchorMode.CompareTo(baseArc.MidAnchorMode);
		}
		if (num == 0)
		{
			num = string.Compare(base.CustomData?.ToString(), baseArc.CustomData?.ToString(), StringComparison.Ordinal);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2Arc.ToJson(this);
		case 3:
		case 4:
			return V3Arc.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseArc baseArc = new BaseArc(this);
		baseArc.ParseCustom();
		return baseArc;
	}
}
