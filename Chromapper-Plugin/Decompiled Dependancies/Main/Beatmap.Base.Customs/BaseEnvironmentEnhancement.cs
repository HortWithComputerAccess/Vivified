using System;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base.Customs;

public class BaseEnvironmentEnhancement : BaseObject
{
	public override ObjectType ObjectType { get; set; } = ObjectType.EnvironmentEnhancement;

	public string ID { get; set; }

	public EnvironmentLookupMethod LookupMethod { get; set; }

	public JSONNode Geometry { get; set; }

	public string Track { get; set; }

	public int? Duplicate { get; set; }

	public JSONNode Active { get; set; }

	public Vector3? Scale { get; set; }

	public Vector3? Position { get; set; }

	public Vector3? Rotation { get; set; }

	public Vector3? LocalPosition { get; set; }

	public Vector3? LocalRotation { get; set; }

	public JSONNode Components { get; set; }

	public int? LightID
	{
		get
		{
			if (Components != null && Components["ILightWithId"] != null && Components["ILightWithId"]["lightID"] != null)
			{
				return Components["ILightWithId"]["lightID"].AsInt;
			}
			return null;
		}
		set
		{
			if (Components != null)
			{
				if (Components["ILightWithId"] != null)
				{
					JSONNode jSONNode = Components["ILightWithId"];
					int? num = value;
					jSONNode["lightID"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
				}
				else if (value.HasValue)
				{
					JSONNode components = Components;
					JSONObject jSONObject = new JSONObject();
					int? num = value;
					jSONObject["lightID"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
					components["ILightWithId"] = jSONObject;
				}
			}
			else if (value.HasValue)
			{
				JSONObject jSONObject2 = new JSONObject();
				int? num = value;
				jSONObject2["lightID"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
				JSONObject value2 = jSONObject2;
				Components = new JSONObject { ["ILightWithId"] = value2 };
			}
		}
	}

	public int? LightType
	{
		get
		{
			if (Components != null && Components["ILightWithId"] != null && Components["ILightWithId"]["type"] != null)
			{
				return Components["ILightWithId"]["type"].AsInt;
			}
			return null;
		}
		set
		{
			if (Components != null)
			{
				if (Components["ILightWithId"] != null)
				{
					JSONNode jSONNode = Components["ILightWithId"];
					int? num = value;
					jSONNode["type"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
				}
				else if (value.HasValue)
				{
					JSONNode components = Components;
					JSONObject jSONObject = new JSONObject();
					int? num = value;
					jSONObject["type"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
					components["ILightWithId"] = jSONObject;
				}
			}
			else if (value.HasValue)
			{
				JSONObject jSONObject2 = new JSONObject();
				int? num = value;
				jSONObject2["type"] = (num.HasValue ? ((JSONNode)num.GetValueOrDefault()) : null);
				JSONObject value2 = jSONObject2;
				Components = new JSONObject { ["ILightWithId"] = value2 };
			}
		}
	}

	public string KeyID
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_id", 
				3 => "id", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyLookupMethod
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_lookupMethod", 
				3 => "lookupMethod", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyGeometry
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_geometry", 
				3 => "geometry", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyTrack
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_track", 
				3 => "track", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public override string CustomKeyTrack => KeyTrack;

	public string KeyDuplicate
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			return mapVersion switch
			{
				2 => "_duplicate", 
				3 => "duplicate", 
				_ => throw new SwitchExpressionException(mapVersion), 
			};
		}
	}

	public string KeyActive
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_active";
			case 3:
			case 4:
				return "active";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyScale
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
				return "scale";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyPosition
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
				return "position";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyRotation
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

	public string KeyLocalPosition
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_localPosition";
			case 3:
			case 4:
				return "localPosition";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyLocalRotation
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

	public string KeyComponents
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_components";
			case 3:
			case 4:
				return "components";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyLightID
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

	public string KeyLightType
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_type";
			case 3:
			case 4:
				return "type";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string GeometryKeyType
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_type";
			case 3:
			case 4:
				return "type";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string GeometryKeyMaterial
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_material";
			case 3:
			case 4:
				return "material";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public override string CustomKeyColor => null;

	public BaseEnvironmentEnhancement()
	{
	}

	public BaseEnvironmentEnhancement(BaseEnvironmentEnhancement other)
	{
		ID = other.ID;
		LookupMethod = other.LookupMethod;
		Geometry = other.Geometry?.Clone();
		Track = other.Track;
		Duplicate = other.Duplicate;
		Active = other.Active;
		Scale = other.Scale;
		Position = other.Position;
		Rotation = other.Rotation;
		LocalPosition = other.LocalPosition;
		LocalRotation = other.LocalRotation;
		Components = other.Components?.Clone();
		LightID = other.LightID;
	}

	public BaseEnvironmentEnhancement(JSONNode node)
	{
		InstantiateHelper(ref node);
	}

	public BaseEnvironmentEnhancement(string toRemove)
	{
		ID = toRemove;
		Active = false;
		LookupMethod = EnvironmentLookupMethod.Contains;
	}

	public override bool HasMatchingTrack(string filter)
	{
		if (filter != null)
		{
			return filter == Track;
		}
		return true;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		return false;
	}

	private static Vector3? ReadVector3OrNull(JSONNode node, string key)
	{
		if (node.HasKey(key) && !node[key].IsNull)
		{
			return node[key].ReadVector3();
		}
		return null;
	}

	public static void WriteVector3(JSONNode node, string key, Vector3? v)
	{
		if (v.HasValue)
		{
			node[key] = new JSONArray();
			node[key].WriteVector3(v.Value);
		}
	}

	private bool Equals(BaseEnvironmentEnhancement other)
	{
		if (ID == other.ID && LookupMethod == other.LookupMethod && Duplicate == other.Duplicate && Active == other.Active && Nullable.Equals(Scale, other.Scale) && Nullable.Equals(Position, other.Position) && Nullable.Equals(LocalPosition, other.LocalPosition) && Nullable.Equals(Rotation, other.Rotation) && Nullable.Equals(LocalRotation, other.LocalRotation) && Nullable.Equals(LightID, other.LightID))
		{
			return Track == other.Track;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((BaseEnvironmentEnhancement)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((((((((((((((((((((ID != null) ? ID.GetHashCode() : 0) * 397) ^ LookupMethod.GetHashCode()) * 397) ^ ((Geometry != null) ? Geometry.GetHashCode() : 0)) * 397) ^ (Duplicate.HasValue ? Duplicate.GetHashCode() : 0)) * 397) ^ ((Active != null) ? Active.GetHashCode() : 0)) * 397) ^ (Scale.HasValue ? Scale.GetHashCode() : 0)) * 397) ^ (Position.HasValue ? Position.GetHashCode() : 0)) * 397) ^ (LocalPosition.HasValue ? LocalPosition.GetHashCode() : 0)) * 397) ^ (Rotation.HasValue ? Rotation.GetHashCode() : 0)) * 397) ^ (LocalRotation.HasValue ? LocalRotation.GetHashCode() : 0)) * 397) ^ ((Components != null) ? Components.GetHashCode() : 0)) * 397) ^ (LightID.HasValue ? LightID.GetHashCode() : 0)) * 397) ^ ((Track != null) ? Track.GetHashCode() : 0);
	}

	protected override void ParseCustom()
	{
	}

	protected internal override JSONNode SaveCustom()
	{
		return null;
	}

	private void InstantiateHelper(ref JSONNode node)
	{
		if (node[KeyGeometry] != null)
		{
			Geometry = node[KeyGeometry];
		}
		else
		{
			ID = node[KeyID]?.Value;
			Enum.TryParse<EnvironmentLookupMethod>(node[KeyLookupMethod]?.Value, out var result);
			LookupMethod = result;
		}
		if (node[KeyTrack] != null)
		{
			Track = node[KeyTrack].Value;
		}
		if (node[KeyDuplicate] != null)
		{
			Duplicate = node[KeyDuplicate].AsInt;
		}
		if (node[KeyActive] != null)
		{
			Active = node[KeyActive].AsBool;
		}
		Scale = ReadVector3OrNull(node, KeyScale);
		Position = ReadVector3OrNull(node, KeyPosition);
		Rotation = ReadVector3OrNull(node, KeyRotation);
		LocalPosition = ReadVector3OrNull(node, KeyLocalPosition);
		LocalRotation = ReadVector3OrNull(node, KeyLocalRotation);
		Components = node[KeyComponents];
		if (node[KeyLightID] != null)
		{
			LightID = node[KeyLightID].AsInt;
		}
		if (node[KeyLightType] != null)
		{
			LightType = node[KeyLightType].AsInt;
		}
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2EnvironmentEnhancement.ToJson(this);
		case 3:
		case 4:
			return V3EnvironmentEnhancement.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		return new BaseEnvironmentEnhancement(this);
	}
}
