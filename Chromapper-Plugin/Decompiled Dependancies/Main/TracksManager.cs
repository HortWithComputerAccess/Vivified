using System;
using System.Collections.Generic;
using Beatmap.Animations;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Serialization;

public class TracksManager : MonoBehaviour
{
	[FormerlySerializedAs("TrackPrefab")]
	[SerializeField]
	private GameObject trackPrefab;

	[FormerlySerializedAs("TracksParent")]
	[SerializeField]
	private Transform tracksParent;

	[FormerlySerializedAs("events")]
	[SerializeField]
	private EventGridContainer eventGrid;

	[SerializeField]
	private AudioTimeSyncController atsc;

	private readonly Dictionary<Vector3, Track> loadedTracks = new Dictionary<Vector3, Track>();

	private readonly Dictionary<string, TrackAnimator> animationTracks = new Dictionary<string, TrackAnimator>();

	private readonly List<BeatmapObjectContainerCollection> objectContainerCollections = new List<BeatmapObjectContainerCollection>();

	private float position;

	public float LowestRotation { get; private set; }

	public float HighestRotation { get; private set; }

	private void Start()
	{
		objectContainerCollections.Add(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Note));
		objectContainerCollections.Add(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Obstacle));
		objectContainerCollections.Add(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Arc));
		objectContainerCollections.Add(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Chain));
		ObjectContainer.FlaggedForDeletionEvent = (Action<ObjectContainer, bool, string>)Delegate.Combine(ObjectContainer.FlaggedForDeletionEvent, new Action<ObjectContainer, bool, string>(FlaggedForDeletion));
	}

	private void OnDestroy()
	{
		ObjectContainer.FlaggedForDeletionEvent = (Action<ObjectContainer, bool, string>)Delegate.Remove(ObjectContainer.FlaggedForDeletionEvent, new Action<ObjectContainer, bool, string>(FlaggedForDeletion));
	}

	private void FlaggedForDeletion(ObjectContainer obj, bool _, string __)
	{
		if (!(obj is EventContainer) || !(obj.ObjectData as BaseEvent).IsLaneRotationEvent())
		{
			return;
		}
		foreach (BeatmapObjectContainerCollection objectContainerCollection in objectContainerCollections)
		{
			objectContainerCollection.RefreshPool();
		}
	}

	public Track CreateTrack(Vector3 rotation)
	{
		if (loadedTracks.TryGetValue(rotation, out var value))
		{
			return value;
		}
		value = UnityEngine.Object.Instantiate(trackPrefab, tracksParent).GetComponent<Track>();
		value.gameObject.name = $"Track [{rotation.x}, {rotation.y}, {rotation.z}]";
		value.AssignRotationValue(rotation);
		value.UpdatePosition(position);
		loadedTracks.Add(rotation, value);
		return value;
	}

	public Track CreateTrack(float rotation)
	{
		float y = FloatModulo(rotation, 360f);
		Vector3 rotation2 = new Vector3(0f, y, 0f);
		return CreateTrack(rotation2);
	}

	public TrackAnimator GetAnimationTrack(string name)
	{
		if (animationTracks.TryGetValue(name, out var value))
		{
			return value;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(trackPrefab, tracksParent);
		gameObject.name = name;
		value = gameObject.AddComponent<TrackAnimator>();
		value.enabled = false;
		value.Atsc = atsc;
		value.Track = gameObject.GetComponent<Track>();
		animationTracks.Add(name, value);
		return value;
	}

	public Track CreateIndividualTrack(BaseGrid obj)
	{
		float num = -1f * obj.JsonTime * EditorScaleController.EditorScale;
		Track component = UnityEngine.Object.Instantiate(trackPrefab, tracksParent).GetComponent<Track>();
		component.UpdatePosition(num);
		float rotationAtTime = GetRotationAtTime(obj.SongBpmTime);
		component.AssignRotationValue(obj.CustomWorldRotation ?? ((JSONNode)new Vector3(0f, rotationAtTime, 0f)));
		component.gameObject.name = $"Track Object {obj.JsonTime}";
		return component;
	}

	public Track GetTrackAtTime(float beatInSongBpm)
	{
		if (!Settings.Instance.RotateTrack)
		{
			return CreateTrack(0f);
		}
		float rotationAtTime = GetRotationAtTime(beatInSongBpm);
		return CreateTrack(rotationAtTime);
	}

	public float GetRotationAtTime(float beatInSongBpm)
	{
		float num = 0f;
		foreach (BaseEvent allRotationEvent in eventGrid.AllRotationEvents)
		{
			if (!(allRotationEvent.SongBpmTime > beatInSongBpm + 0.001f) && (!Mathf.Approximately(allRotationEvent.SongBpmTime, beatInSongBpm) || allRotationEvent.Type != 15))
			{
				num += allRotationEvent.Rotation;
				if (num < LowestRotation)
				{
					LowestRotation = num;
				}
				if (num > HighestRotation)
				{
					HighestRotation = num;
				}
			}
		}
		return num;
	}

	public void RefreshTracks()
	{
		foreach (BeatmapObjectContainerCollection objectContainerCollection in objectContainerCollections)
		{
			foreach (ObjectContainer value in objectContainerCollection.LoadedContainers.Values)
			{
				if (!(value is ObstacleContainer { IsRotatedByNoodleExtensions: not false }) && (!(value.Animator != null) || !value.Animator.AnimatedTrack))
				{
					GetTrackAtTime(value.ObjectData.SongBpmTime).AttachContainer(value);
					value.UpdateGridPosition();
				}
			}
		}
	}

	private float FloatModulo(float x, float m)
	{
		return x - Mathf.Floor(x / m) * m + m - Mathf.Floor((x - Mathf.Floor(x / m) * m + m) / m) * m;
	}

	public void UpdatePosition(float position)
	{
		this.position = position;
		foreach (Track value in loadedTracks.Values)
		{
			value.UpdatePosition(position);
		}
	}
}
