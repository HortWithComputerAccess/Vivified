using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Beatmap.Info;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputBoxFileValidator : MonoBehaviour
{
	public enum FileValidationType
	{
		None,
		Audio
	}

	[SerializeField]
	private GameObject inputMask;

	[SerializeField]
	private Image validationImg;

	[SerializeField]
	private TMP_InputField input;

	[SerializeField]
	private Sprite goodSprite;

	[SerializeField]
	private Color goodColor;

	[SerializeField]
	private Sprite badSprite;

	[SerializeField]
	private Color badColor;

	[SerializeField]
	private string filetypeName;

	[SerializeField]
	private string[] extensions;

	[SerializeField]
	private FileValidationType fileValidationType;

	[SerializeField]
	private bool enableValidation;

	[SerializeField]
	private bool forceStartupValidationAlign;

	private Vector2 startOffset;

	public void Awake()
	{
		RectTransform component = inputMask.GetComponent<RectTransform>();
		startOffset = component.offsetMax;
		BaseInfo baseInfo = ((BeatSaberSongContainer.Instance != null) ? BeatSaberSongContainer.Instance.Info : null);
		if (forceStartupValidationAlign || (enableValidation && Directory.Exists(baseInfo?.Directory)))
		{
			component.offsetMax = new Vector2(startOffset.x - 36f, startOffset.y);
		}
	}

	public void Start()
	{
		OnUpdate();
	}

	public void OnUpdate()
	{
		BaseInfo baseInfo = ((BeatSaberSongContainer.Instance != null) ? BeatSaberSongContainer.Instance.Info : null);
		string text = input.text;
		if (!enableValidation || text.Length == 0 || !Directory.Exists(baseInfo?.Directory))
		{
			if (!forceStartupValidationAlign)
			{
				SetValidationState(visible: false);
			}
			return;
		}
		string text2 = Path.Combine(baseInfo.Directory, text);
		bool flag = File.Exists(text2);
		if (fileValidationType == FileValidationType.Audio)
		{
			flag = flag && FileContentValidationHelper.IsSupportedAudioFormat(text2);
		}
		SetValidationState(visible: true, flag);
	}

	public void SetValidationState(bool visible, bool state = false)
	{
		RectTransform component = inputMask.GetComponent<RectTransform>();
		if (!visible)
		{
			component.offsetMax = startOffset;
			validationImg.gameObject.SetActive(value: false);
			return;
		}
		component.offsetMax = new Vector2(startOffset.x - 36f, startOffset.y);
		validationImg.gameObject.SetActive(value: true);
		if (state)
		{
			validationImg.sprite = goodSprite;
			validationImg.color = goodColor;
		}
		else
		{
			validationImg.sprite = badSprite;
			validationImg.color = badColor;
		}
	}

	public void BrowserForFile()
	{
		ExtensionFilter[] array = new ExtensionFilter[2]
		{
			new ExtensionFilter(filetypeName, extensions),
			new ExtensionFilter("All Files", "*")
		};
		if (BeatSaberSongContainer.Instance.Info == null || string.IsNullOrEmpty(BeatSaberSongContainer.Instance.Info.Directory) || !Directory.Exists(BeatSaberSongContainer.Instance.Info.Directory))
		{
			PersistentUI.Instance.ShowDialogBox("Cannot locate song directory. Did you forget to save your map?", null, PersistentUI.DialogBoxPresetType.Ok);
			OnUpdate();
			return;
		}
		string songDir = BeatSaberSongContainer.Instance.Info.Directory;
		CMInputCallbackInstaller.DisableActionMaps(typeof(InputBoxFileValidator), new Type[1] { typeof(CMInput.IMenusExtendedActions) });
		string[] array2;
		try
		{
			array2 = StandaloneFileBrowser.OpenFilePanel("Open File", songDir, array, multiselect: false);
		}
		catch (DllNotFoundException)
		{
			PersistentUI.Instance.DisplayMessage("File browser not supported on this OS", PersistentUI.DisplayMessageType.Bottom);
			return;
		}
		StartCoroutine(ClearDisabledActionMaps());
		if (array2 == null || array2.Length <= 0)
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(songDir);
		FileInfo file = new FileInfo(array2[0]);
		string fullName = directoryInfo.FullName;
		string fullFile = file.FullName;
		bool ignoreCase = true;
		if (fileValidationType == FileValidationType.Audio && !FileContentValidationHelper.IsSupportedAudioFormat(fullFile))
		{
			PersistentUI.Instance.DisplayMessage("SongEditMenu", "load.error.audio2", PersistentUI.DisplayMessageType.Bottom);
		}
		else if (!fullFile.StartsWith(fullName, ignoreCase, CultureInfo.InvariantCulture))
		{
			if (FileExistsAlready(songDir, file.Name))
			{
				return;
			}
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "files.badpath", delegate(int result)
			{
				if (!FileExistsAlready(songDir, file.Name) && result == 0)
				{
					File.Copy(fullFile, Path.Combine(songDir, file.Name));
					input.text = file.Name;
					OnUpdate();
				}
			}, PersistentUI.DialogBoxPresetType.YesNo);
		}
		else
		{
			input.text = fullFile.Substring(fullName.Length + 1);
			OnUpdate();
		}
	}

	private IEnumerator ClearDisabledActionMaps()
	{
		yield return new WaitForEndOfFrame();
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(InputBoxFileValidator), new Type[1] { typeof(CMInput.IMenusExtendedActions) });
	}

	private bool FileExistsAlready(string songDir, string fileName)
	{
		if (!File.Exists(Path.Combine(songDir, fileName)))
		{
			return false;
		}
		PersistentUI.Instance.ShowDialogBox("SongEditMenu", "files.conflict", delegate(int result)
		{
			if (result == 0)
			{
				input.text = fileName;
				OnUpdate();
			}
		}, PersistentUI.DialogBoxPresetType.YesNo);
		return true;
	}
}
