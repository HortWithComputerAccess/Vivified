using LiteNetLib.Utils;

public class ActionCreateCachingPacketHandler : ActionCachingPacketHandler, IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		ActionCachingPacketHandler.CachedPackets.Add(new Cache
		{
			CacheType = CacheType.Create,
			Object = reader.GetBeatmapAction(identity)
		});
	}
}
