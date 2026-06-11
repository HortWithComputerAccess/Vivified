using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MeasureLinesController : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI measureLinePrefab;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private RectTransform parent;

	[SerializeField]
	private Transform noteGrid;

	[SerializeField]
	private Transform frontNoteGridScaling;

	[SerializeField]
	private Transform measureLineGrid;

	[SerializeField]
	private BPMChangeGridContainer bpmChangeGridContainer;

	[SerializeField]
	private GridChild measureLinesGridChild;

	[SerializeField]
	private BookmarkRenderingController bookmarkRenderingController;

	private readonly List<(float, TextMeshProUGUI)> measureTextsByBeat = new List<(float, TextMeshProUGUI)>();

	private bool init;

	private float previousAtscBeat = -1f;

	private void Start()
	{
		if (!measureTextsByBeat.Any())
		{
			measureTextsByBeat.Add((0f, measureLinePrefab));
		}
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Combine(EditorScaleController.EditorScaleChangedEvent, new Action<float>(EditorScaleUpdated));
	}

	private void LateUpdate()
	{
		if (atsc.CurrentSongBpmTime != previousAtscBeat && init)
		{
			previousAtscBeat = atsc.CurrentSongBpmTime;
			RefreshVisibility();
		}
	}

	private void OnDestroy()
	{
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Remove(EditorScaleController.EditorScaleChangedEvent, new Action<float>(EditorScaleUpdated));
	}

	private void EditorScaleUpdated(float obj)
	{
		RefreshPositions();
	}

	public void RefreshMeasureLines()
	{
		Debug.Log("Refreshing measure lines...");
		init = false;
		Queue<TextMeshProUGUI> queue = new Queue<TextMeshProUGUI>(measureTextsByBeat.Select(((float, TextMeshProUGUI) x) => x.Item2));
		measureTextsByBeat.Clear();
		BeatSaberSongContainer instance = BeatSaberSongContainer.Instance;
		int num = Mathf.FloorToInt(atsc.GetBeatFromSeconds(instance.LoadedSong.length));
		int b = Mathf.FloorToInt(instance.Map.SongBpmTimeToJsonTime(num).Value);
		b = Mathf.Min(num * 10, b);
		int num2;
		for (num2 = 0; num2 <= b; num2++)
		{
			TextMeshProUGUI textMeshProUGUI = ((queue.Count > 0) ? queue.Dequeue() : UnityEngine.Object.Instantiate(measureLinePrefab, parent));
			textMeshProUGUI.text = $"{num2}";
			float num3 = instance.Map.JsonTimeToSongBpmTime(num2).Value;
			textMeshProUGUI.transform.localPosition = new Vector3(0f, num3 * EditorScaleController.EditorScale, 0f);
			measureTextsByBeat.Add((num3, textMeshProUGUI));
		}
		measureLinesGridChild.Size = ((num2 > 1000) ? 1 : 0);
		foreach (TextMeshProUGUI item in queue)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		init = true;
		RefreshVisibility();
		RefreshPositions();
	}

	private void RefreshVisibility()
	{
		float currentSongBpmTime = atsc.CurrentSongBpmTime;
		float num = frontNoteGridScaling.localScale.z / EditorScaleController.EditorScale;
		float num2 = num / 4f;
		foreach (var item3 in measureTextsByBeat)
		{
			float item = item3.Item1;
			TextMeshProUGUI item2 = item3.Item2;
			bool active = item >= currentSongBpmTime - num2 && item <= currentSongBpmTime + num;
			item2.gameObject.SetActive(active);
		}
		bookmarkRenderingController.RefreshVisibility(currentSongBpmTime, num, num2);
	}

	private void RefreshPositions()
	{
		foreach (var item in measureTextsByBeat)
		{
			item.Item2.transform.localPosition = new Vector3(0f, item.Item1 * EditorScaleController.EditorScale, 0f);
		}
	}
}
