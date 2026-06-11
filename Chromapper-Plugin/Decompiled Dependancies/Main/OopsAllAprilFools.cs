using System;
using UnityEngine;
using UnityEngine.UI;

public class OopsAllAprilFools : MonoBehaviour
{
	[SerializeField]
	private Sprite chooChooMapper;

	[SerializeField]
	private Image source;

	private void Start()
	{
		DateTime now = DateTime.Now;
		if (now.Month == 4 && now.Day == 1)
		{
			source.sprite = chooChooMapper;
		}
	}
}
