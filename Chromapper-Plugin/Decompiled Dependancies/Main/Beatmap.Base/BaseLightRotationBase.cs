using System;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightRotationBase : BaseObject
{
	public override ObjectType ObjectType { get; set; } = ObjectType.Event;

	public float Rotation { get; set; }

	public int Direction { get; set; }

	public int EaseType { get; set; }

	public int Loop { get; set; }

	public int UsePrevious { get; set; }

	public override string CustomKeyColor { get; } = "unusedColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightRotationBase()
	{
	}

	protected BaseLightRotationBase(float time, float rotation, int direction, int easeType, int loop, int usePrevious, JSONNode customData = null)
		: base(time, customData)
	{
		Rotation = rotation;
		Direction = direction;
		EaseType = easeType;
		Loop = loop;
		UsePrevious = usePrevious;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseLightRotationBase baseLightRotationBase)
		{
			if (!(Math.Abs(Rotation - baseLightRotationBase.Rotation) < BaseItem.DecimalTolerance) && Direction != baseLightRotationBase.Direction && EaseType != baseLightRotationBase.EaseType && Loop != baseLightRotationBase.Loop)
			{
				return UsePrevious == baseLightRotationBase.UsePrevious;
			}
			return true;
		}
		return false;
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3LightRotationBase.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
