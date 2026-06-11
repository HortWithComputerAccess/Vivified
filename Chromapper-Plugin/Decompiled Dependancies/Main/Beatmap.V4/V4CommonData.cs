using System;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V4;

public static class V4CommonData
{
	public struct Note : IEquatable<Note>
	{
		public int PosX { get; set; }

		public int PosY { get; set; }

		public int Color { get; set; }

		public int CutDirection { get; set; }

		public int AngleOffset { get; set; }

		public static Note GetFromJson(JSONNode node)
		{
			return new Note
			{
				PosX = node["x"].AsInt,
				PosY = node["y"].AsInt,
				Color = node["c"].AsInt,
				CutDirection = node["d"].AsInt,
				AngleOffset = node["a"].AsInt
			};
		}

		public static Note FromBaseNote(BaseNote baseNote)
		{
			return new Note
			{
				PosX = baseNote.PosX,
				PosY = baseNote.PosY,
				Color = baseNote.Color,
				CutDirection = baseNote.CutDirection,
				AngleOffset = baseNote.AngleOffset
			};
		}

		public static Note FromBaseSliderHead(BaseSlider baseSlider)
		{
			return new Note
			{
				PosX = baseSlider.PosX,
				PosY = baseSlider.PosY,
				Color = baseSlider.Color,
				CutDirection = baseSlider.CutDirection,
				AngleOffset = baseSlider.AngleOffset
			};
		}

		public static Note FromBaseArcTail(BaseArc baseArc)
		{
			return new Note
			{
				PosX = baseArc.TailPosX,
				PosY = baseArc.TailPosY,
				CutDirection = baseArc.TailCutDirection
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["x"] = PosX,
				["y"] = PosY,
				["c"] = Color,
				["d"] = CutDirection,
				["a"] = AngleOffset
			};
		}

		public bool Equals(Note other)
		{
			if (PosX == other.PosX && PosY == other.PosY && Color == other.Color && CutDirection == other.CutDirection)
			{
				return AngleOffset == other.AngleOffset;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Note other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((PosX * 397) ^ PosY) * 397) ^ Color) * 397) ^ CutDirection) * 397) ^ AngleOffset;
		}
	}

	public struct Bomb : IEquatable<Bomb>
	{
		public int PosX { get; set; }

		public int PosY { get; set; }

		public static Bomb GetFromJson(JSONNode node)
		{
			return new Bomb
			{
				PosX = node["x"].AsInt,
				PosY = node["y"].AsInt
			};
		}

		public static Bomb FromBaseNote(BaseNote baseNote)
		{
			return new Bomb
			{
				PosX = baseNote.PosX,
				PosY = baseNote.PosY
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["x"] = PosX,
				["y"] = PosY
			};
		}

		public bool Equals(Bomb other)
		{
			if (PosX == other.PosX)
			{
				return PosY == other.PosY;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Bomb other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (PosX * 397) ^ PosY;
		}
	}

	public struct Obstacle : IEquatable<Obstacle>
	{
		public int PosX { get; set; }

		public int PosY { get; set; }

		public float Duration { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public static Obstacle GetFromJson(JSONNode node)
		{
			return new Obstacle
			{
				PosX = node["x"].AsInt,
				PosY = node["y"].AsInt,
				Duration = node["d"].AsFloat,
				Width = node["w"].AsInt,
				Height = node["h"].AsInt
			};
		}

		public static Obstacle FromBaseObstacle(BaseObstacle baseObstacle)
		{
			return new Obstacle
			{
				PosX = baseObstacle.PosX,
				PosY = baseObstacle.PosY,
				Duration = baseObstacle.Duration,
				Width = baseObstacle.Width,
				Height = baseObstacle.Height
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["x"] = PosX,
				["y"] = PosY,
				["d"] = Duration,
				["w"] = Width,
				["h"] = Height
			};
		}

		public bool Equals(Obstacle other)
		{
			if (PosX == other.PosX && PosY == other.PosY && Duration.Equals(other.Duration) && Width == other.Width)
			{
				return Height == other.Height;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Obstacle other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((PosX * 397) ^ PosY) * 397) ^ Duration.GetHashCode()) * 397) ^ Width) * 397) ^ Height;
		}
	}

	public struct Arc : IEquatable<Arc>
	{
		public float HeadControlPointLengthMultiplier { get; set; }

		public float TailControlPointLengthMultiplier { get; set; }

		public int MidAnchorMode { get; set; }

		public static Arc GetFromJson(JSONNode node)
		{
			return new Arc
			{
				HeadControlPointLengthMultiplier = node["m"].AsFloat,
				TailControlPointLengthMultiplier = node["tm"].AsFloat,
				MidAnchorMode = node["a"].AsInt
			};
		}

		public static Arc FromBaseArc(BaseArc baseArc)
		{
			return new Arc
			{
				HeadControlPointLengthMultiplier = baseArc.HeadControlPointLengthMultiplier,
				TailControlPointLengthMultiplier = baseArc.TailControlPointLengthMultiplier,
				MidAnchorMode = baseArc.MidAnchorMode
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["m"] = HeadControlPointLengthMultiplier,
				["tm"] = TailControlPointLengthMultiplier,
				["a"] = MidAnchorMode
			};
		}

		public bool Equals(Arc other)
		{
			if (HeadControlPointLengthMultiplier.Equals(other.HeadControlPointLengthMultiplier) && TailControlPointLengthMultiplier.Equals(other.TailControlPointLengthMultiplier))
			{
				return MidAnchorMode == other.MidAnchorMode;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Arc other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((HeadControlPointLengthMultiplier.GetHashCode() * 397) ^ TailControlPointLengthMultiplier.GetHashCode()) * 397) ^ MidAnchorMode;
		}
	}

	public struct Chain : IEquatable<Chain>
	{
		public int TailPosX { get; set; }

		public int TailPosY { get; set; }

		public int SliceCount { get; set; }

		public float Squish { get; set; }

		public static Chain GetFromJson(JSONNode node)
		{
			return new Chain
			{
				TailPosX = node["tx"].AsInt,
				TailPosY = node["ty"].AsInt,
				SliceCount = node["c"].AsInt,
				Squish = node["s"].AsFloat
			};
		}

		public static Chain FromBaseChain(BaseChain baseChain)
		{
			return new Chain
			{
				TailPosX = baseChain.TailPosX,
				TailPosY = baseChain.TailPosY,
				SliceCount = baseChain.SliceCount,
				Squish = baseChain.Squish
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["tx"] = TailPosX,
				["ty"] = TailPosY,
				["c"] = SliceCount,
				["s"] = Squish
			};
		}

		public bool Equals(Chain other)
		{
			if (TailPosX == other.TailPosX && TailPosY == other.TailPosY && SliceCount == other.SliceCount)
			{
				return Squish.Equals(other.Squish);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Chain other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((TailPosX * 397) ^ TailPosY) * 397) ^ SliceCount) * 397) ^ Squish.GetHashCode();
		}
	}

	public struct RotationEvent : IEquatable<RotationEvent>
	{
		public int Type { get; set; }

		public float Rotation { get; set; }

		public static RotationEvent GetFromJson(JSONNode node)
		{
			return new RotationEvent
			{
				Type = node["t"].AsInt,
				Rotation = node["r"].AsFloat
			};
		}

		public static RotationEvent FromBaseEvent(BaseEvent baseEvent)
		{
			return new RotationEvent
			{
				Type = ((baseEvent.Type != 14) ? 1 : 0),
				Rotation = baseEvent.Rotation
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["t"] = Type,
				["r"] = Rotation
			};
		}

		public bool Equals(RotationEvent other)
		{
			if (Type == other.Type)
			{
				return Rotation.Equals(other.Rotation);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RotationEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Type * 397) ^ Rotation.GetHashCode();
		}
	}

	public struct BasicEvent : IEquatable<BasicEvent>
	{
		public int Type { get; set; }

		public int Value { get; set; }

		public float FloatValue { get; set; }

		public static BasicEvent GetFromJson(JSONNode node)
		{
			return new BasicEvent
			{
				Type = node["t"].AsInt,
				Value = node["i"].AsInt,
				FloatValue = node["f"].AsFloat
			};
		}

		public static BasicEvent FromBaseEvent(BaseEvent baseEvent)
		{
			return new BasicEvent
			{
				Type = baseEvent.Type,
				Value = baseEvent.Value,
				FloatValue = baseEvent.FloatValue
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["t"] = Type,
				["i"] = Value,
				["f"] = FloatValue
			};
		}

		public bool Equals(BasicEvent other)
		{
			if (Type == other.Type && Value == other.Value)
			{
				return FloatValue.Equals(other.FloatValue);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is BasicEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Type * 397) ^ Value) * 397) ^ FloatValue.GetHashCode();
		}
	}

	public struct ColorBoostEvent : IEquatable<ColorBoostEvent>
	{
		public int Boost { get; set; }

		public static ColorBoostEvent GetFromJson(JSONNode node)
		{
			return new ColorBoostEvent
			{
				Boost = node["b"].AsInt
			};
		}

		public static ColorBoostEvent FromBaseEvent(BaseEvent baseEvent)
		{
			return new ColorBoostEvent
			{
				Boost = ((baseEvent.Value == 1) ? 1 : 0)
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject { ["b"] = Boost };
		}

		public bool Equals(ColorBoostEvent other)
		{
			return Boost == other.Boost;
		}

		public override bool Equals(object obj)
		{
			if (obj is ColorBoostEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Boost;
		}
	}

	public struct NJSEvent : IEquatable<NJSEvent>
	{
		public int UsePrevious { get; set; }

		public int Easing { get; set; }

		public float RelativeNJS { get; set; }

		public static NJSEvent GetFromJson(JSONNode node)
		{
			return new NJSEvent
			{
				UsePrevious = node["p"].AsInt,
				Easing = node["e"].AsInt,
				RelativeNJS = node["d"].AsFloat
			};
		}

		public static NJSEvent FromBaseNJSEvent(BaseNJSEvent baseNJSEvent)
		{
			return new NJSEvent
			{
				UsePrevious = baseNJSEvent.UsePrevious,
				Easing = baseNJSEvent.Easing,
				RelativeNJS = baseNJSEvent.RelativeNJS
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["p"] = UsePrevious,
				["e"] = Easing,
				["d"] = RelativeNJS
			};
		}

		public bool Equals(NJSEvent other)
		{
			if (UsePrevious == other.UsePrevious && Easing == other.Easing)
			{
				return RelativeNJS.Equals(other.RelativeNJS);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is NJSEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((UsePrevious * 397) ^ Easing) * 397) ^ RelativeNJS.GetHashCode();
		}
	}

	public struct Waypoint : IEquatable<Waypoint>
	{
		public int PosX { get; set; }

		public int PosY { get; set; }

		public int OffsetDirection { get; set; }

		public static Waypoint GetFromJson(JSONNode node)
		{
			return new Waypoint
			{
				PosX = node["x"].AsInt,
				PosY = node["y"].AsInt,
				OffsetDirection = node["d"].AsInt
			};
		}

		public static Waypoint FromBaseWayPoint(BaseWaypoint baseWayPoint)
		{
			return new Waypoint
			{
				PosX = baseWayPoint.PosX,
				PosY = baseWayPoint.PosY,
				OffsetDirection = baseWayPoint.OffsetDirection
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["x"] = PosX,
				["y"] = PosY,
				["d"] = OffsetDirection
			};
		}

		public bool Equals(Waypoint other)
		{
			if (PosX == other.PosX && PosY == other.PosY)
			{
				return OffsetDirection == other.OffsetDirection;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Waypoint other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((PosX * 397) ^ PosY) * 397) ^ OffsetDirection;
		}
	}

	public struct IndexFilter : IEquatable<IndexFilter>
	{
		public int Type { get; set; }

		public int Param0 { get; set; }

		public int Param1 { get; set; }

		public int Reverse { get; set; }

		public int Chunks { get; set; }

		public int Random { get; set; }

		public int Seed { get; set; }

		public float Limit { get; set; }

		public int LimitAffectsType { get; set; }

		public static IndexFilter FromBaseIndexFilter(BaseIndexFilter baseIndexFilter)
		{
			return new IndexFilter
			{
				Type = baseIndexFilter.Type,
				Param0 = baseIndexFilter.Param0,
				Param1 = baseIndexFilter.Param1,
				Reverse = baseIndexFilter.Reverse,
				Chunks = baseIndexFilter.Chunks,
				Random = baseIndexFilter.Random,
				Seed = baseIndexFilter.Seed,
				Limit = baseIndexFilter.Limit,
				LimitAffectsType = baseIndexFilter.LimitAffectsType
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["f"] = Type,
				["p"] = Param0,
				["t"] = Param1,
				["r"] = Reverse,
				["c"] = Chunks,
				["n"] = Random,
				["s"] = Seed,
				["l"] = Limit,
				["d"] = LimitAffectsType
			};
		}

		public bool Equals(IndexFilter other)
		{
			if (Type == other.Type && Param0 == other.Param0 && Param1 == other.Param1 && Reverse == other.Reverse && Chunks == other.Chunks && Random == other.Random && Seed == other.Seed && Limit.Equals(other.Limit))
			{
				return LimitAffectsType == other.LimitAffectsType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is IndexFilter other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((Type * 397) ^ Param0) * 397) ^ Param1) * 397) ^ Reverse) * 397) ^ Chunks) * 397) ^ Random) * 397) ^ Seed) * 397) ^ Limit.GetHashCode()) * 397) ^ LimitAffectsType;
		}
	}

	public struct LightColorEventBox : IEquatable<LightColorEventBox>
	{
		public float BeatDistribution { get; set; }

		public int BeatDistributionType { get; set; }

		public int Easing { get; set; }

		public float BrightnessDistribution { get; set; }

		public int BrightnessDistributionType { get; set; }

		public int BrightnessAffectFirst { get; set; }

		public static LightColorEventBox GetFromJson(JSONNode node)
		{
			return new LightColorEventBox
			{
				BeatDistribution = node["w"].AsFloat,
				BeatDistributionType = node["d"].AsInt,
				BrightnessDistribution = node["s"].AsFloat,
				BrightnessDistributionType = node["t"].AsInt,
				BrightnessAffectFirst = node["b"].AsInt,
				Easing = node["e"].AsInt
			};
		}

		public static LightColorEventBox FromBaseLightColorEventBox(BaseLightColorEventBox baseLightColorEventBox)
		{
			return new LightColorEventBox
			{
				BeatDistribution = baseLightColorEventBox.BeatDistribution,
				BeatDistributionType = baseLightColorEventBox.BeatDistributionType,
				Easing = baseLightColorEventBox.Easing,
				BrightnessDistribution = baseLightColorEventBox.BrightnessDistribution,
				BrightnessDistributionType = baseLightColorEventBox.BrightnessDistributionType,
				BrightnessAffectFirst = baseLightColorEventBox.BrightnessAffectFirst
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["w"] = BeatDistribution,
				["d"] = BeatDistributionType,
				["s"] = BrightnessDistribution,
				["t"] = BrightnessDistributionType,
				["b"] = BrightnessAffectFirst,
				["e"] = Easing
			};
		}

		public bool Equals(LightColorEventBox other)
		{
			if (BeatDistribution.Equals(other.BeatDistribution) && BeatDistributionType == other.BeatDistributionType && Easing == other.Easing && BrightnessDistribution.Equals(other.BrightnessDistribution) && BrightnessDistributionType == other.BrightnessDistributionType)
			{
				return BrightnessAffectFirst == other.BrightnessAffectFirst;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightColorEventBox other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((BeatDistribution.GetHashCode() * 397) ^ BeatDistributionType) * 397) ^ Easing) * 397) ^ BrightnessDistribution.GetHashCode()) * 397) ^ BrightnessDistributionType) * 397) ^ BrightnessAffectFirst;
		}
	}

	public struct LightColorEvent : IEquatable<LightColorEvent>
	{
		public int Easing { get; set; }

		public int Color { get; set; }

		public float Brightness { get; set; }

		public int TransitionType { get; set; }

		public int Frequency { get; set; }

		public float StrobeBrightness { get; set; }

		public int StrobeFade { get; set; }

		public static LightColorEvent GetFromJson(JSONNode node)
		{
			return new LightColorEvent
			{
				TransitionType = node["p"].AsInt,
				Easing = node["e"].AsInt,
				Color = node["c"].AsInt,
				Brightness = node["b"].AsFloat,
				Frequency = node["f"].AsInt,
				StrobeBrightness = node["sb"].AsFloat,
				StrobeFade = node["sf"].AsInt
			};
		}

		public static LightColorEvent FromBaseLightColorEvent(BaseLightColorBase baseLightColorEvent)
		{
			return new LightColorEvent
			{
				TransitionType = baseLightColorEvent.TransitionType,
				Easing = baseLightColorEvent.Easing,
				Color = baseLightColorEvent.Color,
				Brightness = baseLightColorEvent.Brightness,
				Frequency = baseLightColorEvent.Frequency,
				StrobeBrightness = baseLightColorEvent.StrobeBrightness,
				StrobeFade = baseLightColorEvent.StrobeFade
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["p"] = TransitionType,
				["e"] = Easing,
				["c"] = Color,
				["b"] = Brightness,
				["f"] = Frequency,
				["sb"] = StrobeBrightness,
				["sf"] = StrobeFade
			};
		}

		public bool Equals(LightColorEvent other)
		{
			if (Easing == other.Easing && Color == other.Color && Brightness.Equals(other.Brightness) && TransitionType == other.TransitionType && Frequency == other.Frequency && StrobeBrightness.Equals(other.StrobeBrightness))
			{
				return StrobeFade == other.StrobeFade;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightColorEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((Easing * 397) ^ Color) * 397) ^ Brightness.GetHashCode()) * 397) ^ TransitionType) * 397) ^ Frequency) * 397) ^ StrobeBrightness.GetHashCode()) * 397) ^ StrobeFade;
		}
	}

	public struct LightRotationEventBox : IEquatable<LightRotationEventBox>
	{
		public float BeatDistribution { get; set; }

		public int BeatDistributionType { get; set; }

		public int Easing { get; set; }

		public float RotationDistribution { get; set; }

		public int RotationDistributionType { get; set; }

		public int RotationAffectFirst { get; set; }

		public int Axis { get; set; }

		public int Flip { get; set; }

		public static LightRotationEventBox GetFromJson(JSONNode node)
		{
			return new LightRotationEventBox
			{
				BeatDistribution = node["w"].AsFloat,
				BeatDistributionType = node["d"].AsInt,
				RotationDistribution = node["s"].AsFloat,
				RotationDistributionType = node["t"].AsInt,
				RotationAffectFirst = node["b"].AsInt,
				Easing = node["e"].AsInt,
				Axis = node["a"].AsInt,
				Flip = node["f"].AsInt
			};
		}

		public static LightRotationEventBox FromBaseLightRotationEventBox(BaseLightRotationEventBox baseLightRotationEventBox)
		{
			return new LightRotationEventBox
			{
				BeatDistribution = baseLightRotationEventBox.BeatDistribution,
				BeatDistributionType = baseLightRotationEventBox.BeatDistributionType,
				Easing = baseLightRotationEventBox.Easing,
				RotationDistribution = baseLightRotationEventBox.RotationDistribution,
				RotationDistributionType = baseLightRotationEventBox.RotationDistributionType,
				RotationAffectFirst = baseLightRotationEventBox.RotationAffectFirst,
				Axis = baseLightRotationEventBox.Axis,
				Flip = baseLightRotationEventBox.Flip
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["w"] = BeatDistribution,
				["d"] = BeatDistributionType,
				["s"] = RotationDistribution,
				["t"] = RotationDistributionType,
				["b"] = RotationAffectFirst,
				["e"] = Easing,
				["a"] = Axis,
				["f"] = Flip
			};
		}

		public bool Equals(LightRotationEventBox other)
		{
			if (BeatDistribution.Equals(other.BeatDistribution) && BeatDistributionType == other.BeatDistributionType && Easing == other.Easing && RotationDistribution.Equals(other.RotationDistribution) && RotationDistributionType == other.RotationDistributionType && RotationAffectFirst == other.RotationAffectFirst && Axis == other.Axis)
			{
				return Flip == other.Flip;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightRotationEventBox other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((BeatDistribution.GetHashCode() * 397) ^ BeatDistributionType) * 397) ^ Easing) * 397) ^ RotationDistribution.GetHashCode()) * 397) ^ RotationDistributionType) * 397) ^ RotationAffectFirst) * 397) ^ Axis) * 397) ^ Flip;
		}
	}

	public struct LightRotationEvent : IEquatable<LightRotationEvent>
	{
		public int Easing { get; set; }

		public float Rotation { get; set; }

		public int TransitionType { get; set; }

		public int Direction { get; set; }

		public int Loop { get; set; }

		public static LightRotationEvent GetFromJson(JSONNode node)
		{
			return new LightRotationEvent
			{
				TransitionType = node["p"].AsInt,
				Easing = node["e"].AsInt,
				Rotation = node["r"].AsFloat,
				Direction = node["d"].AsInt,
				Loop = node["l"].AsInt
			};
		}

		public static LightRotationEvent FromBaseLightRotationEvent(BaseLightRotationBase baseLightRotationEvent)
		{
			return new LightRotationEvent
			{
				TransitionType = baseLightRotationEvent.UsePrevious,
				Easing = baseLightRotationEvent.EaseType,
				Rotation = baseLightRotationEvent.Rotation,
				Direction = baseLightRotationEvent.Direction,
				Loop = baseLightRotationEvent.Loop
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["p"] = TransitionType,
				["e"] = Easing,
				["r"] = Rotation,
				["d"] = Direction,
				["l"] = Loop
			};
		}

		public bool Equals(LightRotationEvent other)
		{
			if (Easing == other.Easing && Rotation.Equals(other.Rotation) && TransitionType == other.TransitionType && Direction == other.Direction)
			{
				return Loop == other.Loop;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightRotationEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((Easing * 397) ^ Rotation.GetHashCode()) * 397) ^ TransitionType) * 397) ^ Direction) * 397) ^ Loop;
		}
	}

	public struct LightTranslationEventBox : IEquatable<LightTranslationEventBox>
	{
		public float BeatDistribution { get; set; }

		public int BeatDistributionType { get; set; }

		public int Easing { get; set; }

		public float TranslationDistribution { get; set; }

		public int TranslationDistributionType { get; set; }

		public int TranslationAffectFirst { get; set; }

		public int Axis { get; set; }

		public int Flip { get; set; }

		public static LightTranslationEventBox GetFromJson(JSONNode node)
		{
			return new LightTranslationEventBox
			{
				BeatDistribution = node["w"].AsFloat,
				BeatDistributionType = node["d"].AsInt,
				TranslationDistribution = node["s"].AsFloat,
				TranslationDistributionType = node["t"].AsInt,
				TranslationAffectFirst = node["b"].AsInt,
				Easing = node["e"].AsInt,
				Axis = node["a"].AsInt,
				Flip = node["f"].AsInt
			};
		}

		public static LightTranslationEventBox FromBaseLightTranslationEventBox(BaseLightTranslationEventBox baseLightTranslationEventBox)
		{
			return new LightTranslationEventBox
			{
				BeatDistribution = baseLightTranslationEventBox.BeatDistribution,
				BeatDistributionType = baseLightTranslationEventBox.BeatDistributionType,
				Easing = baseLightTranslationEventBox.Easing,
				TranslationDistribution = baseLightTranslationEventBox.TranslationDistribution,
				TranslationDistributionType = baseLightTranslationEventBox.TranslationDistributionType,
				TranslationAffectFirst = baseLightTranslationEventBox.TranslationAffectFirst,
				Axis = baseLightTranslationEventBox.Axis,
				Flip = baseLightTranslationEventBox.Flip
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["w"] = BeatDistribution,
				["d"] = BeatDistributionType,
				["s"] = TranslationDistribution,
				["t"] = TranslationDistributionType,
				["b"] = TranslationAffectFirst,
				["e"] = Easing,
				["a"] = Axis,
				["f"] = Flip
			};
		}

		public bool Equals(LightTranslationEventBox other)
		{
			if (BeatDistribution.Equals(other.BeatDistribution) && BeatDistributionType == other.BeatDistributionType && Easing == other.Easing && TranslationDistribution.Equals(other.TranslationDistribution) && TranslationDistributionType == other.TranslationDistributionType && TranslationAffectFirst == other.TranslationAffectFirst && Axis == other.Axis)
			{
				return Flip == other.Flip;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightTranslationEventBox other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((BeatDistribution.GetHashCode() * 397) ^ BeatDistributionType) * 397) ^ Easing) * 397) ^ TranslationDistribution.GetHashCode()) * 397) ^ TranslationDistributionType) * 397) ^ TranslationAffectFirst) * 397) ^ Axis) * 397) ^ Flip;
		}
	}

	public struct LightTranslationEvent : IEquatable<LightTranslationEvent>
	{
		public int Easing { get; set; }

		public float Translation { get; set; }

		public int TransitionType { get; set; }

		public static LightTranslationEvent GetFromJson(JSONNode node)
		{
			return new LightTranslationEvent
			{
				TransitionType = node["p"].AsInt,
				Easing = node["e"].AsInt,
				Translation = node["t"].AsFloat
			};
		}

		public static LightTranslationEvent FromBaseLightTranslationEvent(BaseLightTranslationBase baseLightTranslationEvent)
		{
			return new LightTranslationEvent
			{
				TransitionType = baseLightTranslationEvent.UsePrevious,
				Easing = baseLightTranslationEvent.EaseType,
				Translation = baseLightTranslationEvent.Translation
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["p"] = TransitionType,
				["e"] = Easing,
				["t"] = Translation
			};
		}

		public bool Equals(LightTranslationEvent other)
		{
			if (Easing == other.Easing && Translation.Equals(other.Translation))
			{
				return TransitionType == other.TransitionType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightTranslationEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Easing * 397) ^ Translation.GetHashCode()) * 397) ^ TransitionType;
		}
	}

	public struct FxEventBox : IEquatable<FxEventBox>
	{
		public float BeatDistribution { get; set; }

		public int BeatDistributionType { get; set; }

		public int Easing { get; set; }

		public float FxDistribution { get; set; }

		public int FxDistributionType { get; set; }

		public int FxAffectFirst { get; set; }

		public static FxEventBox GetFromJson(JSONNode node)
		{
			return new FxEventBox
			{
				BeatDistribution = node["w"].AsFloat,
				BeatDistributionType = node["d"].AsInt,
				FxDistribution = node["s"].AsFloat,
				FxDistributionType = node["t"].AsInt,
				FxAffectFirst = node["b"].AsInt,
				Easing = node["e"].AsInt
			};
		}

		public static FxEventBox FromBaseFxEventBox(BaseVfxEventEventBox baseVfxEventEventBox)
		{
			return new FxEventBox
			{
				BeatDistribution = baseVfxEventEventBox.BeatDistribution,
				BeatDistributionType = baseVfxEventEventBox.BeatDistributionType,
				FxDistribution = baseVfxEventEventBox.VfxDistribution,
				FxDistributionType = baseVfxEventEventBox.VfxDistributionType,
				FxAffectFirst = baseVfxEventEventBox.VfxAffectFirst,
				Easing = baseVfxEventEventBox.Easing
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["w"] = BeatDistribution,
				["d"] = BeatDistributionType,
				["s"] = FxDistribution,
				["t"] = FxDistributionType,
				["b"] = FxAffectFirst,
				["e"] = Easing
			};
		}

		public bool Equals(FxEventBox other)
		{
			if (BeatDistribution.Equals(other.BeatDistribution) && BeatDistributionType == other.BeatDistributionType && Easing == other.Easing && FxDistribution.Equals(other.FxDistribution) && FxDistributionType == other.FxDistributionType)
			{
				return FxAffectFirst == other.FxAffectFirst;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FxEventBox other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((BeatDistribution.GetHashCode() * 397) ^ BeatDistributionType) * 397) ^ Easing) * 397) ^ FxDistribution.GetHashCode()) * 397) ^ FxDistributionType) * 397) ^ FxAffectFirst;
		}
	}

	public struct FloatFxEvent : IEquatable<FloatFxEvent>
	{
		public int TransitionType { get; set; }

		public float Value { get; set; }

		public int Easing { get; set; }

		public static FloatFxEvent GetFromJson(JSONNode node)
		{
			return new FloatFxEvent
			{
				TransitionType = node["p"].AsInt,
				Easing = node["e"].AsInt,
				Value = node["v"].AsFloat
			};
		}

		public static FloatFxEvent FromFloatFxEventBase(FloatFxEventBase floatFxEvent)
		{
			return new FloatFxEvent
			{
				TransitionType = floatFxEvent.UsePreviousEventValue,
				Value = floatFxEvent.Value,
				Easing = floatFxEvent.Easing
			};
		}

		public JSONNode ToJson()
		{
			return new JSONObject
			{
				["p"] = TransitionType,
				["e"] = Easing,
				["v"] = Value
			};
		}

		public bool Equals(FloatFxEvent other)
		{
			if (TransitionType == other.TransitionType && Value.Equals(other.Value))
			{
				return Easing == other.Easing;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FloatFxEvent other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((TransitionType * 397) ^ Value.GetHashCode()) * 397) ^ Easing;
		}
	}
}
