using System.Collections.Generic;
using Beatmap.Base;
using LiteNetLib.Utils;

public class SelectionDeletedAction : BeatmapAction
{
	public SelectionDeletedAction()
	{
	}

	public SelectionDeletedAction(IEnumerable<BaseObject> deletedData)
		: base(deletedData)
	{
		affectsSeveralObjects = true;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			SpawnObject(datum);
			if (!Networked)
			{
				SelectionController.Select(datum, addsToSelection: true, automaticallyRefreshes: false, addActionEvent: false);
			}
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		RefreshPools(Data);
		RefreshEventAppearance();
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			DeleteObject(datum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		RefreshPools(Data);
		RefreshEventAppearance();
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
