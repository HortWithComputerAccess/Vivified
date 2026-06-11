using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomColorButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Image image;

	public event Action onRightClick;

	public event Action onMiddleClick;

	public void OnPointerClick(PointerEventData data)
	{
		switch (data.button)
		{
		case PointerEventData.InputButton.Middle:
			this.onMiddleClick?.Invoke();
			break;
		case PointerEventData.InputButton.Right:
			this.onRightClick?.Invoke();
			break;
		}
	}
}
