using LiteNetLib.Utils;

public class ActionCreatedPacketHandler : IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		BeatmapActionContainer.AddAction(reader.GetBeatmapAction(identity), perform: true);
	}
}
