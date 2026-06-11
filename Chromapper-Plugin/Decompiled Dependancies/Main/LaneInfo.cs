using System;

public class LaneInfo : IComparable
{
	private readonly int sortOrder;

	public string Name;

	public int Type { get; }

	public LaneInfo(int i, int v)
	{
		sortOrder = v;
		Type = i;
	}

	public int CompareTo(object obj)
	{
		if (obj is LaneInfo laneInfo)
		{
			return sortOrder - laneInfo.sortOrder;
		}
		return 0;
	}
}
