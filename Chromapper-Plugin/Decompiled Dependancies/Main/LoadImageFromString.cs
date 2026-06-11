using System.Collections.Generic;
using UnityEngine;

public class LoadImageFromString : MonoBehaviour
{
	[SerializeField]
	private TextAsset bytes;

	[SerializeField]
	private SpriteRenderer spriteImage;

	private void Start()
	{
		List<byte> list = new List<byte>();
		string[] array = bytes.text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			byte item = byte.Parse(array[i]);
			list.Add(item);
		}
		Texture2D texture2D = new Texture2D(2, 2);
		if (texture2D.LoadImage(list.ToArray()))
		{
			spriteImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.one / 2f);
		}
	}
}
