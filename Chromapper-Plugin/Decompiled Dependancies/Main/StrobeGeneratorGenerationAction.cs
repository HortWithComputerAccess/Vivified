using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Enums;
using LiteNetLib.Utils;

public class StrobeGeneratorGenerationAction : BeatmapAction
{
	private IEnumerable<BaseObject> conflictingData;

	public StrobeGeneratorGenerationAction()
	{
	}

	public StrobeGeneratorGenerationAction(IEnumerable<BaseObject> generated, IEnumerable<BaseObject> conflicting)
		: base(generated)
	{
		affectsSeveralObjects = true;
		conflictingData = conflicting;
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject datum in Data)
		{
			DeleteObject(datum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		foreach (BaseObject conflictingDatum in conflictingData)
		{
			SpawnObject(conflictingDatum);
		}
		BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event).RefreshPool(forceRefresh: true);
		RefreshEventAppearance();
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		foreach (BaseObject conflictingDatum in conflictingData)
		{
			DeleteObject(conflictingDatum, refreshesPool: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
		foreach (BaseObject datum in Data)
		{
			SpawnObject(datum);
		}
		BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event).RefreshPool(forceRefresh: true);
		RefreshEventAppearance();
	}

	public override void Serialize(NetDataWriter writer)
	{
		SerializeBeatmapObjectList(writer, Data);
		SerializeBeatmapObjectList(writer, conflictingData);
	}

	public override void Deserialize(NetDataReader reader)
	{
		Data = DeserializeBeatmapObjectList(reader);
		conflictingData = DeserializeBeatmapObjectList(reader);
	}
}
