using System;
using System.Runtime.CompilerServices;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;
using Beatmap.V2;
using Beatmap.V3;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseObstacle : BaseGrid, ICustomDataObstacle, ICustomData, IChromaObject, INoodleExtensionsObstacle, INoodleExtensionsGrid
{
	private const float mappingExtensionsStartHeightMultiplier = 1.35f;

	private const float mappingExtensionsUnitsToFullHeightWall = 285.7143f;

	private int InternalType;

	private int InternalHeight;

	private int InternalPosY;

	private float duration;

	private float? durationSongBpm;

	public override ObjectType ObjectType { get; set; } = ObjectType.Obstacle;

	public override int PosY
	{
		get
		{
			return InternalPosY;
		}
		set
		{
			InternalPosY = value;
			InternalType = ((value >= 2) ? 1 : 0);
		}
	}

	public int Type
	{
		get
		{
			return InternalType;
		}
		set
		{
			InternalType = value;
			if (value == 1)
			{
				InternalPosY = 2;
				InternalHeight = 3;
			}
			else
			{
				InternalPosY = 0;
				InternalHeight = 5;
			}
		}
	}

	public int Height
	{
		get
		{
			return InternalHeight;
		}
		set
		{
			InternalHeight = value;
		}
	}

	public float Duration
	{
		get
		{
			return duration;
		}
		set
		{
			duration = value;
			RecomputeDurationSongBpm();
		}
	}

	public float DurationSongBpm => durationSongBpm.Value;

	public int Width { get; set; }

	public override float DespawnSongBpmTime => base.SongBpmTime + DurationSongBpm + base.Hjd;

	public virtual JSONNode CustomSize { get; set; }

	public string CustomKeySize
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_scale";
			case 3:
			case 4:
				return "size";
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
		writer.Put(Type);
		writer.Put(Duration);
		writer.Put(Width);
		writer.Put(Height);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Type = reader.GetInt();
		Duration = reader.GetFloat();
		Width = reader.GetInt();
		Height = reader.GetInt();
		base.Deserialize(reader);
	}

	public BaseObstacle()
	{
	}

	private BaseObstacle(BaseObstacle other)
	{
		base.JsonTime = other.JsonTime;
		base.PosX = other.PosX;
		InternalPosY = other.PosY;
		InternalType = other.Type;
		Duration = other.Duration;
		Width = other.Width;
		Height = other.Height;
		base.CustomData = other.SaveCustom().Clone();
		CustomFake = other.CustomFake;
	}

	public BaseObstacle(JSONNode node)
		: this(BeatmapFactory.Obstacle(node))
	{
	}

	public override bool IsChroma()
	{
		if (base.CustomData != null && base.CustomData.HasKey(CustomKeyColor))
		{
			return base.CustomData[CustomKeyColor].IsArray;
		}
		return false;
	}

	public override bool IsNoodleExtensions()
	{
		if (base.CustomData != null)
		{
			if ((!base.CustomData.HasKey("uninteractable") || !base.CustomData["uninteractable"].IsBoolean) && (!base.CustomData.HasKey(CustomKeyLocalRotation) || !base.CustomData[CustomKeyLocalRotation].IsArray) && (!base.CustomData.HasKey(CustomKeyNoteJumpMovementSpeed) || !base.CustomData[CustomKeyNoteJumpMovementSpeed].IsNumber) && (!base.CustomData.HasKey(CustomKeyNoteJumpStartBeatOffset) || !base.CustomData[CustomKeyNoteJumpStartBeatOffset].IsNumber) && (!base.CustomData.HasKey(CustomKeyCoordinate) || !base.CustomData[CustomKeyCoordinate].IsArray) && (!base.CustomData.HasKey(CustomKeyWorldRotation) || (!base.CustomData[CustomKeyWorldRotation].IsArray && !base.CustomData[CustomKeyWorldRotation].IsNumber)))
			{
				if (base.CustomData.HasKey(CustomKeySize))
				{
					return base.CustomData[CustomKeySize].IsArray;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool IsMappingExtensions()
	{
		if (base.PosX > -1000 && base.PosX < 1000 && PosY >= 0 && PosY <= 2 && Width > -1000 && Width < 1000 && Height > -1000 && Height <= 5)
		{
			if (Settings.Instance.MapVersion == 2)
			{
				if (base.PosX >= 0)
				{
					return base.PosX > 3;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseObstacle baseObstacle)
		{
			SaveCustom();
			baseObstacle.SaveCustom();
			if (IsNoodleExtensions() || baseObstacle.IsNoodleExtensions())
			{
				return ToJson().ToString() == other.ToJson().ToString();
			}
			if (base.PosX == baseObstacle.PosX && PosY == baseObstacle.PosY && Width == baseObstacle.Width)
			{
				return Height == baseObstacle.Height;
			}
			return false;
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseObstacle baseObstacle)
		{
			Duration = baseObstacle.Duration;
			Width = baseObstacle.Width;
			InternalPosY = baseObstacle.PosY;
			InternalHeight = baseObstacle.Height;
			InternalType = baseObstacle.Type;
		}
	}

	public ObstacleBounds GetShape()
	{
		float position = (float)base.PosX - 2f;
		int num = Mathf.Clamp(PosY, 0, 2);
		float startHeight = num;
		float height = Mathf.Min(Height, 5 - num);
		float width = Width;
		if (Width >= 1000)
		{
			width = ((float)Width - 1000f) / 1000f;
		}
		if (base.PosX >= 1000)
		{
			position = ((float)base.PosX - 1000f) / 1000f - 2f;
		}
		else if (base.PosX <= -1000)
		{
			position = ((float)base.PosX - 1000f) / 1000f;
		}
		if (Type > 1 && Type < 1000)
		{
			startHeight = (float)Type / 214.28572f;
			height = 3.5f;
		}
		else if (Type >= 1000 && Type <= 4000)
		{
			startHeight = 0f;
			height = ((float)Type - 1000f) / 285.7143f;
		}
		else if (Type > 4000)
		{
			float num2 = Type - 4001;
			startHeight = num2 % 1000f / 285.7143f * 1.35f;
			height = num2 / 1000f / 285.7143f;
		}
		if (base.CustomData == null)
		{
			return new ObstacleBounds(width, height, position, startHeight);
		}
		if (CustomCoordinate != null && CustomCoordinate.IsArray)
		{
			if (CustomCoordinate[0].IsNumber)
			{
				position = CustomCoordinate[0];
			}
			if (CustomCoordinate[1].IsNumber)
			{
				startHeight = CustomCoordinate[1];
			}
		}
		if (CustomSize != null && CustomSize.IsArray)
		{
			if (CustomSize[0].IsNumber)
			{
				width = CustomSize[0];
			}
			if (CustomSize[1].IsNumber)
			{
				height = CustomSize[1];
			}
		}
		return new ObstacleBounds(width, height, position, startHeight);
	}

	public override void RecomputeSongBpmTime()
	{
		base.RecomputeSongBpmTime();
		RecomputeDurationSongBpm();
	}

	private void RecomputeDurationSongBpm()
	{
		durationSongBpm = Map?.JsonTimeToSongBpmTime(base.JsonTime + duration) - songBpmTime;
	}

	protected void InferType()
	{
		int posY = PosY;
		int internalType;
		if (posY != 0)
		{
			if (posY != 2 || Height != 3)
			{
				goto IL_002a;
			}
			internalType = 1;
		}
		else
		{
			if (Height != 5)
			{
				goto IL_002a;
			}
			internalType = 0;
		}
		goto IL_0031;
		IL_002a:
		internalType = Type;
		goto IL_0031;
		IL_0031:
		InternalType = internalType;
	}

	protected override void ParseCustom()
	{
		base.ParseCustom();
		if (!(base.CustomData == null))
		{
			if (base.CustomData.HasKey(CustomKeySize))
			{
				CustomSize = base.CustomData[CustomKeySize];
			}
			else
			{
				CustomSize = null;
			}
		}
	}

	protected internal override JSONNode SaveCustom()
	{
		JSONNode jSONNode = base.SaveCustom();
		if (CustomSize != null)
		{
			jSONNode[CustomKeySize] = CustomSize;
		}
		else
		{
			jSONNode.Remove(CustomKeySize);
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseObstacle baseObstacle))
		{
			return num;
		}
		if (num == 0)
		{
			num = base.PosX.CompareTo(baseObstacle.PosX);
		}
		if (num == 0)
		{
			num = PosY.CompareTo(baseObstacle.PosY);
		}
		if (num == 0)
		{
			num = Type.CompareTo(baseObstacle.Type);
		}
		if (num == 0)
		{
			num = Duration.CompareTo(baseObstacle.Duration);
		}
		if (num == 0)
		{
			num = Width.CompareTo(baseObstacle.Width);
		}
		if (num == 0)
		{
			num = Height.CompareTo(baseObstacle.Height);
		}
		if (num == 0)
		{
			num = string.Compare(base.CustomData?.ToString(), baseObstacle.CustomData?.ToString(), StringComparison.Ordinal);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 3:
		case 4:
			return V3Obstacle.ToJson(this);
		case 2:
			return V2Obstacle.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseObstacle baseObstacle = new BaseObstacle(this);
		baseObstacle.ParseCustom();
		return baseObstacle;
	}
}
