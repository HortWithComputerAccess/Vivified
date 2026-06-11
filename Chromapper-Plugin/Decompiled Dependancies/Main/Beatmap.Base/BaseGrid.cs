using Beatmap.Base.Customs;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public abstract class BaseGrid : BaseObject, IObjectBounds, INoodleExtensionsGrid
{
	public int PosX { get; set; }

	public virtual int PosY { get; set; }

	public int Rotation { get; set; }

	public float Hjd { get; private set; }

	public float Jd { get; private set; }

	public float EditorScale { get; private set; }

	public virtual float SpawnSongBpmTime => base.SongBpmTime - Hjd;

	public virtual float DespawnSongBpmTime => base.SongBpmTime + Hjd;

	public virtual JSONNode CustomAnimation { get; set; }

	public virtual JSONNode CustomCoordinate { get; set; }

	public virtual JSONNode CustomWorldRotation { get; set; }

	public virtual JSONNode CustomLocalRotation { get; set; }

	public virtual JSONNode CustomSpawnEffect { get; set; }

	public virtual JSONNode CustomNoteJumpMovementSpeed { get; set; }

	public virtual JSONNode CustomNoteJumpStartBeatOffset { get; set; }

	public virtual bool CustomFake { get; set; }

	public abstract string CustomKeyAnimation { get; }

	public abstract string CustomKeyCoordinate { get; }

	public abstract string CustomKeyWorldRotation { get; }

	public abstract string CustomKeyLocalRotation { get; }

	public abstract string CustomKeySpawnEffect { get; }

	public abstract string CustomKeyNoteJumpMovementSpeed { get; }

	public abstract string CustomKeyNoteJumpStartBeatOffset { get; }

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(PosX);
		writer.Put(PosY);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		PosX = reader.GetInt();
		PosY = reader.GetInt();
		base.Deserialize(reader);
	}

	protected BaseGrid()
	{
	}

	protected BaseGrid(float time, int posX, int posY, JSONNode customData = null)
		: base(time, customData)
	{
		PosX = posX;
		PosY = posY;
		RecomputeSpawnParameters();
	}

	protected BaseGrid(float jsonTime, float songBpmTime, int posX, int posY, JSONNode customData = null)
		: base(jsonTime, songBpmTime, customData)
	{
		PosX = posX;
		PosY = posY;
		RecomputeSpawnParameters();
	}

	public Vector2 GetCenter()
	{
		return GetPosition() + new Vector2(0f, 0.5f);
	}

	public Vector2 GetPosition()
	{
		return DerivePositionFromData();
	}

	public Vector3 GetScale()
	{
		return Vector3.one;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseGrid baseGrid)
		{
			PosX = baseGrid.PosX;
			PosY = baseGrid.PosY;
		}
	}

	public void RecomputeSpawnParameters()
	{
		float num = CustomNoteJumpMovementSpeed?.AsFloat ?? (BeatSaberSongContainer.Instance?.MapDifficultyInfo?.NoteJumpSpeed).GetValueOrDefault();
		float startBeatOffset = CustomNoteJumpStartBeatOffset?.AsFloat ?? (BeatSaberSongContainer.Instance?.MapDifficultyInfo?.NoteStartBeatOffset).GetValueOrDefault();
		float valueOrDefault = (BeatSaberSongContainer.Instance?.Info?.BeatsPerMinute).GetValueOrDefault();
		Hjd = SpawnParameterHelper.CalculateHalfJumpDuration(num, startBeatOffset, valueOrDefault);
		EditorScale = 100f * num / valueOrDefault;
		Jd = Hjd * EditorScale;
	}

	private Vector2 DerivePositionFromData()
	{
		float x = (float)PosX - 1.5f;
		float y = PosY;
		if (CustomCoordinate != null && CustomCoordinate.IsArray)
		{
			if (CustomCoordinate[0].IsNumber)
			{
				x = (float)CustomCoordinate[0] + 0.5f;
			}
			if (CustomCoordinate[1].IsNumber)
			{
				y = CustomCoordinate[1];
			}
			return new Vector2(x, y);
		}
		if (PosX >= 1000)
		{
			x = (float)PosX / 1000f - 2.5f;
		}
		else if (PosX <= -1000)
		{
			x = (float)PosX / 1000f - 0.5f;
		}
		if (PosY >= 1000 || PosY <= -1000)
		{
			y = (float)PosY / 1000f - 1f;
		}
		return new Vector2(x, y);
	}

	protected override void ParseCustom()
	{
		base.ParseCustom();
		JSONNode jSONNode = base.CustomData;
		CustomAnimation = (((object)jSONNode == null || !jSONNode.HasKey(CustomKeyAnimation)) ? null : base.CustomData?[CustomKeyAnimation]);
		JSONNode jSONNode2 = base.CustomData;
		CustomCoordinate = (((object)jSONNode2 == null || !jSONNode2.HasKey(CustomKeyCoordinate)) ? null : base.CustomData?[CustomKeyCoordinate]);
		JSONNode jSONNode3 = base.CustomData;
		CustomWorldRotation = (((object)jSONNode3 == null || !jSONNode3.HasKey(CustomKeyWorldRotation)) ? null : base.CustomData?[CustomKeyWorldRotation]);
		JSONNode jSONNode4 = base.CustomData;
		CustomLocalRotation = (((object)jSONNode4 == null || !jSONNode4.HasKey(CustomKeyLocalRotation)) ? null : base.CustomData?[CustomKeyLocalRotation]);
		JSONNode jSONNode5 = base.CustomData;
		CustomSpawnEffect = (((object)jSONNode5 != null && jSONNode5.HasKey(CustomKeySpawnEffect)) ? base.CustomData[CustomKeySpawnEffect] : null);
		JSONNode jSONNode6 = base.CustomData;
		CustomNoteJumpMovementSpeed = (((object)jSONNode6 == null || !jSONNode6.HasKey(CustomKeyNoteJumpMovementSpeed)) ? null : base.CustomData?[CustomKeyNoteJumpMovementSpeed]);
		JSONNode jSONNode7 = base.CustomData;
		CustomNoteJumpStartBeatOffset = (((object)jSONNode7 == null || !jSONNode7.HasKey(CustomKeyNoteJumpStartBeatOffset)) ? null : base.CustomData?[CustomKeyNoteJumpStartBeatOffset]);
		RecomputeSpawnParameters();
	}

	protected internal override JSONNode SaveCustom()
	{
		JSONNode jSONNode = base.SaveCustom();
		if (CustomAnimation != null)
		{
			jSONNode[CustomKeyAnimation] = CustomAnimation;
		}
		else
		{
			jSONNode.Remove(CustomKeyAnimation);
		}
		if (CustomCoordinate != null)
		{
			jSONNode[CustomKeyCoordinate] = CustomCoordinate;
		}
		else
		{
			jSONNode.Remove(CustomKeyCoordinate);
		}
		if (CustomWorldRotation != null)
		{
			jSONNode[CustomKeyWorldRotation] = CustomWorldRotation;
		}
		else
		{
			jSONNode.Remove(CustomKeyWorldRotation);
		}
		if (CustomLocalRotation != null)
		{
			jSONNode[CustomKeyLocalRotation] = CustomLocalRotation;
		}
		else
		{
			jSONNode.Remove(CustomKeyLocalRotation);
		}
		if (CustomSpawnEffect != null)
		{
			jSONNode[CustomKeySpawnEffect] = CustomSpawnEffect;
		}
		else
		{
			jSONNode.Remove(CustomKeySpawnEffect);
		}
		if (CustomNoteJumpMovementSpeed != null)
		{
			jSONNode[CustomKeyNoteJumpMovementSpeed] = CustomNoteJumpMovementSpeed;
		}
		else
		{
			jSONNode.Remove(CustomKeyNoteJumpMovementSpeed);
		}
		if (CustomNoteJumpStartBeatOffset != null)
		{
			jSONNode[CustomKeyNoteJumpStartBeatOffset] = CustomNoteJumpStartBeatOffset;
		}
		else
		{
			jSONNode.Remove(CustomKeyNoteJumpStartBeatOffset);
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}
}
