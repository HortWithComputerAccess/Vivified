using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MapEditorUI : MonoBehaviour
{
	[FormerlySerializedAs("mainUIGroup")]
	public CanvasGroup[] MainUIGroup;

	[SerializeField]
	private CanvasScaler[] extraSizeChanges;

	private readonly Dictionary<CanvasGroup, Coroutine> canvasFadeCoroutines = new Dictionary<CanvasGroup, Coroutine>();

	private readonly List<CanvasScaler> canvasScalers = new List<CanvasScaler>();

	private readonly List<float> canvasScalersSizes = new List<float>();

	private void Start()
	{
		CanvasGroup[] mainUIGroup = MainUIGroup;
		for (int i = 0; i < mainUIGroup.Length; i++)
		{
			CanvasScaler component = mainUIGroup[i].transform.parent.GetComponent<CanvasScaler>();
			if (component != null)
			{
				canvasScalers.Add(component);
				canvasScalersSizes.Add(component.referenceResolution.x);
			}
		}
		CanvasScaler[] array = extraSizeChanges;
		foreach (CanvasScaler canvasScaler in array)
		{
			canvasScalers.Add(canvasScaler);
			canvasScalersSizes.Add(canvasScaler.referenceResolution.x);
		}
	}

	public void ToggleUIVisible(bool visible, CanvasGroup group)
	{
		Coroutine value = StartCoroutine(visible ? FadeCanvasGroup(group, group.alpha, 1f) : FadeCanvasGroup(group, group.alpha, 0f));
		if (canvasFadeCoroutines.ContainsKey(group))
		{
			canvasFadeCoroutines[group] = value;
		}
		else
		{
			canvasFadeCoroutines.Add(group, value);
		}
		group.interactable = visible;
		group.blocksRaycasts = visible;
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float time = 0.2f)
	{
		Coroutine coroutine = null;
		if (canvasFadeCoroutines.ContainsKey(group))
		{
			coroutine = canvasFadeCoroutines[group];
		}
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / time;
			if (t > 1f)
			{
				t = 1f;
			}
			group.alpha = Mathf.MoveTowards(start, end, t);
			yield return new WaitForEndOfFrame();
			if (group.alpha == end)
			{
				break;
			}
		}
	}
}
