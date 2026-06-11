using System;
using System.Collections;
using System.Globalization;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace __Scripts.UI.SongEditMenu;

public class ImageBrowser : MonoBehaviour
{
	private static IEnumerator ClearDisabledActionMaps()
	{
		yield return new WaitForEndOfFrame();
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(ImageBrowser), new Type[1] { typeof(CMInput.IMenusExtendedActions) });
	}

	public void BrowseForImage(Action<string> callback)
	{
		ExtensionFilter[] extensions = new ExtensionFilter[2]
		{
			new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
			new ExtensionFilter("All Files", "*")
		};
		string songDir = BeatSaberSongContainer.Instance.Info.Directory;
		CMInputCallbackInstaller.DisableActionMaps(typeof(ImageBrowser), new Type[1] { typeof(CMInput.IMenusExtendedActions) });
		string[] array;
		try
		{
			array = StandaloneFileBrowser.OpenFilePanel("Open File", songDir, extensions, multiselect: false);
		}
		catch (DllNotFoundException)
		{
			PersistentUI.Instance.DisplayMessage("File browser not supported on this OS", PersistentUI.DisplayMessageType.Bottom);
			return;
		}
		StartCoroutine(ClearDisabledActionMaps());
		if (array.Length == 0)
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(songDir);
		FileInfo file = new FileInfo(array[0]);
		string fullName = directoryInfo.FullName;
		string fullFile = file.FullName;
		bool ignoreCase = true;
		if (!fullFile.StartsWith(fullName, ignoreCase, CultureInfo.InvariantCulture))
		{
			if (FileExistsAlready(callback, songDir, file.Name))
			{
				return;
			}
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "files.badpath", delegate(int result)
			{
				if (!FileExistsAlready(callback, songDir, file.Name) && result == 0)
				{
					File.Copy(fullFile, Path.Combine(songDir, file.Name));
					callback(file.Name);
				}
			}, PersistentUI.DialogBoxPresetType.YesNo);
		}
		else
		{
			callback(fullFile.Substring(fullName.Length + 1));
		}
	}

	private bool FileExistsAlready(Action<string> callback, string songDir, string fileName)
	{
		if (!File.Exists(Path.Combine(songDir, fileName)))
		{
			return false;
		}
		PersistentUI.Instance.ShowDialogBox("SongEditMenu", "files.conflict", delegate(int result)
		{
			if (result == 0)
			{
				callback(fileName);
			}
		}, PersistentUI.DialogBoxPresetType.YesNo);
		return true;
	}

	public IEnumerator LoadImageIntoSprite(string relativeImagePath, Image image, bool isOverride)
	{
		string stringToEscape = Path.Combine(BeatSaberSongContainer.Instance.Info.Directory, relativeImagePath);
		RuntimePlatform platform = Application.platform;
		string text = ((platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor) ? Uri.EscapeDataString(stringToEscape) : Uri.EscapeUriString(stringToEscape));
		UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + text);
		yield return request.SendWebRequest();
		Texture2D content = DownloadHandlerTexture.GetContent(request);
		Sprite sprite = Sprite.Create(content, new Rect(0f, 0f, content.width, content.height), Vector2.one / 2f);
		if (isOverride)
		{
			image.overrideSprite = sprite;
		}
		else
		{
			image.sprite = sprite;
		}
	}
}
