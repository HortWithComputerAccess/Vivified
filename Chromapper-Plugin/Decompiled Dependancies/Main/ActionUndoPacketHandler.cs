using System;
using LiteNetLib.Utils;

public class ActionUndoPacketHandler : IPacketHandler
{
	public void HandlePacket(MultiNetListener client, MapperIdentityPacket identity, NetDataReader reader)
	{
		BeatmapActionContainer.Undo(Guid.Parse(reader.GetString()));
	}
}
