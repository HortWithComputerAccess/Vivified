using UnityEngine;
using UnityEngine.EventSystems;

public class ColorPickerMessageSender : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData data)
	{
		switch (data.button)
		{
		case PointerEventData.InputButton.Left:
			SendMessageUpwards("PresetSelect", base.gameObject);
			break;
		case PointerEventData.InputButton.Middle:
			SendMessageUpwards("OverridePreset", base.gameObject);
			break;
		case PointerEventData.InputButton.Right:
			SendMessageUpwards("DeletePreset", base.gameObject);
			break;
		}
	}
}
