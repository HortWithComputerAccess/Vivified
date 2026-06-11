using System.Collections.Generic;
using Beatmap.Base;

namespace Beatmap.Shared;

public class ObjectComparer : IComparer<BaseObject>
{
	public int Compare(BaseObject x, BaseObject y)
	{
		if (x.JsonTime != y.JsonTime)
		{
			return x.JsonTime.CompareTo(y.JsonTime);
		}
		return x.GetHashCode().CompareTo(y.GetHashCode());
	}
}
