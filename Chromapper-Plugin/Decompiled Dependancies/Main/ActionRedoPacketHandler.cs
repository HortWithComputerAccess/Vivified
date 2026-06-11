using System;
using LiteNetLib.Utils;

public class ActionRedoPacketHandler : IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		BeatmapActionContainer.Redo(Guid.Parse(reader.GetString()));
	}
}
