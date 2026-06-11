using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForwardScroll : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public void OnScroll(PointerEventData eventData)
	{
		base.transform.GetComponentInParent<ScrollRect>().OnScroll(eventData);
	}
}
