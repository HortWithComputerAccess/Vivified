using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class BongoCat : MonoBehaviour
{
	[SerializeField]
	private BongoCatPreset[] bongoCats;

	[SerializeField]
	private Transform noteGridHeight;

	[FormerlySerializedAs("Larm")]
	[SerializeField]
	private bool larm;

	[FormerlySerializedAs("Rarm")]
	[SerializeField]
	private bool rarm;

	private SpriteRenderer comp;

	private BongoCatPreset selectedBongoCat;

	private float larmTimeout;

	private float rarmTimeout;

	private void Start()
	{
		selectedBongoCat = bongoCats[0];
		comp = GetComponent<SpriteRenderer>();
		Settings.NotifyBySettingName("BongoCat", UpdateBongoCatState);
		UpdateBongoCatState(Settings.Instance.BongoCat);
	}

	private void Update()
	{
		larmTimeout -= Time.deltaTime;
		rarmTimeout -= Time.deltaTime;
		if (larmTimeout < 0f)
		{
			larm = false;
		}
		if (rarmTimeout < 0f)
		{
			rarm = false;
		}
		comp.sprite = ((!larm) ? (rarm ? selectedBongoCat.LeftUpRightDown : selectedBongoCat.LeftUpRightUp) : (rarm ? selectedBongoCat.LeftDownRightDown : selectedBongoCat.LeftDownRightUp));
	}

	private void UpdateBongoCatState(object obj)
	{
		if (Settings.Instance.BongoCat == -1)
		{
			SpriteRenderer spriteRenderer = comp;
			bool flag = (base.enabled = false);
			spriteRenderer.enabled = flag;
		}
		else
		{
			selectedBongoCat = bongoCats[Settings.Instance.BongoCat];
			SpriteRenderer spriteRenderer2 = comp;
			bool flag = (base.enabled = true);
			spriteRenderer2.enabled = flag;
		}
		float x = base.transform.localPosition.x;
		base.transform.localPosition = new Vector3(x, noteGridHeight.lossyScale.z + selectedBongoCat.YOffset, 0f);
		base.transform.localScale = selectedBongoCat.Scale;
	}

	public void TriggerArm(BaseNote note, NoteGridContainer container)
	{
		if (Settings.Instance.BongoCat != -1 && note.Type != 3)
		{
			BaseNote baseNote = container.MapObjects.Find((BaseNote x) => x.JsonTime > note.JsonTime && x.Type == note.Type);
			float num = 0.125f;
			if (baseNote != null)
			{
				num = Mathf.Clamp((baseNote.SongBpmTime - note.SongBpmTime) * 60f / BeatSaberSongContainer.Instance.Info.BeatsPerMinute / 2f, 0.05f, 0.2f);
			}
			switch (note.Type)
			{
			case 0:
				larm = true;
				larmTimeout = num;
				break;
			case 1:
				rarm = true;
				rarmTimeout = num;
				break;
			}
		}
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("BongoCat");
	}
}
