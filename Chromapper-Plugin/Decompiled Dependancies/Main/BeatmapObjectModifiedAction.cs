using Beatmap.Base;
using Beatmap.Helper;
using LiteNetLib.Utils;

public class BeatmapObjectModifiedAction : BeatmapAction, IMergeableAction
{
	private bool addToSelection;

	private BaseObject editedData;

	private BaseObject editedObject;

	private BaseObject originalData;

	private BaseObject originalObject;

	private BaseObject preMergeOriginalData;

	public ActionMergeType MergeType { get; set; }

	public int MergeCount { get; set; }

	public BeatmapObjectModifiedAction()
	{
	}

	public BeatmapObjectModifiedAction(BaseObject edited, BaseObject originalObject, BaseObject originalData, string comment = "No comment.", bool keepSelection = false, ActionMergeType mergeType = ActionMergeType.None)
		: base(new BaseObject[2] { edited, originalObject }, comment)
	{
		editedObject = edited;
		editedData = BeatmapFactory.Clone(edited);
		this.originalData = originalData;
		this.originalObject = originalObject;
		addToSelection = keepSelection;
		MergeType = mergeType;
	}

	public IMergeableAction TryMerge(IMergeableAction previous)
	{
		if (!CanMerge(previous))
		{
			return null;
		}
		return DoMerge(previous);
	}

	public bool CanMerge(IMergeableAction previous)
	{
		if (!(previous is BeatmapObjectModifiedAction beatmapObjectModifiedAction))
		{
			return false;
		}
		if (MergeType != ActionMergeType.None && previous.MergeType == MergeType && originalObject == beatmapObjectModifiedAction.editedObject)
		{
			return editedData.CompareTo(beatmapObjectModifiedAction.originalData) != 0;
		}
		return false;
	}

	public IMergeableAction DoMerge(IMergeableAction previous)
	{
		if (!(previous is BeatmapObjectModifiedAction beatmapObjectModifiedAction))
		{
			return null;
		}
		BeatmapObjectModifiedAction beatmapObjectModifiedAction2 = new BeatmapObjectModifiedAction(editedObject, beatmapObjectModifiedAction.originalObject, beatmapObjectModifiedAction.originalData, Comment, addToSelection, MergeType);
		beatmapObjectModifiedAction2.MergeCount = beatmapObjectModifiedAction.MergeCount + 1;
		beatmapObjectModifiedAction2.Comment += $" ({beatmapObjectModifiedAction2.MergeCount}x merged)";
		beatmapObjectModifiedAction2.preMergeOriginalData = originalData;
		return beatmapObjectModifiedAction2;
	}

	public override BaseObject DoesInvolveObject(BaseObject obj)
	{
		if (obj != editedObject)
		{
			return null;
		}
		return originalObject;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		if (originalObject != editedObject || editedData.CompareTo(originalData) != 0)
		{
			DeleteObject(editedObject, refreshesPool: false);
			if (originalData != originalObject)
			{
				originalObject.Apply(originalData);
			}
			SpawnObject(originalObject, removeConflicting: false, !inCollection);
		}
		else
		{
			if (originalData != originalObject)
			{
				originalObject.Apply(originalData);
			}
			if (originalObject is BaseBpmEvent)
			{
				BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(originalObject.JsonTime);
			}
			if (!inCollection)
			{
				RefreshPools(Data);
			}
		}
		if (!Networked)
		{
			SelectionController.Select(originalObject, addToSelection, automaticallyRefreshes: true, !inCollection);
		}
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		if (originalObject != editedObject || editedData.CompareTo(originalData) != 0)
		{
			if (Networked && MergeCount > 0)
			{
				DeleteObject(preMergeOriginalData, refreshesPool: false);
				MergeCount = 0;
			}
			else
			{
				DeleteObject(originalObject, refreshesPool: false);
			}
			editedObject.Apply(editedData);
			SpawnObject(editedObject, removeConflicting: false, !inCollection);
		}
		else
		{
			editedObject.Apply(editedData);
			if (originalObject is BaseBpmEvent)
			{
				BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(originalObject.JsonTime);
			}
			if (!inCollection)
			{
				RefreshPools(Data);
			}
		}
		if (!Networked)
		{
			SelectionController.Select(editedObject, addToSelection, automaticallyRefreshes: true, !inCollection);
		}
	}

	public override void Serialize(NetDataWriter writer)
	{
		writer.PutBeatmapObject(editedData);
		writer.PutBeatmapObject(originalData);
		writer.Put(MergeCount);
		if (MergeCount > 0)
		{
			writer.PutBeatmapObject(preMergeOriginalData);
		}
	}

	public override void Deserialize(NetDataReader reader)
	{
		editedData = reader.GetBeatmapObject();
		editedObject = BeatmapFactory.Clone(editedData);
		originalData = reader.GetBeatmapObject();
		originalObject = BeatmapFactory.Clone(originalData);
		MergeCount = reader.GetInt();
		if (MergeCount > 0)
		{
			preMergeOriginalData = reader.GetBeatmapObject();
		}
		Data = new BaseObject[2] { editedObject, originalObject };
	}
}
