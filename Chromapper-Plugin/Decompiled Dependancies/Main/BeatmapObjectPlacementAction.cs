using System.Collections.Generic;
using Beatmap.Base;
using LiteNetLib.Utils;

public class BeatmapObjectPlacementAction : BeatmapAction
{
	private IEnumerable<BaseObject> removedConflictObjects;

	public BeatmapObjectPlacementAction()
	{
	}

	public BeatmapObjectPlacementAction(IEnumerable<BaseObject> placedContainers, IEnumerable<BaseObject> conflictingObjects, string comment)
		: base(placedContainers, comment)
	{
		removedConflictObjects = conflictingObjects;
	}

	public BeatmapObjectPlacementAction(BaseObject placedObject, IEnumerable<BaseObject> conflictingObject, string comment)
		: base(new BaseObject[1] { placedObject }, comment)
	{
		removedConflictObjects = conflictingObject;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			DeleteObject(datum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		RefreshPools(Data);
		foreach (BaseObject removedConflictObject in removedConflictObjects)
		{
			SpawnObject(removedConflictObject);
		}
		RefreshPools(removedConflictObjects);
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject removedConflictObject in removedConflictObjects)
		{
			DeleteObject(removedConflictObject, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		RefreshPools(removedConflictObjects);
		foreach (BaseObject datum in Data)
		{
			SpawnObject(datum);
		}
		RefreshPools(Data);
	}

	public override void Serialize(NetDataWriter writer)
	{
		foreach (BaseObject datum in Data)
		{
			datum.WriteCustom();
		}
		SerializeBeatmapObjectList(writer, Data);
		SerializeBeatmapObjectList(writer, removedConflictObjects);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Data = DeserializeBeatmapObjectList(reader);
		removedConflictObjects = DeserializeBeatmapObjectList(reader);
	}
}
