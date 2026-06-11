using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BeatmapEventInputController : BeatmapInputController<EventContainer>, CMInput.IEventObjectsActions
{
	[SerializeField]
	private EventAppearanceSO eventAppearanceSo;

	[SerializeField]
	private TracksManager tracksManager;

	public void OnInvertEventValue(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && KeybindsController.IsMouseInWindow && context.performed)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging)
			{
				InvertEvent(firstObject);
			}
		}
	}

	public void OnTweakEventMain(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				int modifier = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollEventValue) ? 1 : (-1));
				TweakMain(firstObject, modifier);
			}
		}
	}

	public void OnTweakEventAlternative(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				int modifier = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollEventValue) ? 1 : (-1));
				TweakAlternative(firstObject, modifier);
			}
		}
	}

	public void InvertEvent(EventContainer e)
	{
		BaseObject originalData = BeatmapFactory.Clone(e.ObjectData);
		if (e.EventData.IsLaneRotationEvent())
		{
			e.EventData.Rotation *= -1f;
			tracksManager.RefreshTracks();
		}
		else if (e.EventData.IsColorBoostEvent())
		{
			e.EventData.Value = ((e.EventData.Value <= 0) ? 1 : 0);
		}
		else
		{
			if (!e.EventData.IsLightEvent())
			{
				return;
			}
			if (e.EventData.Value > 0 && e.EventData.Value <= 4)
			{
				e.EventData.Value += 4;
			}
			else if (e.EventData.Value > 4 && e.EventData.Value <= 8)
			{
				e.EventData.Value += 4;
			}
			else if (e.EventData.Value > 8 && e.EventData.Value <= 12)
			{
				e.EventData.Value -= 8;
			}
			RefreshPrevEventContainer(e);
		}
		eventAppearanceSo.SetEventAppearance(e);
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(e.ObjectData, e.ObjectData, originalData));
	}

	protected override bool GetComponentFromTransform(GameObject t, out EventContainer obj)
	{
		return t.transform.parent.TryGetComponent<EventContainer>(out obj);
	}

	public void TweakMain(EventContainer e, int modifier)
	{
		BaseObject baseObject = BeatmapFactory.Clone(e.ObjectData);
		if (e.EventData.IsLightEvent())
		{
			e.EventData.FloatValue += 0.1f * (float)modifier;
			if (e.EventData.FloatValue < 0f)
			{
				e.EventData.FloatValue = 0f;
			}
			RefreshPrevEventContainer(e);
		}
		else if (e.EventData.IsLaneRotationEvent())
		{
			e.EventData.Rotation += 15 * modifier;
			tracksManager.RefreshTracks();
		}
		else if (e.EventData.IsColorBoostEvent())
		{
			e.EventData.Value = ((e.EventData.Value == 0) ? 1 : 0);
		}
		else if (e.EventData.IsBpmEvent())
		{
			e.EventData.FloatValue += modifier;
			if (e.EventData.FloatValue < 1f)
			{
				e.EventData.FloatValue = 1f;
			}
		}
		else
		{
			e.EventData.Value += modifier;
			if (e.EventData.Value < 0)
			{
				e.EventData.Value = 0;
			}
		}
		if (e.EventData.CompareTo(baseObject) != 0)
		{
			eventAppearanceSo.SetEventAppearance(e);
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(e.ObjectData, e.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.EventMainTweak));
		}
	}

	public void TweakAlternative(EventContainer e, int modifier)
	{
		BaseObject baseObject = BeatmapFactory.Clone(e.ObjectData);
		if (e.EventData.IsLightEvent())
		{
			e.EventData.Value += modifier;
			if (e.EventData.Value < 0)
			{
				e.EventData.Value = 0;
			}
			if (e.EventData.Value > 12 && e.EventData.IsLightEvent())
			{
				e.EventData.Value = 12;
			}
			if (e.EventData.CompareTo(baseObject) == 0)
			{
				return;
			}
			RefreshPrevEventContainer(e);
		}
		else if (e.EventData.IsLaneRotationEvent())
		{
			e.EventData.Rotation += modifier;
			tracksManager.RefreshTracks();
		}
		eventAppearanceSo.SetEventAppearance(e);
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(e.ObjectData, e.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.EventAltTweak));
	}

	private void RefreshPrevEventContainer(EventContainer e)
	{
		BaseEvent prev = e.EventData.Prev;
		if (prev != null && BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event).LoadedContainers.TryGetValue(prev, out var value))
		{
			(value as EventContainer).RefreshAppearance();
		}
	}
}
