using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.Networking;

public class TempLoaderController : MonoBehaviour
{
	private const string beatSaverDownloadURL = "https://beatsaver.com/api/download/key/";

	private readonly Regex bsrBeatSaverIdRegex = new Regex("(!bsr )?(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public void OpenTempLoader()
	{
		PersistentUI.Instance.ShowInputBox("SongSelectMenu", "temploader.dialog", TryOpenTempLoader, "temploader.dialog.default");
	}

	private void TryOpenTempLoader(string location)
	{
		if (!string.IsNullOrEmpty(location) && !string.IsNullOrWhiteSpace(location))
		{
			location = location.Trim();
			string value = bsrBeatSaverIdRegex.Matches(location)[0].Groups[2].Value;
			Uri result;
			if (value.ToCharArray().All((char c) => c.IsHex()))
			{
				Uri uri = new Uri("https://beatsaver.com/api/download/key/" + value, UriKind.Absolute);
				SceneTransitionManager.Instance.LoadScene("02_SongEditMenu", GetBeatmapFromLocation(uri));
			}
			else if (!Uri.TryCreate(location, UriKind.Absolute, out result))
			{
				CancelTempLoader("Could not retrieve a proper location to download from.");
			}
			else if (!result.AbsolutePath.ToLower().EndsWith("zip"))
			{
				CancelTempLoader("Provided URL does not point to a zipped file.");
			}
			else
			{
				SceneTransitionManager.Instance.LoadScene("02_SongEditMenu", GetBeatmapFromLocation(result));
			}
		}
	}

	private IEnumerator GetBeatmapFromLocation(Uri uri)
	{
		DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
		UnityWebRequest request = UnityWebRequest.Get(uri);
		request.downloadHandler = downloadHandler;
		request.SetRequestHeader("User-Agent", Application.productName + "/" + Application.version);
		PersistentUI.Instance.LevelLoadSlider.gameObject.SetActive(value: true);
		PersistentUI.Instance.LevelLoadSlider.value = 0f;
		PersistentUI.Instance.LevelLoadSliderLabel.text = "Downloading file... Starting download...";
		request.SendWebRequest();
		while (!request.isDone)
		{
			if (int.TryParse(request.GetResponseHeader("Content-Length"), out var result))
			{
				float num = (float)downloadHandler.data.Length / (float)result;
				PersistentUI.Instance.LevelLoadSlider.value = num;
				float num2 = num * 100f;
				PersistentUI.Instance.LevelLoadSliderLabel.text = $"Downloading file... {num2:F2}% complete.";
			}
			else
			{
				PersistentUI.Instance.LevelLoadSlider.value = Mathf.Sin(Time.time) / 2f + 0.5f;
			}
			if (request.result > UnityWebRequest.Result.Success)
			{
				CancelTempLoader(request.error);
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		if (request.result > UnityWebRequest.Result.Success)
		{
			CancelTempLoader(request.error);
			yield break;
		}
		byte[] downloaded = downloadHandler.data;
		if (downloaded != null)
		{
			PersistentUI.Instance.LevelLoadSlider.value = 1f;
			PersistentUI.Instance.LevelLoadSliderLabel.text = "Extracting contents...";
			yield return new WaitForEndOfFrame();
			try
			{
				ZipArchive source = new ZipArchive(new MemoryStream(downloaded), ZipArchiveMode.Read);
				string text = Path.Combine(Path.GetTempPath(), "ChroMapper Temp Loader", request.GetHashCode().ToString());
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				source.ExtractToDirectory(text);
				downloadHandler.Dispose();
				BaseInfo infoFromFolder = BeatSaberSongUtils.GetInfoFromFolder(text);
				if (infoFromFolder != null)
				{
					PersistentUI.Instance.LevelLoadSliderLabel.text = "Loading song...";
					BeatSaberSongContainer.Instance.Info = infoFromFolder;
				}
				else
				{
					CancelTempLoader("Could not obtain a valid Beatmap from the downloaded content.");
				}
				yield break;
			}
			catch (Exception ex)
			{
				if (ex.GetType().Name == "InvalidDataException")
				{
					CancelTempLoader("Downloaded content was not a valid zip.");
				}
				else
				{
					CancelTempLoader("An unknown error (" + ex.GetType().Name + ") has occurred:\n\n" + ex.Message);
				}
				yield break;
			}
		}
		CancelTempLoader("Downloaded bytes is somehow null, yet the request was successfully completed. WTF!?");
	}

	private void CancelTempLoader(string error)
	{
		PersistentUI.Instance.ShowDialogBox("Temp Loader failed with the following message:\n\n" + error, null, PersistentUI.DialogBoxPresetType.Ok);
		SceneTransitionManager.Instance.CancelLoading("");
		ResetProgressBar();
	}

	private void ResetProgressBar()
	{
		PersistentUI.Instance.LevelLoadSlider.value = 0f;
		PersistentUI.Instance.LevelLoadSlider.gameObject.SetActive(value: false);
		PersistentUI.Instance.LevelLoadSliderLabel.text = "";
	}
}
