using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[FormerlySerializedAs("tooltip")]
	public LocalizedString LocalizedTooltip;

	[FormerlySerializedAs("tooltipOverride")]
	[HideInInspector]
	public string TooltipOverride;

	[FormerlySerializedAs("advancedTooltip")]
	public string AdvancedTooltip;

	public bool TooltipActive;

	private Coroutine routine;

	private void OnDisable()
	{
		OnPointerExit(null);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (routine == null)
		{
			routine = StartCoroutine(TooltipRoutine(0f));
		}
		TooltipActive = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (routine != null)
		{
			StopCoroutine(routine);
			routine = null;
		}
		PersistentUI.Instance.HideTooltip();
		TooltipActive = false;
	}

	private IEnumerator TooltipRoutine(float timeToWait)
	{
		string message = TooltipOverride;
		if (string.IsNullOrEmpty(TooltipOverride))
		{
			message = LocalizedTooltip.GetLocalizedString();
		}
		PersistentUI.Instance.SetTooltip(message, AdvancedTooltip);
		yield return new WaitForSeconds(timeToWait);
		PersistentUI.Instance.ShowTooltip();
	}
}
