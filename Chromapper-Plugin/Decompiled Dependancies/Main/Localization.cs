using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Localization", menuName = "Localization")]
public class Localization : ScriptableObject
{
	public bool OverwriteLocalizationText;

	public int OverwriteLocalizationTextID;

	[FormerlySerializedAs("loadingMessages")]
	[TextArea(3, 10)]
	public string[] LoadingMessages;

	public Sprite[] WaifuSprites;

	public string GetRandomLoadingMessage()
	{
		if (!Settings.Instance.HelpfulLoadingMessages)
		{
			return string.Empty;
		}
		if (!OverwriteLocalizationText)
		{
			return LoadingMessages[Random.Range(0, LoadingMessages.Length)];
		}
		return LoadingMessages[OverwriteLocalizationTextID];
	}

	public Sprite GetRandomWaifuSprite()
	{
		return WaifuSprites[Random.Range(0, WaifuSprites.Length)];
	}
}
