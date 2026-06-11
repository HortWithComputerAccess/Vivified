using System.Collections.Generic;
using Beatmap.Enums;
using SimpleJSON;

namespace Beatmap.Base;

public abstract class BaseEventBoxGroup<T> : BaseObject where T : BaseEventBox
{
	public override ObjectType ObjectType { get; set; } = ObjectType.Event;

	public int ID { get; set; }

	public List<T> Events { get; set; } = new List<T>();

	protected BaseEventBoxGroup()
	{
	}

	protected BaseEventBoxGroup(float time, int id, List<T> events, JSONNode customData = null)
		: base(time, customData)
	{
		ID = id;
		Events = events;
	}

	protected override bool IsConflictingWithObjectAtSameTime(BaseObject other, bool deletion = false)
	{
		if (other is BaseEventBoxGroup<T> baseEventBoxGroup)
		{
			return ID == baseEventBoxGroup.ID;
		}
		return false;
	}
}
