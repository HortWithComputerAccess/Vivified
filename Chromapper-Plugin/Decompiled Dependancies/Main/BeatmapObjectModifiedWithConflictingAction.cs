using System.Collections.Generic;
using Beatmap.Base;
using LiteNetLib.Utils;

public class BeatmapObjectModifiedWithConflictingAction : BeatmapObjectModifiedAction
{
	private IEnumerable<BaseObject> conflictingObjects;

	public BeatmapObjectModifiedWithConflictingAction()
	{
	}

	public BeatmapObjectModifiedWithConflictingAction(BaseObject edited, BaseObject originalObject, BaseObject originalData, IEnumerable<BaseObject> conflicting, string comment = "No comment.")
		: base(edited, originalObject, originalData, comment)
	{
		conflictingObjects = conflicting;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		base.Undo(param);
		foreach (BaseObject conflictingObject in conflictingObjects)
		{
			SpawnObject(conflictingObject);
		}
		RefreshPools(conflictingObjects);
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		base.Redo(param);
		foreach (BaseObject conflictingObject in conflictingObjects)
		{
			DeleteObject(conflictingObject, refreshesPool: false);
		}
		RefreshPools(conflictingObjects);
	}

	public override void Serialize(NetDataWriter writer)
	{
		base.Serialize(writer);
		SerializeBeatmapObjectList(writer, conflictingObjects);
	}

	public override void Deserialize(NetDataReader reader)
	{
		base.Deserialize(reader);
		conflictingObjects = DeserializeBeatmapObjectList(reader);
	}
}
