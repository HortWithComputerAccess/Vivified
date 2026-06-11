using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using LiteNetLib.Utils;

public abstract class BeatmapAction : INetSerializable
{
	public bool Active = true;

	public bool Networked;

	public string Comment = "No comment.";

	public Guid Guid = Guid.NewGuid();

	public IEnumerable<BaseObject> Data;

	public MapperIdentityPacket Identity;

	internal bool inCollection;

	internal bool affectsSeveralObjects;

	public BeatmapAction()
	{
		Networked = true;
	}

	public BeatmapAction(IEnumerable<BaseObject> data, string comment = "No comment.")
	{
		Data = data;
		Comment = comment;
	}

	public abstract void Undo(BeatmapActionContainer.BeatmapActionParams param);

	public abstract void Redo(BeatmapActionContainer.BeatmapActionParams param);

	public abstract void Serialize(NetDataWriter writer);

	public abstract void Deserialize(NetDataReader reader);

	public virtual BaseObject DoesInvolveObject(BaseObject obj)
	{
		if (!Data.Any((BaseObject it) => it.IsConflictingWith(obj)))
		{
			return null;
		}
		return obj;
	}

	protected void RefreshPools(IEnumerable<BaseObject> data)
	{
		foreach (BaseObject item in data.DistinctBy((BaseObject x) => x.ObjectType))
		{
			BeatmapObjectContainerCollection collectionForType = BeatmapObjectContainerCollection.GetCollectionForType(item.ObjectType);
			collectionForType.RefreshPool(forceRefresh: true);
			if (collectionForType is BPMChangeGridContainer bPMChangeGridContainer)
			{
				bPMChangeGridContainer.RefreshModifiedBeat();
			}
		}
	}

	protected void SpawnObject(BaseObject obj, bool removeConflicting = false, bool refreshesPool = false)
	{
		BeatmapObjectContainerCollection.GetCollectionForType(obj.ObjectType).SpawnObject(obj, removeConflicting, refreshesPool, affectsSeveralObjects);
	}

	protected void DeleteObject(BaseObject obj, bool refreshesPool = true)
	{
		BeatmapObjectContainerCollection collectionForType = BeatmapObjectContainerCollection.GetCollectionForType(obj.ObjectType);
		if (Networked && !collectionForType.ContainsObject(obj))
		{
			collectionForType.RemoveConflictingObjects(new BaseObject[1] { obj });
		}
		else
		{
			collectionForType.DeleteObject(obj, triggersAction: false, refreshesPool, "No comment.", affectsSeveralObjects);
		}
	}

	protected void SerializeBeatmapObjectList(NetDataWriter writer, IEnumerable<BaseObject> list)
	{
		writer.Put(list.Count());
		foreach (BaseObject item in list)
		{
			writer.PutBeatmapObject(item);
		}
	}

	protected IEnumerable<BaseObject> DeserializeBeatmapObjectList(NetDataReader reader)
	{
		int num = reader.GetInt();
		List<BaseObject> list = new List<BaseObject>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.GetBeatmapObject());
		}
		return list;
	}

	protected virtual void RefreshEventAppearance()
	{
		List<BaseEvent> list = Data.OfType<BaseEvent>().ToList();
		if (!list.Any())
		{
			return;
		}
		EventGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<EventGridContainer>(ObjectType.Event);
		collectionForType.MarkEventsToBeRelinked(list);
		collectionForType.LinkAllLightEvents();
		foreach (BaseEvent item in list)
		{
			if (item.Prev != null && collectionForType.LoadedContainers.TryGetValue(item.Prev, out var value))
			{
				(value as EventContainer).RefreshAppearance();
			}
			if (collectionForType.LoadedContainers.TryGetValue(item, out var value2))
			{
				(value2 as EventContainer).RefreshAppearance();
			}
		}
	}
}
