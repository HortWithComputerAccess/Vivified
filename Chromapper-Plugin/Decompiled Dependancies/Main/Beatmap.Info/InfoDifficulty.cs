using System.Collections.Generic;
using Beatmap.Base;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Info;

public class InfoDifficulty
{
	private float noteJumpSpeed;

	public InfoDifficultySet ParentSet { get; }

	public string Characteristic => ParentSet.Characteristic;

	public string CustomCharacteristicLabel => ParentSet.CustomCharacteristicLabel;

	public string CustomCharacteristicIconImageFileName => ParentSet.CustomCharacteristicIconImageFileName;

	public string BeatmapFileName { get; set; }

	public string LightshowFileName { get; set; }

	public string Difficulty { get; set; }

	public int DifficultyRank => Difficulty switch
	{
		"ExpertPlus" => 9, 
		"Expert+" => 9, 
		"Expert" => 7, 
		"Hard" => 5, 
		"Normal" => 3, 
		"Easy" => 1, 
		_ => -1, 
	};

	public int EnvironmentNameIndex { get; set; }

	public int ColorSchemeIndex { get; set; }

	public float NoteJumpSpeed
	{
		get
		{
			if (noteJumpSpeed == 0f)
			{
				return DifficultyRank switch
				{
					9 => 16, 
					7 => 12, 
					_ => 10, 
				};
			}
			return noteJumpSpeed;
		}
		set
		{
			noteJumpSpeed = value;
		}
	}

	public float NoteStartBeatOffset { get; set; }

	public List<string> Mappers { get; set; } = new List<string>();

	public List<string> Lighters { get; set; } = new List<string>();

	public string BookmarkFileName => "ChroMapper." + Characteristic + Difficulty + ".bookmarks.dat";

	public JSONObject CustomData { get; set; } = new JSONObject();

	public string CustomLabel { get; set; } = "";

	public bool? CustomOneSaberFlag { get; set; }

	public bool? CustomShowRotationNoteSpawnLinesFlag { get; set; }

	public List<string> CustomInformation { get; set; } = new List<string>();

	public List<string> CustomWarnings { get; set; } = new List<string>();

	public List<string> CustomSuggestions { get; set; } = new List<string>();

	public List<string> CustomRequirements { get; set; } = new List<string>();

	public Color? CustomColorLeft { get; set; }

	public Color? CustomColorRight { get; set; }

	public Color? CustomColorObstacle { get; set; }

	public Color? CustomEnvColorLeft { get; set; }

	public Color? CustomEnvColorRight { get; set; }

	public Color? CustomEnvColorWhite { get; set; }

	public Color? CustomEnvColorBoostLeft { get; set; }

	public Color? CustomEnvColorBoostRight { get; set; }

	public Color? CustomEnvColorBoostWhite { get; set; }

	public InfoDifficulty(InfoDifficultySet parentSet)
	{
		ParentSet = parentSet;
	}

	public void InitDefaultFileNames(int infoMajorVersion)
	{
		BeatmapFileName = Difficulty + Characteristic + ".dat";
		if (infoMajorVersion == 4 && string.IsNullOrWhiteSpace(LightshowFileName))
		{
			LightshowFileName = "Lightshow.dat";
		}
	}

	public void RefreshRequirementsAndWarnings(BaseDifficulty map)
	{
		if (!Settings.Instance.AutomaticModRequirements)
		{
			return;
		}
		foreach (RequirementCheck req in RequirementCheck.requirementsAndSuggestions)
		{
			RequirementCheck.RequirementType requirementType = req.IsRequiredOrSuggested(this, map);
			CustomRequirements.RemoveAll((string x) => x.Equals(req.Name));
			CustomSuggestions.RemoveAll((string x) => x.Equals(req.Name));
			switch (requirementType)
			{
			case RequirementCheck.RequirementType.Requirement:
				CustomRequirements.Add(req.Name);
				break;
			case RequirementCheck.RequirementType.Suggestion:
				CustomSuggestions.Add(req.Name);
				break;
			}
		}
	}
}
