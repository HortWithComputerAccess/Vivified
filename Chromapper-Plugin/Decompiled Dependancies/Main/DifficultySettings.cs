using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Info;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using Beatmap.V4;
using SimpleJSON;

public class DifficultySettings
{
	public bool? ForceOneSaber;

	public bool? ShowRotationNoteSpawnLine;

	private List<string> songCoreInfos;

	private List<string> songCoreWarnings;

	private List<BaseEnvironmentEnhancement> envEnhancements;

	private BaseDifficulty map;

	public string CustomName = "";

	public bool ForceDirty;

	public float NoteJumpMovementSpeed = 16f;

	public float NoteJumpStartBeatOffset;

	public int EnvironmentNameIndex;

	public string EnvironmentName;

	public string Mappers;

	public string Lighters;

	public string LightshowFilePath;

	public InfoDifficulty InfoDifficulty { get; }

	public BaseDifficulty Map
	{
		get
		{
			if (map == null)
			{
				map = BeatSaberSongUtils.GetMapFromInfoFiles(BeatSaberSongContainer.Instance.Info, InfoDifficulty);
			}
			return map;
		}
	}

	public List<BaseEnvironmentEnhancement> EnvEnhancements
	{
		get
		{
			if (envEnhancements == null)
			{
				envEnhancements = GetEnvEnhancementsFromMap();
			}
			return envEnhancements;
		}
		set
		{
			envEnhancements = value;
		}
	}

	public List<string> SongCoreInfos
	{
		get
		{
			if (songCoreInfos == null)
			{
				songCoreInfos = InfoDifficulty.CustomInformation.ToList();
			}
			return songCoreInfos;
		}
		set
		{
			songCoreInfos = value;
		}
	}

	public List<string> SongCoreWarnings
	{
		get
		{
			if (songCoreWarnings == null)
			{
				songCoreWarnings = InfoDifficulty.CustomWarnings.ToList();
			}
			return songCoreWarnings;
		}
		set
		{
			songCoreWarnings = value;
		}
	}

	private string EnvironmentNameFromIndex => BeatSaberSongContainer.Instance.Info.EnvironmentNames.ElementAtOrDefault(InfoDifficulty.EnvironmentNameIndex) ?? "DefaultEnvironment";

	public DifficultySettings(InfoDifficulty infoDifficulty)
	{
		InfoDifficulty = infoDifficulty;
		Revert();
	}

	public DifficultySettings(InfoDifficulty infoDifficulty, bool forceDirty)
		: this(infoDifficulty)
	{
		ForceDirty = forceDirty;
	}

	public bool IsDirty()
	{
		if (!ForceDirty && NoteJumpMovementSpeed == InfoDifficulty.NoteJumpSpeed && NoteJumpStartBeatOffset == InfoDifficulty.NoteStartBeatOffset && !(EnvironmentName != EnvironmentNameFromIndex) && !(LightshowFilePath != InfoDifficulty.LightshowFileName) && !(Mappers != string.Join(',', InfoDifficulty.Mappers)) && !(Lighters != string.Join(',', InfoDifficulty.Lighters)) && !((CustomName ?? "") != (InfoDifficulty.CustomLabel ?? "")) && !SongCoreFlagsChanged() && !EnvRemovalChanged())
		{
			return SongCoreInfoWarningsChanged();
		}
		return true;
	}

	private bool SongCoreFlagsChanged()
	{
		if (InfoDifficulty != null)
		{
			if (ForceOneSaber == InfoDifficulty.CustomOneSaberFlag)
			{
				return ShowRotationNoteSpawnLine != InfoDifficulty.CustomShowRotationNoteSpawnLinesFlag;
			}
			return true;
		}
		return false;
	}

	private bool EnvRemovalChanged()
	{
		if (envEnhancements != null && Map != null)
		{
			if (Map.EnvironmentEnhancements.All(envEnhancements.Contains))
			{
				return Map.EnvironmentEnhancements.Count != envEnhancements.Count;
			}
			return true;
		}
		return false;
	}

	private bool SongCoreInfoWarningsChanged()
	{
		if (songCoreInfos != null && songCoreWarnings != null && InfoDifficulty != null)
		{
			if (InfoDifficulty.CustomInformation.SequenceEqual(songCoreInfos))
			{
				return !InfoDifficulty.CustomWarnings.SequenceEqual(songCoreWarnings);
			}
			return true;
		}
		return false;
	}

	public void Commit()
	{
		ForceDirty = false;
		InfoDifficulty.NoteJumpSpeed = NoteJumpMovementSpeed;
		InfoDifficulty.NoteStartBeatOffset = NoteJumpStartBeatOffset;
		InfoDifficulty.Mappers = (from x in Mappers.Split(',')
			select x.Trim()).ToList();
		InfoDifficulty.Lighters = (from x in Lighters.Split(',')
			select x.Trim()).ToList();
		int num = BeatSaberSongContainer.Instance.Info.EnvironmentNames.IndexOf(EnvironmentName);
		if (num >= 0)
		{
			InfoDifficulty.EnvironmentNameIndex = (EnvironmentNameIndex = num);
		}
		else
		{
			BeatSaberSongContainer.Instance.Info.EnvironmentNames.Add(EnvironmentName);
			InfoDifficulty.EnvironmentNameIndex = (EnvironmentNameIndex = BeatSaberSongContainer.Instance.Info.EnvironmentNames.Count);
		}
		string lightshowFileName = InfoDifficulty.LightshowFileName;
		InfoDifficulty.LightshowFileName = LightshowFilePath;
		BaseDifficulty baseDifficulty = Map;
		if (baseDifficulty != null && baseDifficulty.MajorVersion == 4 && lightshowFileName != LightshowFilePath)
		{
			V4Difficulty.LoadLightsFromLightshowFile(Map, BeatSaberSongContainer.Instance.Info, InfoDifficulty);
		}
		InfoDifficulty.CustomLabel = CustomName;
		InfoDifficulty.CustomInformation = SongCoreInfos;
		InfoDifficulty.CustomWarnings = SongCoreWarnings;
		InfoDifficulty.CustomOneSaberFlag = ForceOneSaber;
		InfoDifficulty.CustomShowRotationNoteSpawnLinesFlag = ShowRotationNoteSpawnLine;
		InfoDifficulty.CustomData?.Remove("_environmentRemoval");
		if (EnvRemovalChanged())
		{
			Map.EnvironmentEnhancements = envEnhancements;
			Map.Save();
		}
	}

	private List<BaseEnvironmentEnhancement> GetEnvEnhancementsFromMap()
	{
		List<BaseEnvironmentEnhancement> list = new List<BaseEnvironmentEnhancement>();
		if (InfoDifficulty.CustomData != null)
		{
			JSONNode.Enumerator enumerator = InfoDifficulty.CustomData["_environmentRemoval"].GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current = enumerator.Current;
				list.Add((Settings.Instance.MapVersion == 3) ? V3EnvironmentEnhancement.GetFromJson(current.Value.Value) : V2EnvironmentEnhancement.GetFromJson(current.Value.Value));
			}
		}
		if (Map != null)
		{
			list.AddRange(Map.EnvironmentEnhancements.Select((BaseEnvironmentEnhancement it) => it.Clone() as BaseEnvironmentEnhancement));
		}
		return list;
	}

	public void Revert()
	{
		NoteJumpMovementSpeed = InfoDifficulty.NoteJumpSpeed;
		NoteJumpStartBeatOffset = InfoDifficulty.NoteStartBeatOffset;
		Mappers = string.Join(',', InfoDifficulty.Mappers);
		Lighters = string.Join(',', InfoDifficulty.Lighters);
		EnvironmentNameIndex = InfoDifficulty.EnvironmentNameIndex;
		EnvironmentName = EnvironmentNameFromIndex;
		LightshowFilePath = InfoDifficulty.LightshowFileName;
		CustomName = InfoDifficulty.CustomLabel;
		envEnhancements = null;
		songCoreInfos = null;
		songCoreWarnings = null;
		ForceOneSaber = InfoDifficulty.CustomOneSaberFlag;
		ShowRotationNoteSpawnLine = InfoDifficulty.CustomShowRotationNoteSpawnLinesFlag;
	}
}
