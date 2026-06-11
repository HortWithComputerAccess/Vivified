using System;
using System.Collections;
using Beatmap.Base;
using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private BeatmapObjectCallbackController callbackController;

	[SerializeField]
	private AnimationCurve anim;

	[SerializeField]
	private float scaleFactor = 1f;

	[SerializeField]
	private bool useColours;

	[SerializeField]
	private Color baseColor;

	[SerializeField]
	private Color red;

	[SerializeField]
	private Color blue;

	[SerializeField]
	private Renderer[] planeRends;

	private Color color;

	private float lastTime = -1f;

	private Vector3 startScale;

	private float t;

	private void Start()
	{
		startScale = base.transform.localScale;
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnEnable()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
		beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(HandleCallback));
	}

	private void OnDisable()
	{
		BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
		beatmapObjectCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(HandleCallback));
	}

	private void OnDestroy()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
	}

	private void OnPlayToggle(bool playing)
	{
		lastTime = -1f;
	}

	private void HandleCallback(bool initial, int index, BaseObject objectData)
	{
		if (objectData.JsonTime == lastTime || !DingOnNotePassingGrid.NoteTypeToDing[(objectData as BaseNote).Type])
		{
			return;
		}
		BaseNote baseNote = (BaseNote)objectData;
		if (useColours)
		{
			Color color;
			switch (baseNote.Type)
			{
			default:
				return;
			case 0:
				color = red;
				break;
			case 1:
				color = blue;
				break;
			}
			this.color = ((lastTime == objectData.JsonTime) ? Color.Lerp(this.color, color, 0.5f) : color);
		}
		if (t <= 0f)
		{
			t = 1f;
			StartCoroutine(VisualFeedbackAnim());
		}
		else
		{
			t = 1f;
		}
		lastTime = objectData.JsonTime;
	}

	private IEnumerator VisualFeedbackAnim()
	{
		while (t > 0f)
		{
			float a = anim.Evaluate(Mathf.Clamp01(t));
			UpdateAppearance(a);
			yield return null;
			t -= Time.deltaTime;
		}
		t = 0f;
		UpdateAppearance(0f);
	}

	private void UpdateAppearance(float a)
	{
		base.transform.localScale = startScale * (1f + 0.1f * a * scaleFactor);
		if (useColours)
		{
			Renderer[] array = planeRends;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material.color = Color.Lerp(baseColor, color, a);
			}
		}
	}
}
