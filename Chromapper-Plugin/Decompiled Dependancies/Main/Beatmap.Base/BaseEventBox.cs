namespace Beatmap.Base;

public abstract class BaseEventBox : BaseItem
{
	public BaseIndexFilter IndexFilter { get; set; }

	public float BeatDistribution { get; set; }

	public int BeatDistributionType { get; set; }

	public int Easing { get; set; }

	protected BaseEventBox()
	{
	}

	protected BaseEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType)
	{
		IndexFilter = indexFilter;
		BeatDistribution = beatDistribution;
		BeatDistributionType = beatDistributionType;
		Easing = 0;
	}

	protected BaseEventBox(BaseIndexFilter indexFilter, float beatDistribution, int beatDistributionType, int easing)
	{
		IndexFilter = indexFilter;
		BeatDistribution = beatDistribution;
		BeatDistributionType = beatDistributionType;
		Easing = easing;
	}
}
