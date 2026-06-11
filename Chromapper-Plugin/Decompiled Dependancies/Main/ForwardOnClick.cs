using UnityEngine;
using UnityEngine.EventSystems;

public class ForwardOnClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private DifficultySelect diffSelector;

	private void Start()
	{
		diffSelector = base.transform.GetComponentInParent<DifficultySelect>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Transform parent = base.transform.parent.parent;
		diffSelector.OnClick(parent);
	}
}
