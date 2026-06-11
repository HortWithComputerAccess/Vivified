using UnityEngine;

[CreateAssetMenu(fileName = "SoundList", menuName = "SoundList")]
public class SoundList : ScriptableObject
{
	public AudioClip[] LongClips;

	public AudioClip[] ShortClips;

	public AudioClip GetRandomClip(bool isShortCut)
	{
		if (isShortCut || LongClips.Length == 0)
		{
			return ShortClips[Random.Range(0, ShortClips.Length)];
		}
		return LongClips[Random.Range(0, LongClips.Length)];
	}
}
