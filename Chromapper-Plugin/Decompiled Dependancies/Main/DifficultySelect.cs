using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beatmap.Base;
using Beatmap.Info;
using Beatmap.V4;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DifficultySelect : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField njsField;

	[SerializeField]
	private TMP_InputField songBeatOffsetField;

	[SerializeField]
	private TMP_Dropdown environmentDropdown;

	[SerializeField]
	private TMP_InputField mappersField;

	[SerializeField]
	private TMP_InputField lightersField;

	[SerializeField]
	private TMP_InputField lightshowFilePathField;

	[SerializeField]
	private CharacteristicSelect characteristicSelect;

	[SerializeField]
	private Color copyColor;

	[SerializeField]
	private EnvRemoval envRemoval;

	[SerializeField]
	private SongCoreInformation songCoreInformation;

	[SerializeField]
	private SongCoreInformation songCoreWarning;

	[SerializeField]
	private SongCoreFlagController songCoreFlagController;

	[SerializeField]
	private Button openEditorButton;

	private readonly HashSet<DifficultyRow> rows = new HashSet<DifficultyRow>();

	private readonly Dictionary<string, string> selectedMemory = new Dictionary<string, string>();

	public Dictionary<string, Dictionary<string, DifficultySettings>> Characteristics;

	private CopySource copySource;

	private Dictionary<string, DifficultySettings> diffs;

	private InfoDifficultySet currentDifficultySet;

	private bool loading;

	private DifficultyRow selected;

	private List<string> environmentNames = new List<string>();

	public BaseDifficulty CurrentDiff
	{
		get
		{
			if (selected == null)
			{
				return null;
			}
			return diffs[selected.Name].Map;
		}
	}

	private BaseInfo MapInfo
	{
		get
		{
			if (!(BeatSaberSongContainer.Instance != null))
			{
				return null;
			}
			return BeatSaberSongContainer.Instance.Info;
		}
	}

	public void Start()
	{
		environmentDropdown.ClearOptions();
		environmentNames.Clear();
		environmentNames.AddRange(SongInfoEditUI.VanillaEnvironments.Select((SongInfoEditUI.Environment it) => it.JsonName).ToList());
		if (MapInfo?.DifficultySets != null)
		{
			Characteristics = (from it in MapInfo.DifficultySets
				group it by it.Characteristic).ToDictionary((IGrouping<string, InfoDifficultySet> characteristic) => characteristic.Key, (IGrouping<string, InfoDifficultySet> characteristic) => (from map in characteristic.SelectMany((InfoDifficultySet i) => i.Difficulties)
				group map by map.Difficulty).ToDictionary((IGrouping<string, InfoDifficulty> grouped) => grouped.Key, (IGrouping<string, InfoDifficulty> grouped) => new DifficultySettings(grouped.First())), StringComparer.OrdinalIgnoreCase);
			foreach (DifficultySettings item in Characteristics.Values.SelectMany((Dictionary<string, DifficultySettings> c) => c.Values))
			{
				if (item.InfoDifficulty.EnvironmentNameIndex == -1)
				{
					item.InfoDifficulty.EnvironmentNameIndex = 0;
				}
				item.EnvironmentNameIndex = item.InfoDifficulty.EnvironmentNameIndex;
				item.EnvironmentName = MapInfo.EnvironmentNames.ElementAtOrDefault(item.InfoDifficulty.EnvironmentNameIndex) ?? "DefaultEnvironment";
			}
			if (!SongInfoEditUI.VanillaEnvironments.Any((SongInfoEditUI.Environment env) => env.JsonName == MapInfo.EnvironmentName))
			{
				environmentNames.Add(MapInfo.EnvironmentName);
			}
			foreach (string environmentName in MapInfo.EnvironmentNames)
			{
				if (!environmentNames.Any((string env) => env == environmentName))
				{
					environmentNames.Add(environmentName);
				}
			}
			if (MapInfo.MajorVersion == 4)
			{
				lightshowFilePathField.interactable = true;
				mappersField.interactable = true;
				lightersField.interactable = true;
			}
			else
			{
				lightshowFilePathField.placeholder.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "not.supported.in.version";
				lightshowFilePathField.interactable = false;
				mappersField.placeholder.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "not.supported.in.version";
				mappersField.interactable = false;
				lightersField.placeholder.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "not.supported.in.version";
				lightersField.interactable = false;
			}
		}
		else
		{
			Characteristics = new Dictionary<string, Dictionary<string, DifficultySettings>>();
		}
		environmentDropdown.AddOptions(environmentNames);
		environmentDropdown.value = 0;
		foreach (Transform item2 in base.transform)
		{
			DifficultyRow row = new DifficultyRow(item2);
			rows.Add(row);
			row.Toggle.onValueChanged.AddListener(delegate(bool val)
			{
				OnChange(row, val);
			});
			row.Button.onClick.AddListener(delegate
			{
				OnClick(row);
			});
			row.NameInput.onValueChanged.AddListener(delegate(string name)
			{
				OnValueChanged(row, name);
			});
			row.Copy.onClick.AddListener(delegate
			{
				SetCopySource(row);
			});
			row.Save.onClick.AddListener(delegate
			{
				SaveDiff(row);
			});
			row.Revert.onClick.AddListener(delegate
			{
				Revertdiff(row);
			});
			row.Paste.onClick.AddListener(delegate
			{
				DoPaste(row);
			});
		}
	}

	public void UpdateOffset()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			if (float.TryParse(songBeatOffsetField.text, out var result))
			{
				difficultySettings.NoteJumpStartBeatOffset = result;
			}
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateNJS()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			if (float.TryParse(njsField.text, out var result))
			{
				difficultySettings.NoteJumpMovementSpeed = result;
			}
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateEnvironment()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.EnvironmentName = environmentDropdown.options[environmentDropdown.value].text;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateLightshowFilePath()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.LightshowFilePath = lightshowFilePathField.text;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateMappers()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.Mappers = mappersField.text;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateLighters()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.Lighters = lightersField.text;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateEnvRemoval()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.EnvEnhancements = envRemoval.EnvRemovalList;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateCustomWarnings()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.SongCoreWarnings = songCoreWarning.InfoList;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateSongCoreInformation()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.SongCoreInfos = songCoreInformation.InfoList;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	public void UpdateSongCoreFlags()
	{
		if (selected != null && diffs.ContainsKey(selected.Name))
		{
			DifficultySettings difficultySettings = diffs[selected.Name];
			difficultySettings.ForceOneSaber = songCoreFlagController.ForceOneSaber;
			difficultySettings.ShowRotationNoteSpawnLine = songCoreFlagController.ShowRotationNoteSpawnLine;
			selected.ShowDirtyObjects(difficultySettings);
		}
	}

	private void Revertdiff(DifficultyRow row)
	{
		DifficultySettings difficultySettings = diffs[row.Name];
		difficultySettings.Revert();
		row.NameInput.text = difficultySettings.CustomName;
		if (row == selected)
		{
			njsField.text = difficultySettings.NoteJumpMovementSpeed.ToString();
			songBeatOffsetField.text = difficultySettings.NoteJumpStartBeatOffset.ToString();
			mappersField.text = difficultySettings.Mappers;
			lightersField.text = difficultySettings.Lighters;
			lightshowFilePathField.text = difficultySettings.LightshowFilePath;
			environmentDropdown.value = environmentNames.IndexOf(difficultySettings.EnvironmentName);
			envRemoval.UpdateFromDiff(difficultySettings.EnvEnhancements);
			songCoreInformation.UpdateFromDiff(difficultySettings.SongCoreInfos);
			songCoreWarning.UpdateFromDiff(difficultySettings.SongCoreWarnings);
			songCoreFlagController.UpdateFromDiff(difficultySettings.ForceOneSaber, difficultySettings.ShowRotationNoteSpawnLine);
		}
		row.ShowDirtyObjects(difficultySettings);
	}

	private BaseDifficulty TryGetExistingMapFromDiff(DifficultySettings diff)
	{
		try
		{
			return diff.Map;
		}
		catch (Exception)
		{
		}
		return null;
	}

	public void SaveAllDiffs()
	{
		foreach (DifficultyRow row in rows)
		{
			if (diffs.ContainsKey(row.Name))
			{
				SaveDiff(row);
			}
		}
	}

	private void SaveDiff(DifficultyRow row)
	{
		BaseInfo info = BeatSaberSongContainer.Instance.Info;
		if (!Directory.Exists(info.Directory))
		{
			info.Save();
		}
		DifficultySettings difficultySettings = diffs[row.Name];
		bool forceDirty = difficultySettings.ForceDirty;
		difficultySettings.Commit();
		row.ShowDirtyObjects(show: false, copy: true);
		InfoDifficulty infoDifficulty = difficultySettings.InfoDifficulty;
		if (!info.DifficultySets.Contains(currentDifficultySet))
		{
			info.DifficultySets.Add(currentDifficultySet);
		}
		if (!currentDifficultySet.Difficulties.Contains(infoDifficulty))
		{
			currentDifficultySet.Difficulties.Add(infoDifficulty);
		}
		BaseDifficulty baseDifficulty = TryGetExistingMapFromDiff(difficultySettings);
		if (baseDifficulty == null)
		{
			baseDifficulty = new BaseDifficulty
			{
				Version = ((info.MajorVersion == 4) ? "4.1.0" : "3.3.0")
			};
			Settings.Instance.MapVersion = baseDifficulty.MajorVersion;
			if (baseDifficulty.MajorVersion == 4)
			{
				V4Difficulty.LoadBpmFromAudioData(baseDifficulty, info);
				V4Difficulty.LoadLightsFromLightshowFile(baseDifficulty, info, infoDifficulty);
			}
		}
		string directoryAndFile = baseDifficulty.DirectoryAndFile;
		infoDifficulty.InitDefaultFileNames(info.MajorVersion);
		baseDifficulty.DirectoryAndFile = Path.Combine(info.Directory, infoDifficulty.BeatmapFileName);
		if (File.Exists(directoryAndFile) && directoryAndFile != baseDifficulty.DirectoryAndFile && !File.Exists(baseDifficulty.DirectoryAndFile))
		{
			if (forceDirty)
			{
				File.Copy(directoryAndFile, baseDifficulty.DirectoryAndFile);
			}
			else
			{
				File.Move(directoryAndFile, baseDifficulty.DirectoryAndFile);
			}
		}
		else
		{
			Settings.Instance.MapVersion = baseDifficulty.MajorVersion;
			baseDifficulty.Save();
		}
		infoDifficulty.RefreshRequirementsAndWarnings(baseDifficulty);
		List<string> list = new List<string>();
		foreach (DifficultySettings item in Characteristics.Values.SelectMany((Dictionary<string, DifficultySettings> c) => c.Values))
		{
			int num = list.IndexOf(item.EnvironmentName);
			if (num == -1)
			{
				list.Add(item.EnvironmentName);
				item.InfoDifficulty.EnvironmentNameIndex = list.Count - 1;
			}
			else
			{
				item.InfoDifficulty.EnvironmentNameIndex = num;
			}
		}
		info.EnvironmentNames = list;
		info.Save();
		characteristicSelect.Recalculate();
		Debug.Log("Saved " + row.Name);
	}

	private void OnValueChanged(DifficultyRow row, string difficultyLabel)
	{
		if (diffs.ContainsKey(row.Name))
		{
			DifficultySettings difficultySettings = diffs[row.Name];
			string text = ((row.Name == "ExpertPlus") ? "Expert+" : row.Name);
			if (difficultyLabel != "" && difficultyLabel != text)
			{
				difficultySettings.CustomName = difficultyLabel;
			}
			else
			{
				difficultySettings.CustomName = null;
			}
			row.ShowDirtyObjects(difficultySettings);
		}
	}

	private void DeselectDiff()
	{
		if (selected != null)
		{
			Image background = selected.Background;
			background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
			BeatSaberSongContainer.Instance.MapDifficultyInfo = null;
			njsField.text = "";
			songBeatOffsetField.text = "";
			mappersField.SetTextWithoutNotify("");
			lightersField.SetTextWithoutNotify("");
			lightshowFilePathField.SetTextWithoutNotify("");
			environmentDropdown.SetValueWithoutNotify(0);
			envRemoval.ClearList();
			songCoreInformation.ClearList();
			songCoreWarning.ClearList();
		}
		selected = null;
		openEditorButton.interactable = false;
	}

	public void OnClick(Transform obj)
	{
		DifficultyRow difficultyRow = rows.First((DifficultyRow it) => it.Obj == obj);
		if (difficultyRow != null)
		{
			OnClick(difficultyRow);
		}
	}

	private void OnClick(DifficultyRow row)
	{
		if (!diffs.ContainsKey(row.Name))
		{
			return;
		}
		DeselectDiff();
		selected = row;
		openEditorButton.interactable = true;
		if (!loading)
		{
			selectedMemory[currentDifficultySet.Characteristic] = selected.Name;
		}
		Image background = selected.Background;
		background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);
		DifficultySettings difficultySettings = diffs[row.Name];
		BeatSaberSongContainer.Instance.MapDifficultyInfo = difficultySettings.InfoDifficulty;
		njsField.text = difficultySettings.NoteJumpMovementSpeed.ToString();
		songBeatOffsetField.text = difficultySettings.NoteJumpStartBeatOffset.ToString();
		mappersField.text = difficultySettings.Mappers;
		lightersField.text = difficultySettings.Lighters;
		BaseInfo info = BeatSaberSongContainer.Instance.Info;
		if (difficultySettings.Map != null || info.MajorVersion != 4)
		{
			BaseDifficulty map = difficultySettings.Map;
			if (map == null || map.MajorVersion != 4)
			{
				lightshowFilePathField.placeholder.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "not.supported.in.version";
				lightshowFilePathField.interactable = false;
				goto IL_0189;
			}
		}
		lightshowFilePathField.interactable = true;
		lightshowFilePathField.text = difficultySettings.LightshowFilePath;
		goto IL_0189;
		IL_0189:
		environmentDropdown.value = environmentNames.IndexOf(difficultySettings.EnvironmentName);
		envRemoval.UpdateFromDiff(difficultySettings.EnvEnhancements);
		songCoreInformation.UpdateFromDiff(difficultySettings.SongCoreInfos);
		songCoreWarning.UpdateFromDiff(difficultySettings.SongCoreWarnings);
		songCoreFlagController.UpdateFromDiff(difficultySettings.ForceOneSaber, difficultySettings.ShowRotationNoteSpawnLine);
	}

	private void DoPaste(DifficultyRow row)
	{
		row.Toggle.isOn = true;
	}

	private void OnChange(DifficultyRow row, bool val)
	{
		if (!val && diffs.ContainsKey(row.Name))
		{
			if (diffs[row.Name].ForceDirty)
			{
				if (row == selected)
				{
					DeselectDiff();
				}
				diffs.Remove(row.Name);
				row.SetInteractable(val: false);
				row.NameInput.text = "";
				row.ShowDirtyObjects(show: false, copy: false);
			}
			else
			{
				PersistentUI.Instance.ShowDialogBox("SongEditMenu", "deletediff.dialog", delegate(int r)
				{
					HandleDeleteDifficulty(row, r);
				}, PersistentUI.DialogBoxPresetType.YesNo, new object[1] { diffs[row.Name].InfoDifficulty.Difficulty });
			}
		}
		else if (val && !diffs.ContainsKey(row.Name))
		{
			InfoDifficulty infoDifficulty = new InfoDifficulty(currentDifficultySet)
			{
				Difficulty = row.Name
			};
			infoDifficulty.InitDefaultFileNames(MapInfo.MajorVersion);
			if (copySource != null)
			{
				DifficultySettings difficultySettings = copySource.DifficultySettings;
				CancelCopy();
				if (difficultySettings != null)
				{
					infoDifficulty.NoteJumpSpeed = difficultySettings.InfoDifficulty.NoteJumpSpeed;
					infoDifficulty.NoteStartBeatOffset = difficultySettings.InfoDifficulty.NoteStartBeatOffset;
					infoDifficulty.Mappers = difficultySettings.InfoDifficulty.Mappers.ToList();
					infoDifficulty.Lighters = difficultySettings.InfoDifficulty.Lighters.ToList();
					infoDifficulty.ColorSchemeIndex = difficultySettings.InfoDifficulty.ColorSchemeIndex;
					infoDifficulty.EnvironmentNameIndex = difficultySettings.InfoDifficulty.EnvironmentNameIndex;
					infoDifficulty.LightshowFileName = difficultySettings.InfoDifficulty.LightshowFileName;
					infoDifficulty.CustomData = difficultySettings.InfoDifficulty.CustomData?.Clone().AsObject;
					infoDifficulty.CustomLabel = difficultySettings.InfoDifficulty.CustomLabel;
					infoDifficulty.CustomOneSaberFlag = difficultySettings.InfoDifficulty.CustomOneSaberFlag;
					infoDifficulty.CustomShowRotationNoteSpawnLinesFlag = difficultySettings.InfoDifficulty.CustomShowRotationNoteSpawnLinesFlag;
					infoDifficulty.CustomInformation = difficultySettings.InfoDifficulty.CustomInformation.ToList();
					infoDifficulty.CustomWarnings = difficultySettings.InfoDifficulty.CustomWarnings.ToList();
					infoDifficulty.CustomSuggestions = difficultySettings.InfoDifficulty.CustomSuggestions.ToList();
					infoDifficulty.CustomRequirements = difficultySettings.InfoDifficulty.CustomRequirements.ToList();
					infoDifficulty.CustomColorLeft = difficultySettings.InfoDifficulty.CustomColorLeft;
					infoDifficulty.CustomColorRight = difficultySettings.InfoDifficulty.CustomColorRight;
					infoDifficulty.CustomEnvColorLeft = difficultySettings.InfoDifficulty.CustomEnvColorLeft;
					infoDifficulty.CustomEnvColorRight = difficultySettings.InfoDifficulty.CustomEnvColorRight;
					infoDifficulty.CustomEnvColorWhite = difficultySettings.InfoDifficulty.CustomEnvColorWhite;
					infoDifficulty.CustomColorObstacle = difficultySettings.InfoDifficulty.CustomColorObstacle;
					infoDifficulty.CustomEnvColorBoostLeft = difficultySettings.InfoDifficulty.CustomEnvColorBoostLeft;
					infoDifficulty.CustomEnvColorBoostRight = difficultySettings.InfoDifficulty.CustomEnvColorBoostRight;
					infoDifficulty.CustomEnvColorBoostWhite = difficultySettings.InfoDifficulty.CustomEnvColorBoostWhite;
					infoDifficulty.BeatmapFileName = difficultySettings.InfoDifficulty.BeatmapFileName;
					if (difficultySettings.Map.MajorVersion == 4)
					{
						string text = Path.Combine(MapInfo.Directory, "Bookmarks", difficultySettings.InfoDifficulty.BookmarkFileName);
						string destFileName = Path.Combine(MapInfo.Directory, "Bookmarks", infoDifficulty.BookmarkFileName);
						if (File.Exists(text))
						{
							File.Copy(text, destFileName, overwrite: true);
						}
					}
				}
			}
			diffs[row.Name] = new DifficultySettings(infoDifficulty, forceDirty: true);
			if (!string.IsNullOrEmpty(diffs[row.Name].CustomName))
			{
				diffs[row.Name].CustomName += " (Copy)";
			}
			row.NameInput.text = diffs[row.Name].CustomName;
			row.ShowDirtyObjects(diffs[row.Name]);
			row.SetInteractable(val: true);
			OnClick(row);
		}
		else if (val)
		{
			row.ShowDirtyObjects(diffs[row.Name]);
			row.SetInteractable(val: true);
			if (!loading)
			{
				OnClick(row);
			}
		}
	}

	private void HandleDeleteDifficulty(DifficultyRow row, int r)
	{
		if (r == 1)
		{
			row.Toggle.isOn = true;
			return;
		}
		InfoDifficulty infoDifficulty = diffs[row.Name].InfoDifficulty;
		string path = Path.Combine(MapInfo.Directory, infoDifficulty.BeatmapFileName);
		if (File.Exists(path))
		{
			FileOperationAPIWrapper.MoveToRecycleBin(path);
		}
		string path2 = Path.Combine(MapInfo.Directory, "Bookmarks", infoDifficulty.BookmarkFileName);
		if (File.Exists(path2))
		{
			FileOperationAPIWrapper.MoveToRecycleBin(path2);
		}
		if (copySource != null && row == copySource.Obj && currentDifficultySet == copySource.CharacteristicSet)
		{
			CancelCopy();
		}
		if (row == selected)
		{
			DeselectDiff();
		}
		currentDifficultySet.Difficulties.Remove(diffs[row.Name].InfoDifficulty);
		if (currentDifficultySet.Difficulties.Count == 0)
		{
			MapInfo.DifficultySets.Remove(currentDifficultySet);
		}
		diffs.Remove(row.Name);
		MapInfo.Save();
		row.SetInteractable(val: false);
		row.NameInput.text = "";
		row.ShowDirtyObjects(show: false, copy: false);
		characteristicSelect.Recalculate();
	}

	private void SetCopySource(DifficultyRow row)
	{
		if (copySource != null && currentDifficultySet == copySource.CharacteristicSet)
		{
			copySource.Obj.CopyImage.color = Color.white;
		}
		if (copySource != null && copySource.Obj == row && currentDifficultySet == copySource.CharacteristicSet)
		{
			CancelCopy();
			return;
		}
		copySource = new CopySource(diffs[row.Name], currentDifficultySet, row);
		SetPasteMode(mode: true);
		row.CopyImage.color = copyColor;
	}

	public void CancelCopy()
	{
		if (copySource != null && currentDifficultySet == copySource.CharacteristicSet)
		{
			copySource.Obj.CopyImage.color = Color.white;
		}
		copySource = null;
		SetPasteMode(mode: false);
	}

	public void SetCharacteristic(string name, bool firstLoad = false)
	{
		DeselectDiff();
		currentDifficultySet = MapInfo?.DifficultySets?.Find((InfoDifficultySet it) => it.Characteristic.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		if (currentDifficultySet == null)
		{
			currentDifficultySet = new InfoDifficultySet
			{
				Characteristic = name
			};
		}
		if (!Characteristics.ContainsKey(name))
		{
			Characteristics.Add(name, new Dictionary<string, DifficultySettings>());
		}
		diffs = Characteristics[name];
		loading = true;
		selectedMemory.TryGetValue(name, out var _);
		foreach (DifficultyRow row in rows)
		{
			bool flag = diffs.ContainsKey(row.Name);
			row.SetInteractable(diffs.ContainsKey(row.Name));
			row.CopyImage.color = ((copySource != null && currentDifficultySet == copySource.CharacteristicSet && copySource.Obj == row) ? copyColor : Color.white);
			row.NameInput.text = (flag ? diffs[row.Name].CustomName : "");
			if (flag)
			{
				row.ShowDirtyObjects(diffs[row.Name]);
				string value2;
				if (firstLoad && Settings.Instance.LastLoadedMap.Equals(MapInfo?.Directory) && Settings.Instance.LastLoadedDiff.Equals(row.Name))
				{
					selectedMemory[name] = row.Name;
					OnClick(row);
				}
				else if (selected == null || (!firstLoad && selectedMemory.TryGetValue(name, out value2) && row.Name.Equals(value2)))
				{
					OnClick(row);
				}
			}
			else
			{
				row.ShowDirtyObjects(show: false, copy: false);
			}
		}
		loading = false;
		SetPasteMode(copySource != null);
		if (selected == null)
		{
			njsField.text = "";
			songBeatOffsetField.text = "";
			mappersField.text = "";
			lightersField.text = "";
			envRemoval.ClearList();
			songCoreInformation.ClearList();
			songCoreWarning.ClearList();
		}
	}

	private void SetPasteMode(bool mode)
	{
		foreach (Transform item in base.transform)
		{
			bool flag = mode && !diffs.ContainsKey(item.name);
			item.Find("Paste").gameObject.SetActive(flag);
			item.Find("Button/Toggle").gameObject.SetActive(!flag);
		}
	}

	public bool IsDirty()
	{
		return Characteristics.Any((KeyValuePair<string, Dictionary<string, DifficultySettings>> it) => it.Value.Any((KeyValuePair<string, DifficultySettings> diff) => diff.Value.IsDirty()));
	}
}
