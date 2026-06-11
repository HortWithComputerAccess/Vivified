using Beatmap.Base;
using Beatmap.Info;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RefreshMapController : MonoBehaviour, CMInput.IRefreshMapActions
{
	[SerializeField]
	private MapLoader loader;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private TMP_FontAsset cancelFontAsset;

	[SerializeField]
	private TMP_FontAsset moreOptionsFontAsset;

	[SerializeField]
	private TMP_FontAsset thingYouCanRefreshFontAsset;

	public void OnRefreshMap(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InitiateRefreshConversation();
		}
	}

	public void InitiateRefreshConversation()
	{
		PersistentUI.Instance.ShowDialogBox("Mapper", "refreshmap", HandleFirstLayerConversation, new string[6] { "refreshmap.notes", "refreshmap.walls", "refreshmap.events", "refreshmap.other", "refreshmap.full", "refreshmap.cancel" }, new TMP_FontAsset[6] { thingYouCanRefreshFontAsset, thingYouCanRefreshFontAsset, thingYouCanRefreshFontAsset, thingYouCanRefreshFontAsset, thingYouCanRefreshFontAsset, cancelFontAsset });
	}

	private void HandleFirstLayerConversation(int res)
	{
		switch (res)
		{
		case 0:
			RefreshMap(notes: true, obstacles: false, events: false, others: false, full: false);
			break;
		case 1:
			RefreshMap(notes: false, obstacles: true, events: false, others: false, full: false);
			break;
		case 2:
			RefreshMap(notes: false, obstacles: false, events: true, others: false, full: false);
			break;
		case 3:
			RefreshMap(notes: false, obstacles: false, events: false, others: true, full: false);
			break;
		case 4:
			RefreshMap(notes: false, obstacles: false, events: false, others: false, full: true);
			break;
		}
	}

	private void RefreshMap(bool notes, bool obstacles, bool events, bool others, bool full)
	{
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		BaseDifficulty mapFromInfoFiles = BeatSaberSongUtils.GetMapFromInfoFiles(BeatSaberSongContainer.Instance.Info, mapDifficultyInfo);
		loader.UpdateMapData(mapFromInfoFiles);
		float currentSongBpmTime = atsc.CurrentSongBpmTime;
		atsc.MoveToSongBpmTime(0f);
		if (full)
		{
			BeatSaberSongContainer.Instance.Map = mapFromInfoFiles;
			loader.HardRefresh();
			atsc.MoveToSongBpmTime(currentSongBpmTime);
			return;
		}
		BeatSaberSongContainer.Instance.Map.BpmEvents = mapFromInfoFiles.BpmEvents;
		loader.LoadObjects(mapFromInfoFiles.BpmEvents);
		if (notes)
		{
			BeatSaberSongContainer.Instance.Map.Notes = mapFromInfoFiles.Notes;
			BeatSaberSongContainer.Instance.Map.Arcs = mapFromInfoFiles.Arcs;
			BeatSaberSongContainer.Instance.Map.Chains = mapFromInfoFiles.Chains;
			loader.LoadObjects(mapFromInfoFiles.Notes);
			loader.LoadObjects(mapFromInfoFiles.Arcs);
			loader.LoadObjects(mapFromInfoFiles.Chains);
		}
		if (obstacles)
		{
			BeatSaberSongContainer.Instance.Map.Obstacles = mapFromInfoFiles.Obstacles;
			loader.LoadObjects(mapFromInfoFiles.Obstacles);
		}
		if (events)
		{
			BeatSaberSongContainer.Instance.Map.Events = mapFromInfoFiles.Events;
			loader.LoadObjects(mapFromInfoFiles.Events);
		}
		if (others)
		{
			BeatSaberSongContainer.Instance.Map.CustomEvents = mapFromInfoFiles.CustomEvents;
			loader.LoadObjects(mapFromInfoFiles.CustomEvents);
		}
		tracksManager.RefreshTracks();
		atsc.MoveToSongBpmTime(currentSongBpmTime);
	}
}
