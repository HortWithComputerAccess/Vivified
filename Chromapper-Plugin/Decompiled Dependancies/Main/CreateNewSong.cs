using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class CreateNewSong : MonoBehaviour
{
	[SerializeField]
	private SongList list;

	private DialogBox createNewSongDialogueBox;

	private TextComponent textComponent;

	private TextBoxComponent folderTextBoxComponent;

	private DropdownComponent versionDropdownComponent;

	private void InitialiseDialogueBox()
	{
		createNewSongDialogueBox = PersistentUI.Instance.CreateNewDialogBox().WithNoTitle().DontDestroyOnClose();
		textComponent = createNewSongDialogueBox.AddComponent<TextComponent>().WithInitialValue("SongSelectMenu", "newmap.dialog");
		folderTextBoxComponent = createNewSongDialogueBox.AddComponent<TextBoxComponent>().WithLabel("SongSelectMenu", "foldername").WithInitialValue("");
		List<string> enumerable = new List<string>
		{
			LocalizationSettings.StringDatabase.GetLocalizedString("SongSelectMenu", "newmap.v4format", null, FallbackBehavior.UseProjectSettings),
			LocalizationSettings.StringDatabase.GetLocalizedString("SongSelectMenu", "newmap.v2format", null, FallbackBehavior.UseProjectSettings)
		};
		versionDropdownComponent = createNewSongDialogueBox.AddComponent<DropdownComponent>().WithLabel("SongSelectMenu", "format.version").WithOptions(enumerable)
			.WithInitialValue(0);
		createNewSongDialogueBox.OnQuickSubmit(HandleNewSong, closeOnQuickSubmit: false);
		createNewSongDialogueBox.AddFooterButton(null, "PersistentUI", "cancel");
		createNewSongDialogueBox.AddFooterButton(null, "PersistentUI", "ok").OnClick(HandleNewSong);
	}

	public void CreateSong()
	{
		if (createNewSongDialogueBox == null)
		{
			InitialiseDialogueBox();
		}
		textComponent.Value = LocalizationSettings.StringDatabase.GetLocalizedString("SongSelectMenu", "newmap.dialog", null, FallbackBehavior.UseProjectSettings);
		folderTextBoxComponent.Value = "";
		createNewSongDialogueBox.Open();
	}

	private void HandleNewSong()
	{
		string value = folderTextBoxComponent.Value;
		if (string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		bool flag = versionDropdownComponent.Value == 0;
		BaseInfo baseInfo = new BaseInfo
		{
			SongName = value,
			Version = (flag ? "4.0.1" : "2.1.0")
		};
		if (string.IsNullOrWhiteSpace(baseInfo.CleanSongName))
		{
			textComponent.Value = LocalizationSettings.StringDatabase.GetLocalizedString("SongSelectMenu", "newmap.dialog.invalid", null, FallbackBehavior.UseProjectSettings);
			return;
		}
		string songDirectory = Path.Combine(list.SelectedFolderPath, baseInfo.CleanSongName);
		if (list.SongInfos.Any((BaseInfo x) => Path.GetFullPath(x.Directory).Equals(Path.GetFullPath(Path.Combine(songDirectory)), StringComparison.CurrentCultureIgnoreCase)))
		{
			textComponent.Value = LocalizationSettings.StringDatabase.GetLocalizedString("SongSelectMenu", "newmap.dialog.duplicate", null, FallbackBehavior.UseProjectSettings);
			return;
		}
		baseInfo.Directory = songDirectory;
		createNewSongDialogueBox.Close();
		InfoDifficultySet item = new InfoDifficultySet
		{
			Characteristic = "Standard"
		};
		baseInfo.DifficultySets.Add(item);
		BeatSaberSongContainer.Instance.SelectSongForEditing(baseInfo);
		PersistentUI.Instance.ShowDialogBox("SongSelectMenu", "newmap.message", null, PersistentUI.DialogBoxPresetType.Ok);
	}
}
