using System.Collections.Generic;
using Beatmap.Base;
using LiteNetLib.Utils;

public class BeatmapObjectDeletionAction : BeatmapAction
{
	public BeatmapObjectDeletionAction()
	{
	}

	public BeatmapObjectDeletionAction(IEnumerable<BaseObject> objs, string comment)
		: base(objs, comment)
	{
	}

	public BeatmapObjectDeletionAction(BaseObject obj, string comment)
		: base(new BaseObject[1] { obj }, comment)
	{
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			SpawnObject(datum);
		}
		RefreshPools(Data);
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			DeleteObject(datum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		RefreshPools(Data);
	}

	public override void Serialize(NetDataWriter writer)
	{
		SerializeBeatmapObjectList(writer, Data);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Data = DeserializeBeatmapObjectList(reader);
	}
}
