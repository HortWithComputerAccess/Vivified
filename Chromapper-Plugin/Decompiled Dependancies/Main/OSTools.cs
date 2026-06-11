using System.Diagnostics;

public class OSTools
{
	public static void OpenFileBrowser(string path)
	{
		path = path.Replace("/", "\\").Replace("\\\\", "\\");
		if (!path.StartsWith("\""))
		{
			path = "\"" + path;
		}
		if (!path.EndsWith("\""))
		{
			path += "\"";
		}
		Process.Start("explorer.exe", path ?? "");
	}
}
