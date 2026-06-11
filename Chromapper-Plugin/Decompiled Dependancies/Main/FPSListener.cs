using UnityEngine;

public class FPSListener : MonoBehaviour
{
	private void Start()
	{
		Settings.NotifyBySettingName("MaximumFPS", UpdateFPS);
		Settings.NotifyBySettingName("VSync", UpdateFPS);
		UpdateFPS(null);
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("MaximumFPS");
		Settings.ClearSettingNotifications("VSync");
	}

	private void UpdateFPS(object _)
	{
		QualitySettings.vSyncCount = (Settings.Instance.VSync ? 1 : 0);
		Application.targetFrameRate = Settings.Instance.MaximumFPS;
	}
}
