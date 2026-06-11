using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Beatmap.Info;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SongList : MonoBehaviour
{
	public enum SongSortType
	{
		Name,
		Modified,
		Artist
	}

	private class FuncComparer<T> : IComparer<T>
	{
		private readonly Comparison<T> comparison;

		protected FuncComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public virtual int Compare(T x, T y)
		{
			int num = comparison(x, y);
			if (num != 0 || x == null)
			{
				return num;
			}
			return x.GetHashCode().CompareTo(y?.GetHashCode());
		}
	}

	private class WithFavouriteComparer : FuncComparer<BaseInfo>
	{
		public WithFavouriteComparer(Comparison<BaseInfo> comparison)
			: base(comparison)
		{
		}

		public override int Compare(BaseInfo a, BaseInfo b)
		{
			if (a?.IsFavourite == b?.IsFavourite)
			{
				return base.Compare(a, b);
			}
			if (a == null || !a.IsFavourite)
			{
				return 1;
			}
			return -1;
		}
	}

	private static readonly IComparer<BaseInfo> sortName = new WithFavouriteComparer((BaseInfo a, BaseInfo b) => string.Compare(a.SongName, b.SongName, StringComparison.InvariantCultureIgnoreCase));

	private static readonly IComparer<BaseInfo> sortModified = new WithFavouriteComparer((BaseInfo a, BaseInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

	private static readonly IComparer<BaseInfo> sortArtist = new WithFavouriteComparer((BaseInfo a, BaseInfo b) => string.Compare(a.SongAuthorName, b.SongAuthorName, StringComparison.InvariantCultureIgnoreCase));

	public SortedSet<BaseInfo> SongInfos = new SortedSet<BaseInfo>(sortName);

	public bool FilteredBySearch;

	[SerializeField]
	private TMP_InputField searchField;

	[SerializeField]
	private Image sortImage;

	[SerializeField]
	private Sprite nameSortSprite;

	[SerializeField]
	private Sprite modifiedSortSprite;

	[SerializeField]
	private Sprite artistSortSprite;

	[SerializeField]
	private Color normalTabColor;

	[SerializeField]
	private Color selectedTabColor;

	[SerializeField]
	private RecyclingListView newList;

	private SongSortType currentSort;

	private List<BaseInfo> filteredSongs = new List<BaseInfo>();

	private static int selectedFolder;

	[SerializeField]
	private GameObject songFolderPrefab;

	private readonly List<GameObject> songFolderObjects = new List<GameObject>();

	private readonly List<string> songFolderPaths = new List<string>();

	private readonly List<bool> songFolderCacheZips = new List<bool>();

	public string SelectedFolderPath => songFolderPaths[selectedFolder];

	public event Action<SongSortType> SortTypeChanged;

	private void Start()
	{
		newList.ItemCallback = delegate(RecyclingListViewItem item, int index)
		{
			if (item is SongListItem songListItem)
			{
				songListItem.AssignSong(filteredSongs[index], searchField.text);
			}
		};
		AddDefaultFolders();
		AddSongCoreFolders();
		currentSort = (SongSortType)Settings.Instance.LastSongSortType;
		ApplySort(currentSort);
		this.SortTypeChanged?.Invoke(currentSort);
		SetSongLocation(selectedFolder);
	}

	private void AddDefaultFolders()
	{
		InitFolderObject("WIP Levels", Settings.Instance.CustomWIPSongsFolder, cacheZips: true);
		InitFolderObject("Custom Levels", Settings.Instance.CustomSongsFolder, cacheZips: false);
	}

	private void AddSongCoreFolders()
	{
		string text = Path.Combine(Path.Combine(Settings.Instance.BeatSaberInstallation, "UserData", "SongCore"), "folders.xml");
		if (!File.Exists(text))
		{
			return;
		}
		foreach (XElement item in XDocument.Load(text).Descendants("folder"))
		{
			string text2 = item.Element("Name")?.Value;
			string text3 = item.Element("Path")?.Value;
			bool.TryParse(item.Element("CacheZIPs")?.Value, out var result);
			if (text2 != null && text3 != null && Directory.Exists(text3))
			{
				InitFolderObject(text2, text3, result);
			}
		}
	}

	private void InitFolderObject(string tabName, string folderPath, bool cacheZips)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(songFolderPrefab, songFolderPrefab.transform.parent, worldPositionStays: true);
		Button component = gameObject.GetComponent<Button>();
		int count = songFolderObjects.Count;
		component.onClick.AddListener(delegate
		{
			SetSongLocation(count);
		});
		GameObject gameObject2 = gameObject.transform.GetChild(0).gameObject;
		gameObject2.GetComponent<TextMeshProUGUI>().text = tabName;
		songFolderObjects.Add(gameObject);
		songFolderPaths.Add(folderPath);
		songFolderCacheZips.Add(cacheZips);
		gameObject.SetActive(value: true);
		if (folderPath == Settings.Instance.CustomWIPSongsFolder || folderPath == Settings.Instance.CustomSongsFolder)
		{
			LocalizeStringEvent component2 = gameObject2.GetComponent<LocalizeStringEvent>();
			component2.StringReference.TableEntryReference = ((folderPath == Settings.Instance.CustomWIPSongsFolder) ? "wip" : "custom");
			component2.enabled = true;
		}
	}

	private void SwitchSort(IComparer<BaseInfo> newSort, Sprite sprite)
	{
		sortImage.sprite = sprite;
		SongInfos = new SortedSet<BaseInfo>(SongInfos, newSort);
		UpdateSongList();
	}

	public void NextSort()
	{
		currentSort = currentSort switch
		{
			SongSortType.Name => SongSortType.Modified, 
			SongSortType.Modified => SongSortType.Artist, 
			_ => SongSortType.Name, 
		};
		ApplySort(currentSort);
		Settings.Instance.LastSongSortType = (int)currentSort;
		this.SortTypeChanged?.Invoke(currentSort);
	}

	public void ApplySort(SongSortType sortType)
	{
		switch (sortType)
		{
		case SongSortType.Name:
			SwitchSort(sortName, nameSortSprite);
			break;
		case SongSortType.Modified:
			SwitchSort(sortModified, modifiedSortSprite);
			break;
		default:
			SwitchSort(sortArtist, artistSortSprite);
			break;
		}
	}

	public void SetSongLocation(int index)
	{
		selectedFolder = index;
		for (int i = 0; i < songFolderObjects.Count; i++)
		{
			songFolderObjects[i].gameObject.GetComponent<Image>().color = ((i == selectedFolder) ? selectedTabColor : normalTabColor);
		}
		TriggerRefresh();
	}

	public void TriggerRefresh()
	{
		StopAllCoroutines();
		StartCoroutine(RefreshCacheZips());
		StartCoroutine(RefreshSongList());
	}

	private IEnumerator RefreshCacheZips()
	{
		if (!songFolderCacheZips[selectedFolder])
		{
			yield break;
		}
		List<FileInfo> list = (from f in new DirectoryInfo(songFolderPaths[selectedFolder]).EnumerateFiles()
			where f.Extension == ".zip"
			select f).ToList();
		if (list.Count > 0)
		{
			string cacheFolderPath = Path.Combine(songFolderPaths[selectedFolder], "ChroMapperZipCache");
			if (Directory.Exists(cacheFolderPath))
			{
				Directory.Delete(cacheFolderPath, recursive: true);
			}
			Directory.CreateDirectory(cacheFolderPath);
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (FileInfo zipFileInfo in list)
			{
				if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.02f)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
				string text = Path.Combine(cacheFolderPath, zipFileInfo.Name);
				Directory.CreateDirectory(text);
				ZipFile.ExtractToDirectory(zipFileInfo.FullName, text);
			}
			InitFolderObject("Cache - " + Path.GetFileNameWithoutExtension(songFolderPaths[selectedFolder]), cacheFolderPath, cacheZips: false);
		}
		songFolderCacheZips[selectedFolder] = false;
	}

	public IEnumerator RefreshSongList()
	{
		IEnumerable<DirectoryInfo> enumerable = from directoryInfo in new DirectoryInfo(songFolderPaths[selectedFolder]).GetDirectories()
			where !directoryInfo.Attributes.HasFlag(FileAttributes.Hidden)
			select directoryInfo;
		SongInfos.Clear();
		newList.Clear();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (DirectoryInfo dir in enumerable)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.02f)
			{
				UpdateSongList();
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			BaseInfo infoFromFolder = BeatSaberSongUtils.GetInfoFromFolder(dir.FullName);
			if (infoFromFolder == null)
			{
				Debug.LogWarning($"No song at location {dir} exists! Is it in a subfolder?");
			}
			else
			{
				SongInfos.Add(infoFromFolder);
			}
		}
		UpdateSongList();
	}

	public void UpdateSongList()
	{
		FilteredBySearch = !string.IsNullOrEmpty(searchField.text);
		if (FilteredBySearch)
		{
			FilterBySearch();
			return;
		}
		filteredSongs = SongInfos.ToList();
		ReloadListItems();
	}

	public void FilterBySearch()
	{
		filteredSongs = SongInfos.Where((BaseInfo x) => x.SongName.IndexOf(searchField.text, StringComparison.InvariantCultureIgnoreCase) >= 0 || x.SongAuthorName.IndexOf(searchField.text, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
		ReloadListItems();
	}

	private void ReloadListItems()
	{
		if (newList.RowCount != filteredSongs.Count)
		{
			newList.RowCount = filteredSongs.Count;
		}
		else
		{
			newList.Refresh();
		}
	}

	public void RemoveSong(BaseInfo mapInfo)
	{
		SongInfos.Remove(mapInfo);
	}

	public void AddSong(BaseInfo song)
	{
		SongInfos.Add(song);
		UpdateSongList();
	}
}
