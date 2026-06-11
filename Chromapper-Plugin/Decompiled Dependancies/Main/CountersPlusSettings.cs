public class CountersPlusSettings : JSONDictionarySetting
{
	public CountersPlusSettings()
	{
		Add("enabled", false);
		Add("Notes", true);
		Add("Notes Per Second", true);
		Add("Swings Per Second", true);
		Add("Red/Blue Ratio", true);
		Add("Bombs", true);
		Add("Arcs", true);
		Add("Chains", true);
		Add("Obstacles", true);
		Add("Events", true);
		Add("BPM Changes", true);
		Add("Current BPM", true);
		Add("NJS Events", true);
		Add("Current NJS", true);
		Add("Current HJD", false);
		Add("Current JD", false);
		Add("Current RT", false);
		Add("Time Spent Mapping", true);
	}
}
