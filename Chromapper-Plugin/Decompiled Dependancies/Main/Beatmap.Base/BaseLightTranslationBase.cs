using System;
using System.Runtime.CompilerServices;
using Beatmap.Enums;
using Beatmap.V3;
using SimpleJSON;

namespace Beatmap.Base;

public class BaseLightTranslationBase : BaseObject
{
	public override ObjectType ObjectType { get; set; } = ObjectType.Event;

	public float Translation { get; set; }

	public int EaseType { get; set; }

	public int UsePrevious { get; set; }

	public override string CustomKeyColor { get; } = "unusedColor";

	public override string CustomKeyTrack { get; } = "unusedKeyTrack";

	public BaseLightTranslationBase()
	{
	}

	protected BaseLightTranslationBase(float time, float translation, int easeType, int usePrevious, JSONNode customData = null)
		: base(time, customData)
	{
		Translation = translation;
		EaseType = easeType;
		UsePrevious = usePrevious;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseLightTranslationBase baseLightTranslationBase)
		{
			if (!(Math.Abs(Translation - baseLightTranslationBase.Translation) < BaseItem.DecimalTolerance) && EaseType != baseLightTranslationBase.EaseType)
			{
				return UsePrevious == baseLightTranslationBase.UsePrevious;
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
			return V3LightTranslationBase.ToJson(this);
		}
		throw new SwitchExpressionException(mapVersion);
	}

	public override BaseItem Clone()
	{
		throw new NotImplementedException();
	}
}
