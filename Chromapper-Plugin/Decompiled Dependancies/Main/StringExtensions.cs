internal static class StringExtensions
{
	public static string StripTMPTags(this string source)
	{
		return source.Replace("<", "<\u200b").Replace(">", "\u200b>");
	}
}
