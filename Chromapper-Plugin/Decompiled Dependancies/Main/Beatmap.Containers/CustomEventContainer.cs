using Beatmap.Base;
using Beatmap.Base.Customs;
using UnityEngine;

namespace Beatmap.Containers;

public class CustomEventContainer : ObjectContainer
{
	private CustomEventGridContainer collection;

	public BaseCustomEvent CustomEventData;

	public override BaseObject ObjectData
	{
		get
		{
			return CustomEventData;
		}
		set
		{
			CustomEventData = (BaseCustomEvent)value;
		}
	}

	public static CustomEventContainer SpawnCustomEvent(BaseCustomEvent data, CustomEventGridContainer collection, ref GameObject prefab)
	{
		CustomEventContainer component = Object.Instantiate(prefab).GetComponent<CustomEventContainer>();
		component.CustomEventData = data;
		component.collection = collection;
		return component;
	}

	public override void UpdateGridPosition()
	{
		base.transform.localPosition = new Vector3(collection.CustomEventTypes.IndexOf(CustomEventData.Type), 0.5f, CustomEventData.SongBpmTime * EditorScaleController.EditorScale);
		UpdateCollisionGroups();
	}
}
