using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadedPluginsController : MonoBehaviour
{
	[SerializeField]
	private GameObject pluginInfoPrefab;

	[SerializeField]
	private VerticalLayoutGroup parentLayoutGroup;

	[SerializeField]
	private SearchableTab searchableTab;

	public int Count => PluginLoader.LoadedPlugins.Count;

	private void Start()
	{
		IEnumerable<Plugin> loadedPlugins = PluginLoader.LoadedPlugins;
		if (!loadedPlugins.Any())
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		foreach (Plugin item in loadedPlugins)
		{
			PluginInfoContainer component = Object.Instantiate(pluginInfoPrefab, base.transform).GetComponent<PluginInfoContainer>();
			component.UpdatePluginInfo(item);
			searchableTab.RegisterSection(component.SearchableSection);
		}
		StartCoroutine(FuckingSetThisShitDirty());
	}

	private IEnumerator FuckingSetThisShitDirty()
	{
		yield return new WaitForSeconds(0.1f);
		parentLayoutGroup.spacing = 15f;
	}
}
