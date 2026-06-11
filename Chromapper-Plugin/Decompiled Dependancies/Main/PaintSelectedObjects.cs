using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Helper;
using UnityEngine;

public class PaintSelectedObjects : MonoBehaviour
{
	[SerializeField]
	private ColorPicker picker;

	public void Paint()
	{
		List<BeatmapAction> list = new List<BeatmapAction>();
		foreach (BaseObject selectedObject in SelectionController.SelectedObjects)
		{
			if (!(selectedObject is BaseBpmEvent) && !(selectedObject is BaseCustomEvent))
			{
				BaseObject originalData = BeatmapFactory.Clone(selectedObject);
				if (DoPaint(selectedObject))
				{
					list.Add(new BeatmapObjectModifiedAction(selectedObject, selectedObject, originalData, "a", keepSelection: true));
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (BaseObject item in SelectionController.SelectedObjects.DistinctBy((BaseObject x) => x.ObjectType))
		{
			BeatmapObjectContainerCollection.GetCollectionForType(item.ObjectType).RefreshPool(forceRefresh: true);
		}
		BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: true, "Painted a selection of objects."));
	}

	private bool DoPaint(BaseObject obj)
	{
		if (obj is BaseEvent baseEvent)
		{
			if (baseEvent.Value == 0)
			{
				return false;
			}
			if (!baseEvent.IsLightEvent(EnvironmentInfoHelper.GetName()))
			{
				return false;
			}
			if (baseEvent.CustomLightGradient != null)
			{
				baseEvent.CustomLightGradient.StartColor = picker.CurrentColor;
				return true;
			}
		}
		obj.CustomColor = picker.CurrentColor;
		return true;
	}
}
