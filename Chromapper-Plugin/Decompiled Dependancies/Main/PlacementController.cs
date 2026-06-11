using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Animations;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class PlacementController<TBo, TBoc, TBocc> : MonoBehaviour, CMInput.IPlacementControllersActions, CMInput.ICancelPlacementActions where TBo : BaseObject where TBoc : ObjectContainer where TBocc : BeatmapObjectContainerCollection
{
	[SerializeField]
	private GameObject objectContainerPrefab;

	[SerializeField]
	private TBo objectData;

	[SerializeField]
	protected BPMChangeGridContainer BpmChangeGridContainer;

	[FormerlySerializedAs("ObjectContainerCollection")]
	[SerializeField]
	internal TBocc objectContainerCollection;

	[FormerlySerializedAs("parentTrack")]
	[SerializeField]
	protected Transform ParentTrack;

	[FormerlySerializedAs("interfaceGridParent")]
	[SerializeField]
	protected Transform InterfaceGridParent;

	[SerializeField]
	protected bool AssignTo360Tracks;

	[SerializeField]
	private ObjectType objectDataType;

	[SerializeField]
	private bool startingActiveState;

	[FormerlySerializedAs("atsc")]
	[SerializeField]
	protected AudioTimeSyncController Atsc;

	[SerializeField]
	private CustomStandaloneInputModule customStandaloneInputModule;

	[FormerlySerializedAs("tracksManager")]
	[SerializeField]
	protected TracksManager TracksManager;

	[FormerlySerializedAs("gridRotation")]
	[SerializeField]
	protected RotationCallbackController GridRotation;

	[FormerlySerializedAs("gridChild")]
	[SerializeField]
	protected GridChild GridChild;

	[SerializeField]
	private Transform noteGridTransform;

	[FormerlySerializedAs("bounds")]
	public Bounds Bounds;

	public bool IsActive;

	private bool applicationFocus;

	private bool applicationFocusChanged;

	protected TBoc DraggedObjectContainer;

	protected TBo draggedObjectData;

	internal TBoc instantiatedContainer;

	[SerializeField]
	protected CameraManager CameraManager;

	protected bool IsDraggingObject;

	protected bool IsDraggingObjectAtTime;

	protected bool IsOnPlacement;

	protected Vector2 MousePosition;

	private TBo originalDraggedObjectData;

	private TBo originalQueued;

	protected List<ObjectContainer> DraggedAttachedSliderContainers = new List<ObjectContainer>();

	protected Dictionary<IndicatorType, List<BaseSlider>> DraggedAttachedSliderDatas = new Dictionary<IndicatorType, List<BaseSlider>>
	{
		{
			IndicatorType.Head,
			new List<BaseSlider>()
		},
		{
			IndicatorType.Tail,
			new List<BaseSlider>()
		}
	};

	private Dictionary<IndicatorType, List<BaseSlider>> originalDraggedAttachedSliderDatas = new Dictionary<IndicatorType, List<BaseSlider>>
	{
		{
			IndicatorType.Head,
			new List<BaseSlider>()
		},
		{
			IndicatorType.Tail,
			new List<BaseSlider>()
		}
	};

	internal TBo queuedData;

	protected bool UsePrecisionPlacement;

	private float roundedJsonTime;

	protected virtual Vector2 precisionOffset { get; } = new Vector2(-0.5f, -1.1f);

	protected virtual Vector2 vanillaOffset { get; } = new Vector2(1.5f, -1.1f);

	protected virtual bool CanClickAndDrag { get; set; } = true;

	internal float RoundedJsonTime
	{
		get
		{
			return roundedJsonTime;
		}
		set
		{
			SongBpmTime = BeatSaberSongContainer.Instance.Map.JsonTimeToSongBpmTime(value).Value;
			roundedJsonTime = value;
		}
	}

	internal float SongBpmTime { get; set; }

	public virtual bool IsValid
	{
		get
		{
			if (!Input.GetMouseButton(1) && !SongTimelineController.IsHovering && IsActive && !BoxSelectionPlacementController.IsSelecting && applicationFocus && !SceneTransitionManager.IsLoading && KeybindsController.IsMouseInWindow && !DeleteToolController.IsActive)
			{
				return !NodeEditorController.IsActive;
			}
			return false;
		}
	}

	public virtual int PlacementXMin => 0;

	public virtual int PlacementXMax => GridOrderController.GetSizeForOrder(GridChild.Order);

	internal virtual void Start()
	{
		queuedData = GenerateOriginalData();
		IsActive = startingActiveState;
	}

	protected virtual void Update()
	{
		if ((IsDraggingObject && !Input.GetMouseButton(0)) || (IsDraggingObjectAtTime && !Input.GetMouseButton(1)))
		{
			noteGridTransform.localPosition = new Vector3(noteGridTransform.localPosition.x, noteGridTransform.localPosition.y, 0f);
			FinishDrag();
		}
		if (Application.isFocused != applicationFocus)
		{
			applicationFocus = Application.isFocused;
			applicationFocusChanged = true;
			return;
		}
		if (applicationFocusChanged)
		{
			applicationFocusChanged = false;
		}
		IEnumerable<Intersections.IntersectionHit> enumerable = Intersections.RaycastAll(CameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition), 11);
		IsOnPlacement = false;
		foreach (Intersections.IntersectionHit item in enumerable)
		{
			if (!IsOnPlacement && item.GameObject.GetComponentInParent(GetType()) != null)
			{
				IsOnPlacement = true;
				break;
			}
		}
		if (PauseManager.IsPaused)
		{
			return;
		}
		if ((!IsValid && ((!IsDraggingObject && !IsDraggingObjectAtTime) || !IsActive)) || !IsOnPlacement)
		{
			ColliderExit();
			return;
		}
		if (instantiatedContainer == null)
		{
			RefreshVisuals();
		}
		if (!instantiatedContainer.gameObject.activeSelf)
		{
			instantiatedContainer.gameObject.SetActive(value: true);
		}
		objectData = queuedData;
		if (enumerable.Any())
		{
			Intersections.IntersectionHit hit = enumerable.OrderBy((Intersections.IntersectionHit i) => i.Distance).First();
			if (!hit.GameObject.transform.IsChildOf(base.transform) || PersistentUI.Instance.DialogBoxIsEnabled)
			{
				ColliderExit();
			}
			else
			{
				if (customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
				{
					return;
				}
				if (BeatmapObjectContainerCollection.TrackFilterID != null && !objectContainerCollection.IgnoreTrackFilter)
				{
					queuedData.CustomTrack = BeatmapObjectContainerCollection.TrackFilterID;
				}
				else
				{
					queuedData.CustomTrack = null;
				}
				CalculateTimes(hit, out var roundedHit, out var jsonTime);
				roundedHit += (Vector3)vanillaOffset;
				RoundedJsonTime = jsonTime;
				float z = SongBpmTime * EditorScaleController.EditorScale;
				Update360Tracks();
				roundedHit = new Vector3(Mathf.Round(roundedHit.x), Mathf.Round(roundedHit.y), z);
				Vector3 vector = ParentTrack.InverseTransformPoint(hit.Bounds.max);
				Vector3 vector2 = ParentTrack.InverseTransformPoint(hit.Bounds.min);
				float max = PlacementXMax;
				float min = PlacementXMin;
				float y = vector.y;
				float y2 = vector2.y;
				float x = roundedHit.x;
				float y3 = roundedHit.y;
				instantiatedContainer.transform.localPosition = new Vector3(Mathf.Clamp(x, min, max), Mathf.Round(Mathf.Clamp(y3, y2, y - 1f)), roundedHit.z);
				queuedData.JsonTime = jsonTime;
				OnPhysicsRaycast(hit, roundedHit);
				if ((IsDraggingObject || IsDraggingObjectAtTime) && queuedData != null)
				{
					TransferQueuedToDraggedObject(ref draggedObjectData, queuedData);
					if (DraggedObjectContainer != null)
					{
						DraggedObjectContainer.UpdateGridPosition();
					}
				}
			}
		}
		else
		{
			ColliderExit();
		}
	}

	private void OnDestroy()
	{
		Intersections.Clear();
	}

	public void OnCancelPlacement(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			CancelPlacement();
		}
	}

	public virtual void OnPlaceObject(InputAction.CallbackContext context)
	{
		if (!customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && KeybindsController.IsMouseInWindow && context.performed && !IsDraggingObject && !IsDraggingObjectAtTime && IsOnPlacement && instantiatedContainer != null && IsValid && !PersistentUI.Instance.DialogBoxIsEnabled)
		{
			TBo val = queuedData;
			if (val != null && val.JsonTime >= 0f && !applicationFocusChanged && instantiatedContainer.gameObject.activeSelf)
			{
				ApplyToMap();
			}
		}
	}

	public void OnInitiateClickandDrag(InputAction.CallbackContext context)
	{
		if (context.performed && CanClickAndDrag)
		{
			Ray ray = CameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition);
			if (instantiatedContainer != null)
			{
				instantiatedContainer.gameObject.SetActive(value: false);
			}
			if (Intersections.Raycast(ray, 9, out var hit))
			{
				ObjectContainer componentInParent = hit.GameObject.GetComponentInParent<ObjectContainer>();
				if (StartDrag(componentInParent))
				{
					IsDraggingObject = true;
				}
			}
		}
		else if (context.canceled && IsDraggingObject && instantiatedContainer != null)
		{
			FinishDrag();
		}
	}

	protected virtual float GetContainerPosZ(ObjectContainer con)
	{
		return (con.ObjectData.SongBpmTime - Atsc.CurrentSongBpmTime) * EditorScaleController.EditorScale;
	}

	public void OnInitiateClickandDragatTime(InputAction.CallbackContext context)
	{
		if (context.performed && CanClickAndDrag)
		{
			if (Intersections.Raycast(CameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition), 9, out var hit))
			{
				ObjectContainer componentInParent = hit.GameObject.GetComponentInParent<ObjectContainer>();
				if (StartDrag(componentInParent))
				{
					IsDraggingObjectAtTime = true;
					float containerPosZ = GetContainerPosZ(componentInParent);
					noteGridTransform.localPosition = new Vector3(noteGridTransform.localPosition.x, noteGridTransform.localPosition.y, containerPosZ);
				}
			}
		}
		else if (context.canceled && IsDraggingObjectAtTime && instantiatedContainer != null)
		{
			noteGridTransform.localPosition = new Vector3(noteGridTransform.localPosition.x, noteGridTransform.localPosition.y, 0f);
			FinishDrag();
		}
	}

	public virtual void OnMousePositionUpdate(InputAction.CallbackContext context)
	{
		MousePosition = Mouse.current.position.ReadValue();
	}

	public void OnPrecisionPlacementToggle(InputAction.CallbackContext context)
	{
		switch (Settings.Instance.PrecisionPlacementMode)
		{
		case PrecisionPlacementMode.Off:
			UsePrecisionPlacement = false;
			break;
		case PrecisionPlacementMode.Hold:
			UsePrecisionPlacement = context.performed;
			break;
		case PrecisionPlacementMode.Toggle:
			if (context.started && !context.performed)
			{
				UsePrecisionPlacement = !UsePrecisionPlacement;
			}
			break;
		}
	}

	protected virtual bool TestForType<T>(Intersections.IntersectionHit hit, ObjectType type) where T : MonoBehaviour
	{
		T componentInParent = hit.GameObject.GetComponentInParent<T>();
		if (componentInParent != null)
		{
			Bounds bounds = componentInParent.GetComponentsInChildren<Renderer>().FirstOrDefault((Renderer it) => it.name == "Grid X").bounds;
			Transform transform = componentInParent.transform;
			Vector3 localScale = transform.localScale;
			Bounds bounds2 = transform.InverseTransformBounds(bounds);
			bounds2.center += transform.localPosition;
			bounds2.extents = new Vector3(bounds2.extents.x * localScale.x, bounds2.extents.y * localScale.y, bounds2.extents.z * localScale.z);
			if (Bounds == default(Bounds))
			{
				Bounds = bounds2;
			}
			else
			{
				Bounds.Encapsulate(bounds2);
			}
			return true;
		}
		return false;
	}

	protected virtual float GetDraggedObjectJsonTime()
	{
		return draggedObjectData.JsonTime;
	}

	protected void CalculateTimes(Intersections.IntersectionHit hit, out Vector3 roundedHit, out float roundedJsonTime)
	{
		float num = (IsDraggingObjectAtTime ? GetDraggedObjectJsonTime() : Atsc.CurrentJsonTime);
		float num2 = 1f / (float)Atsc.GridMeasureSnapping;
		float num3 = num - (float)Math.Round(num / num2, MidpointRounding.AwayFromZero) * num2;
		roundedHit = ParentTrack.InverseTransformPoint(hit.Point);
		float songBpmTime = roundedHit.z / EditorScaleController.EditorScale;
		if (hit.GameObject.transform.parent.name.Contains("Interface"))
		{
			songBpmTime = ParentTrack.InverseTransformPoint(hit.GameObject.transform.parent.position).z / EditorScaleController.EditorScale;
		}
		float num4 = BeatSaberSongContainer.Instance.Map.SongBpmTimeToJsonTime(songBpmTime).Value;
		roundedJsonTime = (float)Math.Round((num4 - num3) / num2, MidpointRounding.AwayFromZero) * num2;
		if (!Atsc.IsPlaying)
		{
			roundedJsonTime += num3;
		}
	}

	private void ColliderExit()
	{
		if (instantiatedContainer != null)
		{
			instantiatedContainer.SafeSetActive(active: false);
		}
	}

	internal virtual void RefreshVisuals()
	{
		instantiatedContainer = UnityEngine.Object.Instantiate(objectContainerPrefab, ParentTrack).GetComponent(typeof(TBoc)) as TBoc;
		instantiatedContainer.Setup();
		instantiatedContainer.OutlineVisible = false;
		IntersectionCollider[] componentsInChildren = instantiatedContainer.GetComponentsInChildren<IntersectionCollider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		ObjectAnimator component = instantiatedContainer.GetComponent<ObjectAnimator>();
		if ((object)component != null)
		{
			component.enabled = false;
		}
		instantiatedContainer.name = $"Hover {objectDataType}";
	}

	private void Update360Tracks()
	{
		if (!AssignTo360Tracks)
		{
			return;
		}
		TracksManager component = objectContainerCollection.GetComponent<TracksManager>();
		if (component == null)
		{
			Debug.LogWarning("Could not find an attached TracksManager.");
			return;
		}
		Track trackAtTime = component.GetTrackAtTime(SongBpmTime);
		if (trackAtTime != null)
		{
			Vector3 localPosition = instantiatedContainer.transform.localPosition;
			ParentTrack = trackAtTime.ObjectParentTransform;
			instantiatedContainer.transform.SetParent(trackAtTime.ObjectParentTransform, worldPositionStays: false);
			instantiatedContainer.transform.localPosition = localPosition;
			instantiatedContainer.transform.localEulerAngles = new Vector3(instantiatedContainer.transform.localEulerAngles.x, 0f, instantiatedContainer.transform.localEulerAngles.z);
		}
	}

	internal virtual void ApplyToMap()
	{
		objectData = queuedData;
		objectContainerCollection.SpawnObject(objectData, out var conflicting);
		BeatmapActionContainer.AddAction(GenerateAction(objectData, conflicting));
		queuedData = BeatmapFactory.Clone(queuedData);
		queuedData.CustomData = null;
	}

	public abstract TBo GenerateOriginalData();

	public abstract BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting);

	public abstract void OnPhysicsRaycast(Intersections.IntersectionHit hit, Vector3 transformedPoint);

	public virtual void ClickAndDragFinished()
	{
	}

	public virtual void CancelPlacement()
	{
	}

	public abstract void TransferQueuedToDraggedObject(ref TBo dragged, TBo queued);

	private bool StartDrag(ObjectContainer con)
	{
		if ((object)con == null || !(con is TBoc) || con.ObjectData.ObjectType != objectDataType || !IsActive)
		{
			return false;
		}
		objectContainerCollection.SilentRemoveObject(con.ObjectData);
		draggedObjectData = con.ObjectData as TBo;
		originalQueued = BeatmapFactory.Clone(queuedData);
		originalDraggedObjectData = BeatmapFactory.Clone(con.ObjectData as TBo);
		queuedData = BeatmapFactory.Clone(draggedObjectData);
		DraggedObjectContainer = con as TBoc;
		DraggedObjectContainer.Dragging = true;
		if (con is NoteContainer noteContainer)
		{
			BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note).ClearSpecialAngles(con.ObjectData);
			StartDragSliders(noteContainer);
		}
		return true;
	}

	private void FinishDrag()
	{
		if (!IsDraggingObject && !IsDraggingObjectAtTime)
		{
			return;
		}
		objectContainerCollection.SpawnObject(draggedObjectData, out var conflicting);
		queuedData = BeatmapFactory.Clone(originalQueued);
		List<BeatmapAction> list = new List<BeatmapAction>();
		if (draggedObjectData.ToString() != originalDraggedObjectData.ToString())
		{
			if (conflicting.Any())
			{
				list.Add(new BeatmapObjectModifiedWithConflictingAction(draggedObjectData, draggedObjectData, originalDraggedObjectData, conflicting, "Modified via alt-click and drag."));
			}
			else
			{
				list.Add(new BeatmapObjectModifiedAction(draggedObjectData, draggedObjectData, originalDraggedObjectData, "Modified via alt-click and drag."));
			}
			SelectionController.SelectionChangedEvent?.Invoke();
		}
		if (DraggedObjectContainer is NoteContainer)
		{
			BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note).RefreshSpecialAngles(draggedObjectData, objectWasSpawned: false, isNatural: false);
			FinishSliderDrag(list);
			ClearDraggedAttachedSliders();
		}
		if (list.Count == 1)
		{
			BeatmapActionContainer.AddAction(list[0]);
		}
		else if (list.Count > 1)
		{
			BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: true, "Modified via alt-click and drag"));
		}
		DraggedObjectContainer.Dragging = false;
		DraggedObjectContainer = null;
		ClickAndDragFinished();
		IsDraggingObject = (IsDraggingObjectAtTime = false);
	}

	protected TBoc ObjectUnderCursor()
	{
		if (customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			return null;
		}
		if (Intersections.Raycast(CameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition), 9, out var hit))
		{
			return hit.GameObject.GetComponentInParent<TBoc>();
		}
		return null;
	}

	private void StartDragSliders(NoteContainer noteContainer)
	{
		BaseNote noteData = noteContainer.NoteData;
		float epsilon = BeatmapObjectContainerCollection.Epsilon;
		ArcGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<ArcGridContainer>(ObjectType.Arc);
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer in collectionForType.LoadedContainers)
		{
			BaseArc baseArc = loadedContainer.Key as BaseArc;
			bool num = Mathf.Abs(baseArc.JsonTime - noteData.JsonTime) < epsilon && baseArc.GetPosition() == noteData.GetPosition();
			bool flag = Mathf.Abs(baseArc.TailJsonTime - noteData.JsonTime) < epsilon && baseArc.GetTailPosition() == noteData.GetPosition();
			if (num)
			{
				originalDraggedAttachedSliderDatas[IndicatorType.Head].Add(BeatmapFactory.Clone(baseArc));
				DraggedAttachedSliderDatas[IndicatorType.Head].Add(baseArc);
				DraggedAttachedSliderContainers.Add(loadedContainer.Value);
				collectionForType.SilentRemoveObject(baseArc);
			}
			else if (flag)
			{
				originalDraggedAttachedSliderDatas[IndicatorType.Tail].Add(BeatmapFactory.Clone(baseArc));
				DraggedAttachedSliderDatas[IndicatorType.Tail].Add(baseArc);
				DraggedAttachedSliderContainers.Add(loadedContainer.Value);
				collectionForType.SilentRemoveObject(baseArc);
			}
		}
		ChainGridContainer collectionForType2 = BeatmapObjectContainerCollection.GetCollectionForType<ChainGridContainer>(ObjectType.Chain);
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer2 in collectionForType2.LoadedContainers)
		{
			BaseChain baseChain = loadedContainer2.Key as BaseChain;
			bool num2 = Mathf.Abs(baseChain.JsonTime - noteData.JsonTime) < epsilon && baseChain.GetPosition() == noteData.GetPosition();
			bool flag2 = Mathf.Abs(baseChain.TailJsonTime - noteData.JsonTime) < epsilon && baseChain.GetTailPosition() == noteData.GetPosition();
			if (num2)
			{
				originalDraggedAttachedSliderDatas[IndicatorType.Head].Add(BeatmapFactory.Clone(baseChain));
				DraggedAttachedSliderDatas[IndicatorType.Head].Add(baseChain);
				DraggedAttachedSliderContainers.Add(loadedContainer2.Value);
				collectionForType2.SilentRemoveObject(baseChain);
			}
			else if (flag2)
			{
				originalDraggedAttachedSliderDatas[IndicatorType.Tail].Add(BeatmapFactory.Clone(baseChain));
				DraggedAttachedSliderDatas[IndicatorType.Tail].Add(baseChain);
				DraggedAttachedSliderContainers.Add(loadedContainer2.Value);
				collectionForType2.SilentRemoveObject(baseChain);
			}
		}
	}

	private void FinishSliderDrag(List<BeatmapAction> actions)
	{
		ArcGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<ArcGridContainer>(ObjectType.Arc);
		ChainGridContainer collectionForType2 = BeatmapObjectContainerCollection.GetCollectionForType<ChainGridContainer>(ObjectType.Chain);
		for (int i = 0; i < DraggedAttachedSliderDatas[IndicatorType.Head].Count; i++)
		{
			BaseSlider baseSlider = DraggedAttachedSliderDatas[IndicatorType.Head][i];
			BaseSlider originalSlider = originalDraggedAttachedSliderDatas[IndicatorType.Head][i];
			if (baseSlider is BaseArc draggedSlider)
			{
				SpawnDraggedSlider(collectionForType, draggedSlider, originalSlider, actions);
			}
			else if (baseSlider is BaseChain draggedSlider2)
			{
				SpawnDraggedSlider(collectionForType2, draggedSlider2, originalSlider, actions);
			}
		}
		for (int j = 0; j < DraggedAttachedSliderDatas[IndicatorType.Tail].Count; j++)
		{
			BaseSlider baseSlider2 = DraggedAttachedSliderDatas[IndicatorType.Tail][j];
			BaseSlider originalSlider2 = originalDraggedAttachedSliderDatas[IndicatorType.Tail][j];
			if (baseSlider2 is BaseArc draggedSlider3)
			{
				SpawnDraggedSlider(collectionForType, draggedSlider3, originalSlider2, actions);
			}
			else if (baseSlider2 is BaseChain draggedSlider4)
			{
				SpawnDraggedSlider(collectionForType2, draggedSlider4, originalSlider2, actions);
			}
		}
	}

	private void SpawnDraggedSlider(BeatmapObjectContainerCollection sliderCollection, BaseSlider draggedSlider, BaseObject originalSlider, List<BeatmapAction> actions)
	{
		sliderCollection.SpawnObject(draggedSlider, out var conflicting);
		if (draggedSlider.ToString() != originalSlider.ToString())
		{
			if (conflicting.Any())
			{
				actions.Add(new BeatmapObjectModifiedWithConflictingAction(draggedSlider, draggedSlider, originalSlider, conflicting, "Modified via alt-click and drag."));
			}
			else
			{
				actions.Add(new BeatmapObjectModifiedAction(draggedSlider, draggedSlider, originalSlider, "Modified via alt-click and drag."));
			}
		}
	}

	private void ClearDraggedAttachedSliders()
	{
		DraggedAttachedSliderContainers.Clear();
		DraggedAttachedSliderDatas[IndicatorType.Head].Clear();
		DraggedAttachedSliderDatas[IndicatorType.Tail].Clear();
		originalDraggedAttachedSliderDatas[IndicatorType.Head].Clear();
		originalDraggedAttachedSliderDatas[IndicatorType.Tail].Clear();
	}
}
