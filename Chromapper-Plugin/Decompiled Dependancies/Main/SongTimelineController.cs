using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SongTimelineController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI timeMesh;

	[SerializeField]
	private TextMeshProUGUI currentBeatMesh;

	[SerializeField]
	private AudioSource mainAudioSource;

	public bool IsClicked;

	private float lastSongTime;

	private float songLength;

	private const string beatFormat = "<mspace=0.4em>{0:0}</mspace><size=20>.<mspace=0.4em>{1:000}</mspace></size>";

	private const string timeFormat = "<mspace=0.4em>{3}{0:0}</mspace>:<mspace=0.4em>{1:00}</mspace><size=20>.<mspace=0.4em>{2:000}</mspace></size>";

	public static bool IsHovering { get; private set; }

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => mainAudioSource.clip != null);
		songLength = mainAudioSource.clip.length;
		slider.value = 0f;
	}

	private void Update()
	{
		if (atsc.CurrentSeconds != lastSongTime)
		{
			if (!IsClicked)
			{
				lastSongTime = atsc.CurrentSeconds;
				slider.value = lastSongTime / songLength;
			}
			int num = Mathf.Abs(Mathf.FloorToInt(atsc.CurrentSeconds % 60f));
			float f = atsc.CurrentSeconds / 60f;
			int num2 = Mathf.Abs((atsc.CurrentSeconds > 0f) ? Mathf.FloorToInt(f) : Mathf.CeilToInt(f));
			int num3 = Mathf.FloorToInt((atsc.CurrentSeconds - (float)Mathf.FloorToInt(atsc.CurrentSeconds)) * 1000f);
			timeMesh.text = string.Format("<mspace=0.4em>{3}{0:0}</mspace>:<mspace=0.4em>{1:00}</mspace><size=20>.<mspace=0.4em>{2:000}</mspace></size>", num2, num, num3, (atsc.CurrentSeconds < 0f) ? "-" : "");
			int num4 = (int)atsc.CurrentJsonTime;
			int num5 = Mathf.FloorToInt((atsc.CurrentJsonTime - (float)Mathf.FloorToInt(atsc.CurrentJsonTime)) * 1000f);
			currentBeatMesh.text = $"<mspace=0.4em>{num4:0}</mspace><size=20>.<mspace=0.4em>{num5:000}</mspace></size>";
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsHovering = false;
	}

	public void TriggerUpdate()
	{
		UpdateSongTimelineSlider(slider.value);
	}

	public void UpdateSongTimelineSlider(float sliderValue)
	{
		if (!atsc.IsPlaying && Input.GetAxis("Mouse ScrollWheel") == 0f && IsClicked)
		{
			if (NodeEditorController.IsActive)
			{
				slider.value = lastSongTime / songLength;
			}
			else
			{
				atsc.SnapToGrid(sliderValue * songLength);
			}
		}
	}
}
