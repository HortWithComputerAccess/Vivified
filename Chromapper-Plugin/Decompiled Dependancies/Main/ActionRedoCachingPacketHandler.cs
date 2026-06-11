using System;
using LiteNetLib.Utils;

public class ActionRedoCachingPacketHandler : ActionCachingPacketHandler, IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		ActionCachingPacketHandler.CachedPackets.Add(new Cache
		{
			CacheType = CacheType.Redo,
			Object = Guid.Parse(reader.GetString())
		});
	}
}
