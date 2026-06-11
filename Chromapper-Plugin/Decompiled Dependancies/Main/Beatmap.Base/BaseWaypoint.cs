using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V2;
using Beatmap.V3;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class BaseWaypoint : BaseObject
{
	public int PosX { get; set; }

	public int PosY { get; set; }

	public int OffsetDirection { get; set; }

	public override ObjectType ObjectType { get; set; } = ObjectType.Waypoint;

	public override string CustomKeyColor { get; } = "unusedKeyColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseWaypoint()
	{
	}

	public BaseWaypoint(BaseWaypoint other)
	{
		base.JsonTime = other.JsonTime;
		PosX = other.PosX;
		PosY = other.PosY;
		OffsetDirection = other.OffsetDirection;
		base.CustomData = other.SaveCustom().Clone();
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseWaypoint baseWaypoint)
		{
			return (double)Vector2.Distance(new Vector2(baseWaypoint.PosX, baseWaypoint.PosY), new Vector2(PosX, PosY)) < 0.1;
		}
		return false;
	}

	public override void Apply(BaseObject originalData)
	{
		base.Apply(originalData);
		if (originalData is BaseWaypoint baseWaypoint)
		{
			OffsetDirection = baseWaypoint.OffsetDirection;
		}
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		return mapVersion switch
		{
			2 => V2Waypoint.ToJson(this), 
			3 => V3Waypoint.ToJson(this), 
			_ => throw new SwitchExpressionException(mapVersion), 
		};
	}

	public override BaseItem Clone()
	{
		return new BaseWaypoint(this);
	}
}
