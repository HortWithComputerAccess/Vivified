using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterSliderRingHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private readonly Vector3 ringHidden = new Vector3(0f, 0f, 1f);

	private readonly Vector3 ringVisible = new Vector3(2.5f, 2.5f, 1f);

	private Coroutine onHandleCoroutine;

	private Image ringImage;

	private RectTransform ringTransform;

	private void Start()
	{
		ringImage = GetComponentsInChildren<Image>().First((Image i) => i.name == "Ring");
		ringTransform = ringImage.rectTransform;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		onHandleCoroutine = StartCoroutine(ScaleRing(grow: true));
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		onHandleCoroutine = StartCoroutine(ScaleRing(grow: false));
	}

	private IEnumerator ScaleRing(bool grow)
	{
		if (onHandleCoroutine != null)
		{
			StopCoroutine(onHandleCoroutine);
		}
		float startTime = Time.time;
		while (true)
		{
			Vector3 localScale = ringTransform.localScale;
			if (grow)
			{
				try
				{
					PersistentUI.Instance.HideTooltip();
				}
				catch (NullReferenceException)
				{
				}
			}
			else if (localScale.x == ringVisible.x)
			{
				try
				{
					PersistentUI.Instance.ShowTooltip();
				}
				catch (NullReferenceException)
				{
				}
			}
			Vector3 target = (grow ? ringVisible : ringHidden);
			localScale = Vector3.MoveTowards(localScale, target, Time.time / startTime * 0.1f);
			ringTransform.localScale = localScale;
			if (localScale.x != target.x)
			{
				yield return new WaitForFixedUpdate();
				continue;
			}
			break;
		}
	}
}
