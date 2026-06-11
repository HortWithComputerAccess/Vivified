using Beatmap.Containers;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Note Appearance SO", fileName = "NoteAppearanceSO")]
public class NoteAppearanceSO : ScriptableObject
{
	[SerializeField]
	private GameObject notePrefab;

	[Space(10f)]
	[SerializeField]
	private Sprite unknownSprite;

	[SerializeField]
	private Sprite arrowSprite;

	[SerializeField]
	private Sprite dotSprite;

	[Space(10f)]
	[SerializeField]
	private Material unknownNoteMaterial;

	[Space(10f)]
	[SerializeField]
	private Material blueNoteSharedMaterial;

	[SerializeField]
	private Material redNoteSharedMaterial;

	[Space(20f)]
	[Header("ChromaToggle Notes")]
	[SerializeField]
	private Sprite deflectSprite;

	[Space(10f)]
	[SerializeField]
	private Material greenNoteSharedMaterial;

	[SerializeField]
	private Material magentaNoteSharedMaterial;

	[Space(10f)]
	[SerializeField]
	private Material monochromeSharedNoteMaterial;

	[SerializeField]
	private Material duochromeSharedNoteMaterial;

	[Space(10f)]
	[SerializeField]
	private Material superNoteSharedMaterial;

	public Color RedColor { get; private set; } = DefaultColors.LeftNote;

	public Color BlueColor { get; private set; } = DefaultColors.RightNote;

	public void UpdateColor(Color red, Color blue)
	{
		RedColor = red;
		BlueColor = blue;
	}

	public void SetNoteAppearance(NoteContainer note)
	{
		if (note.NoteData.Type != 3)
		{
			if (note.NoteData.CutDirection != 8)
			{
				note.SetArrowVisible(b: true);
				note.SetDotVisible(b: false);
			}
			else
			{
				note.SetArrowVisible(b: false);
				note.SetDotVisible(b: true);
			}
			switch (note.NoteData.Type)
			{
			case 0:
				note.SetColor(RedColor);
				break;
			case 1:
				note.SetColor(BlueColor);
				break;
			default:
				note.SetColor(null);
				break;
			}
		}
		else
		{
			note.SetArrowVisible(b: false);
			note.SetDotVisible(b: false);
			note.SetColor(null);
		}
		if (note.NoteData.CustomColor.HasValue)
		{
			note.SetColor(note.NoteData.CustomColor);
		}
		note.Animator.AttachToObject(note.NoteData);
	}
}
