using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class SongCoreFlagController : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown forceOneSaberDropdown;

	[SerializeField]
	private TMP_Dropdown showRotationNoteSpawnLineDropdown;

	[SerializeField]
	private DifficultySelect difficultySelect;

	public bool? ForceOneSaber
	{
		get
		{
			int value = forceOneSaberDropdown.value;
			return value switch
			{
				0 => null, 
				1 => true, 
				2 => false, 
				_ => throw new SwitchExpressionException(value), 
			};
		}
	}

	public bool? ShowRotationNoteSpawnLine
	{
		get
		{
			int value = showRotationNoteSpawnLineDropdown.value;
			return value switch
			{
				0 => null, 
				1 => true, 
				2 => false, 
				_ => throw new SwitchExpressionException(value), 
			};
		}
	}

	public void UpdateFromDiff(bool? forceOneSaber, bool? showRotationNoteSpawnLine)
	{
		TMP_Dropdown tMP_Dropdown = forceOneSaberDropdown;
		int value = (forceOneSaber.HasValue ? ((forceOneSaber == true) ? 1 : 2) : 0);
		tMP_Dropdown.value = value;
		TMP_Dropdown tMP_Dropdown2 = showRotationNoteSpawnLineDropdown;
		value = (showRotationNoteSpawnLine.HasValue ? ((showRotationNoteSpawnLine == true) ? 1 : 2) : 0);
		tMP_Dropdown2.value = value;
	}

	public void UpdateSongCoreFlags()
	{
		difficultySelect.UpdateSongCoreFlags();
	}
}
