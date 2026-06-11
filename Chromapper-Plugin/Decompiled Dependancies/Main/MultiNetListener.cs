using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Beatmap.Base;
using Beatmap.Enums;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class MultiNetListener : INetEventListener, IDisposable
{
	public const int PoseUpdateFramerate = 24;

	protected internal NetManager NetManager;

	protected List<MapperIdentityPacket> Identities = new List<MapperIdentityPacket>();

	protected Dictionary<MapperIdentityPacket, RemotePlayerContainer> RemotePlayers = new Dictionary<MapperIdentityPacket, RemotePlayerContainer>();

	protected Dictionary<MapperIdentityPacket, MapperPosePacket> CachedPosePackets = new Dictionary<MapperIdentityPacket, MapperPosePacket>();

	private Dictionary<PacketId, IPacketHandler> registeredPacketHandlers = new Dictionary<PacketId, IPacketHandler>();

	private CameraController cameraController;

	private AudioTimeSyncController audioTimeSyncController;

	private TracksManager tracksManager;

	private BookmarkManager bookmarkManager;

	private CustomColorsUIController customColors;

	private RemotePlayerContainer remotePlayerPrefab;

	private MultiTimelineController multiTimelineController;

	private float previousCursorSongBeat;

	private float localSongSpeed = 1f;

	private float lastPoseUpdateTime;

	public MultiNetListener()
	{
		NetManager = new NetManager(this);
		remotePlayerPrefab = Resources.Load<RemotePlayerContainer>("Remote Player");
		RegisterPacketHandler(PacketId.MapperIdentity, OnMapperIdentity);
		RegisterPacketHandler(PacketId.MapperPose, OnMapperPose);
		RegisterPacketHandler(PacketId.MapperDisconnect, OnMapperDisconnected);
		RegisterPacketHandler(PacketId.MapperLatency, OnMapperLatency);
	}

	public virtual void Dispose()
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)5);
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
		NetManager.Stop();
	}

	public void RegisterPacketHandler<THandler>(PacketId packetId) where THandler : IPacketHandler, new()
	{
		registeredPacketHandlers[packetId] = new THandler();
	}

	public void RegisterPacketHandler<THandler>(PacketId packetId, THandler instance) where THandler : IPacketHandler
	{
		registeredPacketHandlers[packetId] = instance;
	}

	public void RegisterPacketHandler(PacketId packetId, Action<MultiNetListener, MapperIdentityPacket, NetDataReader> onHandlePacket)
	{
		RegisterPacketHandler(packetId, new DelegatePacketHandler(onHandlePacket));
	}

	public virtual void OnConnectionRequest(ConnectionRequest request)
	{
	}

	public virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
	{
		PersistentUI.Instance.ShowDialogBox("MultiMapping", "multi.connection.network-error", null, PersistentUI.DialogBoxPresetType.Ok, new object[1] { socketError });
	}

	public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
	}

	public virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
	{
		int mapperId = reader.GetInt();
		MapperIdentityPacket identity = Identities.Find((MapperIdentityPacket x) => x.ConnectionId == mapperId);
		OnPacketReceived(peer, identity, reader);
	}

	public virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
	{
	}

	public virtual void OnPeerConnected(NetPeer peer)
	{
	}

	public virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
	}

	public virtual void OnPacketReceived(NetPeer peer, MapperIdentityPacket identity, NetDataReader reader)
	{
		byte b = reader.GetByte();
		if (registeredPacketHandlers.TryGetValue((PacketId)b, out var value))
		{
			value.HandlePacket(this, identity, reader);
		}
		else
		{
			Debug.LogWarning($"No handler for packet {b}");
		}
	}

	public void OnMapperIdentity(MultiNetListener _, MapperIdentityPacket identity, NetDataReader reader)
	{
		Identities.Add(reader.Get<MapperIdentityPacket>());
	}

	public void OnMapperPose(MultiNetListener _, MapperIdentityPacket identity, NetDataReader reader)
	{
		MapperPosePacket mapperPosePacket = reader.Get<MapperPosePacket>();
		if (mapperPosePacket.IsPlayingSong && CachedPosePackets.TryGetValue(identity, out var value))
		{
			mapperPosePacket.SongPosition = value.SongPosition;
		}
		CachedPosePackets[identity] = mapperPosePacket;
		UpdateMapperPose(identity, mapperPosePacket);
	}

	public void UpdateMapperPose(MapperIdentityPacket identity, MapperPosePacket pose)
	{
		if (identity == null || tracksManager == null)
		{
			return;
		}
		if (!RemotePlayers.TryGetValue(identity, out var value) || value == null)
		{
			if (RemotePlayers.ContainsKey(identity))
			{
				RemotePlayers.Remove(identity);
			}
			value = UnityEngine.Object.Instantiate(remotePlayerPrefab);
			RemotePlayers.Add(identity, value);
			value.GetComponent<RemotePlayerContainer>().AssignIdentity(this, identity);
		}
		Transform objectParentTransform = tracksManager.GetTrackAtTime(pose.SongPosition).ObjectParentTransform;
		if (!value.transform.IsChildOf(objectParentTransform))
		{
			value.transform.SetParent(objectParentTransform, worldPositionStays: true);
		}
		value.transform.localPosition = EditorScaleController.EditorScale * pose.SongPosition * Vector3.forward;
		value.CameraTransform.localPosition = pose.Position;
		value.CameraTransform.localRotation = pose.Rotation;
		value.GridTransform.localEulerAngles = objectParentTransform.localEulerAngles;
		multiTimelineController.UpdatePose(identity, pose);
	}

	public void OnMapperDisconnected(MultiNetListener _, MapperIdentityPacket identity, NetDataReader __)
	{
		if (identity != null)
		{
			identity.MapperPeer = null;
			if (RemotePlayers.TryGetValue(identity, out var value))
			{
				UnityEngine.Object.Destroy(value.gameObject);
				RemotePlayers.Remove(identity);
			}
			multiTimelineController.DisconnectMapper(identity);
			CachedPosePackets.Remove(identity);
		}
	}

	public void OnMapperLatency(MultiNetListener _, MapperIdentityPacket identity, NetDataReader reader)
	{
		MapperLatencyPacket mapperLatencyPacket = reader.Get<MapperLatencyPacket>();
		if (RemotePlayers.TryGetValue(identity, out var value))
		{
			value.UpdateLatency(mapperLatencyPacket.Latency);
		}
	}

	public void UpdateCachedPoses()
	{
		foreach (KeyValuePair<MapperIdentityPacket, MapperPosePacket> cachedPosePacket in CachedPosePackets)
		{
			UpdateMapperPose(cachedPosePacket.Key, cachedPosePacket.Value);
		}
	}

	public void SendPacketFrom(MapperIdentityPacket fromPeer, NetPeer toPeer, PacketId packetId, INetSerializable serializable)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(fromPeer.ConnectionId);
		netDataWriter.Put((byte)packetId);
		netDataWriter.Put(serializable);
		toPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	public void SendPacketFrom(MapperIdentityPacket fromPeer, NetPeer toPeer, PacketId packetId)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(fromPeer.ConnectionId);
		netDataWriter.Put((byte)packetId);
		toPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	public void SendPacketFrom(MapperIdentityPacket fromPeer, NetPeer toPeer, byte[] rawPacketData)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(fromPeer.ConnectionId);
		netDataWriter.Put(rawPacketData);
		toPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	public void SendPacketTo(NetPeer toPeer, PacketId packetId, INetSerializable serializable)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)packetId);
		netDataWriter.Put(serializable);
		toPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	public void SendPacketTo(NetPeer toPeer, PacketId packetId, byte[] data)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)packetId);
		netDataWriter.Put(data);
		toPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	public void ManualUpdate()
	{
		NetManager?.PollEvents();
		multiTimelineController?.ManualUpdate();
		if (audioTimeSyncController != null && cameraController != null && (cameraController.MovingCamera || (!audioTimeSyncController.IsPlaying && audioTimeSyncController.CurrentSongBpmTime != previousCursorSongBeat)) && Time.time - lastPoseUpdateTime >= 1f / 24f)
		{
			lastPoseUpdateTime = Time.time;
			previousCursorSongBeat = audioTimeSyncController.CurrentSongBpmTime;
			BroadcastPose();
		}
		foreach (KeyValuePair<MapperIdentityPacket, MapperPosePacket> cachedPosePacket in CachedPosePackets)
		{
			MapperIdentityPacket key = cachedPosePacket.Key;
			MapperPosePacket value = cachedPosePacket.Value;
			if (value.IsPlayingSong)
			{
				value.SongPosition += BeatSaberSongContainer.Instance.Info.BeatsPerMinute / 60f * Time.deltaTime * value.PlayingSongSpeed;
				UpdateMapperPose(key, value);
			}
		}
	}

	public void BroadcastPose(NetPeer targetPeer = null)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)1);
		netDataWriter.Put(new MapperPosePacket
		{
			Position = cameraController.transform.position,
			Rotation = cameraController.transform.rotation,
			SongPosition = audioTimeSyncController.CurrentSongBpmTime,
			IsPlayingSong = audioTimeSyncController.IsPlaying,
			PlayingSongSpeed = localSongSpeed
		});
		if (targetPeer == null)
		{
			NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
		}
		else
		{
			targetPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
		}
	}

	public void SubscribeToCollectionEvents()
	{
		audioTimeSyncController = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note).AudioTimeSyncController;
		AudioTimeSyncController obj = audioTimeSyncController;
		obj.PlayToggle = (Action<bool>)Delegate.Combine(obj.PlayToggle, new Action<bool>(OnTogglePlaying));
		cameraController = Camera.main.GetComponent<CameraController>();
		tracksManager = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note).GetComponent<TracksManager>();
		bookmarkManager = UnityEngine.Object.FindObjectOfType<BookmarkManager>();
		bookmarkManager.BookmarkAdded += MultiNetListener_ObjectSpawnedEvent;
		bookmarkManager.BookmarkDeleted += MultiNetListener_ObjectDeletedEvent;
		RegisterPacketHandler(PacketId.BeatmapObjectCreate, new BookmarkCreatePacketHandler(bookmarkManager));
		RegisterPacketHandler(PacketId.BeatmapObjectDelete, new BookmarkDeletePacketHandler(bookmarkManager));
		customColors = UnityEngine.Object.FindObjectOfType<CustomColorsUIController>();
		customColors.CustomColorsUpdatedEvent += CustomColors_CustomColorsUpdatedEvent;
		RegisterPacketHandler(PacketId.MapColorUpdated, new MapColorUpdatePacketHandler(customColors));
		Settings.NotifyBySettingName("SongSpeed", UpdateLocalSongSpeed);
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Combine(EditorScaleController.EditorScaleChangedEvent, new Action<float>(OnEditorScaleChanged));
		BeatmapActionContainer.ActionCreatedEvent += BeatmapActionContainer_ActionCreatedEvent;
		RegisterPacketHandler<ActionCreatedPacketHandler>(PacketId.ActionCreated);
		BeatmapActionContainer.ActionUndoEvent += BeatmapActionContainer_ActionUndoEvent;
		RegisterPacketHandler<ActionUndoPacketHandler>(PacketId.ActionUndo);
		BeatmapActionContainer.ActionRedoEvent += BeatmapActionContainer_ActionRedoEvent;
		RegisterPacketHandler<ActionRedoPacketHandler>(PacketId.ActionRedo);
		multiTimelineController = new MultiTimelineController(this, bookmarkManager);
		BroadcastPose();
	}

	public void UnsubscribeFromCollectionEvents()
	{
		AudioTimeSyncController obj = audioTimeSyncController;
		obj.PlayToggle = (Action<bool>)Delegate.Remove(obj.PlayToggle, new Action<bool>(OnTogglePlaying));
		bookmarkManager.BookmarkAdded -= MultiNetListener_ObjectSpawnedEvent;
		bookmarkManager.BookmarkDeleted -= MultiNetListener_ObjectDeletedEvent;
		customColors.CustomColorsUpdatedEvent -= CustomColors_CustomColorsUpdatedEvent;
		Settings.ClearSettingNotifications("SongSpeed");
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Remove(EditorScaleController.EditorScaleChangedEvent, new Action<float>(OnEditorScaleChanged));
		BeatmapActionContainer.ActionCreatedEvent -= BeatmapActionContainer_ActionCreatedEvent;
		BeatmapActionContainer.ActionUndoEvent -= BeatmapActionContainer_ActionUndoEvent;
		BeatmapActionContainer.ActionRedoEvent -= BeatmapActionContainer_ActionRedoEvent;
	}

	private void OnEditorScaleChanged(float editorScale)
	{
		UpdateCachedPoses();
	}

	private void OnTogglePlaying(bool isPlaying)
	{
		BroadcastPose();
	}

	private void UpdateLocalSongSpeed(object obj)
	{
		localSongSpeed = (float)obj / 10f;
		BroadcastPose();
	}

	private void MultiNetListener_ObjectSpawnedEvent(BaseObject obj)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)3);
		netDataWriter.Put((byte)obj.ObjectType);
		netDataWriter.Put(obj);
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	private void MultiNetListener_ObjectDeletedEvent(BaseObject obj)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)4);
		netDataWriter.Put((byte)obj.ObjectType);
		netDataWriter.Put(obj);
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	private void BeatmapActionContainer_ActionCreatedEvent(BeatmapAction obj)
	{
		obj.Identity = Settings.Instance.MultiSettings.LocalIdentity;
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)7);
		netDataWriter.PutBeatmapAction(obj);
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	private void BeatmapActionContainer_ActionUndoEvent(BeatmapAction obj)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)8);
		netDataWriter.Put(obj.Guid.ToString());
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	private void BeatmapActionContainer_ActionRedoEvent(BeatmapAction obj)
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)9);
		netDataWriter.Put(obj.Guid.ToString());
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}

	private void CustomColors_CustomColorsUpdatedEvent()
	{
		NetDataWriter netDataWriter = new NetDataWriter();
		netDataWriter.Put(0);
		netDataWriter.Put((byte)10);
		netDataWriter.Put(customColors.CreatePacketFromColors());
		NetManager.SendToAll(netDataWriter, DeliveryMethod.ReliableOrdered);
	}
}
