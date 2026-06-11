using System.Collections;
using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Enums;
using UnityEngine;

public class LegacyNotesConverter : MonoBehaviour
{
	public void ConvertFrom()
	{
		StartCoroutine(ConvertFromLegacy());
	}

	public void ConvertTo()
	{
		StartCoroutine(ConvertToLegacy());
	}

	private IEnumerator ConvertFromLegacy()
	{
		yield return PersistentUI.Instance.FadeInLoadingScreen();
		EventGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<EventGridContainer>(ObjectType.Event);
		Dictionary<int, Color?> dictionary = new Dictionary<int, Color?>();
		foreach (BaseObject loadedObject in collectionForType.LoadedObjects)
		{
			BaseEvent baseEvent = loadedObject as BaseEvent;
			if (dictionary.TryGetValue(baseEvent.Type, out var value))
			{
				if (baseEvent.Value >= 2000000000)
				{
					dictionary[baseEvent.Type] = ColourManager.ColourFromInt(baseEvent.Value);
					collectionForType.DeleteObject(baseEvent, triggersAction: false, refreshesPool: false);
				}
				else if (baseEvent.Value == 1900000001)
				{
					dictionary[baseEvent.Type] = null;
					collectionForType.DeleteObject(baseEvent, triggersAction: false, refreshesPool: false);
				}
				else if (value.HasValue && baseEvent.Value != 0)
				{
					baseEvent.CustomColor = value;
				}
			}
			else
			{
				dictionary.Add(baseEvent.Type, null);
			}
		}
		collectionForType.RefreshPool(forceRefresh: true);
		yield return PersistentUI.Instance.FadeOutLoadingScreen();
	}

	private IEnumerator ConvertToLegacy()
	{
		yield return PersistentUI.Instance.FadeInLoadingScreen();
		yield return PersistentUI.Instance.FadeOutLoadingScreen();
	}
}
