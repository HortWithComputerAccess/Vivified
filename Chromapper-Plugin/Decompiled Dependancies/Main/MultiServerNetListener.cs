using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Beatmap.Info;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class MultiServerNetListener : MultiNetListener, INetAdmin
{
	private AutoSaveController autoSave;

	private List<string> tempBannedIps = new List<string>();

	public MultiServerNetListener(MapperIdentityPacket hostIdentity, int port, AutoSaveController autoSave)
	{
		this.autoSave = autoSave;
		NetManager.Start(port);
		Identities.Add(hostIdentity);
		SubscribeToCollectionEvents();
	}

	public override void Dispose()
	{
		UnsubscribeFromCollectionEvents();
		base.Dispose();
	}

	public override void OnConnectionRequest(ConnectionRequest request)
	{
		if (tempBannedIps.Contains(request.RemoteEndPoint.Address.MapToIPv4().ToString()))
		{
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put("You have been banned by the host.");
			request.Reject(netDataWriter);
			return;
		}
		MapperIdentityPacket mapperIdentityPacket = request.Data.Get<MapperIdentityPacket>();
		string applicationVersion = mapperIdentityPacket.ApplicationVersion;
		string version = Application.version;
		Debug.Log("Request Version " + applicationVersion + " | Host version " + version);
		if (applicationVersion != version)
		{
			NetDataWriter netDataWriter2 = new NetDataWriter();
			netDataWriter2.Put("You are not using the same CM version as host (" + version + ").");
			request.Reject(netDataWriter2);
			return;
		}
		mapperIdentityPacket.ConnectionId = Identities.Count;
		NetPeer netPeer = (mapperIdentityPacket.MapperPeer = request.Accept());
		Identities.Add(mapperIdentityPacket);
		foreach (MapperIdentityPacket identity in Identities)
		{
			if (identity.MapperPeer != null)
			{
				SendPacketFrom(mapperIdentityPacket, identity.MapperPeer, PacketId.MapperIdentity, mapperIdentityPacket);
			}
			SendPacketFrom(identity, netPeer, PacketId.MapperIdentity, identity);
			if (CachedPosePackets.TryGetValue(identity, out var value))
			{
				SendPacketFrom(identity, netPeer, PacketId.MapperPose, value);
			}
		}
		BroadcastPose(netPeer);
		PersistentUI.Instance.StartCoroutine(SaveAndSendMapToPeer(this, autoSave, netPeer));
	}

	public override void OnPacketReceived(NetPeer peer, MapperIdentityPacket identity, NetDataReader reader)
	{
		identity = Identities.Find((MapperIdentityPacket x) => x.MapperPeer == peer);
		byte[] array = new byte[reader.AvailableBytes];
		Array.Copy(reader.RawData, reader.Position, array, 0, reader.AvailableBytes);
		foreach (MapperIdentityPacket identity2 in Identities)
		{
			if (identity2.MapperPeer != null && identity2.MapperPeer != peer)
			{
				SendPacketFrom(identity, identity2.MapperPeer, array);
			}
		}
		base.OnPacketReceived(peer, identity, reader);
	}

	public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
		MapperIdentityPacket mapperIdentityPacket = Identities.Find((MapperIdentityPacket x) => x.MapperPeer == peer);
		if (mapperIdentityPacket == null)
		{
			return;
		}
		if (RemotePlayers.TryGetValue(mapperIdentityPacket, out var value))
		{
			value.UpdateLatency(latency);
		}
		foreach (MapperIdentityPacket identity in Identities)
		{
			if (identity.MapperPeer != null && identity.MapperPeer != peer)
			{
				SendPacketFrom(mapperIdentityPacket, identity.MapperPeer, PacketId.MapperLatency, new MapperLatencyPacket
				{
					Latency = latency
				});
			}
		}
	}

	public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		MapperIdentityPacket mapperIdentityPacket = Identities.Find((MapperIdentityPacket x) => x.MapperPeer == peer);
		if (mapperIdentityPacket == null)
		{
			return;
		}
		foreach (MapperIdentityPacket identity in Identities)
		{
			if (identity.MapperPeer != null && identity.MapperPeer != peer)
			{
				SendPacketFrom(mapperIdentityPacket, identity.MapperPeer, PacketId.MapperDisconnect);
			}
		}
		OnMapperDisconnected(this, mapperIdentityPacket, null);
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
		if (res == 0 && identity.MapperPeer != null)
		{
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put("You have been kicked by the host.");
			identity.MapperPeer.Disconnect(netDataWriter);
		}
	}

	private void HandleBan(int res, MapperIdentityPacket identity)
	{
		if (res == 0 && identity.MapperPeer != null)
		{
			tempBannedIps.Add(identity.MapperPeer.EndPoint.Address.MapToIPv4().ToString());
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put("You have been banned by the host.");
			identity.MapperPeer.Disconnect(netDataWriter);
		}
	}

	internal static IEnumerator SaveAndSendMapToPeer(MultiNetListener listener, AutoSaveController autoSave, NetPeer peer)
	{
		autoSave.Save();
		yield return new WaitWhile(() => !autoSave.IsSaving);
		BaseInfo info = BeatSaberSongContainer.Instance.Info;
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		string text = Path.Combine(info.Directory, info.CleanSongName + ".zip");
		File.Delete(text);
		Dictionary<string, string> filesForArchiving = BeatSaberSongExtensions.GetFilesForArchiving(info);
		using (ZipArchive destination = ZipFile.Open(text, ZipArchiveMode.Create))
		{
			foreach (KeyValuePair<string, string> item in filesForArchiving)
			{
				destination.CreateEntryFromFile(item.Key, item.Value);
			}
		}
		byte[] zipBytes = File.ReadAllBytes(text);
		listener.SendPacketTo(peer, PacketId.SendZip, new MapDataPacket(zipBytes, mapDifficultyInfo.Characteristic, mapDifficultyInfo.Difficulty));
	}
}
