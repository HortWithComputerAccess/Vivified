using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PastNotesWorker : MonoBehaviour
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[FormerlySerializedAs("notesContainer")]
	[SerializeField]
	private NoteGridContainer noteGridContainer;

	[SerializeField]
	private GameObject gridNotePrefab;

	[SerializeField]
	private BeatmapObjectCallbackController callbackController;

	[SerializeField]
	private NoteAppearanceSO noteAppearance;

	private readonly float gridSize = 25f;

	private readonly Dictionary<int, Dictionary<GameObject, Image>> instantiatedNotes = new Dictionary<int, Dictionary<GameObject, Image>>();

	private readonly Dictionary<int, BaseNote> lastByType = new Dictionary<int, BaseNote>();

	private readonly List<BaseObject> lastGroup = new List<BaseObject>();

	private Canvas canvas;

	private Transform notes;

	private float scale;

	private void Start()
	{
		canvas = GetComponent<Canvas>();
		scale = Settings.Instance.PastNotesGridScale;
		canvas.enabled = scale != 0f;
		base.transform.localScale = Vector3.one * (scale + 0.25f);
		if (scale != 0f)
		{
			BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
			beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(NotePassedThreshold));
			AudioTimeSyncController audioTimeSyncController = atsc;
			audioTimeSyncController.TimeChanged = (Action)Delegate.Combine(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
			notes = base.transform.GetChild(0);
			Settings.NotifyBySettingName("PastNotesGridScale", UpdatePastNotesGridScale);
		}
	}

	private void OnDestroy()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
		beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(NotePassedThreshold));
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.TimeChanged = (Action)Delegate.Remove(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
		Settings.ClearSettingNotifications("PastNotesGridScale");
	}

	private void UpdatePastNotesGridScale(object obj)
	{
		scale = (float)obj;
		canvas.enabled = scale != 0f;
		base.transform.localScale = Vector3.one * (scale + 0.25f);
	}

	private void OnTimeChanged()
	{
		if (atsc.IsPlaying)
		{
			return;
		}
		float num = 0f;
		lastGroup.Clear();
		foreach (BaseNote mapObject in noteGridContainer.MapObjects)
		{
			if (num < mapObject.SongBpmTime && mapObject.SongBpmTime < atsc.CurrentSongBpmTime)
			{
				num = mapObject.SongBpmTime;
				lastGroup.Clear();
				if (mapObject.Type != 3)
				{
					lastGroup.Add(mapObject);
				}
			}
			else if (num == mapObject.SongBpmTime && mapObject.Type != 3)
			{
				lastGroup.Add(mapObject);
			}
		}
		foreach (BaseObject item in lastGroup)
		{
			NotePassedThreshold(natural: false, 0, item);
		}
	}

	private void NotePassedThreshold(bool natural, int id, BaseObject obj)
	{
		BaseNote baseNote = obj as BaseNote;
		if (!instantiatedNotes.ContainsKey(baseNote.Type))
		{
			instantiatedNotes.Add(baseNote.Type, new Dictionary<GameObject, Image>());
		}
		if (lastByType.TryGetValue(baseNote.Type, out var value) && value.JsonTime != obj.JsonTime)
		{
			foreach (KeyValuePair<GameObject, Image> item in instantiatedNotes[baseNote.Type])
			{
				item.Key.SetActive(value: false);
			}
		}
		if (baseNote.Type == 3)
		{
			return;
		}
		float num = baseNote.PosX;
		float num2 = baseNote.PosY;
		if (baseNote.CustomCoordinate != null)
		{
			Vector2 vector = baseNote.CustomCoordinate;
			num = vector.x + 2f;
			num2 = vector.y;
		}
		else
		{
			if (num >= 1000f)
			{
				num = num / 1000f - 1f;
			}
			else if (num <= -1000f)
			{
				num = num / 1000f + 1f;
			}
			if (num2 >= 1000f)
			{
				num2 = num2 / 1000f - 1f;
			}
			else if (num2 <= -1000f)
			{
				num2 = num2 / 1000f + 1f;
			}
		}
		Vector3 position = new Vector3(gridSize * num, gridSize * num2, 1f);
		if (instantiatedNotes[baseNote.Type].Any((KeyValuePair<GameObject, Image> x) => x.Key.activeSelf && x.Value.transform.localPosition == position))
		{
			return;
		}
		GameObject gameObject;
		Image image;
		if (instantiatedNotes[baseNote.Type].Any((KeyValuePair<GameObject, Image> x) => !x.Key.activeSelf))
		{
			gameObject = instantiatedNotes[baseNote.Type].First((KeyValuePair<GameObject, Image> x) => !x.Key.activeSelf).Key;
			image = instantiatedNotes[baseNote.Type][gameObject];
			gameObject.SetActive(value: true);
			gameObject.transform.SetSiblingIndex(gameObject.transform.parent.childCount);
			foreach (Transform item2 in gameObject.transform)
			{
				item2.gameObject.SetActive(value: true);
			}
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(gridNotePrefab, notes.transform, worldPositionStays: true);
			image = gameObject.GetComponent<Image>();
			instantiatedNotes[baseNote.Type].Add(gameObject, image);
		}
		Transform obj2 = image.transform;
		obj2.localPosition = position;
		float num3 = scale / 10f + 0.06f;
		obj2.localScale = new Vector3(num3, num3);
		obj2.localEulerAngles = Vector3.forward * NoteContainer.Directionalize(baseNote).z;
		image.color = ((baseNote.Type == 0) ? noteAppearance.RedColor : noteAppearance.BlueColor);
		if (baseNote.CutDirection == 8)
		{
			gameObject.transform.GetChild(0).gameObject.SetActive(value: false);
		}
		else
		{
			gameObject.transform.GetChild(1).gameObject.SetActive(value: false);
		}
		image.enabled = true;
		lastByType[baseNote.Type] = baseNote;
	}
}
