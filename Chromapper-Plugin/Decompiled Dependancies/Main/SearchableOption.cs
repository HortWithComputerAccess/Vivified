using System;
using System.Linq;
using UnityEngine;

public class SearchableOption : MonoBehaviour
{
	public string[] Keywords;

	private bool Matches(string text)
	{
		string[] source = text.Split(' ');
		if (text.Length != 0)
		{
			return source.All((string part) => Keywords.Any((string it) => it.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) >= 0));
		}
		return true;
	}

	public bool UpdateSearch(string text)
	{
		bool flag = Matches(text);
		base.gameObject.SetActive(flag);
		return flag;
	}
}
