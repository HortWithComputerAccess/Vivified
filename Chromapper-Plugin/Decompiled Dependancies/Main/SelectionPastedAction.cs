using System.Collections.Generic;
using Beatmap.Base;
using LiteNetLib.Utils;

public class SelectionPastedAction : BeatmapAction
{
	private IEnumerable<BaseObject> removed;

	public SelectionPastedAction()
	{
	}

	public SelectionPastedAction(IEnumerable<BaseObject> pasteData, IEnumerable<BaseObject> removed)
		: base(pasteData)
	{
		affectsSeveralObjects = true;
		this.removed = removed;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			DeleteObject(datum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		foreach (BaseObject item in removed)
		{
			SpawnObject(item);
		}
		RefreshPools(removed);
		RefreshEventAppearance();
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		if (!Networked)
		{
			SelectionController.DeselectAll();
		}
		foreach (BaseObject datum in Data)
		{
			SpawnObject(datum);
			if (!Networked)
			{
				SelectionController.Select(datum, addsToSelection: true, automaticallyRefreshes: false, addActionEvent: false);
			}
		}
		foreach (BaseObject item in removed)
		{
			DeleteObject(item, refreshesPool: false);
		}
		RefreshPools(Data);
		RefreshEventAppearance();
	}

	public override void Serialize(NetDataWriter writer)
	{
		SerializeBeatmapObjectList(writer, Data);
		SerializeBeatmapObjectList(writer, removed);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Data = DeserializeBeatmapObjectList(reader);
		removed = DeserializeBeatmapObjectList(reader);
	}
}
