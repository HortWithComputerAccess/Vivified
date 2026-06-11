using System;
using System.Collections.Generic;
using System.Linq;
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

public class BaseEvent : BaseObject, ICustomDataEvent, ICustomData, IChromaEvent, IChromaObject, INoodleExtensionsEvent
{
	private int value;

	private float rotation;

	public static readonly int[] LightValueToRotationDegrees = new int[8] { -60, -45, -30, -15, 15, 30, 45, 60 };

	private int[] customLightID;

	protected float? customSpeed;

	public override ObjectType ObjectType { get; set; } = ObjectType.Event;

	public virtual int Type { get; set; }

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			if (IsLaneRotationEvent())
			{
				if (0 <= value && value < LightValueToRotationDegrees.Length)
				{
					rotation = LightValueToRotationDegrees[value];
				}
				else if (value >= 1000 && value <= 1720)
				{
					rotation = value - 1360;
				}
			}
			this.value = value;
		}
	}

	public float FloatValue { get; set; } = 1f;

	public float Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			int num = Array.IndexOf(LightValueToRotationDegrees, Mathf.RoundToInt(value));
			if (num >= 0)
			{
				this.value = num;
			}
			else
			{
				this.value = Mathf.RoundToInt(value) + 1360;
			}
			rotation = value;
		}
	}

	public BaseEvent Prev { get; set; }

	public BaseEvent Next { get; set; }

	public bool IsBlue
	{
		get
		{
			if (Value != 1 && Value != 2 && Value != 3)
			{
				return Value == 4;
			}
			return true;
		}
	}

	public bool IsRed
	{
		get
		{
			if (Value != 5 && Value != 6 && Value != 7)
			{
				return Value == 8;
			}
			return true;
		}
	}

	public bool IsWhite
	{
		get
		{
			if (Value != 9 && Value != 10 && Value != 11)
			{
				return Value == 12;
			}
			return true;
		}
	}

	public bool IsOff => Value == 0;

	public bool IsOn
	{
		get
		{
			if (Value != 1 && Value != 5)
			{
				return Value == 9;
			}
			return true;
		}
	}

	public bool IsFlash
	{
		get
		{
			if (Value != 2 && Value != 6)
			{
				return Value == 10;
			}
			return true;
		}
	}

	public bool IsFade
	{
		get
		{
			if (Value != 3 && Value != 7)
			{
				return Value == 11;
			}
			return true;
		}
	}

	public bool IsTransition
	{
		get
		{
			if (Value != 4 && Value != 8)
			{
				return Value == 12;
			}
			return true;
		}
	}

	public bool IsLegacyChroma => Value >= 2000000000;

	public bool IsPropagation => CustomPropID >= -1;

	public virtual int CustomPropID { get; set; } = -1;

	public virtual int[] CustomLightID
	{
		get
		{
			return customLightID;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				customLightID = null;
			}
			else
			{
				customLightID = value;
			}
		}
	}

	public virtual string CustomLerpType { get; set; }

	public virtual string CustomEasing { get; set; }

	public virtual string CustomNameFilter { get; set; }

	public virtual ChromaLightGradient CustomLightGradient { get; set; }

	public virtual float? CustomStep { get; set; }

	public virtual float? CustomProp { get; set; }

	public virtual float? CustomSpeed
	{
		get
		{
			return customSpeed;
		}
		set
		{
			customSpeed = value;
		}
	}

	public virtual float? CustomRingRotation { get; set; }

	public virtual float? CustomStepMult { get; set; }

	public virtual float? CustomPropMult { get; set; }

	public virtual float? CustomSpeedMult { get; set; }

	public virtual float? CustomPreciseSpeed { get; set; }

	public virtual int? CustomDirection { get; set; }

	public virtual bool? CustomLockRotation { get; set; }

	public virtual float? CustomLaneRotation { get; set; }

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

	public string CustomKeyPropID
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_propID";
			case 3:
			case 4:
				return "propID";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyLightID
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_lightID";
			case 3:
			case 4:
				return "lightID";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyLerpType
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_lerpType";
			case 3:
			case 4:
				return "lerpType";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyEasing
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

	public string CustomKeyLightGradient
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_lightGradient";
			case 3:
			case 4:
				return "lightGradient";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyStep
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_step";
			case 3:
			case 4:
				return "step";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyProp
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_prop";
			case 3:
			case 4:
				return "prop";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeySpeed
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_speed";
			case 3:
			case 4:
				return "speed";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyRingRotation
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
				return "rotation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyDirection
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_direction";
			case 3:
			case 4:
				return "direction";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyLockRotation
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_lockPosition";
			case 3:
			case 4:
				return "lockRotation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyLaneRotation
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
				return "rotation";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyNameFilter
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_nameFilter";
			case 3:
			case 4:
				return "nameFilter";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string CustomKeyStepMult => "_stepMult";

	public string CustomKeyPropMult => "_propMult";

	public string CustomKeySpeedMult => "_speedMult";

	public string CustomKeyPreciseSpeed => "_preciseSpeed";

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(Type);
		writer.Put(Value);
		writer.Put(FloatValue);
		base.Serialize(writer);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Type = reader.GetInt();
		Value = reader.GetInt();
		FloatValue = reader.GetFloat();
		base.Deserialize(reader);
	}

	public BaseEvent()
	{
	}

	public BaseEvent(BaseEvent other)
	{
		base.JsonTime = other.JsonTime;
		Type = other.Type;
		Value = other.Value;
		FloatValue = other.FloatValue;
		base.CustomData = other.SaveCustom().Clone();
	}

	public BaseEvent(JSONNode node)
		: this(BeatmapFactory.Event(node))
	{
	}

	public override bool HasMatchingTrack(string filter)
	{
		return true;
	}

	public override bool IsChroma()
	{
		if (base.CustomData != null)
		{
			if ((!base.CustomData.HasKey(CustomKeyColor) || !base.CustomData[CustomKeyColor].IsArray) && (!base.CustomData.HasKey(CustomKeyLightGradient) || !base.CustomData[CustomKeyLightGradient].IsArray) && (!base.CustomData.HasKey(CustomKeyLightID) || (!base.CustomData[CustomKeyLightID].IsArray && !base.CustomData[CustomKeyLightID].IsNumber)) && (!base.CustomData.HasKey(CustomKeyPropID) || (!base.CustomData[CustomKeyPropID].IsArray && !base.CustomData[CustomKeyPropID].IsNumber)) && (!base.CustomData.HasKey(CustomKeyEasing) || !base.CustomData[CustomKeyEasing].IsString) && (!base.CustomData.HasKey(CustomKeyLerpType) || !base.CustomData[CustomKeyLerpType].IsString) && (!base.CustomData.HasKey(CustomKeyNameFilter) || !base.CustomData[CustomKeyNameFilter].IsString) && (!base.CustomData.HasKey("_reset") || !base.CustomData["_reset"].IsBoolean) && (!base.CustomData.HasKey("_counterSpin") || !base.CustomData["_counterSpin"].IsBoolean) && (!base.CustomData.HasKey(CustomKeyPropMult) || !base.CustomData[CustomKeyPropMult].IsNumber) && (!base.CustomData.HasKey(CustomKeyStepMult) || !base.CustomData[CustomKeyStepMult].IsNumber) && (!base.CustomData.HasKey(CustomKeySpeedMult) || !base.CustomData[CustomKeySpeedMult].IsNumber) && (IsLaneRotationEvent() || !base.CustomData.HasKey(CustomKeyRingRotation) || !base.CustomData[CustomKeyRingRotation].IsNumber) && (!base.CustomData.HasKey(CustomKeyStep) || !base.CustomData[CustomKeyStep].IsNumber) && (!base.CustomData.HasKey(CustomKeyProp) || !base.CustomData[CustomKeyProp].IsNumber) && (!base.CustomData.HasKey(CustomKeySpeed) || !base.CustomData[CustomKeySpeed].IsNumber) && (!base.CustomData.HasKey(CustomKeyDirection) || !base.CustomData[CustomKeyDirection].IsNumber))
			{
				if (base.CustomData.HasKey(CustomKeyLockRotation))
				{
					return base.CustomData[CustomKeyLockRotation].IsBoolean;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool IsNoodleExtensions()
	{
		if (IsLaneRotationEvent() && base.CustomData != null && base.CustomData.HasKey("_rotation"))
		{
			return base.CustomData["_rotation"].IsNumber;
		}
		return false;
	}

	public override bool IsMappingExtensions()
	{
		if (IsLaneRotationEvent() && Value >= 1000)
		{
			return Value <= 1720;
		}
		return false;
	}

	public bool IsLightEvent(string environment = null)
	{
		if (Type != 0 && Type != 1 && Type != 2 && Type != 3 && Type != 4 && Type != 10 && Type != 11 && Type != 6)
		{
			return Type == 7;
		}
		return true;
	}

	public bool IsColorBoostEvent()
	{
		return Type == 5;
	}

	public bool IsRingEvent(string environment = null)
	{
		if (Type != 8)
		{
			return Type == 9;
		}
		return true;
	}

	public bool IsRingZoomEvent(string environment = null)
	{
		return Type == 9;
	}

	public bool IsLaserRotationEvent(string environment = null)
	{
		if (Type != 12)
		{
			return Type == 13;
		}
		return true;
	}

	public bool IsLaneRotationEvent()
	{
		if (Type != 14)
		{
			return Type == 15;
		}
		return true;
	}

	public bool IsExtraEvent(string environment = null)
	{
		if (Type != 10 && Type != 6 && Type != 11)
		{
			return Type == 7;
		}
		return true;
	}

	public bool IsUtilityEvent(string environment = null)
	{
		if (Type != 16 && Type != 17 && Type != 18)
		{
			return Type == 19;
		}
		return true;
	}

	public bool IsSpecialEvent(string environment = null)
	{
		if (Type != 40 && Type != 41 && Type != 42)
		{
			return Type == 43;
		}
		return true;
	}

	public virtual bool IsBpmEvent()
	{
		return Type == 100;
	}

	public Vector2? GetPosition(CreateEventTypeLabels labels, EventGridContainer.PropMode mode, int prop)
	{
		if (mode == EventGridContainer.PropMode.Off)
		{
			return new Vector2((float)labels.EventTypeToLaneId(Type) + 0.5f, 0.5f);
		}
		if (Type != prop)
		{
			return null;
		}
		if (CustomLightID == null)
		{
			return new Vector2(0.5f, 0.5f);
		}
		CustomPropID = labels.LightIdsToPropId(Type, CustomLightID) ?? (-1);
		int num = ((mode == EventGridContainer.PropMode.Prop) ? CustomPropID : (-1));
		if (num < 0)
		{
			num = ((CustomLightID.Length != 0) ? labels.LightIDToEditor(Type, CustomLightID[0]) : (-1));
		}
		return new Vector2((float)num + 1.5f, 0.5f);
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseEvent baseEvent)
		{
			int[] array = CustomLightID;
			int[] otherLightId = baseEvent.CustomLightID;
			bool flag = array?.Length == otherLightId?.Length && (array?.All((int x) => Enumerable.Contains(otherLightId, x)) ?? true);
			return Type == baseEvent.Type && flag;
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseEvent baseEvent)
		{
			Type = baseEvent.Type;
			Value = baseEvent.Value;
			FloatValue = baseEvent.FloatValue;
		}
	}

	protected override void ParseCustom()
	{
		base.ParseCustom();
		JSONNode jSONNode = base.CustomData;
		if ((object)jSONNode != null && jSONNode.HasKey(CustomKeyLightID))
		{
			JSONNode jSONNode2 = base.CustomData[CustomKeyLightID];
			CustomLightID = ((!jSONNode2.IsNumber) ? (from x in jSONNode2.AsArray.Linq
				where x.Value.IsNumber
				select x.Value.AsInt).ToArray() : new int[1] { jSONNode2.AsInt });
		}
		else
		{
			CustomLightID = null;
		}
		JSONNode jSONNode3 = base.CustomData;
		if ((object)jSONNode3 != null && jSONNode3.HasKey(CustomKeyLightGradient))
		{
			JSONNode jSONNode4 = base.CustomData[CustomKeyLightGradient];
			CustomLightGradient = new ChromaLightGradient(jSONNode4["_startColor"], jSONNode4["_endColor"], jSONNode4["_duration"], jSONNode4["_easing"]);
		}
		else
		{
			CustomLightGradient = null;
		}
		JSONNode jSONNode5 = base.CustomData;
		CustomLerpType = (((object)jSONNode5 == null || !jSONNode5.HasKey(CustomKeyLerpType)) ? null : base.CustomData?[CustomKeyLerpType].Value);
		JSONNode jSONNode6 = base.CustomData;
		CustomNameFilter = (((object)jSONNode6 == null || !jSONNode6.HasKey(CustomKeyNameFilter)) ? null : base.CustomData?[CustomKeyNameFilter].Value);
		JSONNode jSONNode7 = base.CustomData;
		CustomEasing = (((object)jSONNode7 == null || !jSONNode7.HasKey(CustomKeyEasing)) ? null : base.CustomData?[CustomKeyEasing].Value);
		JSONNode jSONNode8 = base.CustomData;
		CustomStep = (((object)jSONNode8 == null || !jSONNode8.HasKey(CustomKeyStep)) ? ((float?)null) : base.CustomData?[CustomKeyStep].AsFloat);
		JSONNode jSONNode9 = base.CustomData;
		CustomProp = (((object)jSONNode9 == null || !jSONNode9.HasKey(CustomKeyProp)) ? ((float?)null) : base.CustomData?[CustomKeyProp].AsFloat);
		JSONNode jSONNode10 = base.CustomData;
		CustomSpeed = (((object)jSONNode10 == null || !jSONNode10.HasKey(CustomKeySpeed)) ? ((float?)null) : base.CustomData?[CustomKeySpeed].AsFloat);
		JSONNode jSONNode11 = base.CustomData;
		CustomRingRotation = (((object)jSONNode11 == null || !jSONNode11.HasKey(CustomKeyRingRotation)) ? ((float?)null) : base.CustomData?[CustomKeyRingRotation].AsFloat);
		JSONNode jSONNode12 = base.CustomData;
		CustomDirection = (((object)jSONNode12 == null || !jSONNode12.HasKey(CustomKeyDirection)) ? ((int?)null) : base.CustomData?[CustomKeyDirection].AsInt);
		JSONNode jSONNode13 = base.CustomData;
		CustomLockRotation = (((object)jSONNode13 == null || !jSONNode13.HasKey(CustomKeyLockRotation)) ? ((bool?)null) : base.CustomData?[CustomKeyLockRotation].AsBool);
	}

	protected internal override JSONNode SaveCustom()
	{
		JSONNode jSONNode = base.SaveCustom();
		if (CustomLightID != null)
		{
			jSONNode[CustomKeyLightID] = new JSONArray();
			int[] array = CustomLightID;
			foreach (int num in array)
			{
				jSONNode[CustomKeyLightID].Add(num);
			}
		}
		else
		{
			jSONNode.Remove(CustomKeyLightID);
		}
		if (CustomLightGradient != null)
		{
			jSONNode[CustomKeyLightGradient] = CustomLightGradient.ToJson();
		}
		else
		{
			jSONNode.Remove(CustomKeyLightGradient);
		}
		if (CustomLerpType != null)
		{
			jSONNode[CustomKeyLerpType] = CustomLerpType;
		}
		else
		{
			jSONNode.Remove(CustomKeyLerpType);
		}
		if (CustomNameFilter != null)
		{
			jSONNode[CustomKeyNameFilter] = CustomNameFilter;
		}
		else
		{
			jSONNode.Remove(CustomKeyNameFilter);
		}
		if (CustomEasing != null)
		{
			jSONNode[CustomKeyEasing] = CustomEasing;
		}
		else
		{
			jSONNode.Remove(CustomKeyEasing);
		}
		if (CustomStep.HasValue)
		{
			string customKeyStep = CustomKeyStep;
			float? customStep = CustomStep;
			jSONNode[customKeyStep] = (customStep.HasValue ? ((JSONNode)customStep.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyStep);
		}
		if (CustomProp.HasValue)
		{
			string customKeyProp = CustomKeyProp;
			float? customStep = CustomProp;
			jSONNode[customKeyProp] = (customStep.HasValue ? ((JSONNode)customStep.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyProp);
		}
		if (CustomSpeed.HasValue)
		{
			string customKeySpeed = CustomKeySpeed;
			float? customStep = CustomSpeed;
			jSONNode[customKeySpeed] = (customStep.HasValue ? ((JSONNode)customStep.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeySpeed);
		}
		if (CustomRingRotation.HasValue)
		{
			string customKeyRingRotation = CustomKeyRingRotation;
			float? customStep = CustomRingRotation;
			jSONNode[customKeyRingRotation] = (customStep.HasValue ? ((JSONNode)customStep.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyRingRotation);
		}
		if (CustomDirection.HasValue)
		{
			string customKeyDirection = CustomKeyDirection;
			int? customDirection = CustomDirection;
			jSONNode[customKeyDirection] = (customDirection.HasValue ? ((JSONNode)customDirection.GetValueOrDefault()) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyDirection);
		}
		if (CustomLockRotation.HasValue)
		{
			string customKeyLockRotation = CustomKeyLockRotation;
			bool? customLockRotation = CustomLockRotation;
			jSONNode[customKeyLockRotation] = (customLockRotation.HasValue ? ((JSONNode)(customLockRotation == true)) : null);
		}
		else
		{
			jSONNode.Remove(CustomKeyLockRotation);
		}
		SetCustomData(jSONNode);
		return jSONNode;
	}

	public override int CompareTo(BaseObject other)
	{
		int num = base.CompareTo(other);
		if (!(other is BaseEvent baseEvent))
		{
			return num;
		}
		if (num == 0)
		{
			num = Type.CompareTo(baseEvent.Type);
		}
		if (num == 0)
		{
			num = Value.CompareTo(baseEvent.Value);
		}
		if (num == 0)
		{
			num = FloatValue.CompareTo(baseEvent.FloatValue);
		}
		if (num == 0)
		{
			int[] array = customLightID;
			int[] array2 = baseEvent.customLightID;
			if (array != null)
			{
				if (array2 == null)
				{
					return 1;
				}
				int num2 = Mathf.Min(customLightID.Length, baseEvent.customLightID.Length);
				for (int i = 0; i < num2; i++)
				{
					num = customLightID[i].CompareTo(baseEvent.customLightID[i]);
					if (num != 0)
					{
						return num;
					}
				}
				return customLightID.Length.CompareTo(baseEvent.customLightID.Length);
			}
			if (array2 != null)
			{
				return -1;
			}
		}
		if (num == 0)
		{
			num = string.Compare(base.CustomData?.ToString(), baseEvent.CustomData?.ToString(), StringComparison.Ordinal);
		}
		return num;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2Event.ToJson(this);
		case 3:
		case 4:
			return Type switch
			{
				14 => V3RotationEvent.ToJson(this), 
				15 => V3RotationEvent.ToJson(this), 
				5 => V3ColorBoostEvent.ToJson(this), 
				_ => V3BasicEvent.ToJson(this), 
			};
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		BaseEvent baseEvent = new BaseEvent(this);
		baseEvent.ParseCustom();
		baseEvent.CustomPropID = CustomPropID;
		return baseEvent;
	}
}
