using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EventPlacement : PlacementController<BaseEvent, EventContainer, EventGridContainer>, CMInput.IEventPlacementActions
{
	[FormerlySerializedAs("eventAppearanceSO")]
	[SerializeField]
	private EventAppearanceSO eventAppearanceSo;

	[SerializeField]
	private ColorPicker colorPicker;

	[SerializeField]
	private TMP_InputField laserSpeedInputField;

	[SerializeField]
	private Toggle chromaToggle;

	[SerializeField]
	private Toggle redEventToggle;

	[SerializeField]
	private ToggleColourDropdown dropdown;

	[SerializeField]
	private CreateEventTypeLabels labels;

	public bool PlacePrecisionRotation;

	public int PrecisionRotationValue;

	private bool earlyRotationPlaceNow;

	private bool negativeRotations;

	private bool isHalfFloatValuePressed;

	private bool isZeroFloatValuePressed;

	internal int queuedValue = 5;

	internal float queuedFloatValue = 1f;

	internal float queuedRotation = 30f;

	protected override Vector2 vanillaOffset { get; } = new Vector2(-0.5f, -1.1f);

	public static bool CanPlaceChromaEvents => Settings.Instance.PlaceChromaColor;

	public void OnRotation15Degrees(InputAction.CallbackContext context)
	{
		if (queuedData.IsLaneRotationEvent() && context.performed)
		{
			UpdateRotation(negativeRotations ? (-15f) : 15f);
		}
	}

	public void OnRotation30Degrees(InputAction.CallbackContext context)
	{
		if (queuedData.IsLaneRotationEvent() && context.performed)
		{
			UpdateRotation(negativeRotations ? (-30f) : 30f);
		}
	}

	public void OnRotation45Degrees(InputAction.CallbackContext context)
	{
		if (queuedData.IsLaneRotationEvent() && context.performed)
		{
			UpdateRotation(negativeRotations ? (-45f) : 45f);
		}
	}

	public void OnRotation60Degrees(InputAction.CallbackContext context)
	{
		if (queuedData.IsLaneRotationEvent() && context.performed)
		{
			UpdateRotation(negativeRotations ? (-60f) : 60f);
		}
	}

	public void OnNegativeRotationModifier(InputAction.CallbackContext context)
	{
		negativeRotations = context.performed;
	}

	public void OnHalfFloatValueModifier(InputAction.CallbackContext context)
	{
		isHalfFloatValuePressed = context.performed;
	}

	public void OnZeroFloatValueModifier(InputAction.CallbackContext context)
	{
		isZeroFloatValuePressed = context.performed;
	}

	public void OnRotateInPlaceLeft(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PlaceRotationNow(right: false, earlyRotationPlaceNow);
		}
	}

	public void OnRotateInPlaceRight(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PlaceRotationNow(right: true, earlyRotationPlaceNow);
		}
	}

	public void OnRotateInPlaceModifier(InputAction.CallbackContext context)
	{
		earlyRotationPlaceNow = context.performed;
	}

	public void SetGridSize(int gridSize = 16)
	{
		foreach (Transform item in base.transform)
		{
			string text = item.name;
			if (!(text == "Event Grid Front Scaling Offset"))
			{
				if (text == "Event Interface Scaling Offset")
				{
					Vector3 localScale = item.transform.localScale;
					localScale.x = (float)gridSize / 10f;
					item.transform.localScale = localScale;
				}
			}
			else
			{
				Vector3 localScale2 = item.transform.localScale;
				localScale2.x = (float)gridSize / 10f;
				item.transform.localScale = localScale2;
			}
		}
		GridChild.Size = gridSize;
	}

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> container)
	{
		return new BeatmapObjectPlacementAction(spawned, container, "Placed an Event.");
	}

	public override BaseEvent GenerateOriginalData()
	{
		return new BaseEvent();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit _, Vector3 __)
	{
		instantiatedContainer.transform.localPosition = new Vector3(instantiatedContainer.transform.localPosition.x + 0.5f, 0.5f, instantiatedContainer.transform.localPosition.z);
		if (objectContainerCollection.PropagationEditing == EventGridContainer.PropMode.Off)
		{
			queuedData.Type = labels.LaneIdToEventType(Mathf.FloorToInt(instantiatedContainer.transform.localPosition.x));
			queuedData.CustomLightID = null;
		}
		else
		{
			int num = Mathf.FloorToInt(instantiatedContainer.transform.localPosition.x - 1f);
			queuedData.Type = objectContainerCollection.EventTypeToPropagate;
			if (num >= 0)
			{
				int[] customLightID = ((objectContainerCollection.PropagationEditing == EventGridContainer.PropMode.Prop) ? labels.PropIdToLightIds(objectContainerCollection.EventTypeToPropagate, num) : new int[1] { labels.EditorToLightID(objectContainerCollection.EventTypeToPropagate, num) });
				queuedData.CustomLightID = customLightID;
			}
			else
			{
				queuedData.CustomLightID = null;
			}
		}
		if (CanPlaceChromaEvents && dropdown.Visible && queuedData.IsLightEvent(EnvironmentInfoHelper.GetName()) && queuedData.Value != 0)
		{
			queuedData.CustomColor = colorPicker.CurrentColor;
		}
		else
		{
			queuedData.CustomColor = null;
		}
		UpdateQueuedValue(queuedValue);
		UpdateQueuedFloatValue(queuedFloatValue);
		UpdateQueuedRotation(queuedRotation);
		UpdateAppearance();
	}

	public void UpdateQueuedValue(int value)
	{
		queuedData.Value = value;
		if ((queuedData.IsLaserRotationEvent() || queuedData.IsUtilityEvent()) && int.TryParse(laserSpeedInputField.text, out var result))
		{
			queuedData.Value = result;
		}
		if (queuedData.IsColorBoostEvent())
		{
			queuedData.Value = ((queuedData.Value > 0) ? 1 : 0);
		}
	}

	public void UpdateValue(int value)
	{
		queuedValue = value;
		UpdateQueuedValue(queuedValue);
		UpdateAppearance();
	}

	public void UpdateQueuedFloatValue(float value)
	{
		if (!queuedData.IsLightEvent())
		{
			queuedData.FloatValue = 1f;
		}
		else if (isZeroFloatValuePressed)
		{
			queuedData.FloatValue = 0f;
		}
		else if (isHalfFloatValuePressed)
		{
			queuedData.FloatValue = value * 0.5f;
		}
		else
		{
			queuedData.FloatValue = value;
		}
	}

	public void UpdateFloatValue(float value)
	{
		queuedFloatValue = value;
		UpdateQueuedFloatValue(queuedFloatValue);
		UpdateAppearance();
	}

	private void UpdateQueuedRotation(float rotation)
	{
		if (queuedData.IsLaneRotationEvent())
		{
			queuedData.Rotation = rotation;
		}
	}

	public void UpdateRotation(float rotation)
	{
		queuedRotation = rotation;
		UpdateQueuedRotation(queuedRotation);
		UpdateAppearance();
	}

	public void SwapColors(bool red)
	{
		if (queuedData.IsLightEvent() && queuedValue < 2000000000 && queuedValue != 0 && (!red || queuedValue < 5) && (red || queuedValue < 1 || queuedValue >= 5))
		{
			if (queuedValue > 0 && queuedValue <= 4)
			{
				queuedValue += 4;
			}
			else if (queuedValue > 4 && queuedValue <= 8)
			{
				queuedValue += 4;
			}
			else if (queuedValue > 8 && queuedValue <= 12)
			{
				queuedValue -= 8;
			}
		}
	}

	private void UpdateAppearance()
	{
		if ((object)instantiatedContainer == null)
		{
			RefreshVisuals();
		}
		instantiatedContainer.EventData = queuedData;
		eventAppearanceSo.SetEventAppearance(instantiatedContainer, final: false);
	}

	public void PlaceChroma(bool v)
	{
		Settings.Instance.PlaceChromaColor = v;
	}

	internal override void ApplyToMap()
	{
		BaseEvent baseEvent = queuedData;
		if (baseEvent.IsLaneRotationEvent() && !GridRotation.IsActive)
		{
			PersistentUI.Instance.ShowDialogBox("Mapper", "360warning", null, PersistentUI.DialogBoxPresetType.Ok);
			return;
		}
		base.ApplyToMap();
		if (baseEvent.IsLaneRotationEvent())
		{
			TracksManager.RefreshTracks();
		}
		queuedData = new BaseEvent(baseEvent);
		queuedData.CustomData = null;
	}

	public override void TransferQueuedToDraggedObject(ref BaseEvent dragged, BaseEvent queued)
	{
		dragged.JsonTime = queued.JsonTime;
		dragged.Type = queued.Type;
		if (dragged.CustomData != null && queued.CustomData != null)
		{
			if (queued.CustomData?[queued.CustomKeyPropID] != null)
			{
				dragged.GetOrCreateCustom()[dragged.CustomKeyPropID] = queued.CustomData[queued.CustomKeyPropID];
			}
			if (queued.CustomLightID != null)
			{
				dragged.CustomLightID = queued.CustomLightID;
			}
		}
	}

	internal void PlaceRotationNow(bool right, bool early)
	{
		if (!GridRotation.IsActive)
		{
			return;
		}
		int rotationType = (early ? 14 : 15);
		float epsilon = 1f / Mathf.Pow(10f, Settings.Instance.TimeValueDecimalPrecision);
		BaseEvent baseEvent = objectContainerCollection.AllRotationEvents.Find((BaseEvent x) => x.JsonTime - epsilon < Atsc.CurrentJsonTime && x.JsonTime + epsilon > Atsc.CurrentJsonTime && x.Type == rotationType);
		int num = (right ? 4 : 3);
		if (baseEvent != null)
		{
			num = baseEvent.Value;
		}
		if (baseEvent != null && ((num == 4 && !right) || (num == 3 && right)))
		{
			num = baseEvent.Value;
			objectContainerCollection.DeleteObject(baseEvent, triggersAction: false);
			BeatmapActionContainer.AddAction(new BeatmapObjectDeletionAction(baseEvent, "Deleted by PlaceRotationNow."));
		}
		else if ((num < 7 && right) || (num > 0 && !right))
		{
			if (baseEvent != null)
			{
				num += (right ? 1 : (-1));
			}
			BaseEvent baseEvent2 = new BaseEvent
			{
				JsonTime = Atsc.CurrentJsonTime,
				Type = rotationType,
				Value = num
			};
			objectContainerCollection.SpawnObject(baseEvent2, out var conflicting);
			BeatmapActionContainer.AddAction(GenerateAction(baseEvent2, conflicting));
		}
		queuedData = BeatmapFactory.Clone(queuedData);
		TracksManager.RefreshTracks();
	}

	public override void ClickAndDragFinished()
	{
		TracksManager.RefreshTracks();
	}
}
