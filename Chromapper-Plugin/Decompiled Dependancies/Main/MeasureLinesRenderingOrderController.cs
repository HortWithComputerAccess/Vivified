using UnityEngine;

public class MeasureLinesRenderingOrderController : MonoBehaviour
{
	[SerializeField]
	private Canvas effectingCanvas;

	private void Start()
	{
		Settings.NotifyBySettingName("MeasureLinesShowOnTop", UpdateCanvasOrder);
		UpdateCanvasOrder(Settings.Instance.MeasureLinesShowOnTop);
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("MeasureLinesShowOnTop");
	}

	private void UpdateCanvasOrder(object obj)
	{
		effectingCanvas.sortingLayerName = (((bool)obj) ? "Background" : "Default");
	}
}
