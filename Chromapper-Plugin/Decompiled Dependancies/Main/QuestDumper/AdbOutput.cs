namespace QuestDumper;

public readonly struct AdbOutput(string stdOut, string errorOut)
{
	public readonly string StdOut = stdOut;

	public readonly string ErrorOut = errorOut;

	public override string ToString()
	{
		return "StdOut: " + StdOut + ", ErrorOut: " + ErrorOut;
	}
}
