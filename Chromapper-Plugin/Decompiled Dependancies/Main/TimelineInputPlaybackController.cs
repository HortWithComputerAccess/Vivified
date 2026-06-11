using System;
using UnityEngine;

public class TimelineInputPlaybackController : MonoBehaviour
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	private bool resume;

	private void OnEnable()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnDestroy()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	public void PointerDown()
	{
		if (atsc.IsPlaying)
		{
			atsc.TogglePlaying();
			resume = true;
		}
	}

	public void PointerUp()
	{
		if (resume && !atsc.IsPlaying)
		{
			atsc.TogglePlaying();
		}
		resume = false;
	}

	private void OnPlayToggle(bool playing)
	{
		resume = false;
	}
}
