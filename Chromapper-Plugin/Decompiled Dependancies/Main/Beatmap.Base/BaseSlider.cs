using Beatmap.Base.Customs;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public abstract class BaseSlider : BaseGrid, ICustomDataSlider, ICustomData, IChromaObject, INoodleExtensionsSlider, INoodleExtensionsGrid
{
	private float tailJsonTime;

	private float? tailSongBpmTime;

	public int Color { get; set; }

	public int CutDirection { get; set; }

	public int AngleOffset { get; set; }

	public float TailJsonTime
	{
		get
		{
			return tailJsonTime;
		}
		set
		{
			tailJsonTime = value;
			RecomputeTailSongBpmTime();
		}
	}

	public float TailSongBpmTime => tailSongBpmTime.Value;

	public int TailPosX { get; set; }

	public int TailPosY { get; set; }

	public int TailRotation { get; set; }

	public JSONNode CustomTailCoordinate { get; set; }

	public abstract string CustomKeyTailCoordinate { get; }

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Color);
		writer.Put(CutDirection);
		writer.Put(AngleOffset);
		writer.Put(TailJsonTime);
		writer.Put(TailSongBpmTime);
		writer.Put(TailPosX);
		writer.Put(TailPosY);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Color = reader.GetInt();
		CutDirection = reader.GetInt();
		AngleOffset = reader.GetInt();
		tailJsonTime = reader.GetFloat();
		tailSongBpmTime = reader.GetFloat();
		TailPosX = reader.GetInt();
		TailPosY = reader.GetInt();
		base.Deserialize(reader);
	}

	protected BaseSlider()
	{
		TailJsonTime = 0f;
	}

	protected BaseSlider(float time, int posX, int posY, int color, int cutDirection, int angleOffset, float tailTime, int tailPosX, int tailPosY, JSONNode customData = null)
		: base(time, posX, posY, customData)
	{
		Color = color;
		CutDirection = cutDirection;
		AngleOffset = angleOffset;
		TailJsonTime = tailTime;
		TailPosX = tailPosX;
		TailPosY = tailPosY;
	}

	protected BaseSlider(float jsonTime, float songBpmTime, int posX, int posY, int color, int cutDirection, int angleOffset, float tailJsonTime, float tailSongBpmTime, int tailPosX, int tailPosY, JSONNode customData = null)
		: base(jsonTime, songBpmTime, posX, posY, customData)
	{
		Color = color;
		CutDirection = cutDirection;
		AngleOffset = angleOffset;
		this.tailJsonTime = tailJsonTime;
		this.tailSongBpmTime = tailSongBpmTime;
		TailPosX = tailPosX;
		TailPosY = tailPosY;
	}

	public override void RecomputeSongBpmTime()
	{
		base.RecomputeSongBpmTime();
		RecomputeTailSongBpmTime();
	}

	private void RecomputeTailSongBpmTime()
	{
		tailSongBpmTime = Map?.JsonTimeToSongBpmTime(TailJsonTime);
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseSlider baseSlider)
		{
			if (Mathf.Abs(TailJsonTime - baseSlider.TailJsonTime) < BeatmapObjectContainerCollection.Epsilon && (double)Vector2.Distance(GetPosition(), baseSlider.GetPosition()) < 0.1 && (double)Vector2.Distance(GetTailPosition(), baseSlider.GetTailPosition()) < 0.1)
			{
				return CutDirection == baseSlider.CutDirection;
			}
			return false;
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseSlider baseSlider)
		{
			Color = baseSlider.Color;
			CutDirection = baseSlider.CutDirection;
			AngleOffset = baseSlider.AngleOffset;
			TailJsonTime = baseSlider.TailJsonTime;
			TailPosX = baseSlider.TailPosX;
			TailPosY = baseSlider.TailPosY;
		}
	}

	public virtual void SwapHeadAndTail()
	{
		float num = base.JsonTime;
		base.JsonTime = tailJsonTime;
		TailJsonTime = num;
		int tailPosX = TailPosX;
		int posX = base.PosX;
		int num2 = (base.PosX = tailPosX);
		num2 = (TailPosX = posX);
		posX = TailPosY;
		tailPosX = PosY;
		num2 = (PosY = posX);
		num2 = (TailPosY = tailPosX);
	}

	public Vector2 GetTailPosition()
	{
		return DerivePositionFromTailData();
	}

	private Vector2 DerivePositionFromTailData()
	{
		float x = (float)TailPosX - 1.5f;
		float y = TailPosY;
		if (CustomTailCoordinate != null && CustomTailCoordinate.IsArray)
		{
			if (CustomTailCoordinate[0].IsNumber)
			{
				x = (float)CustomTailCoordinate[0] + 0.5f;
			}
			if (CustomTailCoordinate[1].IsNumber)
			{
				y = CustomTailCoordinate[1];
			}
			return new Vector2(x, y);
		}
		if (TailPosX >= 1000)
		{
			x = (float)TailPosX / 1000f - 2.5f;
		}
		else if (TailPosX <= -1000)
		{
			x = (float)TailPosX / 1000f - 0.5f;
		}
		if (TailPosY >= 1000 || TailPosY <= -1000)
		{
			y = (float)TailPosY / 1000f - 1f;
		}
		return new Vector2(x, y);
	}

	protected override void ParseCustom()
	{
		base.ParseCustom();
		JSONNode jSONNode = base.CustomData;
		CustomTailCoordinate = (((object)jSONNode == null || !jSONNode.HasKey(CustomKeyTailCoordinate)) ? null : base.CustomData?[CustomKeyTailCoordinate]);
	}

	protected JSONNode SaveCustomFromNotes(BaseNote head, BaseNote tail)
	{
		JSONNode jSONNode = head.SaveCustom();
		tail.SaveCustom();
		JSONNode jSONNode2 = tail.CustomData;
		if ((object)jSONNode2 != null && jSONNode2.HasKey(CustomKeyCoordinate))
		{
			CustomTailCoordinate = tail.CustomData[CustomKeyCoordinate];
			jSONNode[CustomKeyTailCoordinate] = CustomTailCoordinate;
		}
		return jSONNode;
	}

	protected internal override JSONNode SaveCustom()
	{
		JSONNode jSONNode = base.SaveCustom();
		if (CustomTailCoordinate != null)
		{
			jSONNode[CustomKeyTailCoordinate] = CustomTailCoordinate;
		}
		else
		{
			jSONNode.Remove(CustomKeyTailCoordinate);
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseSlider baseSlider))
		{
			return num;
		}
		if (num == 0)
		{
			num = base.PosX.CompareTo(baseSlider.PosX);
		}
		if (num == 0)
		{
			num = PosY.CompareTo(baseSlider.PosY);
		}
		if (num == 0)
		{
			num = CutDirection.CompareTo(baseSlider.CutDirection);
		}
		if (num == 0)
		{
			num = AngleOffset.CompareTo(baseSlider.AngleOffset);
		}
		if (num == 0)
		{
			num = TailJsonTime.CompareTo(baseSlider.TailJsonTime);
		}
		if (num == 0)
		{
			num = TailPosX.CompareTo(baseSlider.TailPosX);
		}
		if (num == 0)
		{
			num = TailPosY.CompareTo(baseSlider.TailPosY);
		}
		return num;
	}
}
