using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using Beatmap.Enums;
using SimpleJSON;
using UnityEngine;

public abstract class BeatmapObjectContainerCollection : MonoBehaviour
{
	public static readonly int ChunkSize = 5;

	public static float Epsilon = 0.001f;

	private static BookmarkManager bookmarkManager;

	private static readonly Dictionary<ObjectType, BeatmapObjectContainerCollection> loadedCollections = new Dictionary<ObjectType, BeatmapObjectContainerCollection>();

	public AudioTimeSyncController AudioTimeSyncController;

	public BeatmapObjectCallbackController SpawnCallbackController;

	public BeatmapObjectCallbackController DespawnCallbackController;

	public Transform GridTransform;

	public Transform PoolTransform;

	public bool UseChunkLoadingWhenPlaying;

	public int ChunksLoadedWhilePlaying = 2;

	public bool IgnoreTrackFilter;

	private readonly Queue<ObjectContainer> pooledContainers = new Queue<ObjectContainer>();

	public Dictionary<BaseObject, ObjectContainer> LoadedContainers = new Dictionary<BaseObject, ObjectContainer>();

	public List<BaseObject> ObjectsWithContainers = new List<BaseObject>();

	private float previousAtscBeat = -1f;

	private int previousChunk = -1;

	private static BookmarkManager bookmarkManagerInstance => bookmarkManager = ((bookmarkManager != null) ? bookmarkManager : UnityEngine.Object.FindObjectOfType<BookmarkManager>());

	[Obsolete("LoadedObjects allocates a copy of the backing list of objects. Please avoid this unless you absolutely cannot grab a more precise type.")]
	public abstract List<BaseObject> LoadedObjects { get; }

	public static string TrackFilterID { get; private set; }

	public abstract ObjectType ContainerType { get; }

	public event Action<BaseObject> ContainerSpawnedEvent;

	public event Action<BaseObject> ContainerDespawnedEvent;

	private void Awake()
	{
		ObjectContainer.FlaggedForDeletionEvent = (Action<ObjectContainer, bool, string>)Delegate.Combine(ObjectContainer.FlaggedForDeletionEvent, new Action<ObjectContainer, bool, string>(DeleteObject));
		loadedCollections[ContainerType] = this;
		SubscribeToCallbacks();
	}

	private void Start()
	{
		UpdateEpsilon(Settings.Instance.TimeValueDecimalPrecision);
		Settings.NotifyBySettingName("TimeValueDecimalPrecision", UpdateEpsilon);
	}

	internal virtual void LateUpdate()
	{
		if ((!AudioTimeSyncController.IsPlaying || UseChunkLoadingWhenPlaying) && AudioTimeSyncController.CurrentSongBpmTime != previousAtscBeat)
		{
			previousAtscBeat = AudioTimeSyncController.CurrentSongBpmTime;
			int num = (int)Math.Round((double)previousAtscBeat / (double)ChunkSize, MidpointRounding.AwayFromZero);
			if (num != previousChunk)
			{
				RefreshPool();
				previousChunk = num;
			}
		}
	}

	private void OnDestroy()
	{
		ObjectContainer.FlaggedForDeletionEvent = (Action<ObjectContainer, bool, string>)Delegate.Remove(ObjectContainer.FlaggedForDeletionEvent, new Action<ObjectContainer, bool, string>(DeleteObject));
		loadedCollections.Remove(ContainerType);
		UnsubscribeToCallbacks();
	}

	private void UpdateEpsilon(object precision)
	{
		Epsilon = 1f / Mathf.Pow(10f, (int)precision);
		JSONNumber.DecimalPrecision = (int)precision;
	}

	public static BeatmapObjectContainerCollection GetCollectionForType(ObjectType type)
	{
		loadedCollections.TryGetValue(type, out var value);
		return value;
	}

	public static T GetCollectionForType<T>(ObjectType type) where T : BeatmapObjectContainerCollection
	{
		loadedCollections.TryGetValue(type, out var value);
		return value as T;
	}

	public static T GetCollectionForType<T, TBaseObject>() where T : BeatmapObjectContainerCollection where TBaseObject : BaseObject
	{
		Type typeFromHandle = typeof(TBaseObject);
		if ((object)typeFromHandle != null)
		{
			ObjectType objectType;
			if (typeFromHandle == typeof(BaseNote))
			{
				objectType = ObjectType.Note;
			}
			else if (typeFromHandle == typeof(BaseObstacle))
			{
				objectType = ObjectType.Obstacle;
			}
			else if (typeFromHandle == typeof(BaseEvent))
			{
				objectType = ObjectType.Event;
			}
			else if (typeFromHandle == typeof(BaseArc))
			{
				objectType = ObjectType.Arc;
			}
			else if (typeFromHandle == typeof(BaseChain))
			{
				objectType = ObjectType.Chain;
			}
			else if (typeFromHandle == typeof(BaseBpmEvent))
			{
				objectType = ObjectType.BpmChange;
			}
			else if (typeFromHandle == typeof(BaseCustomEvent))
			{
				objectType = ObjectType.CustomEvent;
			}
			else if (typeFromHandle == typeof(BaseBookmark))
			{
				objectType = ObjectType.Bookmark;
			}
			else if (typeFromHandle == typeof(BaseNJSEvent))
			{
				objectType = ObjectType.NJSEvent;
			}
			else
			{
				if (!(typeFromHandle == typeof(BaseEnvironmentEnhancement)))
				{
					goto IL_00fb;
				}
				objectType = ObjectType.EnvironmentEnhancement;
			}
			ObjectType key = objectType;
			loadedCollections.TryGetValue(key, out var value);
			return value as T;
		}
		goto IL_00fb;
		IL_00fb:
		throw new ArgumentException("TBaseObject");
	}

	public static void RefreshAllPools(bool forceRefresh = false)
	{
		foreach (BeatmapObjectContainerCollection value in loadedCollections.Values)
		{
			value.RefreshPool(forceRefresh);
		}
	}

	public virtual void RefreshPool(bool forceRefresh = false)
	{
		float num = Mathf.Pow(10f, -9f);
		if (AudioTimeSyncController.IsPlaying)
		{
			float num2 = (UseChunkLoadingWhenPlaying ? ((float)(ChunksLoadedWhilePlaying * ChunkSize)) : SpawnCallbackController.Offset);
			float num3 = (UseChunkLoadingWhenPlaying ? ((float)(-ChunksLoadedWhilePlaying * ChunkSize)) : DespawnCallbackController.Offset);
			RefreshPool(AudioTimeSyncController.CurrentSongBpmTime + num3 - num, AudioTimeSyncController.CurrentSongBpmTime + num2 + num, forceRefresh);
		}
		else
		{
			int num4 = (int)Math.Round((double)previousAtscBeat / (double)ChunkSize, MidpointRounding.AwayFromZero);
			int num5 = Mathf.RoundToInt(Settings.Instance.ChunkDistance / 2);
			RefreshPool((float)((num4 - num5) * ChunkSize) - num, (float)((num4 + num5) * ChunkSize) + num, forceRefresh);
		}
	}

	public abstract void RefreshPool(float lowerBound, float upperBound, bool forceRefresh = false);

	protected void CreateContainerFromPool(BaseObject obj)
	{
		if (!obj.HasAttachedContainer)
		{
			if (!pooledContainers.Any())
			{
				CreateNewObject();
			}
			ObjectContainer objectContainer = pooledContainers.Dequeue();
			objectContainer.ObjectData = obj;
			objectContainer.transform.localEulerAngles = Vector3.zero;
			objectContainer.UpdateGridPosition();
			objectContainer.SafeSetActive(active: true);
			UpdateContainerData(objectContainer, obj);
			objectContainer.SetOutlineColor(SelectionController.SelectedColor, automaticallyShowOutline: false);
			objectContainer.OutlineVisible = SelectionController.IsObjectSelected(obj);
			PluginLoader.BroadcastEvent<ObjectLoadedAttribute, ObjectContainer>(objectContainer);
			LoadedContainers.Add(obj, objectContainer);
			ObjectsWithContainers.Add(obj);
			obj.HasAttachedContainer = true;
			OnContainerSpawn(objectContainer, obj);
			this.ContainerSpawnedEvent?.Invoke(obj);
		}
	}

	protected internal void RecycleContainer(BaseObject obj)
	{
		if (obj.HasAttachedContainer)
		{
			ObjectContainer objectContainer = LoadedContainers[obj];
			objectContainer.ObjectData = null;
			objectContainer.SafeSetActive(active: false);
			LoadedContainers.Remove(obj);
			ObjectsWithContainers.Remove(obj);
			pooledContainers.Enqueue(objectContainer);
			OnContainerDespawn(objectContainer, obj);
			obj.HasAttachedContainer = false;
			this.ContainerDespawnedEvent?.Invoke(obj);
		}
	}

	private void CreateNewObject()
	{
		ObjectContainer objectContainer = CreateContainer();
		objectContainer.gameObject.SetActive(value: false);
		objectContainer.Setup();
		objectContainer.transform.SetParent(GridTransform);
		pooledContainers.Enqueue(objectContainer);
	}

	public void RemoveConflictingObjects(IEnumerable<BaseObject> newObjects)
	{
		RemoveConflictingObjects(newObjects, out var _);
	}

	public abstract void RemoveConflictingObjects(IEnumerable<BaseObject> newObjects, out List<BaseObject> conflicting);

	public void DeleteObject(ObjectContainer obj, bool triggersAction = true, string comment = "No comment.", bool inCollectionOfDeletes = false)
	{
		DeleteObject(obj.ObjectData, triggersAction, refreshesPool: true, comment);
	}

	public void DeleteObject(ObjectContainer obj, bool triggersAction = true, string comment = "No comment.")
	{
		DeleteObject(obj.ObjectData, triggersAction, refreshesPool: true, comment);
	}

	public abstract void DeleteObject(BaseObject obj, bool triggersAction = true, bool refreshesPool = true, string comment = "No comment.", bool inCollectionOfDeletes = false, bool deselect = true);

	public abstract void SilentRemoveObject(BaseObject obj);

	protected void SetTrackFilter()
	{
		PersistentUI.Instance.ShowInputBox("Filter notes and obstacles shown while editing to a certain track ID.\n\nIf you dont know what you're doing, turn back now.", HandleTrackFilter);
	}

	private void HandleTrackFilter(string res)
	{
		TrackFilterID = ((string.IsNullOrEmpty(res) || string.IsNullOrWhiteSpace(res)) ? null : res);
		RefreshAllPools(forceRefresh: true);
	}

	public void SpawnObject(BaseObject obj, bool removeConflicting = true, bool refreshesPool = true, bool inCollectionOfSpawns = false)
	{
		SpawnObject(obj, out var _, removeConflicting, refreshesPool, inCollectionOfSpawns);
	}

	public abstract void SpawnObject(BaseObject obj, out List<BaseObject> conflicting, bool removeConflicting = true, bool refreshesPool = true, bool inCollectionOfSpawns = false);

	public abstract bool ContainsObject(BaseObject obj);

	public static void RefreshFutureObjectsPosition(float jsonTime)
	{
		foreach (ObjectType item in new List<ObjectType>
		{
			ObjectType.BpmChange,
			ObjectType.Note,
			ObjectType.Event,
			ObjectType.Obstacle,
			ObjectType.CustomNote,
			ObjectType.CustomEvent,
			ObjectType.Arc,
			ObjectType.Chain,
			ObjectType.Bookmark,
			ObjectType.Waypoint,
			ObjectType.NJSEvent
		})
		{
			BeatmapObjectContainerCollection collectionForType = GetCollectionForType(item);
			if (collectionForType == null)
			{
				continue;
			}
			foreach (BaseObject loadedObject in collectionForType.LoadedObjects)
			{
				if (loadedObject.JsonTime > jsonTime)
				{
					loadedObject.RecomputeSongBpmTime();
				}
				else if (collectionForType is ChainGridContainer || collectionForType is ArcGridContainer)
				{
					if ((loadedObject as BaseSlider).TailJsonTime > jsonTime)
					{
						loadedObject.RecomputeSongBpmTime();
					}
				}
				else if (collectionForType is ObstacleGridContainer && (loadedObject as BaseObstacle).Duration + loadedObject.JsonTime > jsonTime)
				{
					loadedObject.RecomputeSongBpmTime();
				}
			}
			foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer in collectionForType.LoadedContainers)
			{
				if (loadedContainer.Key.JsonTime > jsonTime)
				{
					loadedContainer.Value.UpdateGridPosition();
				}
				else if (collectionForType is ObstacleGridContainer)
				{
					if (loadedContainer.Key.JsonTime + (loadedContainer.Key as BaseObstacle).Duration > jsonTime)
					{
						loadedContainer.Value.UpdateGridPosition();
					}
				}
				else if ((collectionForType is ChainGridContainer || collectionForType is ArcGridContainer) && (loadedContainer.Key as BaseSlider).TailJsonTime > jsonTime)
				{
					loadedContainer.Value.UpdateGridPosition();
				}
			}
		}
		foreach (BookmarkContainer bookmarkContainer in bookmarkManagerInstance.bookmarkContainers)
		{
			if (bookmarkContainer.Data.JsonTime > jsonTime)
			{
				bookmarkContainer.Data.RecomputeSongBpmTime();
			}
		}
		bookmarkManagerInstance.RefreshBookmarkTimelinePositions();
		bookmarkManagerInstance.RefreshBookTooltips();
	}

	protected virtual void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
	}

	protected virtual void OnObjectDelete(BaseObject obj, bool inCollection = false)
	{
	}

	protected virtual void OnObjectSpawned(BaseObject obj, bool inCollection = false)
	{
	}

	protected virtual void OnContainerSpawn(ObjectContainer container, BaseObject obj)
	{
	}

	protected virtual void OnContainerDespawn(ObjectContainer container, BaseObject obj)
	{
	}

	public virtual void DoPostObjectsSpawnedWorkflow()
	{
	}

	public virtual void DoPostObjectsDeleteWorkflow()
	{
		RefreshPool();
	}

	public abstract ObjectContainer CreateContainer();

	internal abstract void SubscribeToCallbacks();

	internal abstract void UnsubscribeToCallbacks();
}
public abstract class BeatmapObjectContainerCollection<T> : BeatmapObjectContainerCollection where T : BaseObject
{
	public List<T> MapObjects = new List<T>();

	[Obsolete("LoadedObjects allocates a copy of the backing list of objects. Please avoid this unless you absolutely cannot grab a more precise type.")]
	public override List<BaseObject> LoadedObjects => MapObjects.ConvertAll((Converter<T, BaseObject>)((T it) => it));

	public event Action<T> ObjectSpawnedEvent;

	public event Action<T> ObjectDeletedEvent;

	public Span<T> GetBetween(float jsonTime, float jsonTime2)
	{
		if (MapObjects.Count == 0)
		{
			return Span<T>.Empty;
		}
		Span<T> span = MapObjects.AsSpan();
		int num = span.BinarySearchBy(jsonTime, (T obj) => obj.JsonTime);
		int num2 = span.BinarySearchBy(jsonTime2, (T obj) => obj.JsonTime);
		if (num < 0)
		{
			num = ~num;
		}
		if (num2 < 0)
		{
			num2 = ~num2;
		}
		while (num > 0 && span[num].JsonTime >= jsonTime)
		{
			num--;
		}
		if (span[num].JsonTime < jsonTime)
		{
			num++;
		}
		for (; num2 < span.Length && span[num2].JsonTime <= jsonTime2; num2++)
		{
		}
		int num3 = num2 - num;
		if (num3 <= 0)
		{
			return Span<T>.Empty;
		}
		return span.Slice(num, num3);
	}

	public void RemoveConflictingObjects(IEnumerable<T> newObjects)
	{
		RemoveConflictingObjects(newObjects, out var _);
	}

	public override void RemoveConflictingObjects(IEnumerable<BaseObject> newObjects, out List<BaseObject> conflicting)
	{
		RemoveConflictingObjects(newObjects.OfType<T>(), out var conflicting2);
		conflicting = conflicting2.ConvertAll((Converter<T, BaseObject>)((T it) => it));
	}

	public void RemoveConflictingObjects(IEnumerable<T> newObjects, out List<T> conflicting)
	{
		conflicting = new List<T>();
		foreach (T newObject in newObjects)
		{
			Debug.Log($"Performing conflicting check at {newObject.JsonTime}");
			Span<T> between = GetBetween(newObject.JsonTime - 0.1f, newObject.JsonTime + 0.1f);
			for (int i = 0; i < between.Length; i++)
			{
				T val = between[i];
				if (val.IsConflictingWith(newObject) && newObject != val)
				{
					conflicting.Add(val);
				}
			}
		}
		conflicting.ForEach(delegate(T conflict)
		{
			DeleteObject(conflict, triggersAction: false, refreshesPool: false);
		});
		Debug.Log($"Removed {conflicting.Count} conflicting {ContainerType}s.");
	}

	public override void RefreshPool(float lowerBound, float upperBound, bool forceRefresh = false)
	{
		Span<T> span = MapObjects.AsSpan();
		if (forceRefresh)
		{
			while (ObjectsWithContainers.Count > 0)
			{
				RecycleContainer(ObjectsWithContainers[0]);
			}
		}
		else
		{
			Span<BaseObject> span2 = ObjectsWithContainers.AsSpan();
			for (int num = span2.Length - 1; num >= 0; num--)
			{
				BaseObject baseObject = span2[num];
				BaseObject baseObject2 = baseObject;
				if (!(baseObject2 is BaseObstacle baseObstacle))
				{
					if (!(baseObject2 is BaseSlider baseSlider))
					{
						if (baseObject2 == null)
						{
							continue;
						}
					}
					else if (baseSlider.SongBpmTime > upperBound || baseSlider.TailSongBpmTime < lowerBound)
					{
						goto IL_00d7;
					}
				}
				else if (baseObstacle.SongBpmTime > upperBound || baseObstacle.SongBpmTime + baseObstacle.Duration < lowerBound)
				{
					goto IL_00d7;
				}
				if (!(baseObject.SongBpmTime > upperBound) && !(baseObject.SongBpmTime < lowerBound) && baseObject.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
				{
					continue;
				}
				goto IL_00d7;
				IL_00d7:
				RecycleContainer(baseObject);
			}
		}
		if (span.Length == 0)
		{
			return;
		}
		int num2 = span.BinarySearchBy(lowerBound, (T obj) => obj.SongBpmTime);
		int num3 = span.BinarySearchBy(upperBound, (T obj) => obj.SongBpmTime);
		if (num2 < 0)
		{
			num2 = ~num2;
		}
		if (num3 < 0)
		{
			num3 = ~num3;
		}
		for (; num3 < span.Length && span[num3].SongBpmTime <= upperBound; num3++)
		{
		}
		int length = num3 - num2;
		Span<T> span3 = span.Slice(num2, length);
		for (int num4 = 0; num4 < span3.Length; num4++)
		{
			T val = span3[num4];
			if (val.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				CreateContainerFromPool(val);
			}
		}
		T val2 = span[0];
		if (!(val2 is BaseObstacle) && !(val2 is BaseSlider))
		{
			return;
		}
		for (int num5 = 0; num5 < num2; num5++)
		{
			T val3 = span[num5];
			if (val3.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				if (val3 is BaseObstacle baseObstacle2 && baseObstacle2.SongBpmTime < lowerBound && baseObstacle2.SongBpmTime + baseObstacle2.Duration >= lowerBound)
				{
					CreateContainerFromPool(val3);
				}
				else if (val3 is BaseSlider baseSlider2 && baseSlider2.SongBpmTime < lowerBound && baseSlider2.TailSongBpmTime >= lowerBound)
				{
					CreateContainerFromPool(val3);
				}
			}
		}
	}

	public override void DeleteObject(BaseObject obj, bool triggersAction = true, bool refreshesPool = true, string comment = "No comment.", bool inCollectionOfDeletes = false, bool deselect = true)
	{
		if (obj is T obj2)
		{
			DeleteObject(obj2, triggersAction, refreshesPool, comment, inCollectionOfDeletes, deselect);
		}
	}

	public void DeleteObject(T obj, bool triggersAction = true, bool refreshesPool = true, string comment = "No comment.", bool inCollectionOfDeletes = false, bool deselect = true)
	{
		if (TryBinarySearch(obj, out var index))
		{
			T obj2 = MapObjects[index];
			RecycleContainer(obj2);
			MapObjects.RemoveAt(index);
			if (deselect)
			{
				SelectionController.Deselect(obj2, triggersAction);
			}
			if (triggersAction)
			{
				BeatmapActionContainer.AddAction(new BeatmapObjectDeletionAction(obj2, comment));
			}
			if (refreshesPool)
			{
				RefreshPool();
			}
			OnObjectDelete(obj2, inCollectionOfDeletes);
			this.ObjectDeletedEvent?.Invoke(obj2);
		}
	}

	public override void SilentRemoveObject(BaseObject obj)
	{
		if (obj is T tObj && TryBinarySearch(tObj, out var index))
		{
			MapObjects.RemoveAt(index);
		}
	}

	private bool TryBinarySearch(T tObj, out int index)
	{
		index = MapObjects.BinarySearch(tObj);
		if (index < 0)
		{
			Debug.LogError("This object is not in the collection and appears to be a ghost. Please report this.");
			return false;
		}
		if (MapObjects[index] == tObj)
		{
			return true;
		}
		if (MapObjects[index].CompareTo(tObj) == 0)
		{
			return true;
		}
		for (int i = index + 1; i < MapObjects.Count - 1 && MapObjects[i].JsonTime <= tObj.JsonTime; i++)
		{
			if (MapObjects[i].CompareTo(tObj) == 0)
			{
				index = i;
				return true;
			}
		}
		int num = index - 1;
		while (num > 0 && MapObjects[num].JsonTime >= tObj.JsonTime)
		{
			if (MapObjects[num].CompareTo(tObj) == 0)
			{
				index = num;
				return true;
			}
			num--;
		}
		Debug.LogError("Binary Search returned no matching object. Please report this.");
		return false;
	}

	public override void SpawnObject(BaseObject obj, out List<BaseObject> conflicting, bool removeConflicting = true, bool refreshesPool = true, bool inCollectionOfSpawns = false)
	{
		conflicting = new List<BaseObject>();
		if (obj is T obj2)
		{
			SpawnObject(obj2, out var conflicting2, removeConflicting, refreshesPool, inCollectionOfSpawns);
			for (int i = 0; i < conflicting2.Count; i++)
			{
				conflicting.Add(conflicting2[i]);
			}
		}
	}

	public void SpawnObject(T obj, bool removeConflicting = true, bool refreshesPool = true, bool inCollectionOfSpawns = false)
	{
		SpawnObject(obj, out var _, removeConflicting, refreshesPool, inCollectionOfSpawns);
	}

	public void SpawnObject(T obj, out List<T> conflicting, bool removeConflicting = true, bool refreshesPool = true, bool inCollectionOfSpawns = false)
	{
		obj.WriteCustom();
		if (removeConflicting)
		{
			RemoveConflictingObjects(new T[1] { obj }, out conflicting);
		}
		else
		{
			conflicting = new List<T>();
		}
		int num = MapObjects.BinarySearch(obj);
		int index = ((num >= 0) ? num : (~num));
		MapObjects.Insert(index, obj);
		OnObjectSpawned(obj, inCollectionOfSpawns);
		this.ObjectSpawnedEvent?.Invoke(obj);
		if (refreshesPool)
		{
			RefreshPool();
		}
	}

	public override bool ContainsObject(BaseObject obj)
	{
		if (obj is T obj2)
		{
			return ContainsObject(obj2);
		}
		return false;
	}

	public bool ContainsObject(T obj)
	{
		return MapObjects.BinarySearch(obj) >= 0;
	}
}
