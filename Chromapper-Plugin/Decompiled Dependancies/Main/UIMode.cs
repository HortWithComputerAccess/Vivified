using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UIMode : MonoBehaviour, CMInput.IUIModeActions
{
	public static UIModeType SelectedMode;

	private Vector3 savedCamPosition = Vector3.zero;

	private Quaternion savedCamRotation = Quaternion.identity;

	public static Action<UIModeType> UIModeSwitched;

	public static Action PreviewModeSwitched;

	[SerializeField]
	private GameObject modesGameObject;

	[SerializeField]
	private RectTransform selected;

	[SerializeField]
	private CameraManager cameraManager;

	[SerializeField]
	private GameObject[] gameObjectsWithRenderersToToggle;

	[SerializeField]
	private Transform[] thingsThatRequireAMoveForPreview;

	[FormerlySerializedAs("_rotationCallbackController")]
	[SerializeField]
	private RotationCallbackController rotationCallbackController;

	[SerializeField]
	private AudioTimeSyncController atsc;

	private readonly List<TextMeshProUGUI> modes = new List<TextMeshProUGUI>();

	private readonly List<Renderer> renderers = new List<Renderer>();

	private readonly List<Canvas> canvases = new List<Canvas>();

	private CanvasGroup canvasGroup;

	private static readonly List<Action<object>> actions = new List<Action<object>>();

	private MapEditorUI mapEditorUi;

	private Coroutine showUI;

	private Coroutine slideSelectionCoroutine;

	private static readonly int enableNoteSurfaceGridLine = Shader.PropertyToID("_EnableNoteSurfaceGridLine");

	public static bool PreviewMode { get; private set; }

	public static bool AnimationMode { get; private set; }

	private void Awake()
	{
		mapEditorUi = base.transform.GetComponentInParent<MapEditorUI>();
		modes.AddRange(modesGameObject.transform.GetComponentsInChildren<TextMeshProUGUI>());
		canvasGroup = GetComponent<CanvasGroup>();
		UIModeSwitched = null;
		PreviewModeSwitched = null;
		SelectedMode = UIModeType.Normal;
		savedCamPosition = Settings.Instance.SavedPositions[0]?.Position ?? savedCamPosition;
		savedCamRotation = Settings.Instance.SavedPositions[0]?.Rotation ?? savedCamRotation;
	}

	private void Start()
	{
		GameObject[] array = gameObjectsWithRenderersToToggle;
		foreach (GameObject gameObject in array)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Length != 0)
			{
				renderers.AddRange(componentsInChildren);
			}
			else
			{
				canvases.AddRange(gameObject.GetComponentsInChildren<Canvas>());
			}
		}
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		Shader.SetGlobalFloat(enableNoteSurfaceGridLine, 1f);
	}

	public void OnToggleUIMode(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ToggleUIMode(forward: true);
		}
	}

	public void OnToggleUIModeReverse(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ToggleUIMode(forward: false);
		}
	}

	void CMInput.IUIModeActions.OnToggleUIModeNormal(InputAction.CallbackContext context)
	{
		if (context.performed && SelectedMode != UIModeType.Normal)
		{
			UpdateCameraOnUIModeToggle(UIModeType.Normal);
			SetUIMode(UIModeType.Normal);
		}
	}

	void CMInput.IUIModeActions.OnToggleUIModeHideUI(InputAction.CallbackContext context)
	{
		if (context.performed && SelectedMode != UIModeType.HideUI)
		{
			UpdateCameraOnUIModeToggle(UIModeType.HideUI);
			SetUIMode(UIModeType.HideUI);
		}
	}

	void CMInput.IUIModeActions.OnToggleUIModeHideGrids(InputAction.CallbackContext context)
	{
		if (context.performed && SelectedMode != UIModeType.HideGrids)
		{
			UpdateCameraOnUIModeToggle(UIModeType.HideGrids);
			SetUIMode(UIModeType.HideGrids);
		}
	}

	void CMInput.IUIModeActions.OnToggleUIModePreview(InputAction.CallbackContext context)
	{
		if (context.performed && SelectedMode != UIModeType.Preview)
		{
			UpdateCameraOnUIModeToggle(UIModeType.Preview);
			SetUIMode(UIModeType.Preview);
		}
	}

	void CMInput.IUIModeActions.OnToggleUIModePlaying(InputAction.CallbackContext context)
	{
		if (context.performed && SelectedMode != UIModeType.Playing)
		{
			UpdateCameraOnUIModeToggle(UIModeType.Playing);
			SetUIMode(UIModeType.Playing);
		}
	}

	private void ToggleUIMode(bool forward)
	{
		int num = selected.parent.GetSiblingIndex() + (forward ? 1 : (-1));
		if (num < 0)
		{
			num = modes.Count - 1;
		}
		if (num >= modes.Count)
		{
			num = 0;
		}
		UpdateCameraOnUIModeToggle((UIModeType)num);
		SetUIMode(num);
	}

	private void UpdateCameraOnUIModeToggle(UIModeType mode)
	{
		int siblingIndex = selected.parent.GetSiblingIndex();
		if (siblingIndex == 4 && mode != (UIModeType)siblingIndex)
		{
			cameraManager.SelectCamera(CameraType.Editing);
			cameraManager.SelectedCameraController.transform.SetPositionAndRotation(savedCamPosition, savedCamRotation);
		}
		else if (mode == UIModeType.Playing)
		{
			Transform transform = cameraManager.SelectedCameraController.transform;
			savedCamPosition = transform.position;
			savedCamRotation = transform.rotation;
			cameraManager.SelectCamera(CameraType.Playing);
		}
	}

	private void OnPlayToggle(bool playing)
	{
		if (PreviewMode)
		{
			CanvasGroup[] mainUIGroup = mapEditorUi.MainUIGroup;
			foreach (CanvasGroup canvasGroup in mainUIGroup)
			{
				if (canvasGroup.name == "Song Timeline")
				{
					mapEditorUi.ToggleUIVisible(!playing, canvasGroup);
				}
			}
		}
		if (SelectedMode == UIModeType.Playing)
		{
			cameraManager.SelectedCameraController.SetLockState(playing);
		}
	}

	public void SetUIMode(UIModeType mode, bool showUIChange = true)
	{
		SetUIMode((int)mode, showUIChange);
	}

	public void SetUIMode(int modeID, bool showUIChange = true)
	{
		bool previewMode = PreviewMode;
		SelectedMode = (UIModeType)modeID;
		UIModeType selectedMode = SelectedMode;
		PreviewMode = selectedMode == UIModeType.Playing || selectedMode == UIModeType.Preview;
		AnimationMode = PreviewMode && Settings.Instance.Animations;
		if (previewMode != PreviewMode)
		{
			PreviewModeSwitched?.Invoke();
		}
		UIModeSwitched?.Invoke(SelectedMode);
		selected.SetParent(modes[modeID].transform, worldPositionStays: true);
		slideSelectionCoroutine = StartCoroutine(SlideSelection());
		if (showUIChange)
		{
			showUI = StartCoroutine(ShowUI());
		}
		switch (SelectedMode)
		{
		case UIModeType.Normal:
			HideStuff(showUI: true, showExtras: true, showMainGrid: true, showCanvases: true, showPlacement: true);
			break;
		case UIModeType.HideUI:
			HideStuff(showUI: false, showExtras: true, showMainGrid: true, showCanvases: true, showPlacement: true);
			break;
		case UIModeType.HideGrids:
			HideStuff(showUI: false, showExtras: false, showMainGrid: true, showCanvases: true, showPlacement: true);
			break;
		case UIModeType.Preview:
		case UIModeType.Playing:
			HideStuff(showUI: false, showExtras: false, showMainGrid: false, showCanvases: false, showPlacement: false);
			break;
		}
		foreach (Action<object> action in actions)
		{
			action?.Invoke(SelectedMode);
		}
	}

	private void HideStuff(bool showUI, bool showExtras, bool showMainGrid, bool showCanvases, bool showPlacement)
	{
		CanvasGroup[] mainUIGroup = mapEditorUi.MainUIGroup;
		foreach (CanvasGroup canvasGroup in mainUIGroup)
		{
			mapEditorUi.ToggleUIVisible(showUI, canvasGroup);
		}
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = showExtras;
		}
		foreach (Canvas canvase in canvases)
		{
			canvase.enabled = showCanvases;
		}
		List<CameraController> list = cameraManager.CameraControllers.Where((CameraController x) => x.LockedOntoNoteGrid).ToList();
		foreach (CameraController item in list)
		{
			item.LockedOntoNoteGrid = false;
		}
		if (showPlacement)
		{
			Shader.SetGlobalFloat(enableNoteSurfaceGridLine, 1f);
			Transform[] array = thingsThatRequireAMoveForPreview;
			for (int i = 0; i < array.Length; i++)
			{
				Transform obj = array[i].transform;
				Vector3 localPosition = obj.localPosition;
				float y = ((!(obj.name == "Rotating")) ? 0f : 0.05f);
				localPosition.y = y;
				obj.localPosition = localPosition;
			}
		}
		else
		{
			Shader.SetGlobalFloat(enableNoteSurfaceGridLine, 0f);
			Transform[] array = thingsThatRequireAMoveForPreview;
			foreach (Transform obj2 in array)
			{
				Transform transform = obj2.transform;
				Vector3 localPosition2 = transform.localPosition;
				if (obj2.name == "Note Interface Scaling Offset")
				{
					if (!showMainGrid)
					{
						localPosition2.y = 2000f;
					}
				}
				else
				{
					localPosition2.y = 2000f;
				}
				transform.localPosition = localPosition2;
			}
		}
		foreach (CameraController item2 in list)
		{
			item2.LockedOntoNoteGrid = true;
		}
		atsc.RefreshGridSnapping();
	}

	private IEnumerator ShowUI()
	{
		if (showUI != null)
		{
			StopCoroutine(showUI);
		}
		float startTime = Time.time;
		float startAlpha = canvasGroup.alpha;
		while (canvasGroup.alpha != 1f)
		{
			float t = Mathf.Clamp01((Time.time - startTime) / 0.2f);
			canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
			yield return new WaitForFixedUpdate();
		}
		yield return new WaitForSeconds(1f);
		startTime = Time.time;
		startAlpha = canvasGroup.alpha;
		while (canvasGroup.alpha != 0f)
		{
			float t2 = Mathf.Clamp01((Time.time - startTime) / 0.2f);
			canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t2);
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator SlideSelection()
	{
		if (slideSelectionCoroutine != null)
		{
			StopCoroutine(slideSelectionCoroutine);
		}
		float startTime = Time.time;
		Vector3 startLocalPosition = selected.localPosition;
		while (selected.localPosition.x != 0f)
		{
			float num = Mathf.Clamp01((Time.time - startTime) / 0.5f);
			float t = 1f - Mathf.Pow(1f - num, 3f);
			selected.localPosition = Vector3.Lerp(startLocalPosition, Vector3.zero, t);
			yield return new WaitForFixedUpdate();
		}
	}

	public static void NotifyOnUIModeChange(Action<object> callback)
	{
		if (callback != null)
		{
			actions.Add(callback);
		}
	}

	public static void ClearUIModeNotifications()
	{
		actions.Clear();
	}
}
