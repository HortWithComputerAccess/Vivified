using System;
using LiteNetLib.Utils;

public class ActionUndoCachingPacketHandler : ActionCachingPacketHandler, IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		ActionCachingPacketHandler.CachedPackets.Add(new Cache
		{
			CacheType = CacheType.Undo,
			Object = Guid.Parse(reader.GetString())
		});
	}
}
