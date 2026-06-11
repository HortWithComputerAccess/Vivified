using System.Collections;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BeatmapInputController<T> : MonoBehaviour, CMInput.IBeatmapObjectsActions where T : ObjectContainer
{
	[FormerlySerializedAs("customStandaloneInputModule")]
	[SerializeField]
	protected CustomStandaloneInputModule CustomStandaloneInputModule;

	protected bool IsSelecting;

	[SerializeField]
	private CameraManager cameraManager;

	private bool massSelect;

	protected Vector2 MousePosition;

	private float timeWhenFirstSelecting;

	private void Update()
	{
		if (CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			return;
		}
		GlobalIntersectionCache.firstHit = null;
		if (ObstaclePlacement.IsPlacing)
		{
			timeWhenFirstSelecting = Time.time;
		}
		else
		{
			if (!IsSelecting || Time.time - timeWhenFirstSelecting < 0.5f)
			{
				return;
			}
			foreach (Intersections.IntersectionHit item in Intersections.RaycastAll(cameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition), 9))
			{
				if (GetComponentFromTransform(item.GameObject, out var obj) && !SelectionController.IsObjectSelected(obj.ObjectData))
				{
					SelectionController.Select(obj.ObjectData, addsToSelection: true);
					obj.selectionStateChanged = true;
				}
			}
		}
	}

	public void OnDeleteTool(InputAction.CallbackContext context)
	{
		if (DeleteToolController.IsActive && context.performed)
		{
			OnQuickDelete(context);
		}
	}

	public void OnQuickDelete(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && Application.isFocused)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging && context.performed)
			{
				StartCoroutine(CompleteDelete(firstObject));
			}
		}
	}

	public void OnSelectObjects(InputAction.CallbackContext context)
	{
		if (CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) || ObstaclePlacement.IsPlacing)
		{
			return;
		}
		IsSelecting = context.performed;
		if (!context.performed)
		{
			return;
		}
		timeWhenFirstSelecting = Time.time;
		RaycastFirstObject(out var firstObject);
		if (!(firstObject == null))
		{
			BaseObject objectData = firstObject.ObjectData;
			if (massSelect && SelectionController.SelectedObjects.Count() == 1 && SelectionController.SelectedObjects.First() != objectData)
			{
				SelectionController.SelectBetween(SelectionController.SelectedObjects.First(), objectData, addsToSelection: true);
			}
			else if (SelectionController.IsObjectSelected(objectData))
			{
				SelectionController.Deselect(objectData);
				firstObject.selectionStateChanged = true;
			}
			else if (!SelectionController.IsObjectSelected(objectData))
			{
				SelectionController.Select(objectData, addsToSelection: true);
				firstObject.selectionStateChanged = true;
			}
		}
	}

	public void OnMousePositionUpdate(InputAction.CallbackContext context)
	{
		MousePosition = context.ReadValue<Vector2>();
	}

	public void OnJumptoObjectTime(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null)
			{
				BeatmapObjectContainerCollection.GetCollectionForType(firstObject.ObjectData.ObjectType).AudioTimeSyncController.MoveToSongBpmTime(firstObject.ObjectData.SongBpmTime);
			}
		}
	}

	public void OnMassSelectModifier(InputAction.CallbackContext context)
	{
		massSelect = context.performed;
	}

	protected virtual bool GetComponentFromTransform(GameObject t, out T obj)
	{
		return t.TryGetComponent<T>(out obj);
	}

	protected void RaycastFirstObject(out T firstObject)
	{
		Ray ray = cameraManager.SelectedCameraController.Camera.ScreenPointToRay(MousePosition);
		if (GlobalIntersectionCache.firstHit == null && Intersections.Raycast(ray, 9, out var hit))
		{
			GlobalIntersectionCache.firstHit = hit.GameObject;
		}
		if (GlobalIntersectionCache.firstHit != null)
		{
			T componentInParent = GlobalIntersectionCache.firstHit.GetComponentInParent<T>();
			if (componentInParent != null)
			{
				firstObject = componentInParent;
				return;
			}
		}
		firstObject = null;
	}

	public IEnumerator CompleteDelete(T obj)
	{
		yield return null;
		BeatmapObjectContainerCollection.GetCollectionForType(obj.ObjectData.ObjectType).DeleteObject(obj.ObjectData, triggersAction: true, refreshesPool: true, "Deleted by the user.");
	}
}
