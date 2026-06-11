using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SongCoreInformation : MonoBehaviour
{
	[SerializeField]
	private GameObject listContainer;

	[SerializeField]
	private GameObject listItemPrefab;

	[SerializeField]
	private DifficultySelect difficultySelect;

	[SerializeField]
	private bool isWarning;

	private readonly List<SongCoreInformationListItem> songCoreInfoListItems = new List<SongCoreInformationListItem>();

	public List<string> InfoList => songCoreInfoListItems.Select((SongCoreInformationListItem it) => it.Value).ToList();

	public void AddItem()
	{
		AddItem("");
		UpdateSongCoreInfo();
		StartCoroutine(WaitToScroll());
	}

	private void AddItem(string text)
	{
		SongCoreInformationListItem component = Object.Instantiate(listItemPrefab, listContainer.transform).GetComponent<SongCoreInformationListItem>();
		component.Setup(this, text);
		songCoreInfoListItems.Add(component);
	}

	public void Remove(SongCoreInformationListItem listItem)
	{
		songCoreInfoListItems.Remove(listItem);
		Object.Destroy(listItem.gameObject);
		UpdateSongCoreInfo();
	}

	public IEnumerator WaitToScroll(int y = 0)
	{
		yield return new WaitForEndOfFrame();
		listContainer.GetComponentInParent<ScrollRect>().normalizedPosition = new Vector2(0f, y);
	}

	public void ClearList()
	{
		foreach (SongCoreInformationListItem songCoreInfoListItem in songCoreInfoListItems)
		{
			Object.Destroy(songCoreInfoListItem.gameObject);
		}
		songCoreInfoListItems.Clear();
	}

	public void UpdateFromDiff(List<string> localSongCoreInfos)
	{
		ClearList();
		foreach (string localSongCoreInfo in localSongCoreInfos)
		{
			AddItem(localSongCoreInfo);
		}
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(WaitToScroll(1));
		}
	}

	public void UpdateSongCoreInfo()
	{
		if (isWarning)
		{
			difficultySelect.UpdateCustomWarnings();
		}
		else
		{
			difficultySelect.UpdateSongCoreInformation();
		}
	}
}
