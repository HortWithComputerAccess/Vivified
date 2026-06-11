using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace QuestDumper;

public static class Adb
{
	private const string platformToolsDownloadGeneric = "https://dl.google.com/android/repository/platform-tools-latest-";

	private static readonly Lazy<string> extractAdbPath = new Lazy<string>(() => Settings.AndroidPlatformTools);

	private static readonly Lazy<string> chroMapperAdbPath = new Lazy<string>(() => Path.Combine(extractAdbPath.Value, "platform-tools", "adb" + (IsWindows ? ".exe" : "")));

	private static bool listeningToShutdown;

	private static bool IsWindows
	{
		get
		{
			if (Application.platform != RuntimePlatform.WindowsPlayer)
			{
				return Application.platform == RuntimePlatform.WindowsEditor;
			}
			return true;
		}
	}

	private static string GetFullPath(string fileName)
	{
		if (File.Exists(fileName))
		{
			return Path.GetFullPath(fileName);
		}
		fileName = Path.GetFileName(fileName);
		if (!Settings.Instance.IncludePathForADB)
		{
			return null;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("PATH");
		try
		{
			return (from path in environmentVariable?.Split(Path.PathSeparator)
				select Path.Combine(path, fileName)).FirstOrDefault(File.Exists);
		}
		catch (ArgumentException)
		{
			UnityEngine.Debug.LogWarning("Environment variable contains illegal characters in path. ADB will be disabled.");
			return null;
		}
	}

	private static string GetADBUrl()
	{
		return "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
	}

	public static IEnumerator DownloadADB([CanBeNull] Action<UnityWebRequest> onSuccess, [CanBeNull] Action<UnityWebRequest, Exception> onError, Action<UnityWebRequest, bool> progressUpdate)
	{
		DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
		using UnityWebRequest www = UnityWebRequest.Get(GetADBUrl());
		www.downloadHandler = downloadHandler;
		UnityWebRequestAsyncOperation request = www.SendWebRequest();
		while (!request.isDone)
		{
			progressUpdate?.Invoke(www, arg2: false);
			yield return null;
		}
		if (www.result == UnityWebRequest.Result.ConnectionError)
		{
			onError?.Invoke(www, null);
			yield break;
		}
		byte[] downloaded = downloadHandler.data;
		if (downloaded == null)
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		progressUpdate?.Invoke(www, arg2: true);
		string extractPath = extractAdbPath.Value;
		Task task = Task.Run(delegate
		{
			using MemoryStream stream = new MemoryStream(downloaded);
			using ZipArchive source = new ZipArchive(stream, ZipArchiveMode.Read);
			Directory.CreateDirectory(extractPath);
			source.ExtractToDirectory(extractPath);
			downloadHandler.Dispose();
		});
		while (!task.IsCompleted)
		{
			yield return null;
		}
		onSuccess?.Invoke(www);
	}

	public static IEnumerator RemoveADB()
	{
		string value = chroMapperAdbPath.Value;
		string adbFolder = Path.GetDirectoryName(value);
		if (File.Exists(value) || Directory.Exists(adbFolder))
		{
			yield return Task.Run(async delegate
			{
				await KillServer();
				Directory.Delete(adbFolder, recursive: true);
			}).AsCoroutine();
		}
	}

	public static bool IsAdbInstalled([CanBeNull] out string adbPath)
	{
		adbPath = GetFullPath(chroMapperAdbPath.Value);
		return adbPath != null;
	}

	private static Process BuildProcess(string arguments)
	{
		if (!IsAdbInstalled(out var adbPath) || adbPath == null)
		{
			throw new InvalidOperationException($"Could not find {adbPath} in PATH or location on {Environment.OSVersion.Platform}");
		}
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				FileName = adbPath,
				CreateNoWindow = true,
				Arguments = arguments
			}
		};
		if (process.StartInfo.FileName != adbPath)
		{
			throw new InvalidOperationException("UNITY IS BEING DUMB WHY IS PROCESS USING " + process.StartInfo.FileName + " INSTEAD OF " + adbPath);
		}
		return process;
	}

	private static void ListenToUnityShutdown()
	{
		if (listeningToShutdown)
		{
			return;
		}
		listeningToShutdown = true;
		Application.quitting += async delegate
		{
			if (IsAdbInstalled(out var _))
			{
				await KillServer().ConfigureAwait(continueOnCapturedContext: false);
			}
		};
	}

	private static string EscapeStringFix(string s)
	{
		return "\"\\\"" + s + "\\\"\"";
	}

	private static Task<AdbOutput> RunADBCommand(string arguments)
	{
		ListenToUnityShutdown();
		return Task.Run(delegate
		{
			using Process process = BuildProcess(arguments);
			process.Start();
			StringBuilder standardOutputBuilder = new StringBuilder();
			StringBuilder errorOutputBuilder = new StringBuilder();
			process.OutputDataReceived += delegate(object _, DataReceivedEventArgs args)
			{
				if (args.Data != null)
				{
					standardOutputBuilder.AppendLine(args.Data);
				}
			};
			process.ErrorDataReceived += delegate(object _, DataReceivedEventArgs args)
			{
				if (args.Data != null)
				{
					errorOutputBuilder.AppendLine(args.Data);
				}
			};
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			process.CancelOutputRead();
			process.CancelErrorRead();
			return new AdbOutput(standardOutputBuilder.Replace("\r\n", "\n").ToString().Trim(), errorOutputBuilder.Replace("\r\n", "\n").ToString().Trim());
		});
	}

	public static async Task<(bool, AdbOutput)> IsQuest(string device)
	{
		AdbOutput item = await RunADBCommand("-s " + device + " shell getprop ro.product.manufacturer");
		return (item.StdOut.Contains("Oculus"), item);
	}

	public static async Task<AdbOutput> KillServer()
	{
		return await RunADBCommand("kill-server");
	}

	public static async Task<(string, AdbOutput)> GetModel(string device)
	{
		AdbOutput item = await RunADBCommand("-s " + device + " shell getprop ro.product.model");
		return (item.StdOut, item);
	}

	public static async Task<(List<string>, AdbOutput)> GetDevices()
	{
		AdbOutput item = await RunADBCommand("devices");
		if (!string.IsNullOrEmpty(item.ErrorOut))
		{
			return (null, item);
		}
		if (!item.StdOut.StartsWith("List of devices attached\n"))
		{
			return (new List<string>(), new AdbOutput(item.StdOut, item.ErrorOut));
		}
		return ((from s in item.StdOut.Substring("List of devices attached\n".Length).Split('\n')
			select s.Substring(0, s.IndexOf("\t", StringComparison.Ordinal)).Replace("\n", "").Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).ToList(), item);
	}

	public static async Task<AdbOutput> Mkdir(string devicePath, string serial, bool makeParents = true, string permission = "770")
	{
		string text = (makeParents ? "-p" : "");
		return await RunADBCommand("-s " + serial + " shell mkdir " + EscapeStringFix(devicePath) + " " + text + " -m " + permission);
	}

	public static async Task<AdbOutput> Push(string localPath, string devicePath, string serial)
	{
		return await RunADBCommand("-s " + serial + " push \"" + localPath + "\" \"" + devicePath + "\"");
	}

	public static async Task<AdbOutput> Pull(string devicePath, string localPath, string serial)
	{
		return await RunADBCommand("-s " + serial + " pull \"" + devicePath + "\" \"" + localPath + "\"");
	}

	public static async Task<AdbOutput> Initialize()
	{
		return await RunADBCommand("start-server");
	}
}
