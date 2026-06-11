using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxSelectionPlacementController : PlacementController<BaseEvent, EventContainer, EventGridContainer>, CMInput.IBoxSelectActions
{
	[SerializeField]
	public CustomEventGridContainer CustomCollection;

	[SerializeField]
	public EventGridContainer EventGridContainer;

	[SerializeField]
	public CreateEventTypeLabels Labels;

	private readonly HashSet<BaseObject> selected = new HashSet<BaseObject>();

	private readonly List<ObjectType> selectedTypes = new List<ObjectType>();

	private HashSet<BaseObject> alreadySelected = new HashSet<BaseObject>();

	private bool keybindPressed;

	private Vector3 originPos;

	private Intersections.IntersectionHit previousHit;

	private Vector3 transformed;

	public static bool IsSelecting { get; private set; }

	protected override bool CanClickAndDrag { get; set; }

	public override bool IsValid
	{
		get
		{
			if (Settings.Instance.BoxSelect)
			{
				if (!keybindPressed)
				{
					return IsSelecting;
				}
				return true;
			}
			return false;
		}
	}

	public override int PlacementXMin => int.MinValue;

	public override int PlacementXMax => int.MaxValue;

	private void OnDrawGizmos()
	{
		if (Application.isPlaying && (object)instantiatedContainer != null)
		{
			Gizmos.color = Color.red;
			BoxCollider component = instantiatedContainer.GetComponent<BoxCollider>();
			if (!(component == null))
			{
				Bounds bounds = new Bounds
				{
					center = component.bounds.center,
					size = instantiatedContainer.transform.lossyScale / 2f
				};
				Gizmos.DrawMesh(instantiatedContainer.GetComponentInChildren<MeshFilter>().mesh, bounds.center, instantiatedContainer.transform.rotation, bounds.size);
			}
		}
	}

	public void OnActivateBoxSelect(InputAction.CallbackContext context)
	{
		keybindPressed = context.performed;
	}

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return null;
	}

	public override BaseEvent GenerateOriginalData()
	{
		return new BaseEvent();
	}

	protected override bool TestForType<T>(Intersections.IntersectionHit hit, ObjectType type)
	{
		if (base.TestForType<T>(hit, type))
		{
			selectedTypes.Add(type);
			return true;
		}
		return false;
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 transformedPoint)
	{
		previousHit = hit;
		transformed = transformedPoint;
		Vector3 vector = ParentTrack.InverseTransformPoint(hit.Point);
		vector = ((!UsePrecisionPlacement) ? new Vector3(Mathf.Ceil(Math.Min(Math.Max(vector.x, Bounds.min.x + 0.01f), Bounds.max.x)), Mathf.Ceil(Math.Min(Math.Max(vector.y, 0.01f), 3f)), vector.z) : new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), vector.z));
		instantiatedContainer.transform.localPosition = vector - new Vector3(0.5f, 1f, 0f);
		if (!IsSelecting)
		{
			Bounds = default(Bounds);
			selectedTypes.Clear();
			TestForType<EventPlacement>(hit, ObjectType.Event);
			TestForType<NotePlacement>(hit, ObjectType.Note);
			TestForType<ObstaclePlacement>(hit, ObjectType.Obstacle);
			TestForType<CustomEventPlacement>(hit, ObjectType.CustomEvent);
			TestForType<BPMChangePlacement>(hit, ObjectType.BpmChange);
			TestForType<ArcPlacement>(hit, ObjectType.Arc);
			TestForType<ChainPlacement>(hit, ObjectType.Chain);
			TestForType<NJSEventPlacement>(hit, ObjectType.NJSEvent);
			instantiatedContainer.transform.localScale = Vector3.right + Vector3.up;
			Vector3 localScale = instantiatedContainer.transform.localScale;
			instantiatedContainer.transform.localPosition -= new Vector3(localScale.x / 2f, 0f, 0f);
			return;
		}
		Vector3 vector2 = originPos;
		float x = 0f;
		float num = 0f;
		if (vector.x <= originPos.x + 1f)
		{
			x = -1f;
			vector2.x += 1f;
		}
		if (vector.y <= originPos.y)
		{
			num = -1f;
			vector2.y += 1f;
		}
		instantiatedContainer.transform.localPosition = vector2;
		Vector3 vector3 = vector + new Vector3(x, num, 0.5f) - vector2;
		float y = Mathf.Max(vector3.y, 1f);
		if (num < 0f)
		{
			y = Mathf.Min(-1f, vector3.y);
		}
		vector3 = new Vector3(vector3.x, y, vector3.z);
		instantiatedContainer.transform.localScale = vector3;
		float num2 = instantiatedContainer.transform.localPosition.z / EditorScaleController.EditorScale;
		float num3 = (instantiatedContainer.transform.localPosition.z + vector3.z) / EditorScaleController.EditorScale;
		if (num2 > num3)
		{
			float num4 = num3;
			float num5 = num2;
			num2 = num4;
			num3 = num5;
		}
		SelectionController.ForEachObjectBetweenSongBpmTimeByGroup(num2, num3, hasNoteOrObstacle: true, hasEvent: true, hasBpmChange: true, hasNjsEvent: true, delegate(BeatmapObjectContainerCollection bocc, BaseObject bo)
		{
			if (selectedTypes.Contains(bo.ObjectType) && bo.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				float num7 = instantiatedContainer.transform.localPosition.x + instantiatedContainer.transform.localScale.x;
				float num8 = instantiatedContainer.transform.localPosition.x;
				if (num8 < num7)
				{
					float num9 = num8;
					float num10 = num7;
					num7 = num9;
					num8 = num10;
				}
				float num11 = instantiatedContainer.transform.localPosition.y + instantiatedContainer.transform.localScale.y;
				float num12 = instantiatedContainer.transform.localPosition.y;
				if (num11 < num12)
				{
					float num13 = num12;
					float num10 = num11;
					num11 = num13;
					num12 = num10;
				}
				Vector2 vector4 = new Vector2(num7, num12);
				if (bo is IObjectBounds objectBounds)
				{
					vector4 = objectBounds.GetCenter();
				}
				else if (!(bo is BaseBpmEvent))
				{
					if (bo is BaseEvent baseEvent)
					{
						Vector2? position = baseEvent.GetPosition(Labels, EventGridContainer.PropagationEditing, EventGridContainer.EventTypeToPropagate);
						if (!position.HasValue)
						{
							return;
						}
						vector4 = new Vector2((position?.x + Bounds.min.x).GetValueOrDefault(), position?.y ?? 0f);
					}
					else if (bo is BaseCustomEvent baseCustomEvent)
					{
						vector4 = new Vector2((float)CustomCollection.CustomEventTypes.IndexOf(baseCustomEvent.Type) + Bounds.min.x + 0.5f, 0.5f);
					}
				}
				if (!(vector4.x < num7) && !(vector4.x > num8) && !(vector4.y < num12) && !(vector4.y >= num11) && !alreadySelected.Contains(bo) && selected.Add(bo))
				{
					SelectionController.Select(bo, addsToSelection: true, automaticallyRefreshes: false, addActionEvent: false);
				}
			}
		});
		BaseObject[] array = SelectionController.SelectedObjects.ToArray();
		foreach (BaseObject baseObject in array)
		{
			if (!selected.Contains(baseObject) && !alreadySelected.Contains(baseObject))
			{
				SelectionController.Deselect(baseObject, removeActionEvent: false);
			}
		}
		selected.Clear();
	}

	public override void OnMousePositionUpdate(InputAction.CallbackContext context)
	{
		if (!IsValid && IsSelecting)
		{
			StartCoroutine(WaitABitFuckOffOtherPlacementControllers());
		}
		base.OnMousePositionUpdate(context);
	}

	internal override void ApplyToMap()
	{
		if (!IsSelecting)
		{
			IsSelecting = true;
			originPos = instantiatedContainer.transform.localPosition;
			alreadySelected = new HashSet<BaseObject>(SelectionController.SelectedObjects);
		}
		else
		{
			StartCoroutine(WaitABitFuckOffOtherPlacementControllers());
		}
	}

	private IEnumerator WaitABitFuckOffOtherPlacementControllers()
	{
		yield return new WaitForSeconds(0.1f);
		IsSelecting = false;
		selected.Clear();
		OnPhysicsRaycast(previousHit, transformed);
		SelectionController.SelectionChangedEvent?.Invoke();
	}

	public override void CancelPlacement()
	{
		if (!IsSelecting)
		{
			return;
		}
		IsSelecting = false;
		foreach (BaseObject item in selected)
		{
			SelectionController.Deselect(item, removeActionEvent: false);
		}
		SelectionController.SelectionChangedEvent?.Invoke();
	}

	public override void TransferQueuedToDraggedObject(ref BaseEvent dragged, BaseEvent queued)
	{
	}
}
