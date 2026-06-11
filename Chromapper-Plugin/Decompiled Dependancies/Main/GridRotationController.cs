using System;
using UnityEngine;

public class GridRotationController : MonoBehaviour
{
	private static readonly int rotation = Shader.PropertyToID("_Rotation");

	public Action ObjectRotationChangedEvent;

	public RotationCallbackController RotationCallback;

	[SerializeField]
	private Vector3 rotationPoint = LoadInitialMap.PlatformOffset;

	[SerializeField]
	private bool rotateTransform = true;

	private float targetRotation;

	private float currentRotation;

	private void Start()
	{
		Shader.SetGlobalFloat(rotation, 0f);
		if (RotationCallback != null)
		{
			Init();
		}
	}

	private void LateUpdate()
	{
		if (Settings.Instance.RotateTrack)
		{
			ChangeRotation(Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime / 0.15f));
		}
	}

	private void OnDestroy()
	{
		RotationCallbackController rotationCallback = RotationCallback;
		rotationCallback.RotationChangedEvent = (Action<bool, float>)Delegate.Remove(rotationCallback.RotationChangedEvent, new Action<bool, float>(RotationChanged));
		Settings.ClearSettingNotifications("RotateTrack");
	}

	public void Init()
	{
		base.enabled = false;
		if (RotationCallback.IsActive)
		{
			base.enabled = true;
			RotationCallbackController rotationCallback = RotationCallback;
			rotationCallback.RotationChangedEvent = (Action<bool, float>)Delegate.Combine(rotationCallback.RotationChangedEvent, new Action<bool, float>(RotationChanged));
			Settings.NotifyBySettingName("RotateTrack", UpdateRotateTrack);
		}
	}

	private void UpdateRotateTrack(object obj)
	{
		if ((bool)obj)
		{
			ChangeRotation(RotationCallback.Rotation);
		}
		else
		{
			ChangeRotation(0f);
		}
	}

	private void RotationChanged(bool natural, float rotation)
	{
		if (RotationCallback.IsActive && Settings.Instance.RotateTrack)
		{
			targetRotation = rotation;
			if (!natural)
			{
				ChangeRotation(rotation);
			}
		}
	}

	private void ChangeRotation(float rotation)
	{
		if (rotateTransform)
		{
			base.transform.RotateAround(rotationPoint, Vector3.up, rotation - currentRotation);
		}
		currentRotation = rotation;
		ObjectRotationChangedEvent?.Invoke();
		Shader.SetGlobalFloat(GridRotationController.rotation, rotation);
	}
}
