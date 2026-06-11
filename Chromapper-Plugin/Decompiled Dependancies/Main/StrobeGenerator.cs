using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class StrobeGenerator : MonoBehaviour
{
	[FormerlySerializedAs("eventsContainer")]
	[SerializeField]
	private EventGridContainer eventGridContainer;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private StrobeGeneratorUIDropdown ui;

	public void ToggleUI()
	{
		ui.ToggleDropdown(!StrobeGeneratorUIDropdown.IsActive);
	}

	public void GenerateStrobe(IEnumerable<StrobeGeneratorPass> passes)
	{
		List<BaseObject> list = new List<BaseObject>();
		IEnumerable<BaseEvent> source = SelectionController.SelectedObjects.Where((BaseObject x) => x is BaseEvent).Cast<BaseEvent>();
		List<BaseObject> list2 = new List<BaseObject>();
		foreach (IGrouping<int, BaseEvent> item in from x in source
			group x by x.Type)
		{
			int key = item.Key;
			foreach (KeyValuePair<EventGridContainer.PropMode, IEnumerable<IGrouping<int, BaseEvent>>> item2 in (from y in item
				group y by (y.CustomLightID != null) ? EventGridContainer.PropMode.Light : EventGridContainer.PropMode.Off).ToDictionary((IGrouping<EventGridContainer.PropMode, BaseEvent> z) => z.Key, (IGrouping<EventGridContainer.PropMode, BaseEvent> modeGroup) => (modeGroup.Key != EventGridContainer.PropMode.Off) ? (from y in modeGroup
				group y by y.CustomLightID[0]) : (from y in modeGroup
				group y by 1)))
			{
				EventGridContainer.PropMode key2 = item2.Key;
				foreach (IGrouping<int, BaseEvent> item3 in item2.Value)
				{
					int[] propID = item3.FirstOrDefault()?.CustomLightID;
					if (item3.Count() < 2)
					{
						continue;
					}
					IOrderedEnumerable<BaseEvent> source2 = item3.OrderByDescending((BaseEvent x) => x.JsonTime);
					BaseEvent baseEvent = source2.First();
					BaseEvent baseEvent2 = source2.Last();
					Span<BaseEvent> between = eventGridContainer.GetBetween(baseEvent2.JsonTime, baseEvent.JsonTime);
					List<BaseEvent> list3 = new List<BaseEvent>(between.Length);
					for (int num = 0; num < between.Length; num++)
					{
						BaseEvent baseEvent3 = between[num];
						if (baseEvent3.Type == baseEvent2.Type && ((baseEvent2.CustomLightID == null && baseEvent3.CustomLightID == null) || (baseEvent2.CustomLightID != null && baseEvent3.CustomLightID != null && Enumerable.Contains(baseEvent3.CustomLightID, baseEvent2.CustomLightID[0]))))
						{
							list3.Add(baseEvent3);
							list2.Add(baseEvent3);
						}
					}
					list3.TrimExcess();
					foreach (StrobeGeneratorPass pass in passes)
					{
						List<BaseEvent> source3 = list3.FindAll((BaseEvent it) => pass.IsEventValidForPass(it));
						if (source3.Count() < 2)
						{
							continue;
						}
						List<BaseEvent> strobePassGenerated = pass.StrobePassForLane(source3.OrderBy((BaseEvent x) => x.JsonTime), key, key2, propID).ToList();
						list.RemoveAll((BaseObject x) => strobePassGenerated.Any((BaseEvent y) => y.IsConflictingWith(x)));
						list.AddRange(strobePassGenerated);
					}
				}
			}
		}
		list.OrderBy((BaseObject x) => x.JsonTime);
		if (list.Count <= 0)
		{
			return;
		}
		foreach (BaseEvent item4 in list2)
		{
			eventGridContainer.DeleteObject(item4, triggersAction: false, refreshesPool: false, "No comment.", inCollectionOfDeletes: true);
		}
		foreach (BaseEvent item5 in list)
		{
			item5.WriteCustom();
			eventGridContainer.SpawnObject(item5, removeConflicting: false, refreshesPool: false, inCollectionOfSpawns: true);
		}
		eventGridContainer.RefreshPool(forceRefresh: true);
		eventGridContainer.LinkAllLightEvents();
		eventGridContainer.RefreshEventsAppearance(list.Cast<BaseEvent>());
		SelectionController.DeselectAll();
		SelectionController.SelectedObjects = new HashSet<BaseObject>(list);
		SelectionController.SelectionChangedEvent?.Invoke();
		SelectionController.RefreshSelectionMaterial(triggersAction: false);
		BeatmapActionContainer.AddAction(new StrobeGeneratorGenerationAction(list.ToArray(), list2.ToArray()));
	}
}
