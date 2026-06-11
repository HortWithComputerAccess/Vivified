using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class BPMTapperController : MonoBehaviour, CMInput.IBPMTapperActions
{
	public static bool IsActive;

	private static bool swap;

	[FormerlySerializedAs("_bpmText")]
	[SerializeField]
	private TextMeshProUGUI bpmText;

	private readonly List<float> taps = new List<float>();

	private bool isTapping;

	private float t1;

	private float timeSinceLastTap;

	public void Reset()
	{
		isTapping = false;
		StopAllCoroutines();
		bpmText.text = "Tap...";
		taps.Clear();
	}

	private void Start()
	{
		bpmText.text = "";
	}

	public void OnToggleBPMTapper(InputAction.CallbackContext context)
	{
		if (context.performed && UIMode.SelectedMode == UIModeType.Normal)
		{
			swap = !swap;
			StopAllCoroutines();
			StartCoroutine(UpdateGroup(swap, base.transform as RectTransform));
		}
	}

	public void Close()
	{
		swap = false;
		StartCoroutine(UpdateGroup(swap, base.transform as RectTransform));
	}

	private IEnumerator UpdateGroup(bool enabled, RectTransform group)
	{
		float dest = (enabled ? 120 : (-200));
		float og = group.anchoredPosition.y;
		float t = 0f;
		while ((double)t < 0.4)
		{
			t += Time.deltaTime;
			group.anchoredPosition = new Vector2(group.anchoredPosition.x, Mathf.Lerp(og, dest, t));
			og = group.anchoredPosition.y;
			yield return new WaitForEndOfFrame();
		}
		if (!enabled)
		{
			Reset();
		}
		group.anchoredPosition = new Vector2(group.anchoredPosition.x, dest);
		IsActive = enabled;
	}

	public void Tap()
	{
		timeSinceLastTap = 0f;
		if (!isTapping)
		{
			isTapping = true;
			StartCoroutine(WaitForReset());
			bpmText.text = "Tap...";
			t1 = Time.time;
		}
		else
		{
			float item = Time.time - t1;
			t1 = Time.time;
			taps.Add(item);
			bpmText.text = Math.Round(CalculateBpm(), 2).ToString();
		}
	}

	private float CalculateBpm()
	{
		float num = taps.Average();
		return 1000f / (num * 1000f) * 60f;
	}

	private IEnumerator WaitForReset()
	{
		while (timeSinceLastTap < 3f)
		{
			timeSinceLastTap += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Reset();
	}
}
