using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base.Customs;
using UnityEngine;
using UnityEngine.UI;

public class EnvRemoval : MonoBehaviour
{
	[SerializeField]
	private GameObject listContainer;

	[SerializeField]
	private GameObject listItemPrefab;

	[SerializeField]
	private DifficultySelect difficultySelect;

	private readonly List<EnvRemovalListItem> envRemovalList = new List<EnvRemovalListItem>();

	public List<BaseEnvironmentEnhancement> EnvRemovalList => envRemovalList.Select((EnvRemovalListItem it) => it.Value).ToList();

	public void AddItem()
	{
		BaseEnvironmentEnhancement baseEnvironmentEnhancement = new BaseEnvironmentEnhancement("");
		baseEnvironmentEnhancement.Active = false;
		AddItem(baseEnvironmentEnhancement);
		UpdateEnvRemoval();
		StartCoroutine(WaitToScroll());
	}

	public void AddItem(BaseEnvironmentEnhancement text)
	{
		EnvRemovalListItem component = Object.Instantiate(listItemPrefab, listContainer.transform).GetComponent<EnvRemovalListItem>();
		component.Setup(this, text);
		envRemovalList.Add(component);
	}

	public void Remove(EnvRemovalListItem item)
	{
		envRemovalList.Remove(item);
		Object.Destroy(item.gameObject);
		UpdateEnvRemoval();
	}

	public IEnumerator WaitToScroll(int y = 0)
	{
		yield return new WaitForEndOfFrame();
		listContainer.GetComponentInParent<ScrollRect>().normalizedPosition = new Vector2(0f, y);
	}

	public void ClearList()
	{
		foreach (EnvRemovalListItem envRemoval in envRemovalList)
		{
			Object.Destroy(envRemoval.gameObject);
		}
		envRemovalList.Clear();
	}

	public void UpdateFromDiff(List<BaseEnvironmentEnhancement> localEnvRemoval)
	{
		ClearList();
		foreach (BaseEnvironmentEnhancement item in localEnvRemoval)
		{
			AddItem(item);
		}
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(WaitToScroll(1));
		}
	}

	public void UpdateEnvRemoval()
	{
		difficultySelect.UpdateEnvRemoval();
	}
}
