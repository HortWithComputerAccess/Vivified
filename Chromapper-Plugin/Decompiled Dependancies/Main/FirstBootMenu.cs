using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;
using QuestDumper;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class FirstBootMenu : MonoBehaviour
{
	private static readonly string oculusStoreBeatSaberFolderName = "hyperbolic-magnetism-beat-saber";

	[SerializeField]
	private GameObject directoryCanvas;

	[SerializeField]
	private TMP_InputField directoryField;

	[SerializeField]
	private TMP_Dropdown graphicsDropdown;

	[SerializeField]
	private TMP_Dropdown lightingDropdown;

	[SerializeField]
	private GameObject helpPanel;

	[SerializeField]
	private InputBoxFileValidator validation;

	private readonly Regex appManifestRegex = new Regex("\\s\"installdir\"\\s+\"(.+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly Regex libraryRegex = new Regex("\\s\"\\d\"\\s+\"(.+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Settings.Instance.MaximumFPS;
		if (Settings.ValidateDirectory())
		{
			Debug.Log("Auto loaded directory");
			FirstBootRequirementsMet();
			return;
		}
		if (SystemInfo.graphicsMemorySize <= 1024)
		{
			graphicsDropdown.value = 2;
		}
		else
		{
			graphicsDropdown.value = 1;
		}
		string text = GuessBeatSaberInstallationDirectory();
		if (!string.IsNullOrEmpty(text))
		{
			directoryField.text = text;
			ValidateQuiet();
		}
		directoryCanvas.SetActive(value: true);
	}

	public void InstallAdb()
	{
		StartCoroutine(AdbUI.DoDownload());
	}

	private void SetFromTextbox()
	{
		string text = directoryField.text;
		if (text != null)
		{
			Settings.Instance.BeatSaberInstallation = Path.GetFullPath(text);
		}
	}

	public void SetDirectoryButtonPressed()
	{
		SetFromTextbox();
		if (Settings.ValidateDirectory(ErrorFeedbackWithContinue))
		{
			SetDefaults();
			FirstBootRequirementsMet();
		}
		else
		{
			validation.SetValidationState(visible: true);
		}
	}

	public void SetDefaults()
	{
		switch (graphicsDropdown.value)
		{
		case 2:
			Settings.Instance.ChromaticAberration = false;
			Settings.Instance.SimpleBlocks = true;
			Settings.Instance.SolidChainLink = true;
			Settings.Instance.Reflections = false;
			Settings.Instance.HighQualityBloom = false;
			Settings.Instance.DisplayFloatValueText = false;
			Settings.Instance.SpectrogramEditorQuality = 1;
			Settings.Instance.SpectrogramSlices = 0;
			break;
		case 1:
			Settings.Instance.ChromaticAberration = false;
			Settings.Instance.SimpleBlocks = true;
			Settings.Instance.Reflections = false;
			Settings.Instance.SpectrogramSlices = 0;
			Settings.Instance.CameraAA = 2;
			break;
		case 0:
			Settings.Instance.Offset_Spawning = 8;
			Settings.Instance.Offset_Despawning = 2;
			Settings.Instance.ChunkDistance = 10;
			Settings.Instance.SpectrogramSlices = 5;
			Settings.Instance.CameraAA = 4;
			break;
		}
		switch (lightingDropdown.value)
		{
		case 0:
			Settings.Instance.NoteColorMultiplier = 1f;
			Settings.Instance.ArrowColorMultiplier = 1.72f;
			Settings.Instance.ArrowColorWhiteBlend = 0.75f;
			Settings.Instance.ObstacleOpacity = 0.25f;
			Settings.Instance.AlternateLighting = false;
			break;
		case 1:
			Settings.Instance.NoteColorMultiplier = 0.3f;
			Settings.Instance.ArrowColorMultiplier = 3f;
			Settings.Instance.ArrowColorWhiteBlend = 0.25f;
			Settings.Instance.ObstacleOpacity = 0.1f;
			Settings.Instance.AlternateLighting = true;
			break;
		case 2:
			Settings.Instance.NoteColorMultiplier = 1f;
			Settings.Instance.ArrowColorMultiplier = 3f;
			Settings.Instance.ArrowColorWhiteBlend = 1f;
			Settings.Instance.ObstacleOpacity = 0.25f;
			Settings.Instance.AlternateLighting = true;
			break;
		}
	}

	public void ErrorFeedback(string s)
	{
		DoErrorFeedback(s, continueAfter: false);
	}

	public void ErrorFeedbackWithContinue(string s)
	{
		DoErrorFeedback(s, continueAfter: true);
	}

	private void DoErrorFeedback(string s, bool continueAfter)
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString("FirstBoot", s, null, FallbackBehavior.UseProjectSettings);
		PersistentUI.Instance.ShowDialogBox("FirstBoot", "validate.dialog", continueAfter ? new Action<int>(HandleGenerateMissingFoldersWithContinue) : new Action<int>(HandleGenerateMissingFolders), PersistentUI.DialogBoxPresetType.YesNo, new object[1] { localizedString });
	}

	internal void HandleGenerateMissingFolders(int res)
	{
		HandleGenerateMissingFolders(res, continueAfter: false);
	}

	internal void HandleGenerateMissingFoldersWithContinue(int res)
	{
		HandleGenerateMissingFolders(res, continueAfter: true);
	}

	internal void HandleGenerateMissingFolders(int res, bool continueAfter)
	{
		if (res == 0)
		{
			Debug.Log("Creating directories that do not exist...");
			if (!Directory.Exists(Settings.Instance.BeatSaberInstallation))
			{
				Directory.CreateDirectory(Settings.Instance.BeatSaberInstallation);
			}
			if (!Directory.Exists(Settings.Instance.CustomSongsFolder))
			{
				Directory.CreateDirectory(Settings.Instance.CustomSongsFolder);
			}
			if (!Directory.Exists(Settings.Instance.CustomWIPSongsFolder))
			{
				Directory.CreateDirectory(Settings.Instance.CustomWIPSongsFolder);
			}
			SetDefaults();
			FirstBootRequirementsMet();
		}
	}

	public void FirstBootRequirementsMet()
	{
		ColourHistory.Load();
		CustomPlatformsLoader.Instance.Init();
		SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu");
	}

	public void ToggleHelp()
	{
		helpPanel.SetActive(!helpPanel.activeSelf);
	}

	private string GuessBeatSaberInstallationDirectory()
	{
		string text = GuessSteamInstallationDirectory();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return GuessOculusInstallationDirectory();
	}

	private string GuessSteamInstallationDirectory()
	{
		string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 620980";
		string valueName = "InstallLocation";
		try
		{
			string text = (string)Registry.GetValue(keyName, valueName, "");
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return GuessSteamInstallationDirectoryComplex();
		}
		catch (Exception ex)
		{
			Debug.Log("Error reading Steam registry key" + ex);
			return "";
		}
	}

	private string GuessSteamInstallationDirectoryComplex()
	{
		string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam";
		string valueName = "InstallPath";
		List<string> list = new List<string>();
		string text = (string)Registry.GetValue(keyName, valueName, "");
		list.Add(text);
		string path = text + "\\steamapps\\libraryfolders.vdf";
		if (File.Exists(path))
		{
			using StreamReader streamReader = new StreamReader(path);
			string input = streamReader.ReadToEnd();
			foreach (Match item in libraryRegex.Matches(input))
			{
				if (Directory.Exists(item.Groups[1].Value))
				{
					list.Add(item.Groups[1].Value);
				}
			}
		}
		foreach (string item2 in list)
		{
			string path2 = item2 + "\\steamapps\\appmanifest_620980.acf";
			if (!File.Exists(path2))
			{
				continue;
			}
			using StreamReader streamReader2 = new StreamReader(path2);
			string input2 = streamReader2.ReadToEnd();
			GroupCollection groups = appManifestRegex.Matches(input2)[0].Groups;
			string text2 = item2 + "\\steamapps\\common\\" + groups[1].Value;
			if (Directory.Exists(text2))
			{
				return text2;
			}
		}
		return "";
	}

	private string GuessOculusInstallationDirectory()
	{
		string text = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Oculus VR, LLC\\Oculus";
		try
		{
			string registryValue = "InitialAppLibrary";
			string text2 = "Software";
			string text3 = TryRegistryWithPath(text + "\\Config", registryValue, text2, oculusStoreBeatSaberFolderName, "");
			if (!string.IsNullOrEmpty(text3))
			{
				return text3;
			}
			registryValue = "Base";
			text3 = TryRegistryWithPath(text, registryValue, text2, text2, oculusStoreBeatSaberFolderName);
			if (Directory.Exists(text3))
			{
				return text3;
			}
			return TryOculusStoreLibraryLocations();
		}
		catch (Exception ex)
		{
			Debug.Log("Error guessing Oculus Beat Saber Directory" + ex);
			return "";
		}
	}

	private string TryRegistryWithPath(string registryKey, string registryValue, string path1, string path2, string path3)
	{
		string text = (string)Registry.GetValue(registryKey, registryValue, "");
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		string text2 = ((!string.IsNullOrEmpty(path3)) ? Path.Combine(text, path1, path2, path3) : Path.Combine(text, path1, path2));
		if (Directory.Exists(text2))
		{
			return text2;
		}
		return "";
	}

	private string TryOculusStoreLibraryLocations()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Oculus VR, LLC\\Oculus\\Libraries");
		if (registryKey == null)
		{
			return "";
		}
		string[] subKeyNames = registryKey.GetSubKeyNames();
		foreach (string text in subKeyNames)
		{
			object value = registryKey.OpenSubKey(text).GetValue("OriginalPath");
			if (value == null || !string.IsNullOrEmpty((string)value))
			{
				string text2 = Path.Combine((string)value, "Software", oculusStoreBeatSaberFolderName);
				if (Directory.Exists(text2))
				{
					return text2;
				}
			}
		}
		return "";
	}

	public void OpenFolderBrowser()
	{
		StandaloneFileBrowser.OpenFolderPanelAsync("Installation Directory", "", multiselect: false, delegate(string[] paths)
		{
			if (paths.Length != 0)
			{
				string text = paths[0];
				Settings instance = Settings.Instance;
				string beatSaberInstallation = (directoryField.text = text);
				instance.BeatSaberInstallation = beatSaberInstallation;
				validation.SetValidationState(visible: true, Settings.ValidateDirectory(ErrorFeedback));
			}
		});
	}

	public void ValidateQuiet()
	{
		SetFromTextbox();
		validation.SetValidationState(visible: true, Settings.ValidateDirectory());
	}
}
