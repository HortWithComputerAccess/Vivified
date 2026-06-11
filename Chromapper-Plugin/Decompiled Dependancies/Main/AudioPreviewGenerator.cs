using System;
using UnityEngine;

public class AudioPreviewGenerator : MonoBehaviour
{
	private static readonly int viewStart = Shader.PropertyToID("_ViewStart");

	private static readonly int viewEnd = Shader.PropertyToID("_ViewEnd");

	private static readonly int editorScale = Shader.PropertyToID("_EditorScale");

	private static readonly int songBpm = Shader.PropertyToID("_SongBPM");

	[SerializeField]
	private AudioManager audioManager;

	[GradientUsage(true)]
	[SerializeField]
	private Gradient spectrogramGradient2d;

	[SerializeField]
	private SongInfoEditUI songInfoEditUI;

	[SerializeField]
	private GameObject previewGameObject;

	[SerializeField]
	private RectTransform previewSelection;

	private float previewDuration;

	private float previewTime;

	private void Start()
	{
		SongInfoEditUI obj = songInfoEditUI;
		obj.TempSongLoadedEvent = (Action)Delegate.Combine(obj.TempSongLoadedEvent, new Action(TempSongLoaded));
	}

	private void OnDestroy()
	{
		SongInfoEditUI obj = songInfoEditUI;
		obj.TempSongLoadedEvent = (Action)Delegate.Remove(obj.TempSongLoadedEvent, new Action(TempSongLoaded));
	}

	private void TempSongLoaded()
	{
		if (BeatSaberSongContainer.Instance.LoadedSong == null)
		{
			previewGameObject.SetActive(value: false);
			return;
		}
		Shader.SetGlobalFloat(viewStart, 0f);
		Shader.SetGlobalFloat(viewEnd, BeatSaberSongContainer.Instance.LoadedSongLength);
		Shader.SetGlobalFloat(editorScale, 1f);
		Shader.SetGlobalFloat(songBpm, 120f);
		ColorBufferManager.GenerateBuffersForGradient(spectrogramGradient2d);
		SampleBufferManager.GenerateSamplesBuffer(BeatSaberSongContainer.Instance.LoadedSong);
		audioManager.GenerateFFT(BeatSaberSongContainer.Instance.LoadedSong, Settings.Instance.SpectrogramSampleSize, 1);
		UpdatePreviewSelection();
		previewGameObject.SetActive(value: true);
	}

	public void UpdatePreviewStart(string start)
	{
		if (float.TryParse(start, out previewTime))
		{
			UpdatePreviewSelection();
		}
	}

	public void UpdatePreviewDuration(string duration)
	{
		if (float.TryParse(duration, out previewDuration))
		{
			UpdatePreviewSelection();
		}
	}

	private void UpdatePreviewSelection()
	{
		if (!(BeatSaberSongContainer.Instance.LoadedSong == null))
		{
			float length = BeatSaberSongContainer.Instance.LoadedSong.length;
			float num = ((base.transform.parent as RectTransform).sizeDelta.x + (base.transform as RectTransform).sizeDelta.x) / length;
			previewSelection.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, previewTime * num, previewDuration * num);
		}
	}
}
