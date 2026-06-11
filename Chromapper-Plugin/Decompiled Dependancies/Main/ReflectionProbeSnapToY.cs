using System;
using UnityEngine;

public class ReflectionProbeSnapToY : MonoBehaviour
{
	private PlatformDescriptor descriptor;

	private Camera mainCamera;

	private void Start()
	{
		mainCamera = Camera.main;
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(LoadPlatform));
	}

	private void Update()
	{
		if ((object)descriptor != null && Settings.Instance.Reflections)
		{
			Vector3 forward = mainCamera.transform.forward;
			Vector3 up = mainCamera.transform.up;
			Vector3 position = mainCamera.transform.position;
			Vector3 direction = descriptor.transform.InverseTransformDirection(forward);
			Vector3 vector = descriptor.transform.InverseTransformDirection(up);
			Vector3 position2 = descriptor.transform.InverseTransformPoint(position);
			direction.y *= -1f;
			vector.y *= -1f;
			position2.y *= -1f;
			forward = descriptor.transform.TransformDirection(direction);
			up = descriptor.transform.TransformDirection(up);
			position = descriptor.transform.TransformPoint(position2);
			base.transform.position = position;
			base.transform.LookAt(position + forward, up);
		}
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(LoadPlatform));
	}

	private void LoadPlatform(PlatformDescriptor obj)
	{
		descriptor = obj;
	}
}
