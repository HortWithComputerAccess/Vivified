using System;
using System.Collections;
using System.Collections.Generic;
using Beatmap.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraController : MonoBehaviour, CMInput.ICameraActions
{
	private static CameraController instance;

	[SerializeField]
	private Vector3[] presetPositions;

	[SerializeField]
	private Vector3[] presetRotations;

	[SerializeField]
	private float movementSpeed;

	[SerializeField]
	private float mouseSensitivity;

	[SerializeField]
	private Transform noteGridTransform;

	[FormerlySerializedAs("_uiMode")]
	[SerializeField]
	private UIMode uiMode;

	[SerializeField]
	private CustomStandaloneInputModule customStandaloneInputModule;

	[FormerlySerializedAs("_rotationCallbackController")]
	public RotationCallbackController RotationCallbackController;

	[FormerlySerializedAs("camera")]
	public Camera Camera;

	[SerializeField]
	private UniversalRenderPipelineAsset urpAsset;

	[Header("Debug")]
	[SerializeField]
	private float x;

	[SerializeField]
	private float y;

	[SerializeField]
	private float z;

	[SerializeField]
	private float mouseX;

	[SerializeField]
	private float mouseY;

	[SerializeField]
	private bool playerCamera;

	[SerializeField]
	private ObjectAnimator cameraAnimator;

	[SerializeField]
	private AudioTimeSyncController atsc;

	private readonly Type[] actionMapsDisabledWhileMoving = new Type[16]
	{
		typeof(CMInput.IPlacementControllersActions),
		typeof(CMInput.INotePlacementActions),
		typeof(CMInput.IEventPlacementActions),
		typeof(CMInput.ISavingActions),
		typeof(CMInput.ITimelineActions),
		typeof(CMInput.IPlatformSoloLightGroupActions),
		typeof(CMInput.IPlaybackActions),
		typeof(CMInput.IBeatmapObjectsActions),
		typeof(CMInput.INoteObjectsActions),
		typeof(CMInput.IEventObjectsActions),
		typeof(CMInput.IObstacleObjectsActions),
		typeof(CMInput.ICustomEventsContainerActions),
		typeof(CMInput.IBPMTapperActions),
		typeof(CMInput.IEventUIActions),
		typeof(CMInput.IUIModeActions),
		typeof(CMInput.IBoxSelectActions)
	};

	private Vector2 savedMousePos = Vector2.zero;

	private UniversalAdditionalCameraData cameraExtraData;

	private bool canMoveCamera;

	private bool lockOntoNoteGrid;

	private bool secondSetOfLocations;

	private bool setLocation;

	private List<float> playerTrackTimes = new List<float>();

	private List<TrackAnimator> playerTracks = new List<TrackAnimator>();

	private TrackAnimator currentTrack;

	private bool ignoreInitialMouseMovement;

	private int framesAfterRightClick;

	private bool forwardHeld;

	private bool backwardHeld;

	private bool leftHeld;

	private bool rightHeld;

	private bool elevateHeld;

	private bool lowerHeld;

	public bool LockedOntoNoteGrid
	{
		get
		{
			return lockOntoNoteGrid;
		}
		set
		{
			Transform obj = base.transform;
			obj.SetParent((!value) ? null : noteGridTransform);
			obj.localScale = Vector3.one;
			lockOntoNoteGrid = value;
		}
	}

	public bool MovingCamera => canMoveCamera;

	public void AddPlayerTrack(float time, TrackAnimator track)
	{
		playerTrackTimes.Add(time);
		playerTracks.Add(track);
	}

	public void ClearPlayerTracks()
	{
		playerTrackTimes.Clear();
		playerTracks.Clear();
	}

	private void Start()
	{
		Camera.fieldOfView = (playerCamera ? Settings.Instance.PlayerCameraFOV : Settings.Instance.CameraFOV);
		cameraExtraData = Camera.GetUniversalAdditionalCameraData();
		UpdateAA(Settings.Instance.CameraAA);
		UpdateRenderScale(Settings.Instance.RenderScale);
		UpdatePlayerCameraOffsetZ(Settings.Instance.PlayerCameraOffsetZ);
		Settings.NotifyBySettingName("CameraAA", UpdateAA);
		Settings.NotifyBySettingName("RenderScale", UpdateRenderScale);
		Settings.NotifyBySettingName("PlayerCameraOffsetZ", UpdatePlayerCameraOffsetZ);
		if (!playerCamera)
		{
			instance = this;
			OnLocation(0);
			LockedOntoNoteGrid = true;
		}
		else
		{
			RotationCallbackController rotationCallbackController = RotationCallbackController;
			rotationCallbackController.RotationChangedEvent = (Action<bool, float>)Delegate.Combine(rotationCallbackController.RotationChangedEvent, new Action<bool, float>(OnRotation));
		}
	}

	private void Update()
	{
		if (PauseManager.IsPaused || SceneTransitionManager.IsLoading)
		{
			return;
		}
		Camera.fieldOfView = (playerCamera ? Settings.Instance.PlayerCameraFOV : Settings.Instance.CameraFOV);
		if (playerCamera)
		{
			if (!UIMode.AnimationMode)
			{
				return;
			}
			List<float> list = playerTrackTimes;
			if (list != null && list.Count != 0)
			{
				int num = playerTrackTimes.BinarySearch(atsc.CurrentJsonTime);
				int num2 = ((num < 0) ? (~num - 1) : num);
				if (num2 < 0)
				{
					DisconnectPlayerTrack();
				}
				else if (playerTracks[num2] != currentTrack)
				{
					DisconnectPlayerTrack();
					cameraAnimator.ResetData();
					currentTrack = playerTracks[num2];
					cameraAnimator.transform.SetParent(currentTrack.Track.ObjectParentTransform);
					cameraAnimator.LocalTarget = cameraAnimator.AnimationThis.transform;
					cameraAnimator.WorldTarget = cameraAnimator.transform;
					cameraAnimator.enabled = true;
					cameraAnimator.TargetType = ObjectAnimator.TargetTypes.Transform;
					currentTrack.Children.Add(cameraAnimator);
					currentTrack.OnChildrenChanged();
				}
			}
		}
		else if (canMoveCamera)
		{
			if (CMInputCallbackInstaller.IsActionMapDisabled(typeof(CMInput.ICameraActions)))
			{
				canMoveCamera = false;
				x = (y = (z = (mouseY = (mouseX = 0f))));
				return;
			}
			HandleCameraHeldMovementKeys();
			SetLockState(lockMouse: true);
			movementSpeed = Settings.Instance.Camera_MovementSpeed;
			mouseSensitivity = Settings.Instance.Camera_MouseSensitivity;
			float num3 = movementSpeed * Time.deltaTime;
			Vector3 translation = num3 * new Vector3(x, 0f, z);
			base.transform.Translate(translation);
			base.transform.Translate(num3 * y * Vector3.up, Space.World);
			Vector3 eulerAngles = base.transform.eulerAngles;
			float num4 = eulerAngles.x;
			num4 = ((num4 > 180f) ? (num4 - 360f) : num4);
			eulerAngles.x = Mathf.Clamp(num4 + (0f - mouseY), -89.5f, 89.5f);
			eulerAngles.y += mouseX;
			eulerAngles.z = 0f;
			base.transform.eulerAngles = eulerAngles;
		}
		else
		{
			z = (x = 0f);
			SetLockState(lockMouse: false);
		}
	}

	private void UpdateAA(object aaValue)
	{
		switch ((int)aaValue)
		{
		case 0:
			cameraExtraData.antialiasing = AntialiasingMode.None;
			break;
		case 1:
			cameraExtraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
			break;
		case 2:
			cameraExtraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			cameraExtraData.antialiasingQuality = AntialiasingQuality.Low;
			break;
		case 3:
			cameraExtraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			cameraExtraData.antialiasingQuality = AntialiasingQuality.Medium;
			break;
		case 4:
			cameraExtraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			cameraExtraData.antialiasingQuality = AntialiasingQuality.High;
			break;
		}
	}

	private void UpdateRenderScale(object renderScale)
	{
		urpAsset.renderScale = Mathf.Sqrt((float)(int)renderScale / 100f);
	}

	private void UpdatePlayerCameraOffsetZ(object posZ)
	{
		if (playerCamera)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z = (float)posZ / -0.6f;
			base.transform.localPosition = localPosition;
		}
	}

	public void SetLockState(bool lockMouse)
	{
		bool flag = Cursor.lockState == CursorLockMode.Locked;
		if (lockMouse && !flag)
		{
			instance.savedMousePos = Mouse.current.position.ReadValue();
			mouseX = 0f;
			mouseY = 0f;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else if (!lockMouse && flag)
		{
			Cursor.lockState = CursorLockMode.None;
			Mouse.current.WarpCursorPosition(instance.savedMousePos);
		}
	}

	private void HandleCameraHeldMovementKeys()
	{
		x = 0f;
		if (leftHeld)
		{
			x -= 1f;
		}
		if (rightHeld)
		{
			x += 1f;
		}
		y = 0f;
		if (elevateHeld)
		{
			y += 1f;
		}
		if (lowerHeld)
		{
			y -= 1f;
		}
		z = 0f;
		if (forwardHeld)
		{
			z += 1f;
		}
		if (backwardHeld)
		{
			z -= 1f;
		}
	}

	public void OnMoveCamera(InputAction.CallbackContext context)
	{
		Vector2 vector = context.ReadValue<Vector2>();
		x = vector.x;
		z = vector.y;
	}

	public void OnElevateCamera(InputAction.CallbackContext context)
	{
		elevateHeld = context.performed;
	}

	public void OnLowerCamera(InputAction.CallbackContext context)
	{
		lowerHeld = context.performed;
	}

	public void OnMoveCameraLeft(InputAction.CallbackContext context)
	{
		leftHeld = context.performed;
	}

	public void OnMoveCameraRight(InputAction.CallbackContext context)
	{
		rightHeld = context.performed;
	}

	public void OnMoveCameraForward(InputAction.CallbackContext context)
	{
		forwardHeld = context.performed;
	}

	public void OnMoveCameraBackward(InputAction.CallbackContext context)
	{
		backwardHeld = context.performed;
	}

	public void OnRotateCamera(InputAction.CallbackContext context)
	{
		if (!canMoveCamera)
		{
			return;
		}
		if (ignoreInitialMouseMovement)
		{
			framesAfterRightClick++;
			if (framesAfterRightClick <= 3)
			{
				mouseX = 0f;
				mouseY = 0f;
				return;
			}
			if (framesAfterRightClick <= 8)
			{
				Vector2 vector = context.ReadValue<Vector2>();
				vector.x = Mathf.Clamp(vector.x, -0.1f, 0.1f);
				vector.y = Mathf.Clamp(vector.y, -0.1f, 0.1f);
				mouseX = vector.x * mouseSensitivity / 10f;
				mouseY = vector.y * mouseSensitivity / 10f;
				return;
			}
			ignoreInitialMouseMovement = false;
		}
		Vector2 vector2 = context.ReadValue<Vector2>();
		mouseX = vector2.x * mouseSensitivity / 10f;
		mouseY = vector2.y * mouseSensitivity / 10f;
	}

	public void OnHoldtoMoveCamera(InputAction.CallbackContext context)
	{
		if (!customStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			if (context.performed && !canMoveCamera)
			{
				mouseX = 0f;
				mouseY = 0f;
				ignoreInitialMouseMovement = true;
				framesAfterRightClick = 0;
			}
			canMoveCamera = context.performed;
			if (canMoveCamera)
			{
				CMInputCallbackInstaller.DisableActionMaps(typeof(CameraController), actionMapsDisabledWhileMoving);
			}
			else if (context.canceled)
			{
				CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(CameraController), actionMapsDisabledWhileMoving);
			}
		}
	}

	public void OnAttachtoNoteGrid(InputAction.CallbackContext context)
	{
		if (RotationCallbackController.IsActive && context.performed && noteGridTransform.gameObject.activeInHierarchy && !playerCamera)
		{
			LockedOntoNoteGrid = !LockedOntoNoteGrid;
		}
	}

	public void OnToggleFullscreen(InputAction.CallbackContext context)
	{
		if (!Application.isEditor && context.performed)
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
	}

	public void OnLocation1(InputAction.CallbackContext context)
	{
		OnLocation(0);
	}

	public void OnLocation2(InputAction.CallbackContext context)
	{
		OnLocation(1);
	}

	public void OnLocation3(InputAction.CallbackContext context)
	{
		OnLocation(2);
	}

	public void OnLocation4(InputAction.CallbackContext context)
	{
		OnLocation(3);
	}

	private void OnDisable()
	{
		Settings.ClearSettingNotifications("CameraAA");
		Settings.ClearSettingNotifications("RenderScale");
		Settings.ClearSettingNotifications("PlayerCameraOffsetZ");
		instance = null;
	}

	public void OnSecondSetModifier(InputAction.CallbackContext context)
	{
		secondSetOfLocations = context.performed;
	}

	public void OnOverwriteLocationModifier(InputAction.CallbackContext context)
	{
		setLocation = context.performed;
	}

	public static void ClearCameraMovement()
	{
		if ((object)instance != null)
		{
			instance.x = (instance.y = (instance.z = (instance.mouseX = (instance.mouseY = 0f))));
		}
	}

	private void OnLocation(int id)
	{
		if (!playerCamera)
		{
			if (secondSetOfLocations)
			{
				id += 4;
			}
			if (setLocation)
			{
				Settings.Instance.SavedPositions[id] = new CameraPosition(base.transform.position, base.transform.rotation);
			}
			else if (Settings.Instance.SavedPositions[id] != null)
			{
				base.transform.SetPositionAndRotation(Settings.Instance.SavedPositions[id].Position, Settings.Instance.SavedPositions[id].Rotation);
			}
		}
	}

	private void OnRotation(bool natural, float rotation)
	{
		if (natural)
		{
			StartCoroutine(RotationCoroutine(Quaternion.Euler(0f, rotation, 0f)));
		}
		else
		{
			cameraAnimator.LocalTarget.localEulerAngles = new Vector3(0f, rotation, 0f);
		}
	}

	private IEnumerator RotationCoroutine(Quaternion current)
	{
		float t = 0f;
		Quaternion previous = cameraAnimator.LocalTarget.localRotation;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			cameraAnimator.LocalTarget.localRotation = Quaternion.SlerpUnclamped(previous, current, t);
			yield return new WaitForEndOfFrame();
		}
	}

	private void DisconnectPlayerTrack()
	{
		if (!(currentTrack == null))
		{
			currentTrack.Children.Remove(cameraAnimator);
			currentTrack.OnChildrenChanged();
			currentTrack = null;
			cameraAnimator.ResetData();
			cameraAnimator.enabled = false;
		}
	}
}
