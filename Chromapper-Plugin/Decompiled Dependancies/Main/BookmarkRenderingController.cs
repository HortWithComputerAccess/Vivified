using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base.Customs;
using TMPro;
using UnityEngine;

public class BookmarkRenderingController : MonoBehaviour
{
	private class CachedBookmark
	{
		public readonly BaseBookmark MapBookmark;

		public readonly TextMeshProUGUI Text;

		public string Name;

		public Color Color;

		public CachedBookmark(BaseBookmark bookmark, TextMeshProUGUI text)
		{
			MapBookmark = bookmark;
			Text = text;
			Name = bookmark.Name;
			Color = bookmark.Color;
		}
	}

	[SerializeField]
	private BookmarkManager manager;

	[SerializeField]
	private Transform gridBookmarksParent;

	private List<CachedBookmark> renderedBookmarks = new List<CachedBookmark>();

	private void Start()
	{
		BookmarkManager bookmarkManager = manager;
		bookmarkManager.BookmarksUpdated = (Action)Delegate.Combine(bookmarkManager.BookmarksUpdated, new Action(UpdateRenderedBookmarks));
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Combine(EditorScaleController.EditorScaleChangedEvent, new Action<float>(OnEditorScaleChange));
		Settings.NotifyBySettingName("DisplayGridBookmarks", DisplayRenderedBookmarks);
		Settings.NotifyBySettingName("GridBookmarksHasLine", RefreshBookmarkGridLine);
	}

	public void ClearCachedBookmarks()
	{
		for (int num = renderedBookmarks.Count - 1; num >= 0; num--)
		{
			CachedBookmark cachedBookmark = renderedBookmarks[num];
			UnityEngine.Object.Destroy(cachedBookmark.Text.gameObject);
			renderedBookmarks.Remove(cachedBookmark);
		}
	}

	private void DisplayRenderedBookmarks(object _)
	{
		UpdateRenderedBookmarks();
	}

	private void UpdateRenderedBookmarks()
	{
		List<BaseBookmark> bookmarks = BeatSaberSongContainer.Instance.Map.Bookmarks;
		if (bookmarks.Count < renderedBookmarks.Count)
		{
			for (int num = renderedBookmarks.Count - 1; num >= 0; num--)
			{
				CachedBookmark cachedBookmark = renderedBookmarks[num];
				if (!bookmarks.Contains(cachedBookmark.MapBookmark))
				{
					UnityEngine.Object.Destroy(cachedBookmark.Text.gameObject);
					renderedBookmarks.Remove(cachedBookmark);
					break;
				}
			}
			return;
		}
		if (bookmarks.Count > renderedBookmarks.Count)
		{
			foreach (BaseBookmark bookmark in bookmarks)
			{
				if (renderedBookmarks.All((CachedBookmark x) => x.MapBookmark != bookmark))
				{
					TextMeshProUGUI text = CreateGridBookmark(bookmark);
					renderedBookmarks.Add(new CachedBookmark(bookmark, text));
				}
			}
			return;
		}
		foreach (CachedBookmark renderedBookmark in renderedBookmarks)
		{
			string text2 = renderedBookmark.MapBookmark.Name;
			Color color = renderedBookmark.MapBookmark.Color;
			if (renderedBookmark.Name != text2 || renderedBookmark.Color != color)
			{
				SetGridBookmarkNameColor(renderedBookmark.Text, color, text2);
				renderedBookmark.Name = text2;
				renderedBookmark.Color = color;
			}
		}
	}

	private void OnEditorScaleChange(float newScale)
	{
		foreach (CachedBookmark renderedBookmark in renderedBookmarks)
		{
			SetBookmarkPos(renderedBookmark.Text.rectTransform, renderedBookmark.MapBookmark.SongBpmTime);
		}
	}

	private void SetBookmarkPos(RectTransform rect, float songBpmTime)
	{
		rect.anchoredPosition3D = new Vector3(-4.5f, songBpmTime * EditorScaleController.EditorScale, 0f);
	}

	private TextMeshProUGUI CreateGridBookmark(BaseBookmark bookmark)
	{
		GameObject obj = new GameObject("GridBookmark", typeof(TextMeshProUGUI));
		RectTransform rectTransform = (RectTransform)obj.transform;
		rectTransform.SetParent(gridBookmarksParent);
		SetBookmarkPos(rectTransform, bookmark.SongBpmTime);
		rectTransform.sizeDelta = Vector2.one;
		rectTransform.localRotation = Quaternion.identity;
		TextMeshProUGUI component = obj.GetComponent<TextMeshProUGUI>();
		component.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		component.alignment = TextAlignmentOptions.Left;
		component.fontSize = 0.4f;
		component.enableWordWrapping = false;
		component.raycastTarget = false;
		component.fontMaterial.renderQueue = 3150;
		SetGridBookmarkNameColor(component, bookmark.Color, bookmark.Name);
		return component;
	}

	private void RefreshBookmarkGridLine(object _)
	{
		foreach (CachedBookmark renderedBookmark in renderedBookmarks)
		{
			SetGridBookmarkNameColor(renderedBookmark.Text, renderedBookmark.Color, renderedBookmark.Name);
		}
	}

	private void SetGridBookmarkNameColor(TextMeshProUGUI text, Color color, string name)
	{
		string hex = HEXFromColor(color, inclAlpha: false);
		SetText(0);
		text.ForceMeshUpdate();
		if (text.textBounds.size.x < 2f)
		{
			SetText((int)((2f - text.textBounds.size.x) / 0.0642f));
		}
		void SetText(int spaceNumber)
		{
			string text2 = ((spaceNumber <= 0) ? null : new string(' ', spaceNumber));
			text.text = (Settings.Instance.GridBookmarksHasLine ? ("<mark=" + hex + "50><voffset=0.06><s> <indent=3.92> </s></voffset> " + name + text2 + "<color=#00000000>.</color>") : ("<mark=" + hex + "50><voffset=0.06> <indent=3.92> </voffset> " + name + text2 + "<color=#00000000>.</color>"));
		}
	}

	private string HEXFromColor(Color color, bool inclAlpha = true)
	{
		if (!inclAlpha)
		{
			return "#" + ColorUtility.ToHtmlStringRGB(color);
		}
		return "#" + ColorUtility.ToHtmlStringRGBA(color);
	}

	public void RefreshVisibility(float currentSongBpm, float songBpmBeatsAhead, float songBpmBeatsBehind)
	{
		foreach (CachedBookmark renderedBookmark in renderedBookmarks)
		{
			float songBpmTime = renderedBookmark.MapBookmark.SongBpmTime;
			TextMeshProUGUI text = renderedBookmark.Text;
			bool active = songBpmTime >= currentSongBpm - songBpmBeatsBehind && songBpmTime <= currentSongBpm + songBpmBeatsAhead;
			text.gameObject.SetActive(active);
			SetBookmarkPos((RectTransform)text.transform, songBpmTime);
		}
	}

	private void OnDestroy()
	{
		BookmarkManager bookmarkManager = manager;
		bookmarkManager.BookmarksUpdated = (Action)Delegate.Remove(bookmarkManager.BookmarksUpdated, new Action(UpdateRenderedBookmarks));
		EditorScaleController.EditorScaleChangedEvent = (Action<float>)Delegate.Remove(EditorScaleController.EditorScaleChangedEvent, new Action<float>(OnEditorScaleChange));
		Settings.ClearSettingNotifications("DisplayGridBookmarks");
		Settings.ClearSettingNotifications("GridBookmarksHasLine");
	}
}
