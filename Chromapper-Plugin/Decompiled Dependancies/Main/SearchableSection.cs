using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchableSection : MonoBehaviour
{
	[SerializeField]
	private List<SearchableOption> options;

	public void RegisterOption(SearchableOption option)
	{
		options.Add(option);
	}

	public bool UpdateSearch(string text)
	{
		bool flag = options.Select(delegate(SearchableOption it)
		{
			if (it == null)
			{
				Debug.LogWarning("Missing searchable option in " + base.name);
				return false;
			}
			return it.UpdateSearch(text);
		}).ToList().Any((bool it) => it);
		base.gameObject.SetActive(flag);
		return flag;
	}
}
