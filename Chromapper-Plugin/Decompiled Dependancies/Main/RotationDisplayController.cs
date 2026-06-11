using System;
using TMPro;
using UnityEngine;

public class RotationDisplayController : MonoBehaviour
{
	[SerializeField]
	private RotationCallbackController rotationCallback;

	[SerializeField]
	private TextMeshProUGUI display;

	private void Start()
	{
		base.gameObject.SetActive(rotationCallback.IsActive);
		RotationCallbackController rotationCallbackController = rotationCallback;
		rotationCallbackController.RotationChangedEvent = (Action<bool, float>)Delegate.Combine(rotationCallbackController.RotationChangedEvent, new Action<bool, float>(RotationChanged));
	}

	private void OnDestroy()
	{
		RotationCallbackController rotationCallbackController = rotationCallback;
		rotationCallbackController.RotationChangedEvent = (Action<bool, float>)Delegate.Remove(rotationCallbackController.RotationChangedEvent, new Action<bool, float>(RotationChanged));
	}

	private void RotationChanged(bool natural, float rotation)
	{
		if (Settings.Instance.Reset360DisplayOnCompleteTurn)
		{
			display.text = $"{BetterModulo(rotation, 360f)}°";
		}
		else
		{
			display.text = $"{rotation}°";
		}
	}

	private float BetterModulo(float x, float m)
	{
		return (x % m + m) % m;
	}
}
