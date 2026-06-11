using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevConsole : MonoBehaviour, ILogHandler, CMInput.IDebugActions
{
	internal class Logline
	{
		public readonly string StackTrace;

		public readonly LogType Type;

		public readonly string Message;

		public Logline(LogType type, string message, string stackTrace)
		{
			Type = type;
			Message = message;
			StackTrace = stackTrace;
		}
	}

	private const bool devConsoleInEditor = false;

	private const int maxLines = 500;

	[SerializeField]
	private LogLineUI logRow;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Transform rowParent;

	private readonly List<string> lines = new List<string>();

	private readonly List<LogLineUI> uiElements = new List<LogLineUI>();

	private readonly ConcurrentQueue<Logline> backlog = new ConcurrentQueue<Logline>();

	private readonly Dictionary<string, string> loadedPluginAssemblies = new Dictionary<string, string>();

	private StreamWriter writer;

	private readonly Dictionary<LogType, string> logColors = new Dictionary<LogType, string>
	{
		{
			LogType.Log,
			"#FFFFFF"
		},
		{
			LogType.Assert,
			"#32AD10"
		},
		{
			LogType.Error,
			"#F02B2B"
		},
		{
			LogType.Exception,
			"#AF3DFF"
		},
		{
			LogType.Warning,
			"#EBCF34"
		}
	};

	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		backlog.Enqueue(new Logline(logType, string.Format(format, args), null));
	}

	public void LogException(Exception exception, UnityEngine.Object context)
	{
		KeyValuePair<string, string> keyValuePair = loadedPluginAssemblies.FirstOrDefault((KeyValuePair<string, string> p) => p.Value == exception.Source);
		if (keyValuePair.Key != null)
		{
			Debug.LogWarning("The following exception is caused by the '" + keyValuePair.Key + "' plugin, please check for an update or remove it!");
		}
		backlog.Enqueue(new Logline(LogType.Exception, $"[{exception.GetType()}] {exception.Message}", exception.StackTrace));
	}

	public void OnEnable()
	{
		Hide();
		if (!Application.isEditor)
		{
			string path = Path.Combine(Application.persistentDataPath, "ChroMapper.log");
			writer = new StreamWriter(path);
			Debug.unityLogger.logHandler = this;
			Application.logMessageReceived += LogCallback;
			SceneManager.sceneLoaded += SceneLoaded;
			PluginLoader.PluginsLoadedEvent = (Action<Plugin[]>)Delegate.Combine(PluginLoader.PluginsLoadedEvent, new Action<Plugin[]>(UpdateLoadedPluginAssemblies));
		}
	}

	public void OnDisable()
	{
		Application.logMessageReceived -= LogCallback;
		SceneManager.sceneLoaded -= SceneLoaded;
		PluginLoader.PluginsLoadedEvent = (Action<Plugin[]>)Delegate.Remove(PluginLoader.PluginsLoadedEvent, new Action<Plugin[]>(UpdateLoadedPluginAssemblies));
	}

	private void UpdateLoadedPluginAssemblies(Plugin[] plugins)
	{
		loadedPluginAssemblies.Clear();
		foreach (Plugin plugin in plugins)
		{
			loadedPluginAssemblies.Add(plugin.Name, plugin.AssemblyName.Name);
		}
	}

	private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		int num = (arg0.name.Contains("Mapper") ? 30 : 10);
		GetComponent<RectTransform>().anchoredPosition = new Vector3(10f, num, 0f);
	}

	public void Update()
	{
		Logline result;
		while (backlog.TryDequeue(out result))
		{
			ShowLogline(result);
		}
	}

	private void FixedUpdate()
	{
		writer?.Flush();
	}

	private void LogCallback(string condition, string stackTrace, LogType type)
	{
		ShowLogline(new Logline(type, condition, stackTrace));
	}

	private void ShowLogline(Logline logline)
	{
		if ((Settings.Instance.ShowNonImportantErrors && logline.Type == LogType.Error) || logline.Type == LogType.Exception)
		{
			scrollRect.gameObject.SetActive(value: true);
		}
		Debug.developerConsoleVisible = false;
		lines.Add(logline.Message);
		writer.WriteLine("[" + logline.Type.ToString() + "] " + logline.Message);
		if (!string.IsNullOrWhiteSpace(logline.StackTrace))
		{
			writer.WriteLine(logline.StackTrace);
		}
		LogLineUI logLineUI;
		if (uiElements.Count >= 500)
		{
			logLineUI = uiElements[0];
			uiElements.RemoveAt(0);
			logLineUI.transform.SetAsLastSibling();
		}
		else
		{
			logLineUI = UnityEngine.Object.Instantiate(logRow, rowParent);
		}
		uiElements.Add(logLineUI);
		logLineUI.gameObject.SetActive(value: true);
		logLineUI.SetupReport(logline, lines);
		logLineUI.TextMesh.text = "<color=" + logColors[logline.Type] + ">" + logline.Message + "</color>\n";
		StopCoroutine("ScrollToBottom");
		StartCoroutine("ScrollToBottom");
	}

	private IEnumerator ScrollToBottom()
	{
		yield return new WaitForEndOfFrame();
		scrollRect.verticalNormalizedPosition = 0f;
	}

	public void Clear()
	{
		lines.Clear();
		foreach (LogLineUI uiElement in uiElements)
		{
			uiElement.gameObject.SetActive(value: false);
		}
		StopCoroutine("ScrollToBottom");
		StartCoroutine("ScrollToBottom");
	}

	public static void OpenFolder(string subfolder = null)
	{
		try
		{
			string text = Application.persistentDataPath;
			if (!string.IsNullOrWhiteSpace(subfolder))
			{
				text = Path.Combine(text, subfolder);
				Directory.CreateDirectory(subfolder);
			}
			OSTools.OpenFileBrowser(text);
		}
		catch
		{
			Debug.LogWarning("Failed to open log directory");
		}
	}

	public void Hide()
	{
		scrollRect.gameObject.SetActive(value: false);
	}

	public void OnToggleDebugConsole(InputAction.CallbackContext context)
	{
		scrollRect.gameObject.SetActive(!scrollRect.gameObject.activeSelf);
		if (scrollRect.gameObject.activeSelf && toggle.isOn)
		{
			StopCoroutine("ScrollToBottom");
			StartCoroutine("ScrollToBottom");
		}
	}
}
