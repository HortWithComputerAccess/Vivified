using System;
using System.Collections;
using Beatmap.Containers;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.Serialization;

public class LoadInitialMap : MonoBehaviour
{
	public static Action<PlatformDescriptor> PlatformLoadedEvent;

	public static Action<PlatformColors> PlatformColorsRefreshedEvent;

	public static PlatformDescriptor Platform;

	public static Action LevelLoadedEvent;

	public static readonly Vector3 PlatformOffset = new Vector3(0f, -0.5f, -1.5f);

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private RotationCallbackController rotationController;

	[FormerlySerializedAs("notesContainer")]
	[Space]
	[SerializeField]
	private NoteGridContainer noteGridContainer;

	[FormerlySerializedAs("obstaclesContainer")]
	[SerializeField]
	private ObstacleGridContainer obstacleGridContainer;

	[FormerlySerializedAs("arcsContainer")]
	[SerializeField]
	private ArcGridContainer arcGridContainer;

	[FormerlySerializedAs("chainsContainer")]
	[SerializeField]
	private ChainGridContainer chainGridContainer;

	[SerializeField]
	private EventGridContainer eventGridContainer;

	[SerializeField]
	private MapLoader loader;

	[FormerlySerializedAs("PlatformPrefabs")]
	[Space]
	[SerializeField]
	private GameObject[] platformPrefabs;

	private void Awake()
	{
		SceneTransitionManager.Instance.AddLoadRoutine(LoadMap());
	}

	private void Start()
	{
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Combine(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(UpdatePlatformColors));
	}

	private void OnDestroy()
	{
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Remove(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(UpdatePlatformColors));
	}

	public IEnumerator LoadMap()
	{
		if (!(BeatSaberSongContainer.Instance == null))
		{
			PersistentUI.Instance.LevelLoadSliderLabel.text = "";
			yield return new WaitUntil(() => atsc.Initialized);
			BaseInfo info = BeatSaberSongContainer.Instance.Info;
			InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
			bool flag = false;
			int environmentIDFromString = SongInfoEditUI.GetEnvironmentIDFromString(info.EnvironmentNames[mapDifficultyInfo.EnvironmentNameIndex]);
			if (!string.IsNullOrEmpty(info.CustomEnvironmentMetadata.Name) && CustomPlatformsLoader.Instance.GetAllEnvironmentIds().IndexOf(info.CustomEnvironmentMetadata.Name) >= 0)
			{
				flag = true;
			}
			GameObject gameObject = ((platformPrefabs[environmentIDFromString] == null) ? platformPrefabs[0] : platformPrefabs[environmentIDFromString]);
			if (flag)
			{
				gameObject = CustomPlatformsLoader.Instance.LoadPlatform(info.CustomEnvironmentMetadata.Name, gameObject);
			}
			PlatformDescriptor component = (flag ? gameObject : UnityEngine.Object.Instantiate(gameObject, PlatformOffset, Quaternion.identity)).GetComponent<PlatformDescriptor>();
			EventContainer.ModifyTypeMode = component.SortMode;
			PopulateColorsFromMapInfo(component);
			UpdateObjectContainerColors(component.Colors);
			PlatformLoadedEvent(component);
			Platform = component;
			loader.UpdateMapData(BeatSaberSongContainer.Instance.Map);
			loader.HardRefresh();
			LevelLoadedEvent?.Invoke();
		}
	}

	private static void PopulateColorsFromMapInfo(PlatformDescriptor platformDescriptor)
	{
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		platformDescriptor.Colors = platformDescriptor.DefaultColors.Clone();
		if (mapDifficultyInfo.CustomColorLeft.HasValue)
		{
			platformDescriptor.Colors.RedNoteColor = mapDifficultyInfo.CustomColorLeft.Value;
		}
		if (mapDifficultyInfo.CustomColorRight.HasValue)
		{
			platformDescriptor.Colors.BlueNoteColor = mapDifficultyInfo.CustomColorRight.Value;
		}
		if (mapDifficultyInfo.CustomColorObstacle.HasValue)
		{
			platformDescriptor.Colors.ObstacleColor = mapDifficultyInfo.CustomColorObstacle.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorLeft.HasValue)
		{
			platformDescriptor.Colors.RedColor = mapDifficultyInfo.CustomEnvColorLeft.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorRight.HasValue)
		{
			platformDescriptor.Colors.BlueColor = mapDifficultyInfo.CustomEnvColorRight.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorWhite.HasValue)
		{
			platformDescriptor.Colors.WhiteColor = mapDifficultyInfo.CustomEnvColorWhite.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorBoostLeft.HasValue)
		{
			platformDescriptor.Colors.RedBoostColor = mapDifficultyInfo.CustomEnvColorBoostLeft.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorBoostRight.HasValue)
		{
			platformDescriptor.Colors.BlueBoostColor = mapDifficultyInfo.CustomEnvColorBoostRight.Value;
		}
		if (mapDifficultyInfo.CustomEnvColorBoostWhite.HasValue)
		{
			platformDescriptor.Colors.WhiteBoostColor = mapDifficultyInfo.CustomEnvColorBoostWhite.Value;
		}
	}

	private void UpdateObjectContainerColors(PlatformColors platformColors)
	{
		Color redNoteColor = platformColors.RedNoteColor;
		Color blueNoteColor = platformColors.BlueNoteColor;
		noteGridContainer.UpdateColor(redNoteColor, blueNoteColor);
		arcGridContainer.UpdateColor(redNoteColor, blueNoteColor);
		chainGridContainer.UpdateColor(redNoteColor, blueNoteColor);
		obstacleGridContainer.UpdateColor(platformColors.ObstacleColor);
		eventGridContainer.UpdateColor(platformColors.RedColor, platformColors.RedBoostColor, platformColors.BlueColor, platformColors.BlueBoostColor, platformColors.WhiteColor, platformColors.WhiteBoostColor);
	}

	private void UpdatePlatformColors()
	{
		PlatformColors platformColors = Platform.Colors.Clone();
		PopulateColorsFromMapInfo(Platform);
		UpdateObjectContainerColors(Platform.Colors);
		PlatformColors colors = Platform.Colors;
		if (platformColors.ObstacleColor != colors.ObstacleColor)
		{
			obstacleGridContainer.RefreshPool(true);
		}
		if (platformColors.BlueNoteColor != colors.BlueNoteColor || platformColors.RedNoteColor != colors.RedNoteColor)
		{
			noteGridContainer.RefreshPool(forceRefresh: true);
			arcGridContainer.RefreshPool(forceRefresh: true);
			chainGridContainer.RefreshPool(forceRefresh: true);
		}
		if (platformColors.BlueColor != colors.BlueColor || platformColors.RedColor != colors.RedColor || platformColors.WhiteColor != colors.WhiteColor || platformColors.BlueBoostColor != colors.BlueBoostColor || platformColors.RedBoostColor != colors.RedBoostColor || platformColors.WhiteBoostColor != colors.WhiteBoostColor)
		{
			eventGridContainer.RefreshPool(forceRefresh: true);
		}
		PlatformColorsRefreshedEvent?.Invoke(Platform.Colors);
	}
}
