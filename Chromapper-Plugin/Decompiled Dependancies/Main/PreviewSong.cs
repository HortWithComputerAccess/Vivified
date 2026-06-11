using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewSong : MonoBehaviour
{
	private static readonly int songTimeSeconds = Shader.PropertyToID("_SongTimeSeconds");

	[SerializeField]
	private Image progressBar;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private TMP_InputField previewStartTime;

	[SerializeField]
	private TMP_InputField previewDuration;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite startSprite;

	[SerializeField]
	private Sprite stopSprite;

	private readonly float lengthOffset = 1.4f;

	private float length = 10f;

	private bool playing = true;

	private double startTime;

	public void Start()
	{
		PlayClip();
	}

	public void Update()
	{
		if (!playing)
		{
			Shader.SetGlobalFloat(songTimeSeconds, -100f);
			return;
		}
		float num = (float)(AudioSettings.dspTime - startTime);
		float num2 = length - num;
		if (num2 <= 0f)
		{
			PlayClip();
		}
		else if ((double)num2 < 1.25)
		{
			audioSource.volume = Settings.Instance.SongVolume * 0.64f * num2 * num2;
		}
		else if ((double)num < 0.2)
		{
			audioSource.volume = Settings.Instance.SongVolume * 5f * num;
		}
		else
		{
			audioSource.volume = Settings.Instance.SongVolume;
		}
		Shader.SetGlobalFloat(songTimeSeconds, audioSource.time);
		float fillAmount = ((num > length) ? 0f : (num / length));
		progressBar.fillAmount = fillAmount;
	}

	public void PlayClip()
	{
		if (playing)
		{
			progressBar.fillAmount = 0f;
			image.sprite = startSprite;
			audioSource.Stop();
			playing = false;
			Shader.SetGlobalFloat(songTimeSeconds, -100f);
		}
		else
		{
			if (!float.TryParse(previewDuration.text, out length))
			{
				return;
			}
			length -= lengthOffset;
			if (!float.TryParse(previewStartTime.text, out var result))
			{
				return;
			}
			if (audioSource.clip == null)
			{
				PersistentUI.Instance.ShowDialogBox("SongEditMenu", "preview.valid", null, PersistentUI.DialogBoxPresetType.Ok);
				return;
			}
			if (length + result > audioSource.clip.length)
			{
				length = audioSource.clip.length - result;
			}
			playing = true;
			startTime = AudioSettings.dspTime;
			audioSource.time = result;
			audioSource.Play();
			image.sprite = stopSprite;
		}
	}
}
