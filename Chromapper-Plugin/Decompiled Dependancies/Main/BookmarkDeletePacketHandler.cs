using System;
using Beatmap.Base.Customs;
using LiteNetLib.Utils;

[Obsolete("TODO: Refactor bookmarks to use the same systems as every other BeatmapObject")]
public class BookmarkDeletePacketHandler : IPacketHandler
{
	private BookmarkManager bookmarkManager;

	public BookmarkDeletePacketHandler(BookmarkManager bookmarkManager)
	{
		this.bookmarkManager = bookmarkManager;
	}

	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		if (reader.GetBeatmapObject() is BaseBookmark baseBookmark)
		{
			bookmarkManager.DeleteBookmarkAtTime(baseBookmark.JsonTime, triggerEvent: false);
		}
	}
}
