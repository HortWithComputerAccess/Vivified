using System;
using UnityEngine;

public class MultiClientSubscribeBroadcaster : MonoBehaviour
{
	private void Start()
	{
		MultiClientNetListener? multiMapperConnection = BeatSaberSongContainer.Instance.MultiMapperConnection;
		multiMapperConnection?.SubscribeToCollectionEvents();
		multiMapperConnection?.UpdateCachedPoses();
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoadedEvent));
	}

	private void LevelLoadedEvent()
	{
		ActionCachingPacketHandler.FlushCache();
	}

	private void OnDestroy()
	{
		MultiClientNetListener? multiMapperConnection = BeatSaberSongContainer.Instance.MultiMapperConnection;
		multiMapperConnection?.UnsubscribeFromCollectionEvents();
		multiMapperConnection?.Dispose();
		BeatSaberSongContainer.Instance.MultiMapperConnection = null;
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoadedEvent));
	}
}
