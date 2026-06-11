using System;
using System.Runtime.CompilerServices;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V3;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseChain : BaseSlider, ICustomDataChain, ICustomDataSlider, ICustomData, IChromaObject, INoodleExtensionsSlider, INoodleExtensionsGrid
{
	public static readonly Vector3 ChainHeadScale = new Vector3(1f, 0.6f, 1f);

	public override ObjectType ObjectType { get; set; } = ObjectType.Chain;

	public int SliceCount { get; set; }

	public float Squish { get; set; }

	public override string CustomKeyColor => "color";

	public override string CustomKeyTrack => "track";

	public override string CustomKeyTailCoordinate => "tailCoordinates";

	public override string CustomKeyAnimation => "animation";

	public override string CustomKeyCoordinate => "coordinates";

	public override string CustomKeyWorldRotation => "worldRotation";

	public override string CustomKeyLocalRotation => "localRotation";

	public override string CustomKeySpawnEffect => "spawnEffect";

	public override string CustomKeyNoteJumpMovementSpeed => "noteJumpMovementSpeed";

	public override string CustomKeyNoteJumpStartBeatOffset => "noteJumpStartBeatOffset";

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(SliceCount);
		writer.Put(Squish);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		SliceCount = reader.GetInt();
		Squish = reader.GetFloat();
		base.Deserialize(reader);
	}

	public BaseChain()
	{
	}

	public BaseChain(BaseChain other)
	{
		base.JsonTime = other.JsonTime;
		base.Color = other.Color;
		base.PosX = other.PosX;
		PosY = other.PosY;
		base.CutDirection = other.CutDirection;
		base.TailJsonTime = other.TailJsonTime;
		base.TailPosX = other.TailPosX;
		base.TailPosY = other.TailPosY;
		SliceCount = other.SliceCount;
		Squish = other.Squish;
		base.CustomData = other.CustomData.Clone();
	}

	public BaseChain(BaseNote start, BaseNote end)
	{
		base.JsonTime = start.JsonTime;
		base.Color = start.Color;
		base.PosX = start.PosX;
		PosY = start.PosY;
		base.CutDirection = start.CutDirection;
		base.TailJsonTime = end.JsonTime;
		base.TailPosX = end.PosX;
		base.TailPosY = end.PosY;
		SliceCount = 5;
		Squish = 1f;
		base.CustomData = SaveCustomFromNotes(start, end);
	}

	public BaseChain(JSONNode node)
		: this(BeatmapFactory.Chain(node))
	{
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseChain baseChain)
		{
			if (base.IsConflictingWithObjectAtSameTime(other, false) && SliceCount == baseChain.SliceCount)
			{
				return Squish == baseChain.Squish;
			}
			return false;
		}
		return false;
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
		if (base.PosX <= -1000 || base.PosX >= 1000 || PosY < 0 || PosY > 2 || base.TailPosX <= -1000 || base.TailPosX >= 1000 || base.TailPosY < 0 || base.TailPosY > 2 || (base.CutDirection >= 1000 && base.CutDirection <= 1360) || (base.CutDirection >= 2000 && base.CutDirection <= 2360))
		{
			return !IsNoodleExtensions();
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseChain baseChain)
		{
			SliceCount = baseChain.SliceCount;
			Squish = baseChain.Squish;
		}
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseChain baseChain))
		{
			return num;
		}
		if (num == 0)
		{
			num = SliceCount.CompareTo(baseChain.SliceCount);
		}
		if (num == 0)
		{
			num = Squish.CompareTo(baseChain.Squish);
		}
		if (num == 0)
		{
			num = string.Compare(base.CustomData?.ToString(), baseChain.CustomData?.ToString(), StringComparison.Ordinal);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if ((uint)(mapVersion - 3) <= 1u)
		{
			return V3Chain.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		BaseChain baseChain = new BaseChain(this);
		baseChain.ParseCustom();
		return baseChain;
	}
}
