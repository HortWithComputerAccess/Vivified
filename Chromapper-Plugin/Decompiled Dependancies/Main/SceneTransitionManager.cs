using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneTransitionManager : MonoBehaviour
{
	private static readonly Queue<IEnumerator> externalRoutines = new Queue<IEnumerator>();

	[FormerlySerializedAs("darkThemeSO")]
	[SerializeField]
	private DarkThemeSO darkThemeSo;

	private Coroutine loadingCoroutine;

	public static bool IsLoading { get; private set; }

	public static SceneTransitionManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
		Instance = this;
	}

	public void LoadScene(string scene, params IEnumerator[] routines)
	{
		if (!IsLoading)
		{
			darkThemeSo.DarkThemeifyUI();
			Cursor.lockState = CursorLockMode.None;
			IsLoading = true;
			externalRoutines.Clear();
			foreach (IEnumerator item in routines)
			{
				externalRoutines.Enqueue(item);
			}
			loadingCoroutine = StartCoroutine(SceneTransition(scene));
		}
	}

	public void CancelLoading(string message)
	{
		if (IsLoading && loadingCoroutine != null)
		{
			StopCoroutine(loadingCoroutine);
			IsLoading = false;
			loadingCoroutine = null;
			StartCoroutine(CancelLoadingTransitionAndDisplay(message));
		}
	}

	public void AddLoadRoutine(IEnumerator routine)
	{
		if (IsLoading)
		{
			externalRoutines.Enqueue(routine);
		}
	}

	public void AddAsyncLoadRoutine(IEnumerator routine)
	{
		if (IsLoading)
		{
			externalRoutines.Enqueue(routine);
		}
	}

	private IEnumerator CancelSongLoadingRoutine()
	{
		while (IsLoading)
		{
			yield return new WaitForEndOfFrame();
			if (Input.GetKey(KeyCode.Escape) && !PersistentUI.Instance.DialogBoxIsEnabled)
			{
				PersistentUI.Instance.ShowDialogBox("PersistentUI", "songloading", HandleCancelSongLoading, PersistentUI.DialogBoxPresetType.YesNo);
			}
		}
	}

	private void HandleCancelSongLoading(int res)
	{
		if (res == 0)
		{
			StopAllCoroutines();
			IsLoading = false;
			PersistentUI.Instance.LevelLoadSlider.value = 1f;
			PersistentUI.Instance.LevelLoadSliderLabel.text = "Canceling...";
			LoadScene("02_SongEditMenu");
		}
	}

	private IEnumerator SceneTransition(string scene)
	{
		yield return PersistentUI.Instance.FadeInLoadingScreen();
		yield return StartCoroutine(RunExternalRoutines());
		yield return SceneManager.LoadSceneAsync(scene);
		if (scene.StartsWith("03"))
		{
			StartCoroutine(CancelSongLoadingRoutine());
		}
		yield return StartCoroutine(RunExternalRoutines());
		darkThemeSo.DarkThemeifyUI();
		OptionsController.IsActive = false;
		PersistentUI.Instance.LevelLoadSlider.gameObject.SetActive(value: false);
		PersistentUI.Instance.LevelLoadSliderLabel.text = "";
		yield return PersistentUI.Instance.FadeOutLoadingScreen();
		IsLoading = false;
		loadingCoroutine = null;
	}

	private IEnumerator RunExternalRoutines()
	{
		while (externalRoutines.Count > 0)
		{
			yield return StartCoroutine(externalRoutines.Dequeue());
		}
	}

	private IEnumerator CancelLoadingTransitionAndDisplay(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			PersistentUI.MessageDisplayer.NotificationMessage notifiation = new PersistentUI.MessageDisplayer.NotificationMessage(LocalizationSettings.StringDatabase.GetLocalizedString("SongEditMenu", key, null, FallbackBehavior.UseProjectSettings), PersistentUI.DisplayMessageType.Bottom);
			yield return PersistentUI.Instance.DisplayMessage(notifiation);
		}
		yield return PersistentUI.Instance.FadeOutLoadingScreen();
	}
}
