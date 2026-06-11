using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Info;
using TMPro;
using UnityEngine;

public class LoadedDifficultySelectController : MonoBehaviour
{
	[SerializeField]
	private MapLoader mapLoader;

	[SerializeField]
	private TMP_Dropdown dropdown;

	public static Action LoadedDifficultyChangedEvent;

	[SerializeField]
	private BeatmapActionContainer beatmapActionContainer;

	[SerializeField]
	private AutoSaveController autoSaveController;

	private int queuedDropdownValue;

	private int previousDropdownValue;

	private List<InfoDifficulty> setDifficulties;

	private void Start()
	{
		setDifficulties = BeatSaberSongContainer.Instance.MapDifficultyInfo.ParentSet.Difficulties.ToList();
		IEnumerable<TMP_Dropdown.OptionData> collection = setDifficulties.Select((InfoDifficulty x) => (!Settings.Instance.DisplayDiffDetailsInEditor) ? new TMP_Dropdown.OptionData(x.Difficulty) : new TMP_Dropdown.OptionData((!string.IsNullOrWhiteSpace(x.CustomLabel)) ? x.CustomLabel : x.Difficulty));
		dropdown.options = new List<TMP_Dropdown.OptionData>(collection);
		dropdown.value = setDifficulties.IndexOf(BeatSaberSongContainer.Instance.MapDifficultyInfo);
		previousDropdownValue = dropdown.value;
		if (BeatSaberSongContainer.Instance.MultiMapperConnection != null)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (setDifficulties.Count == 1)
		{
			dropdown.interactable = false;
		}
		else
		{
			dropdown.onValueChanged.AddListener(OnDropdownChange);
		}
	}

	private void OnDropdownChange(int value)
	{
		queuedDropdownValue = value;
		if (beatmapActionContainer.ContainsUnsavedActions)
		{
			PersistentUI.Instance.ShowDialogBox("Mapper", "save.unsaved.changes.switch", UnsavedChangesDialogueResult, PersistentUI.DialogBoxPresetType.YesNoCancel);
		}
		else
		{
			SelectDifficulty(queuedDropdownValue);
		}
	}

	private void UnsavedChangesDialogueResult(int result)
	{
		switch (result)
		{
		case 0:
			autoSaveController.Save();
			SelectDifficulty(queuedDropdownValue);
			break;
		case 1:
			SelectDifficulty(queuedDropdownValue);
			break;
		default:
			dropdown.SetValueWithoutNotify(previousDropdownValue);
			break;
		}
	}

	private void SelectDifficulty(int value)
	{
		while (autoSaveController.IsSaving)
		{
		}
		BeatSaberSongContainer.Instance.MapDifficultyInfo = setDifficulties[value];
		BaseDifficulty mapFromInfoFiles = BeatSaberSongUtils.GetMapFromInfoFiles(BeatSaberSongContainer.Instance.Info, BeatSaberSongContainer.Instance.MapDifficultyInfo);
		mapLoader.UpdateMapData(mapFromInfoFiles);
		mapLoader.HardRefresh();
		BeatSaberSongContainer.Instance.Map = mapFromInfoFiles;
		previousDropdownValue = value;
		beatmapActionContainer.ClearBeatmapActions();
		SelectionController.DeselectAll();
		LoadedDifficultyChangedEvent?.Invoke();
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}
}
