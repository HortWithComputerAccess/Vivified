using Beatmap.Info;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrentDifficultyDisplay : MonoBehaviour
{
	private TextMeshProUGUI textMesh;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
		BaseInfo info = BeatSaberSongContainer.Instance.Info;
		string text = ((info.SongSubName != "") ? (info.SongAuthorName + " - " + info.SongName + " " + info.SongSubName + "\n") : (info.SongAuthorName + " - " + info.SongName + "\n"));
		string text2 = "";
		if (Settings.Instance.DisplaySongDetailsInEditor)
		{
			text2 += text;
		}
		textMesh.text = text2;
	}
}
