using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Beatmap.Info;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SongListItem : RecyclingListViewItem, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	private static readonly Dictionary<string, WeakReference<Sprite>> cache = new Dictionary<string, WeakReference<Sprite>>();

	private static readonly Dictionary<string, float> durationCache = new Dictionary<string, float>();

	private static bool hasAppliedThisFrame;

	private static string durationCachePath;

	private static JSONObject songCoreCache;

	private static bool saveRunning;

	private static readonly byte[] oggBytes = new byte[6] { 79, 103, 103, 83, 0, 4 };

	private static readonly byte[] vorbisBytes = new byte[6] { 118, 111, 114, 98, 105, 115 };

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI artist;

	[SerializeField]
	private TextMeshProUGUI folder;

	[SerializeField]
	private TextMeshProUGUI duration;

	[SerializeField]
	private TextMeshProUGUI bpm;

	[SerializeField]
	private Image favouritePreviewImage;

	[SerializeField]
	private Image cover;

	[SerializeField]
	private Sprite defaultCover;

	[SerializeField]
	private GameObject rightPanel;

	[SerializeField]
	private Toggle favouriteToggle;

	private Image bg;

	private bool ignoreToggle;

	private string previousSearch = "";

	private BaseInfo mapInfo;

	private SongList songList;

	private void Start()
	{
		rightPanel.SetActive(value: false);
		bg = GetComponent<Image>();
		songList = UnityEngine.Object.FindObjectOfType<SongList>();
		InitCache();
	}

	private static void InitCache()
	{
		if (songCoreCache != null)
		{
			return;
		}
		durationCachePath = Path.Combine(Settings.Instance.BeatSaberInstallation, "UserData", "SongCore", "SongDurationCache.dat");
		if (!File.Exists(durationCachePath))
		{
			songCoreCache = new JSONObject();
			return;
		}
		try
		{
			using StreamReader streamReader = new StreamReader(durationCachePath);
			songCoreCache = JSON.Parse(streamReader.ReadToEnd()).AsObject;
			JSONNode.Enumerator enumerator = songCoreCache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current = enumerator.Current;
				durationCache[Path.GetFullPath(current.Key)] = current.Value["duration"].AsFloat;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error trying to read from file {durationCachePath}\n{arg}");
		}
	}

	private void Update()
	{
		hasAppliedThisFrame = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (BeatSaberSongContainer.Instance != null && mapInfo != null)
		{
			BeatSaberSongContainer.Instance.SelectSongForEditing(mapInfo);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		rightPanel.SetActive(value: true);
		bg.color = new Color(0.35f, 0.35f, 0.36f, 1f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		rightPanel.SetActive(value: false);
		bg.color = new Color(0.31f, 0.31f, 0.31f, 1f);
	}

	private string HighlightSubstring(string s, string search)
	{
		string text = s.StripTMPTags();
		int num = text.IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
		if (num < 0 || search.Length <= 0)
		{
			return text;
		}
		return text.Substring(0, num) + "<color=#ff0000ff>" + text.Substring(num, search.Length) + "</color>" + text.Substring(num + search.Length);
	}

	public void AssignSong(BaseInfo mapInfo, string searchFieldText)
	{
		if (this.mapInfo != mapInfo || !(previousSearch == searchFieldText))
		{
			StopCoroutine("LoadImage");
			StopCoroutine("LoadDuration");
			previousSearch = searchFieldText;
			this.mapInfo = mapInfo;
			string text = HighlightSubstring(mapInfo.SongName, searchFieldText);
			string text2 = HighlightSubstring(mapInfo.SongAuthorName, searchFieldText);
			title.text = text + " <size=50%><i>" + mapInfo.SongSubName.StripTMPTags() + "</i></size>";
			artist.text = text2;
			folder.text = mapInfo.Directory;
			duration.text = "-:--";
			bpm.text = $"{mapInfo.BeatsPerMinute:N0}";
			ignoreToggle = true;
			favouriteToggle.isOn = this.mapInfo.IsFavourite;
			favouritePreviewImage.gameObject.SetActive(this.mapInfo.IsFavourite);
			ignoreToggle = false;
			StartCoroutine("LoadImage");
			if (mapInfo.SongDurationMetadata > 0f)
			{
				SetDuration(mapInfo.SongDurationMetadata);
			}
			else
			{
				StartCoroutine("LoadDuration");
			}
		}
	}

	private IEnumerator LoadImage()
	{
		string fullPath = Path.Combine(mapInfo.Directory, mapInfo.CoverImageFilename);
		if (cache.TryGetValue(fullPath, out var value) && value.TryGetTarget(out var target))
		{
			cover.sprite = target;
			yield break;
		}
		cover.sprite = defaultCover;
		if (!File.Exists(fullPath))
		{
			yield break;
		}
		RuntimePlatform platform = Application.platform;
		string text = ((platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor) ? Uri.EscapeDataString(fullPath) : Uri.EscapeUriString(fullPath));
		UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + text);
		yield return www.SendWebRequest();
		Texture2D newTex = ((DownloadHandlerTexture)www.downloadHandler).texture;
		if (newTex == null)
		{
			Debug.LogWarning("Cover image file exists but the texture failed to load.");
			yield break;
		}
		newTex.wrapMode = TextureWrapMode.Clamp;
		while (hasAppliedThisFrame)
		{
			yield return new WaitForEndOfFrame();
		}
		hasAppliedThisFrame = true;
		Sprite sprite = Sprite.Create(newTex, new Rect(0f, 0f, newTex.width, newTex.height), new Vector2(0f, 0f), 100f);
		cover.sprite = sprite;
		cache[fullPath] = new WeakReference<Sprite>(sprite);
	}

	private void SetDuration(string path, float length)
	{
		SetDuration(this, path, length);
	}

	public static void SetDuration(MonoBehaviour crTarget, string path, float length)
	{
		InitCache();
		JSONNode valueOrDefault = songCoreCache.GetValueOrDefault(path, new JSONObject { ["id"] = "CMCachedDuration" });
		valueOrDefault["duration"] = length;
		songCoreCache.Add(path, valueOrDefault);
		crTarget.StartCoroutine(SaveCachedDurations());
		durationCache[path] = length;
		if (crTarget is SongListItem songListItem)
		{
			songListItem.SetDuration(length);
		}
	}

	private static IEnumerator SaveCachedDurations()
	{
		if (!saveRunning)
		{
			saveRunning = true;
			if (!File.Exists(durationCachePath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(durationCachePath) ?? throw new InvalidOperationException("Directory was null?"));
			}
			yield return new WaitForSeconds(3f);
			using (StreamWriter streamWriter = new StreamWriter(durationCachePath, append: false))
			{
				streamWriter.Write(songCoreCache.ToString());
			}
			saveRunning = false;
		}
	}

	private void SetDuration(float length)
	{
		int num = Mathf.RoundToInt(length);
		int num2 = num / 60;
		int num3 = num % 60;
		duration.text = $"{num2}:{num3:D2}";
	}

	private IEnumerator LoadDuration()
	{
		string cacheKey = Path.GetFullPath(mapInfo.Directory);
		string fullPath = Path.Combine(mapInfo.Directory, mapInfo.SongFilename);
		if (!File.Exists(fullPath))
		{
			yield break;
		}
		yield return null;
		if (durationCache.TryGetValue(cacheKey, out var value) && value >= 0f)
		{
			SetDuration(value);
			yield break;
		}
		float lengthFromOgg = GetLengthFromOgg(fullPath);
		if (lengthFromOgg >= 0f)
		{
			SetDuration(cacheKey, lengthFromOgg);
			yield break;
		}
		yield return BeatSaberSongExtensions.LoadAudio(mapInfo, delegate(AudioClip clip)
		{
			SetDuration(cacheKey, clip.length);
		});
	}

	private static bool FindBytes(Stream fs, BinaryReader br, byte[] bytes, int searchLength)
	{
		for (int i = 0; i < searchLength; i++)
		{
			if (br.ReadByte() == bytes[0])
			{
				byte[] array = br.ReadBytes(bytes.Length - 1);
				if (array[0] == bytes[1] && array[1] == bytes[2] && array[2] == bytes[3] && array[3] == bytes[4] && (array[4] & bytes[5]) == bytes[5])
				{
					return true;
				}
				int num = Array.IndexOf(array, bytes[0]);
				if (num != -1)
				{
					fs.Position += num - (bytes.Length - 1);
					i += num;
				}
				else
				{
					i += bytes.Length - 1;
				}
			}
		}
		return false;
	}

	public static float GetLengthFromOgg(string oggFile)
	{
		using FileStream fileStream = File.OpenRead(oggFile);
		using BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.ASCII);
		fileStream.Position = 24L;
		if (!FindBytes(fileStream, binaryReader, vorbisBytes, 256))
		{
			Debug.Log("Could not find rate for " + oggFile);
			return -1f;
		}
		fileStream.Position += 5L;
		int num = binaryReader.ReadInt32();
		long num2 = -1L;
		for (int i = 0; i < 10; i++)
		{
			int num3 = (i + 1) * 6144 * -1;
			int num4 = Math.Max((int)(-num3 - fileStream.Length), 0);
			if (num4 >= 6144)
			{
				break;
			}
			fileStream.Seek(num3 + num4, SeekOrigin.End);
			if (FindBytes(fileStream, binaryReader, oggBytes, 6144 - num4))
			{
				num2 = binaryReader.ReadInt64();
				break;
			}
		}
		if (num2 == -1)
		{
			Debug.Log("Could not find lastSample for " + oggFile);
			return -1f;
		}
		return (float)num2 / (float)num;
	}

	public void OnFavourite(bool isFavourite)
	{
		if (!ignoreToggle)
		{
			songList.RemoveSong(mapInfo);
			mapInfo.IsFavourite = isFavourite;
			favouritePreviewImage.gameObject.SetActive(isFavourite);
			songList.AddSong(mapInfo);
		}
	}
}
