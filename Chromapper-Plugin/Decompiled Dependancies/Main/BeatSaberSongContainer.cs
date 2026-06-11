using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Beatmap.Base;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;

public class BeatSaberSongContainer : MonoBehaviour
{
	[FormerlySerializedAs("loadedSong")]
	public AudioClip LoadedSong;

	public BaseInfo Info;

	public InfoDifficulty MapDifficultyInfo;

	[FormerlySerializedAs("map")]
	public BaseDifficulty Map;

	[NonSerialized]
	public MultiClientNetListener? MultiMapperConnection;

	[HideInInspector]
	public int LoadedSongSamples;

	[HideInInspector]
	public int LoadedSongFrequency;

	[HideInInspector]
	public float LoadedSongLength;

	public static BeatSaberSongContainer Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			UnityEngine.Object.Destroy(Instance.gameObject);
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void SelectSongForEditing(BaseInfo info)
	{
		Info = info;
		SceneTransitionManager.Instance.LoadScene("02_SongEditMenu");
	}

	public void ConnectToMultiSession(string ip, int port, MapperIdentityPacket identity)
	{
		MultiMapperConnection = new MultiClientNetListener(ip, port, identity);
		SceneTransitionManager.Instance.LoadScene("03_Mapper", DownloadAndLaunchMap());
	}

	public void ConnectToMultiSession(string roomCode, MapperIdentityPacket identity)
	{
		MultiMapperConnection = new MultiClientNetListener(roomCode, identity);
		SceneTransitionManager.Instance.LoadScene("03_Mapper", DownloadAndLaunchMap());
	}

	private IEnumerator DownloadAndLaunchMap()
	{
		PersistentUI.Instance.LevelLoadSlider.gameObject.SetActive(value: true);
		PersistentUI.Instance.LevelLoadSlider.value = 0f;
		PersistentUI.Instance.LevelLoadSliderLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiMapping", "multi.session.downloading", null, FallbackBehavior.UseProjectSettings);
		yield return new WaitUntil(() => MultiMapperConnection?.MapData != null);
		string text = Path.Combine(Path.GetTempPath(), "ChroMapper Multi Mapping", MultiMapperConnection?.MapData.GetHashCode().ToString());
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		new ZipArchive(new MemoryStream(MultiMapperConnection.MapData.ZipBytes), ZipArchiveMode.Read).ExtractToDirectory(text);
		BaseInfo infoFromFolder = BeatSaberSongUtils.GetInfoFromFolder(text);
		if (infoFromFolder != null)
		{
			PersistentUI.Instance.LevelLoadSliderLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiMapping", "multi.session.loading", null, FallbackBehavior.UseProjectSettings);
			Info = infoFromFolder;
			MapDifficultyInfo = infoFromFolder.DifficultySets.Find((InfoDifficultySet set) => set.Characteristic == MultiMapperConnection.MapData.Characteristic).Difficulties.Find((InfoDifficulty diff) => diff.Difficulty == MultiMapperConnection.MapData.Diff);
			Map = BeatSaberSongUtils.GetMapFromInfoFiles(infoFromFolder, MapDifficultyInfo);
			Settings.Instance.MapVersion = Map.MajorVersion;
			yield return BeatSaberSongExtensions.LoadAudio(infoFromFolder, delegate(AudioClip clip)
			{
				LoadedSong = clip;
				LoadedSongSamples = clip.samples;
				LoadedSongFrequency = clip.frequency;
				LoadedSongLength = clip.length;
			}, infoFromFolder.SongTimeOffset);
		}
		PersistentUI.Instance.LevelLoadSlider.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		MultiMapperConnection?.ManualUpdate();
	}
}
