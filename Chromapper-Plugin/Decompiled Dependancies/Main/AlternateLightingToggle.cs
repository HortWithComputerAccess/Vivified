using UnityEngine;

public class AlternateLightingToggle : MonoBehaviour
{
	[Tooltip("This GameObject will be disabled if the AlternateLighting setting does not match this value.")]
	[SerializeField]
	private bool alternateLighting;

	private void Start()
	{
		Settings.NotifyBySettingName("AlternateLighting", OnAlternateLightingChanged);
		OnAlternateLightingChanged(Settings.Instance.AlternateLighting);
	}

	private void OnAlternateLightingChanged(object obj)
	{
		base.gameObject.SetActive(Settings.Instance.AlternateLighting == alternateLighting);
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("AlternateLighting");
	}
}
