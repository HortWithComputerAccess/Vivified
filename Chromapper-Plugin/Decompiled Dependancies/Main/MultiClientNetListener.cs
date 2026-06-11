using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class MultiClientNetListener : MultiNetListener
{
	public MapDataPacket MapData { get; private set; }

	public MultiClientNetListener(string ip, int port, MapperIdentityPacket identity)
	{
		NetManager.Start();
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(identity);
		NetManager.Connect(ip, port, netDataWriter);
		RegisterPacketHandler(PacketId.SendZip, OnZipData);
		RegisterPacketHandler<ActionCreateCachingPacketHandler>(PacketId.ActionCreated);
		RegisterPacketHandler<ActionUndoCachingPacketHandler>(PacketId.ActionUndo);
		RegisterPacketHandler<ActionRedoCachingPacketHandler>(PacketId.ActionRedo);
	}

	public MultiClientNetListener(string roomCode, MapperIdentityPacket identity)
	{
		NetManager.Start();
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(roomCode);
		netDataWriter.Put(identity);
		string host = new Uri(Settings.Instance.MultiSettings.ChroMapTogetherServerUrl).Host;
		Debug.Log("Attempting to contact ChroMapTogether server at " + host + ":6969...");
		NetManager.Connect(host, 6969, netDataWriter);
		RegisterPacketHandler(PacketId.SendZip, OnZipData);
		RegisterPacketHandler<ActionCreateCachingPacketHandler>(PacketId.ActionCreated);
		RegisterPacketHandler<ActionUndoCachingPacketHandler>(PacketId.ActionUndo);
		RegisterPacketHandler<ActionRedoCachingPacketHandler>(PacketId.ActionRedo);
	}

	public void OnZipData(MultiNetListener _, MapperIdentityPacket __, NetDataReader reader)
	{
		MapData = reader.Get<MapDataPacket>();
	}

	public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
		MapperIdentityPacket key = Identities.Find((MapperIdentityPacket it) => it.ConnectionId == 0);
		if (RemotePlayers.TryGetValue(key, out var value))
		{
			value.UpdateLatency(latency);
		}
	}

	public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
	{
		SceneTransitionManager.Instance.CancelLoading(string.Empty);
		SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu");
		base.OnNetworkError(endPoint, socketError);
	}

	public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		if (disconnectInfo.Reason == DisconnectReason.ConnectionRejected)
		{
			PersistentUI.Instance.ShowDialogBox("MultiMapping", "multi.connection.reject", null, PersistentUI.DialogBoxPresetType.Ok);
		}
		else
		{
			PersistentUI.Instance.ShowDialogBox("MultiMapping", "multi.connection.host-lost", null, PersistentUI.DialogBoxPresetType.Ok, new object[1] { disconnectInfo.Reason });
		}
		SceneTransitionManager.Instance.CancelLoading(string.Empty);
		SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu");
	}
}
