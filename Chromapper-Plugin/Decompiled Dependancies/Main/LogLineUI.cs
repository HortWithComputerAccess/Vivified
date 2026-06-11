using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LogLineUI : MonoBehaviour
{
	private const string bugReportsSubfolder = "Bug Reports";

	private const int lastLinesCount = 20;

	public TextMeshProUGUI TextMesh;

	[SerializeField]
	[FormerlySerializedAs("ReportButton")]
	private Button reportButton;

	private string previousMessages = "";

	private DevConsole.Logline logline;

	private bool sentReport;

	private static readonly string seperator = new string('-', 50);

	internal void SetupReport(DevConsole.Logline logline, List<string> lines)
	{
		this.logline = logline;
		reportButton.gameObject.SetActive(logline.Type == LogType.Exception);
		reportButton.image.color = Color.cyan;
		previousMessages = ((logline.Type == LogType.Exception) ? string.Join("\n", lines.Skip(lines.Count - 20)) : "");
		sentReport = false;
	}

	public void SendReport()
	{
		if (!sentReport)
		{
			sentReport = true;
			StartCoroutine(GenerateBugReport());
		}
		DevConsole.OpenFolder("Bug Reports");
	}

	private static string GenerateSystemInfo()
	{
		return "APP: ChroMapper " + Application.version + ", Unity " + Application.unityVersion + " (" + Environment.CommandLine + ")\nCPU: " + SystemInfo.processorType + " (" + SystemInfo.processorCount + " cores)\nGPU: " + SystemInfo.graphicsDeviceName + "\nRAM: " + SystemInfo.systemMemorySize + " MB\nOS: " + SystemInfo.operatingSystem;
	}

	private static string GeneratePluginList()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Plugin loadedPlugin in PluginLoader.LoadedPlugins)
		{
			stringBuilder.AppendLine($"{loadedPlugin.Name} - {loadedPlugin.Version}");
		}
		return stringBuilder.ToString();
	}

	private string Heading(string text, bool first = false)
	{
		return (first ? "" : "\n\n\n") + seperator + "\n" + text + "\n" + seperator + "\n";
	}

	public IEnumerator GenerateBugReport()
	{
		yield return CreateAsync(Heading("System information:", first: true) + GenerateSystemInfo() + Heading("Installed plugins:") + GeneratePluginList() + Heading("Exception:") + logline.Message + "\n" + logline.StackTrace + Heading("Recent log messages before error:") + previousMessages, "ChroMapper " + Application.version + " bug report info");
		reportButton.image.color = Color.green;
	}

	private IEnumerator WriteErrorToFile(string text)
	{
		string text2 = Path.Combine(Application.persistentDataPath, "Bug Reports");
		Directory.CreateDirectory(text2);
		string path = Path.Combine(text2, $"{DateTime.Now:yyyy_MM_dd-HH_mm_ss}.txt");
		yield return File.WriteAllTextAsync(path, text);
	}

	private IEnumerator CreateAsync(string text, string title = "Untitled", string language = "csharp", int visibility = 1, string expiration = "N")
	{
		yield return WriteErrorToFile(text);
	}
}
