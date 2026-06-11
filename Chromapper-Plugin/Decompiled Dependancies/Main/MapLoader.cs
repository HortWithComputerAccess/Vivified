using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Base.Customs;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
	[SerializeField]
	private TracksManager manager;

	[SerializeField]
	private NoteLanesController noteLanesController;

	[Space]
	[SerializeField]
	private Transform containerCollectionsContainer;

	private BaseDifficulty map;

	public void UpdateMapData(BaseDifficulty m)
	{
		map = m;
		map.ConvertCustomBpmToOfficial();
	}

	public void HardRefresh()
	{
		LoadObjects(map.BpmEvents);
		if (Settings.Instance.Load_Others)
		{
			LoadObjects(map.CustomEvents);
		}
		if (Settings.Instance.Load_Others)
		{
			LoadObjects(map.EnvironmentEnhancements);
		}
		if (Settings.Instance.Load_Notes)
		{
			LoadObjects(map.Notes);
		}
		if (Settings.Instance.Load_Obstacles)
		{
			LoadObjects(map.Obstacles);
		}
		if (Settings.Instance.Load_Events)
		{
			LoadObjects(map.Events);
		}
		if (Settings.Instance.Load_Notes)
		{
			LoadObjects(map.Arcs);
			LoadObjects(map.Chains);
		}
		if (Settings.Instance.Load_Notes || Settings.Instance.Load_Obstacles)
		{
			LoadObjects(map.NJSEvents);
		}
		manager.RefreshTracks();
	}

	public void LoadObjects<T>(List<T> objects) where T : BaseObject
	{
		BeatmapObjectContainerCollection<T> collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<BeatmapObjectContainerCollection<T>, T>();
		if (collectionForType == null)
		{
			return;
		}
		objects.Sort();
		collectionForType.MapObjects = objects;
		if (objects is List<BaseEvent> list)
		{
			manager.RefreshTracks();
			EventGridContainer obj = collectionForType as EventGridContainer;
			obj.AllRotationEvents = list.FindAll((BaseEvent it) => it.IsLaneRotationEvent());
			obj.AllBoostEvents = list.FindAll((BaseEvent it) => it.IsColorBoostEvent());
			obj.AllBpmEvents = list.FindAll((BaseEvent it) => it.IsBpmEvent());
			obj.AllUtilityEvents = list.FindAll((BaseEvent it) => it.IsUtilityEvent());
			obj.AllLaserRotationEvents = list.FindAll((BaseEvent it) => it.IsLaserRotationEvent());
			obj.LinkAllLightEvents();
		}
		if (objects is List<BaseCustomEvent>)
		{
			(collectionForType as CustomEventGridContainer).LoadAll();
		}
		collectionForType.RefreshPool(forceRefresh: true);
	}
}
