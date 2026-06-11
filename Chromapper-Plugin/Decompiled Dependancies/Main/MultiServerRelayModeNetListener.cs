using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

public class MultiServerRelayModeNetListener : MultiClientNetListener, INetAdmin
{
	private AutoSaveController autoSave;

	private List<string> tempBannedIps = new List<string>();

	public MultiServerRelayModeNetListener(string roomCode, MapperIdentityPacket identity, AutoSaveController autoSave)
		: base(roomCode, identity)
	{
		this.autoSave = autoSave;
		SubscribeToCollectionEvents();
		RegisterPacketHandler(PacketId.CMT_RequestZip, OnRequestZip);
		RegisterPacketHandler(PacketId.CMT_IncomingMapper, OnIncomingMapper);
	}

	public override void Dispose()
	{
		UnsubscribeFromCollectionEvents();
		base.Dispose();
	}

	public void OnRequestZip(MultiNetListener _, MapperIdentityPacket identity, NetDataReader reader)
	{
		PersistentUI.Instance.StartCoroutine(MultiServerNetListener.SaveAndSendMapToPeer(this, autoSave, NetManager.FirstPeer));
	}

	public void OnIncomingMapper(MultiNetListener listener, MapperIdentityPacket identity, NetDataReader reader)
	{
		string text = (identity.Ip = reader.GetString());
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(identity.ConnectionId);
		if (!IPAddress.TryParse(text, out var _))
		{
			netDataWriter.Put("IP Address was invalid.");
			SendPacketTo(NetManager.FirstPeer, PacketId.CMT_KickMapper, netDataWriter.Data);
		}
		else if (tempBannedIps.Contains(text))
		{
			netDataWriter.Put("The host has banned you.");
			SendPacketTo(NetManager.FirstPeer, PacketId.CMT_KickMapper, netDataWriter.Data);
		}
		else
		{
			SendPacketTo(NetManager.FirstPeer, PacketId.CMT_AcceptMapper, netDataWriter.Data);
			BroadcastPose();
		}
	}

	public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
	}

	public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		PersistentUI.Instance.ShowDialogBox("MultiMapping", "multi.connection.server-lost", null, PersistentUI.DialogBoxPresetType.Ok, new object[1] { disconnectInfo.Reason });
	}

	public void Kick(MapperIdentityPacket identity)
	{
		PersistentUI instance = PersistentUI.Instance;
		Action<int> result = delegate(int res)
		{
			HandleKick(res, identity);
		};
		object[] args = new string[1] { identity.Name };
		instance.ShowDialogBox("MultiMapping", "multi.kick", result, PersistentUI.DialogBoxPresetType.YesNo, args);
	}

	public void Ban(MapperIdentityPacket identity)
	{
		PersistentUI instance = PersistentUI.Instance;
		Action<int> result = delegate(int res)
		{
			HandleBan(res, identity);
		};
		object[] args = new string[1] { identity.Name };
		instance.ShowDialogBox("MultiMapping", "multi.ban", result, PersistentUI.DialogBoxPresetType.YesNo, args);
	}

	private void HandleKick(int res, MapperIdentityPacket identity)
	{
		if (res == 0)
		{
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put(identity.ConnectionId);
			netDataWriter.Put("You have been kicked by the host.");
			SendPacketTo(NetManager.FirstPeer, PacketId.CMT_KickMapper, netDataWriter.Data);
		}
	}

	private void HandleBan(int res, MapperIdentityPacket identity)
	{
		if (res == 0)
		{
			tempBannedIps.Add(identity.Ip);
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put(identity.ConnectionId);
			netDataWriter.Put("You have been banned by the host.");
			SendPacketTo(NetManager.FirstPeer, PacketId.CMT_KickMapper, netDataWriter.Data);
		}
	}
}
