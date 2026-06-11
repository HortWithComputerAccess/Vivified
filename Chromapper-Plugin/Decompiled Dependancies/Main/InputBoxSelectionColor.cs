using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputBoxSelectionColor : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Color normal;

	[SerializeField]
	private Color selected;

	public void OnDeselect(BaseEventData eventData)
	{
		base.gameObject.GetComponent<Image>().color = normal;
	}

	public void OnSelect(BaseEventData eventData)
	{
		base.gameObject.GetComponent<Image>().color = selected;
	}
}
