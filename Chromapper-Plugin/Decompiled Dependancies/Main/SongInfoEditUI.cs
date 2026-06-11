using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Beatmap.Base;
using Beatmap.Info;
using QuestDumper;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongInfoEditUI : MenuBase
{
	public class Environment
	{
		public readonly string HumanName;

		public readonly string JsonName;

		public Environment(string humanName, string jsonName)
		{
			HumanName = humanName;
			JsonName = jsonName;
		}
	}

	public static List<Environment> VanillaEnvironments = new List<Environment>
	{
		new Environment("Default", "DefaultEnvironment"),
		new Environment("Big Mirror", "BigMirrorEnvironment"),
		new Environment("Triangle", "TriangleEnvironment"),
		new Environment("Nice", "NiceEnvironment"),
		new Environment("K/DA", "KDAEnvironment"),
		new Environment("Monstercat", "MonstercatEnvironment"),
		new Environment("Dragons", "DragonsEnvironment"),
		new Environment("Origins", "OriginsEnvironment"),
		new Environment("Crab Rave", "CrabRaveEnvironment"),
		new Environment("Panic! At The Disco", "PanicEnvironment"),
		new Environment("Rocket League", "RocketEnvironment"),
		new Environment("Green Day", "GreenDayEnvironment"),
		new Environment("Green Day Grenade", "GreenDayGrenadeEnvironment"),
		new Environment("Timbaland", "TimbalandEnvironment"),
		new Environment("FitBeat", "FitBeatEnvironment"),
		new Environment("Linkin Park", "LinkinParkEnvironment"),
		new Environment("BTS", "BTSEnvironment"),
		new Environment("Kaleidoscope", "KaleidoscopeEnvironment"),
		new Environment("Interscope", "InterscopeEnvironment"),
		new Environment("Skrillex", "SkrillexEnvironment"),
		new Environment("Billie", "BillieEnvironment"),
		new Environment("Spooky", "HalloweenEnvironment"),
		new Environment("Gaga", "GagaEnvironment"),
		new Environment("Glass Desert", "GlassDesertEnvironment")
	};

	public static List<string> CharacteristicDropdownToBeatmapName = new List<string> { "Standard", "NoArrows", "OneSaber", "360Degree", "90Degree", "Legacy", "Lightshow", "Lawless" };

	[SerializeField]
	private AudioSource previewAudio;

	[SerializeField]
	private TextMeshProUGUI songInfoHeaderTitle;

	[SerializeField]
	private DifficultySelect difficultySelect;

	[SerializeField]
	private TMP_InputField nameField;

	[SerializeField]
	private TMP_InputField subNameField;

	[SerializeField]
	private TMP_InputField songAuthorField;

	[SerializeField]
	private TMP_InputField authorField;

	[SerializeField]
	private TMP_InputField coverImageField;

	[SerializeField]
	private TMP_InputField bpmField;

	[SerializeField]
	private TMP_InputField prevStartField;

	[SerializeField]
	private TMP_InputField prevDurField;

	[SerializeField]
	private TMP_Dropdown customPlatformsDropdown;

	[SerializeField]
	private TMP_InputField audioPath;

	[SerializeField]
	private TMP_InputField offset;

	[SerializeField]
	private Image revertInfoButtonImage;

	[SerializeField]
	private ContributorsController contributorController;

	[SerializeField]
	private CharacteristicCustomPropertyController characteristicCustomPropertyController;

	private Coroutine reloadSongDataCoroutine;

	public Action TempSongLoadedEvent;

	[SerializeField]
	private GameObject questExportButton;

	private string initialCustomEnvironmentHash;

	private BaseInfo Info => BeatSaberSongContainer.Instance.Info;

	private GameObject ContributorWrapper => contributorController.transform.parent.gameObject;

	private MapExporter exporter => new MapExporter(Info);

	private void Start()
	{
		if (BeatSaberSongContainer.Instance == null)
		{
			SceneManager.LoadScene(0);
			return;
		}
		questExportButton.SetActive(Adb.IsAdbInstalled(out var _));
		ContributorWrapper.SetActive(value: true);
		LoadFromSong();
	}

	public static int GetEnvironmentIDFromString(string environment)
	{
		return VanillaEnvironments.TakeWhile((Environment i) => i.JsonName != environment).Count();
	}

	public static bool TryGetEnvironmentNameFromID(int id, out string environmentName)
	{
		if (id >= VanillaEnvironments.Count)
		{
			environmentName = null;
			return false;
		}
		environmentName = VanillaEnvironments[id].JsonName;
		return true;
	}

	protected override GameObject GetDefault()
	{
		return nameField.gameObject;
	}

	public override void OnLeaveMenu(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ReturnToSongList();
		}
	}

	public void SaveToSong()
	{
		Info.SongName = nameField.text;
		Info.SongSubName = subNameField.text;
		Info.SongAuthorName = songAuthorField.text;
		Info.LevelAuthorName = authorField.text;
		Info.CoverImageFilename = coverImageField.text;
		bool num = Info.SongFilename != audioPath.text;
		bool flag = Info.SongFilename == Info.SongPreviewFilename;
		if (num && flag)
		{
			Info.SongPreviewFilename = audioPath.text;
		}
		string path = Path.Combine(Info.Directory, Info.SongPreviewFilename);
		if (!flag && !File.Exists(path))
		{
			Info.SongPreviewFilename = audioPath.text;
		}
		Info.SongFilename = audioPath.text;
		Info.BeatsPerMinute = GetTextValue(bpmField);
		Info.PreviewStartTime = GetTextValue(prevStartField);
		Info.PreviewDuration = GetTextValue(prevDurField);
		Info.SongTimeOffset = GetTextValue(offset);
		if (Info.SongTimeOffset != 0f)
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "songtimeoffset.warning", null, PersistentUI.DialogBoxPresetType.Ok);
		}
		else
		{
			offset.interactable = false;
		}
		if (Info.CustomData == null)
		{
			Info.CustomData = new JSONObject();
		}
		if (customPlatformsDropdown.value > 0)
		{
			Info.CustomEnvironmentMetadata.Name = customPlatformsDropdown.captionText.text;
			if (CustomPlatformsLoader.Instance.GetAllEnvironments().TryGetValue(customPlatformsDropdown.captionText.text, out var value))
			{
				Info.CustomEnvironmentMetadata.Hash = value.Md5Hash;
			}
			else
			{
				Info.CustomEnvironmentMetadata.Hash = initialCustomEnvironmentHash;
			}
		}
		else
		{
			Info.CustomEnvironmentMetadata.Name = null;
			Info.CustomEnvironmentMetadata.Hash = null;
		}
		contributorController.Commit();
		Info.CustomContributors = contributorController.Contributors;
		characteristicCustomPropertyController.CommitToInfo();
		Info.Save();
		if (previewAudio.clip != null)
		{
			SongListItem.SetDuration(this, Path.GetFullPath(Info.Directory), previewAudio.clip.length);
		}
		coverImageField.GetComponent<InputBoxFileValidator>().OnUpdate();
		audioPath.GetComponent<InputBoxFileValidator>().OnUpdate();
		ReloadAudio();
		PersistentUI.Instance.DisplayMessage("SongEditMenu", "saved", PersistentUI.DisplayMessageType.Bottom);
	}

	public void LoadFromSong()
	{
		nameField.text = Info.SongName;
		subNameField.text = Info.SongSubName;
		songAuthorField.text = Info.SongAuthorName;
		authorField.text = Info.LevelAuthorName;
		songInfoHeaderTitle.text = $"Song Info (v{Info.MajorVersion})";
		if (Info.MajorVersion == 4)
		{
			authorField.placeholder.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "not.supported.in.version";
			authorField.interactable = false;
		}
		BroadcastMessage("OnValidate");
		coverImageField.text = Info.CoverImageFilename;
		audioPath.text = Info.SongFilename;
		offset.text = Info.SongTimeOffset.ToString(CultureInfo.InvariantCulture);
		if (Info.SongTimeOffset != 0f)
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "songtimeoffset.warning", null, PersistentUI.DialogBoxPresetType.Ok);
		}
		else
		{
			offset.interactable = false;
		}
		bpmField.text = Info.BeatsPerMinute.ToString(CultureInfo.InvariantCulture);
		prevStartField.text = Info.PreviewStartTime.ToString(CultureInfo.InvariantCulture);
		prevDurField.text = Info.PreviewDuration.ToString(CultureInfo.InvariantCulture);
		List<string> allEnvironmentIds = CustomPlatformsLoader.Instance.GetAllEnvironmentIds();
		customPlatformsDropdown.ClearOptions();
		customPlatformsDropdown.AddOptions(new List<string> { "None" });
		customPlatformsDropdown.AddOptions(allEnvironmentIds);
		bool num = !string.IsNullOrEmpty(Info.CustomEnvironmentMetadata.Name);
		bool flag = allEnvironmentIds.Contains(Info.CustomEnvironmentMetadata.Name);
		if (num && !flag)
		{
			customPlatformsDropdown.AddOptions(new List<string> { Info.CustomEnvironmentMetadata.Name });
			initialCustomEnvironmentHash = Info.CustomEnvironmentMetadata.Hash;
		}
		customPlatformsDropdown.value = CustomPlatformFromSong();
		contributorController.UndoChanges();
		characteristicCustomPropertyController.UndoChanges();
		ReloadAudio();
	}

	private int CustomPlatformFromSong()
	{
		if (!string.IsNullOrEmpty(Info.CustomEnvironmentMetadata.Name))
		{
			List<string> allEnvironmentIds = CustomPlatformsLoader.Instance.GetAllEnvironmentIds();
			int num = allEnvironmentIds.IndexOf(Info.CustomEnvironmentMetadata.Name);
			if (num == -1)
			{
				return allEnvironmentIds.Count + 1;
			}
			return num + 1;
		}
		return 0;
	}

	public void ReloadAudio()
	{
		StartCoroutine(LoadAudio());
	}

	private IEnumerator LoadAudio(bool useTemp = true, bool applySongTimeOffset = false)
	{
		if (!Directory.Exists(Info.Directory))
		{
			yield break;
		}
		string text = Path.Combine(Info.Directory, useTemp ? audioPath.text : Info.SongFilename);
		Debug.Log("Loading audio");
		if (File.Exists(text))
		{
			if (!FileContentValidationHelper.IsSupportedAudioFormat(text))
			{
				SceneTransitionManager.Instance.CancelLoading("load.error.audio2");
				yield break;
			}
			yield return BeatSaberSongExtensions.LoadAudio(Info, delegate(AudioClip clip)
			{
				previewAudio.clip = clip;
				BeatSaberSongContainer.Instance.LoadedSong = clip;
				BeatSaberSongContainer.Instance.LoadedSongSamples = clip.samples;
				BeatSaberSongContainer.Instance.LoadedSongFrequency = clip.frequency;
				BeatSaberSongContainer.Instance.LoadedSongLength = clip.length;
				if (useTemp)
				{
					TempSongLoadedEvent?.Invoke();
				}
			}, float.Parse(offset.text), useTemp ? audioPath.text : null);
		}
		else
		{
			SceneTransitionManager.Instance.CancelLoading("load.error.audio3");
			Debug.Log("Song does not exist! WTF!?");
			Debug.Log(text);
		}
	}

	public void DeleteMap()
	{
		PersistentUI.Instance.ShowDialogBox("SongEditMenu", "delete.dialog", HandleDeleteMap, PersistentUI.DialogBoxPresetType.YesNo, new object[1] { Info.SongName });
	}

	private void HandleDeleteMap(int result)
	{
		if (result == 0)
		{
			FileOperationAPIWrapper.MoveToRecycleBin(Info.Directory);
			ReturnToSongList();
		}
	}

	public async void ExportToQuest()
	{
		await exporter.ExportToQuest();
	}

	public void PackageZip()
	{
		if (exporter.PackageZip())
		{
			PersistentUI.Instance.DisplayMessage("SongEditMenu", "package.zip.success", PersistentUI.DisplayMessageType.Bottom);
			if (Settings.Instance.OpenFileExplorerAfterCreatingZip)
			{
				exporter.OpenSelectedMapInFileBrowser();
			}
		}
		else
		{
			PersistentUI.Instance.DisplayMessage("SongEditMenu", "package.zip.error", PersistentUI.DisplayMessageType.Bottom);
		}
	}

	public void OpenSelectedMapInFileBrowser()
	{
		exporter.OpenSelectedMapInFileBrowser();
	}

	private void SaveAllFields()
	{
		if (IsDirty())
		{
			SaveToSong();
		}
		if (difficultySelect.IsDirty())
		{
			difficultySelect.SaveAllDiffs();
		}
	}

	public void ReturnToSongList()
	{
		if (!PersistentUI.Instance.DialogBoxIsEnabled)
		{
			CheckForChanges(HandleReturnToSongList);
		}
	}

	public void HandleReturnToSongList(int r)
	{
		if (r == 0)
		{
			SaveAllFields();
		}
		if (r != 2)
		{
			SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu");
		}
	}

	public void EditMapButtonPressed()
	{
		if (BeatSaberSongContainer.Instance.MapDifficultyInfo == null || PersistentUI.Instance.DialogBoxIsEnabled)
		{
			return;
		}
		bool load_Notes = Settings.Instance.Load_Notes;
		bool load_Obstacles = Settings.Instance.Load_Obstacles;
		bool load_Events = Settings.Instance.Load_Events;
		bool load_Others = Settings.Instance.Load_Others;
		if (!(load_Notes || load_Obstacles || load_Events || load_Others))
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "load.warning", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		if (!(load_Notes && load_Obstacles && load_Events && load_Others))
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "load.warning2", null, PersistentUI.DialogBoxPresetType.Ok);
		}
		CheckForChanges(HandleEditMapButtonPressed);
	}

	private void HandleEditMapButtonPressed(int r)
	{
		if (r == 0)
		{
			SaveAllFields();
		}
		if (r == 2)
		{
			return;
		}
		BaseDifficulty currentDiff = difficultySelect.CurrentDiff;
		PersistentUI.UpdateBackground(Info);
		if (currentDiff == null)
		{
			if (File.Exists(Path.Combine(BeatSaberSongContainer.Instance.Info.Directory, BeatSaberSongContainer.Instance.MapDifficultyInfo.BeatmapFileName)))
			{
				PersistentUI.Instance.ShowDialogBox("The selected difficulty could not be parsed.\nThis is either invalid json or an unsupported version.", null, PersistentUI.DialogBoxPresetType.Ok);
			}
			else
			{
				PersistentUI.Instance.ShowDialogBox("The selected difficulty doesn't exist! Have you saved after creating it?", null, PersistentUI.DialogBoxPresetType.Ok);
			}
			return;
		}
		Debug.Log("Transitioning...");
		Settings.Instance.LastLoadedMap = Info.Directory;
		Settings.Instance.LastLoadedChar = BeatSaberSongContainer.Instance.MapDifficultyInfo.Characteristic;
		Settings.Instance.LastLoadedDiff = BeatSaberSongContainer.Instance.MapDifficultyInfo.Difficulty;
		BeatSaberSongContainer.Instance.Map = currentDiff;
		Settings.Instance.MapVersion = currentDiff.MajorVersion;
		currentDiff.ValidateBpmEventsAndObjectTimes(Info.BeatsPerMinute);
		SceneTransitionManager.Instance.LoadScene("03_Mapper", LoadAudio(useTemp: false, applySongTimeOffset: true));
	}

	private bool CheckForChanges(Action<int> callback)
	{
		if (IsDirty())
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "unsaved.warning", callback, PersistentUI.DialogBoxPresetType.YesNoCancel);
			return true;
		}
		if (difficultySelect.IsDirty())
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "unsaveddiff.warning", callback, PersistentUI.DialogBoxPresetType.YesNoCancel);
			return true;
		}
		if (contributorController.IsDirty())
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "unsavedcontributor.warning", callback, PersistentUI.DialogBoxPresetType.YesNoCancel);
			return true;
		}
		if (characteristicCustomPropertyController.IsDirty())
		{
			PersistentUI.Instance.ShowDialogBox("SongEditMenu", "unsavedcharacteristics.warning", callback, PersistentUI.DialogBoxPresetType.YesNoCancel);
			return true;
		}
		callback(0);
		return false;
	}

	public void EditContributors()
	{
		if (!PersistentUI.Instance.DialogBoxIsEnabled)
		{
			GameObject contributorWrapper = ContributorWrapper;
			contributorWrapper.SetActive(!contributorWrapper.activeSelf);
		}
	}

	public void UndoChanges()
	{
		reloadSongDataCoroutine = StartCoroutine(SpinReloadSongDataButton());
		if (ContributorWrapper.activeSelf)
		{
			contributorController.UndoChanges();
		}
		else
		{
			LoadFromSong();
		}
	}

	private IEnumerator SpinReloadSongDataButton()
	{
		if (reloadSongDataCoroutine != null)
		{
			StopCoroutine(reloadSongDataCoroutine);
		}
		float startTime = Time.time;
		Transform transform1 = revertInfoButtonImage.transform;
		Vector3 rotation = transform1.rotation.eulerAngles;
		rotation.z = -330f;
		transform1.rotation = Quaternion.Euler(rotation);
		while (true)
		{
			float z = rotation.z;
			float t = Time.time / startTime * 0.075f;
			z = (rotation.z = Mathf.Lerp(z, 30f, t));
			transform1.rotation = Quaternion.Euler(rotation);
			if (z >= 25f)
			{
				break;
			}
			yield return new WaitForFixedUpdate();
		}
		rotation.z = 30f;
		transform1.rotation = Quaternion.Euler(rotation);
	}

	private static float GetTextValue(TMP_InputField inputfield)
	{
		if (!float.TryParse(inputfield.text, out var result) && !float.TryParse(inputfield.placeholder.GetComponent<TMP_Text>().text, out result))
		{
			return 0f;
		}
		return result;
	}

	private bool IsDirty()
	{
		if (!(Info.SongName != nameField.text) && !(Info.SongSubName != subNameField.text) && !(Info.SongAuthorName != songAuthorField.text) && !(Info.LevelAuthorName != authorField.text) && !(Info.CoverImageFilename != coverImageField.text) && !(Info.SongFilename != audioPath.text) && NearlyEqual(Info.BeatsPerMinute, GetTextValue(bpmField)) && NearlyEqual(Info.PreviewStartTime, GetTextValue(prevStartField)) && NearlyEqual(Info.PreviewDuration, GetTextValue(prevDurField)) && NearlyEqual(Info.SongTimeOffset, GetTextValue(offset)) && customPlatformsDropdown.value == CustomPlatformFromSong() && !contributorController.IsDirty())
		{
			return characteristicCustomPropertyController.IsDirty();
		}
		return true;
	}

	private static bool NearlyEqual(float a, float b, float epsilon = 0.01f)
	{
		if (!a.Equals(b))
		{
			return Math.Abs(a - b) < epsilon;
		}
		return true;
	}
}
