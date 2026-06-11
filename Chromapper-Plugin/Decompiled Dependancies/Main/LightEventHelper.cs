public static class LightEventHelper
{
	public static bool IsBlueFromValue(int value)
	{
		if (value != 1 && value != 2 && value != 3)
		{
			return value == 4;
		}
		return true;
	}
}
