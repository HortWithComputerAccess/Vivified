using System;
using System.Runtime.CompilerServices;
using Beatmap.V3;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base;

public class FloatFxEventBase : FxEventBase<float>, IEquatable<FloatFxEventBase>
{
	public int Easing;

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		if (mapVersion == 3)
		{
			return V3FloatFxEvent.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		return new FloatFxEventBase
		{
			JsonTime = JsonTime,
			UsePreviousEventValue = UsePreviousEventValue,
			Value = Value,
			Easing = Easing
		};
	}

	public bool Equals(FloatFxEventBase other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Easing == other.Easing && Mathf.Approximately(JsonTime, other.JsonTime) && UsePreviousEventValue == other.UsePreviousEventValue)
		{
			return Mathf.Approximately(Value, other.Value);
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
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((FloatFxEventBase)obj);
	}

	public override int GetHashCode()
	{
		return (((((Easing * 397) ^ JsonTime.GetHashCode()) * 397) ^ UsePreviousEventValue) * 397) ^ Value.GetHashCode();
	}
}
