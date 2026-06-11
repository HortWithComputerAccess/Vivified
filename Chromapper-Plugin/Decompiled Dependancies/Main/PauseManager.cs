using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuestDumper;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour, CMInput.IPauseMenuActions
{
	public static bool IsPaused;

	[SerializeField]
	private CanvasGroup loadingCanvasGroup;

	[SerializeField]
	private AnimationCurve fadeInCurve;

	[SerializeField]
	private AnimationCurve fadeOutCurve;

	[SerializeField]
	private UIMode uiMode;

	[SerializeField]
	private AutoSaveController saveController;

	[SerializeField]
	private GameObject questSaveButton;

	private readonly IEnumerable<Type> disabledActionMaps = from t in typeof(CMInput).GetNestedTypes()
		where t.IsInterface && t != typeof(CMInput.IUtilsActions) && t != typeof(CMInput.IPauseMenuActions)
		select t;

	private PlatformDescriptor platform;

	private UIModeType previousUIModeType;

	private void Awake()
	{
		questSaveButton.SetActive(Adb.IsAdbInstalled(out var _));
	}

	private void Start()
	{
		OptionsController.OptionsLoadedEvent = (Action)Delegate.Combine(OptionsController.OptionsLoadedEvent, new Action(OptionsLoaded));
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	private void OnDestroy()
	{
		IsPaused = false;
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
		OptionsController.OptionsLoadedEvent = (Action)Delegate.Remove(OptionsController.OptionsLoadedEvent, new Action(OptionsLoaded));
	}

	public void OnPauseEditor(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			TogglePause();
		}
	}

	private void OptionsLoaded()
	{
		if (IsPaused)
		{
			TogglePause();
		}
	}

	private void PlatformLoaded(PlatformDescriptor descriptor)
	{
		platform = descriptor;
	}

	public void TogglePause()
	{
		IsPaused = !IsPaused;
		if (IsPaused)
		{
			CMInputCallbackInstaller.DisableActionMaps(typeof(PauseManager), disabledActionMaps);
			previousUIModeType = UIMode.SelectedMode;
			uiMode.SetUIMode(UIModeType.Normal, showUIChange: false);
		}
		else
		{
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(PauseManager), disabledActionMaps);
			uiMode.SetUIMode(previousUIModeType, showUIChange: false);
		}
		StartCoroutine(TransitionMenu());
	}

	public void Quit(bool save)
	{
		if (save)
		{
			saveController.CheckAndSave(AutoSaveController.SaveType.Menu);
		}
		else
		{
			PersistentUI.Instance.ShowDialogBox("Mapper", "save", SaveAndExitResult, PersistentUI.DialogBoxPresetType.YesNoCancel);
		}
	}

	public void SaveAndExitToMenu()
	{
		saveController.Save();
		if (BeatSaberSongContainer.Instance.MultiMapperConnection != null)
		{
			SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu", WaitForSaveToFinish());
		}
		else
		{
			SceneTransitionManager.Instance.LoadScene("02_SongEditMenu", WaitForSaveToFinish());
		}
	}

	private IEnumerator WaitForSaveToFinish()
	{
		yield return new WaitUntil(() => !saveController.IsSaving);
	}

	public void SaveAndQuitCM()
	{
		saveController.Save();
		Application.Quit();
	}

	public void ExitToMenu()
	{
		PersistentUI.Instance.ShowDialogBox("Mapper", "save", SaveAndExitResult, PersistentUI.DialogBoxPresetType.YesNoCancel);
	}

	public void CloseCM()
	{
		PersistentUI.Instance.ShowDialogBox("Mapper", "quit.save", SaveAndQuitCmResult, PersistentUI.DialogBoxPresetType.YesNoCancel);
	}

	private void SaveAndExitResult(int result)
	{
		switch (result)
		{
		case 0:
			saveController.CheckAndSave(AutoSaveController.SaveType.Menu);
			break;
		case 1:
			if (BeatSaberSongContainer.Instance.MultiMapperConnection != null)
			{
				SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu", WaitForSaveToFinish());
			}
			else
			{
				SceneTransitionManager.Instance.LoadScene("02_SongEditMenu", WaitForSaveToFinish());
			}
			break;
		}
	}

	private void SaveAndQuitCmResult(int result)
	{
		switch (result)
		{
		case 0:
			saveController.CheckAndSave(AutoSaveController.SaveType.Quit);
			break;
		case 1:
			Application.Quit();
			break;
		}
	}

	private IEnumerator TransitionMenu()
	{
		if (IsPaused)
		{
			yield return FadeInLoadingScreen(loadingCanvasGroup);
		}
		else
		{
			yield return FadeOutLoadingScreen(loadingCanvasGroup);
		}
	}

	public Coroutine FadeInLoadingScreen(CanvasGroup group)
	{
		return StartCoroutine(FadeInLoadingScreen(Settings.Instance.InstantEscapeMenuTransitions ? 999f : 2f, loadingCanvasGroup));
	}

	private IEnumerator FadeInLoadingScreen(float rate, CanvasGroup group)
	{
		group.blocksRaycasts = true;
		group.interactable = true;
		float t = 0f;
		while (t < 1f)
		{
			group.alpha = fadeInCurve.Evaluate(t);
			t += Time.deltaTime * rate;
			yield return null;
		}
		group.alpha = 1f;
	}

	public Coroutine FadeOutLoadingScreen(CanvasGroup group)
	{
		return StartCoroutine(FadeOutLoadingScreen(Settings.Instance.InstantEscapeMenuTransitions ? 999f : 2f, group));
	}

	private IEnumerator FadeOutLoadingScreen(float rate, CanvasGroup group)
	{
		float t = 1f;
		while (t > 0f)
		{
			group.alpha = fadeOutCurve.Evaluate(t);
			t -= Time.deltaTime * rate;
			yield return null;
		}
		group.alpha = 0f;
		group.blocksRaycasts = false;
		group.interactable = false;
	}
}
