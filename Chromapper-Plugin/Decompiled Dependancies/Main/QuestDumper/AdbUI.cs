using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;

namespace QuestDumper;

public static class AdbUI
{
	public static void OnDownloadFail([CanBeNull] UnityWebRequest www, [CanBeNull] Exception e)
	{
		OnDownloadFail((e != null) ? e.Message : www?.error);
	}

	public static void OnDownloadFail(string message)
	{
		PersistentUI.Instance.ShowDialogBox("Options", "quest.adb_error_download", null, PersistentUI.DialogBoxPresetType.Ok, new object[1] { message });
	}

	public static IEnumerator DoDownload()
	{
		DialogBox dialog = PersistentUI.Instance.CreateNewDialogBox();
		dialog.WithTitle("Options", "quest.downloading");
		ProgressBarComponent progressBarComponent = dialog.AddComponent<ProgressBarComponent>();
		progressBarComponent.WithCustomLabelFormatter((float f) => LocalizationSettings.StringDatabase.GetLocalizedString("Options", "quest.downloading_progress", new object[1] { f * 100f }));
		dialog.Open();
		yield return Adb.DownloadADB(null, delegate(UnityWebRequest www, Exception e)
		{
			dialog.Close();
			OnDownloadFail(www, e);
		}, delegate(UnityWebRequest request, bool extracting)
		{
			Debug.Log("Download at " + (request.downloadProgress * 100f).ToString(CultureInfo.InvariantCulture));
			if (!extracting)
			{
				progressBarComponent.UpdateProgressBar(request.downloadProgress);
			}
			else
			{
				progressBarComponent.WithCustomLabelFormatter((float f) => LocalizationSettings.StringDatabase.GetLocalizedString("Options", "quest.extracting_download", null, FallbackBehavior.UseProjectSettings));
				progressBarComponent.UpdateProgressBar(request.downloadProgress);
			}
		});
		Debug.Log("Finished extracting, starting ADB");
		Task<AdbOutput> initialize = Adb.Initialize();
		yield return initialize.AsCoroutine();
		string value = initialize.Result.ErrorOut?.Trim().Replace("* daemon not running; starting now at tcp:5037\n* daemon started successfully", "");
		if (!initialize.IsCompleted || initialize.Exception != null || !string.IsNullOrEmpty(value))
		{
			dialog.Close();
			OnDownloadFail(null, initialize.Exception);
		}
		else
		{
			dialog.Clear();
			dialog.WithTitle("Options", "quest.adb_finished_downloading");
			dialog.AddFooterButton(null, "Ok");
		}
	}

	public static IEnumerator DoRemove()
	{
		DialogBox dialog = PersistentUI.Instance.CreateNewDialogBox();
		dialog.WithTitle("Options", "quest.uninstalling_adb");
		dialog.Open();
		yield return Adb.RemoveADB();
		dialog.Close();
	}
}
