using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SearchableTab : MonoBehaviour
{
	[SerializeField]
	private List<SearchableSection> sections;

	[SerializeField]
	private RectTransform layoutGroup;

	[SerializeField]
	private GameObject tab;

	public void RegisterSection(SearchableSection section)
	{
		sections.Add(section);
	}

	public void RemoveSection(SearchableSection section)
	{
		sections.Remove(section);
	}

	public bool UpdateSearch(string text)
	{
		bool flag = sections.Select((SearchableSection it) => it.UpdateSearch(text)).ToList().Any((bool it) => it);
		tab.SetActive(flag);
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
		return flag;
	}
}
