using UnityEngine;
using UnityEngine.Serialization;

public class NoteLanesController : MonoBehaviour
{
	[FormerlySerializedAs("noteGrid")]
	public Transform NoteGrid;

	[SerializeField]
	private GridChild notePlacementGridChild;

	private void Start()
	{
		Settings.NotifyBySettingName("NoteLanes", UpdateNoteLanes);
		UpdateNoteLanes(4);
		if (Settings.NonPersistentSettings.ContainsKey("NoteLanes"))
		{
			Settings.NonPersistentSettings["NoteLanes"] = 4;
		}
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("NoteLanes");
	}

	public void UpdateNoteLanes(object value)
	{
		if (int.TryParse(value.ToString(), out var result) && result >= 4)
		{
			result -= result % 2;
			notePlacementGridChild.Size = result / 2;
			NoteGrid.localScale = new Vector3((float)result / 10f, 1f, NoteGrid.localScale.z);
		}
	}
}
