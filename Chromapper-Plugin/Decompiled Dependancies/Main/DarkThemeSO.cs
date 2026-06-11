using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DarkThemeSO", menuName = "Map/Dark Theme SO")]
public class DarkThemeSO : ScriptableObject
{
	[FormerlySerializedAs("BeonReplacement")]
	[SerializeField]
	private TMP_FontAsset beonReplacement;

	public TMP_FontAsset TekoReplacement;

	[FormerlySerializedAs("BeonUnityReplacement")]
	[SerializeField]
	private Font beonUnityReplacement;

	[FormerlySerializedAs("TekoUnityReplacement")]
	[SerializeField]
	private Font tekoUnityReplacement;

	public void DarkThemeifyUI()
	{
		if (!Settings.Instance.DarkTheme)
		{
			return;
		}
		TextMeshProUGUI[] array = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in array)
		{
			if (!(textMeshProUGUI == null) && !(textMeshProUGUI.font == null))
			{
				if (textMeshProUGUI.font.name.Contains("Beon"))
				{
					textMeshProUGUI.font = beonReplacement;
				}
				if (textMeshProUGUI.font.name.Contains("Teko"))
				{
					textMeshProUGUI.font = TekoReplacement;
				}
			}
		}
		Text[] array2 = Resources.FindObjectsOfTypeAll<Text>();
		foreach (Text text in array2)
		{
			if (!(text == null) && !(text.font == null))
			{
				if (text.font.name.Contains("Beon"))
				{
					text.font = beonUnityReplacement;
				}
				if (text.font.name.Contains("Teko"))
				{
					text.font = tekoUnityReplacement;
				}
			}
		}
	}
}
