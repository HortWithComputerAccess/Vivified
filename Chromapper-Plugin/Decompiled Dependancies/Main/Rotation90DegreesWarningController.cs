using System;
using TMPro;
using UnityEngine;

public class Rotation90DegreesWarningController : MonoBehaviour
{
	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private RotationCallbackController rotationCallback;

	[SerializeField]
	private TextMeshProUGUI rotationDisplay;

	private void Start()
	{
		if (BeatSaberSongContainer.Instance.MapDifficultyInfo.Characteristic == "90Degree")
		{
			RotationCallbackController rotationCallbackController = rotationCallback;
			rotationCallbackController.RotationChangedEvent = (Action<bool, float>)Delegate.Combine(rotationCallbackController.RotationChangedEvent, new Action<bool, float>(RotationChangedEvent));
		}
	}

	private void OnDestroy()
	{
		if (BeatSaberSongContainer.Instance.MapDifficultyInfo.Characteristic == "90Degree")
		{
			RotationCallbackController rotationCallbackController = rotationCallback;
			rotationCallbackController.RotationChangedEvent = (Action<bool, float>)Delegate.Remove(rotationCallbackController.RotationChangedEvent, new Action<bool, float>(RotationChangedEvent));
		}
	}

	private void RotationChangedEvent(bool natural, float rotation)
	{
		rotationDisplay.color = ((rotation < -45f || rotation > 45f) ? Color.red : Color.white);
	}
}
