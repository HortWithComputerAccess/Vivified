using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class UpdateChecker : MonoBehaviour
{
	private static DateTime lastCheck;

	private static int latestVersion = -1;

	[FormerlySerializedAs("showWhenUpdateIsAvailable")]
	public GameObject ShowWhenUpdateIsAvailable;

	private readonly string parentDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;

	private ProcessStartInfo startInfo;

	private void Awake()
	{
		StartCoroutine(CheckForUpdates());
	}

	public void LaunchUpdate()
	{
		Process.Start(startInfo);
		Application.Quit();
	}

	private IEnumerator CheckForUpdates()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		startInfo = new ProcessStartInfo("CML.exe")
		{
			WorkingDirectory = parentDir
		};
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i] == "--launcher")
			{
				startInfo.WorkingDirectory = Path.GetDirectoryName(commandLineArgs[i + 1]);
				startInfo.FileName = Path.GetFileName(commandLineArgs[i + 1]);
			}
		}
		if (!File.Exists(Path.Combine(startInfo.WorkingDirectory, startInfo.FileName)))
		{
			ShowWhenUpdateIsAvailable.SetActive(value: false);
			yield break;
		}
		string channel = ((Settings.Instance.ReleaseChannel == 1) ? "dev" : "stable");
		if (int.Parse(Application.version.Split('.').Last()) != 0 && (latestVersion < 0 || DateTime.Now.Subtract(lastCheck).TotalHours > 1.0))
		{
			StartCoroutine(GetLatestVersion(Settings.Instance.ReleaseServer, channel, VersionCheckCb));
		}
		else
		{
			VersionCheckCb(latestVersion);
		}
	}

	private void VersionCheckCb(int v)
	{
		int num = int.Parse(Application.version.Split('.').Last());
		latestVersion = v;
		lastCheck = DateTime.Now;
		ShowWhenUpdateIsAvailable.SetActive(num != 0 && num < latestVersion);
	}

	public static IEnumerator GetLatestVersion(string server, string channel, Action<int> callback)
	{
		int result;
		using (UnityWebRequest request = UnityWebRequest.Get(server + "/" + channel))
		{
			yield return request.SendWebRequest();
			int.TryParse(request.downloadHandler.text, out result);
		}
		callback(result);
	}
}
