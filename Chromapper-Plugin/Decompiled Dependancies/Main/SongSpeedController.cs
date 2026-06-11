using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SongSpeedController : MonoBehaviour, CMInput.ISongSpeedActions
{
	[FormerlySerializedAs("source")]
	public AudioSource Source;

	[SerializeField]
	private TextMeshProUGUI songDisplayText;

	private float songSpeed = 10f;

	private void Start()
	{
		Settings.NotifyBySettingName("SongSpeed", UpdateSongSpeed);
		Settings.NonPersistentSettings["SongSpeed"] = 10;
	}

	private void Update()
	{
		if (!(songDisplayText.color.a <= 0f))
		{
			float a = songDisplayText.color.a - Time.deltaTime;
			songDisplayText.color = new Color(1f, 1f, 1f, a);
		}
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("SongSpeed");
	}

	public void OnDecreaseSongSpeed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			songSpeed -= Settings.Instance.SongSpeedChangeAmount / 2f;
			songSpeed = 10f - Mathf.Round((10f - songSpeed) / Settings.Instance.SongSpeedChangeAmount * 2f) * Settings.Instance.SongSpeedChangeAmount / 2f;
			if (songSpeed < 0.5f)
			{
				songSpeed = 0.5f;
			}
			Settings.ManuallyNotifySettingUpdatedEvent("SongSpeed", songSpeed);
			UpdateSongSpeed(songSpeed);
		}
	}

	public void OnIncreaseSongSpeed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			songSpeed += Settings.Instance.SongSpeedChangeAmount / 2f;
			songSpeed = 10f - Mathf.Round((10f - songSpeed) / Settings.Instance.SongSpeedChangeAmount * 2f) * Settings.Instance.SongSpeedChangeAmount / 2f;
			if (songSpeed > 30f)
			{
				songSpeed = 30f;
			}
			Settings.ManuallyNotifySettingUpdatedEvent("SongSpeed", songSpeed);
			UpdateSongSpeed(songSpeed);
		}
	}

	public void UpdateSongSpeed(object value)
	{
		float num = (float)Convert.ChangeType(value, typeof(float));
		Source.pitch = num / 10f;
		songSpeed = num;
		songDisplayText.color = Color.white;
		songDisplayText.text = $"{songSpeed * 10f}%";
	}
}
