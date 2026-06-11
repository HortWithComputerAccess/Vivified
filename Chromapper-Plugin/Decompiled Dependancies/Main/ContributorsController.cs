using System.Collections;
using System.Collections.Generic;
using System.Linq;
using __Scripts.UI.SongEditMenu;
using Beatmap.Info;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ContributorsController : MonoBehaviour
{
	public ImageBrowser ImageBrowser;

	[SerializeField]
	private GameObject listContainer;

	[SerializeField]
	private GameObject listItemPrefab;

	public readonly List<BaseContributor> Contributors = new List<BaseContributor>();

	private int initialItemsCount;

	private readonly List<ContributorListItem> items = new List<ContributorListItem>();

	private void Start()
	{
		base.transform.parent.gameObject.SetActive(value: false);
		UndoChanges();
	}

	public void UndoChanges()
	{
		HandleRemoveAllContributors(0);
		foreach (BaseContributor customContributor in BeatSaberSongContainer.Instance.Info.CustomContributors)
		{
			ContributorListItem component = Object.Instantiate(listItemPrefab, listContainer.transform).GetComponent<ContributorListItem>();
			component.Setup(customContributor, this);
			Contributors.Add(customContributor);
			items.Add(component);
		}
		initialItemsCount = items.Count;
	}

	public void RemoveContributor(ContributorListItem item)
	{
		items.Remove(item);
		Object.Destroy(item.gameObject);
		Contributors.Remove(item.Contributor);
	}

	public void RemoveAllContributors()
	{
		PersistentUI.Instance.ShowDialogBox("Contributors", "removeall", HandleRemoveAllContributors, PersistentUI.DialogBoxPresetType.YesNo);
	}

	public void AddNewContributor()
	{
		BaseContributor baseContributor = new BaseContributor("", "", "");
		ContributorListItem component = Object.Instantiate(listItemPrefab, listContainer.transform).GetComponent<ContributorListItem>();
		component.Setup(baseContributor, this, dirty: true);
		Contributors.Add(baseContributor);
		items.Add(component);
		StartCoroutine(WaitToScroll());
	}

	public IEnumerator WaitToScroll()
	{
		yield return new WaitForEndOfFrame();
		listContainer.GetComponentInParent<ScrollRect>().normalizedPosition = new Vector2(0f, 0f);
	}

	private void HandleRemoveAllContributors(int res)
	{
		if (res > 0)
		{
			return;
		}
		foreach (ContributorListItem item in items)
		{
			Object.Destroy(item.gameObject);
		}
		items.Clear();
		Contributors.Clear();
	}

	public bool IsDirty()
	{
		if (items.Count == initialItemsCount)
		{
			return items.Any((ContributorListItem it) => it.Dirty);
		}
		return true;
	}

	public void Commit()
	{
		foreach (ContributorListItem item in items)
		{
			item.Commit();
		}
	}
}
