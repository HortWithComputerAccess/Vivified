using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionCachingPacketHandler
{
	public class Cache
	{
		public CacheType CacheType;

		public object Object;
	}

	public enum CacheType
	{
		Create,
		Undo,
		Redo
	}

	protected static List<Cache> CachedPackets = new List<Cache>();

	public static void FlushCache()
	{
		Debug.Log("Flushing beatmap action cache...");
		foreach (Cache cachedPacket in CachedPackets)
		{
			switch (cachedPacket.CacheType)
			{
			case CacheType.Create:
				BeatmapActionContainer.AddAction(cachedPacket.Object as BeatmapAction, perform: true);
				break;
			case CacheType.Undo:
				BeatmapActionContainer.Undo((Guid)cachedPacket.Object);
				break;
			case CacheType.Redo:
				BeatmapActionContainer.Redo((Guid)cachedPacket.Object);
				break;
			}
		}
		CachedPackets.Clear();
	}
}
