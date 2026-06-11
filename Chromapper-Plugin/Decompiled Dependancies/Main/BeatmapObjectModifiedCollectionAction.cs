using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using LiteNetLib.Utils;

public class BeatmapObjectModifiedCollectionAction : BeatmapAction
{
	private List<BaseObject> editedObjects;

	private List<BaseObject> originalObjects;

	private readonly float firstBpmEventJsonTime;

	public BeatmapObjectModifiedCollectionAction()
	{
	}

	public BeatmapObjectModifiedCollectionAction(List<BaseObject> editedObjects, List<BaseObject> originalObjects, string comment = "No comment.")
		: base(editedObjects.Concat(originalObjects), comment)
	{
		this.editedObjects = editedObjects;
		this.originalObjects = originalObjects;
		firstBpmEventJsonTime = Data.OfType<BaseBpmEvent>().DefaultIfEmpty().Min((BaseBpmEvent x) => x?.JsonTime ?? (-1f));
	}

	public override BaseObject DoesInvolveObject(BaseObject obj)
	{
		BaseObject baseObject = editedObjects.Find((BaseObject x) => x == obj);
		if (baseObject == null)
		{
			baseObject = originalObjects.Find((BaseObject x) => x == obj);
		}
		return baseObject;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject editedObject in editedObjects)
		{
			DeleteObject(editedObject, refreshesPool: false);
		}
		foreach (BaseObject originalObject in originalObjects)
		{
			SpawnObject(originalObject);
			if (!Networked)
			{
				SelectionController.Select(originalObject, addsToSelection: true, automaticallyRefreshes: true, addActionEvent: false);
			}
		}
		if (firstBpmEventJsonTime >= 0f)
		{
			BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(firstBpmEventJsonTime);
		}
		RefreshPools(Data);
		RefreshEventAppearance();
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject originalObject in originalObjects)
		{
			DeleteObject(originalObject, refreshesPool: false);
		}
		foreach (BaseObject editedObject in editedObjects)
		{
			SpawnObject(editedObject);
			if (!Networked)
			{
				SelectionController.Select(editedObject, addsToSelection: true, automaticallyRefreshes: true, addActionEvent: false);
			}
		}
		if (firstBpmEventJsonTime >= 0f)
		{
			BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(firstBpmEventJsonTime);
		}
		RefreshPools(Data);
		RefreshEventAppearance();
	}

	public override void Serialize(NetDataWriter writer)
	{
		SerializeBeatmapObjectList(writer, editedObjects);
		SerializeBeatmapObjectList(writer, originalObjects);
	}

	public override void Deserialize(NetDataReader reader)
	{
		editedObjects = DeserializeBeatmapObjectList(reader).ToList();
		originalObjects = DeserializeBeatmapObjectList(reader).ToList();
		Data = editedObjects.Concat(originalObjects);
	}
}
