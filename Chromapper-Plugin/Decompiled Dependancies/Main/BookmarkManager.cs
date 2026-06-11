using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class BookmarkManager : MonoBehaviour, CMInput.IBookmarksActions
{
	public const float CanvasWidthOffset = -20f;

	private static readonly System.Random rng = new System.Random();

	[SerializeField]
	private GameObject bookmarkContainerPrefab;

	[FormerlySerializedAs("atsc")]
	public AudioTimeSyncController Atsc;

	[FormerlySerializedAs("tipc")]
	public TimelineInputPlaybackController Tipc;

	[SerializeField]
	private RectTransform timelineCanvas;

	[SerializeField]
	private BookmarkRenderingController bookmarkRenderingController;

	public InputAction.CallbackContext ShiftContext;

	internal List<BookmarkContainer> bookmarkContainers = new List<BookmarkContainer>();

	public Action BookmarksUpdated;

	private float previousCanvasWidth;

	private DialogBox createBookmarkDialogBox;

	private TextBoxComponent bookmarkName;

	private NestedColorPickerComponent bookmarkColor;

	public event Action<BaseObject> BookmarkAdded;

	public event Action<BaseObject> BookmarkDeleted;

	private IEnumerator Start()
	{
		createBookmarkDialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Mapper", "bookmark.dialog").DontDestroyOnClose();
		bookmarkName = createBookmarkDialogBox.AddComponent<TextBoxComponent>().WithLabel("Mapper", "bookmark.dialog.name").WithInitialValue("Mapper", "bookmark.dialog.default");
		bookmarkColor = createBookmarkDialogBox.AddComponent<NestedColorPickerComponent>().WithLabel("Mapper", "bookmark.dialog.color").WithAlpha();
		createBookmarkDialogBox.OnQuickSubmit(delegate
		{
			CreateNewBookmark(bookmarkName.Value, bookmarkColor.Value);
		});
		createBookmarkDialogBox.AddFooterButton(null, "PersistentUI", "cancel");
		createBookmarkDialogBox.AddFooterButton(delegate
		{
			CreateNewBookmark(bookmarkName.Value, bookmarkColor.Value);
		}, "PersistentUI", "ok");
		yield return new WaitForSeconds(0.1f);
		ConvertBookmarkTimesFromOldDevVersions();
		bookmarkContainers = InstantiateBookmarkContainers();
		Settings.NotifyBySettingName("BookmarkTimelineWidth", UpdateBookmarkWidth);
		Settings.NotifyBySettingName("BookmarkTooltipTimeInfo", UpdateBookmarkTooltip);
		Settings.NotifyBySettingName("BookmarkTimelineBrightness", UpdateBookmarkBrightness);
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Combine(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(RefreshBookmarksFromLoadedDifficulty));
		BookmarksUpdated();
	}

	private void RefreshBookmarksFromLoadedDifficulty()
	{
		foreach (BookmarkContainer bookmarkContainer in bookmarkContainers)
		{
			UnityEngine.Object.Destroy(bookmarkContainer.gameObject);
		}
		bookmarkRenderingController.ClearCachedBookmarks();
		bookmarkContainers = InstantiateBookmarkContainers();
		BookmarksUpdated();
	}

	private List<BookmarkContainer> InstantiateBookmarkContainers()
	{
		return (from it in BeatSaberSongContainer.Instance.Map.Bookmarks.Select(delegate(BaseBookmark bookmark)
			{
				BookmarkContainer component = UnityEngine.Object.Instantiate(bookmarkContainerPrefab, base.transform).GetComponent<BookmarkContainer>();
				component.name = bookmark.Name;
				component.Init(this, bookmark);
				component.RefreshPosition(timelineCanvas.sizeDelta.x + -20f);
				return component;
			})
			orderby it.Data.JsonTime
			select it).ToList();
	}

	private void ConvertBookmarkTimesFromOldDevVersions()
	{
		BaseDifficulty map = BeatSaberSongContainer.Instance.Map;
		string bookmarksUseOfficialBpmEventsKey = map.BookmarksUseOfficialBpmEventsKey;
		bool flag = !map.CustomData.HasKey(bookmarksUseOfficialBpmEventsKey) || !map.CustomData[bookmarksUseOfficialBpmEventsKey].IsBoolean || !map.CustomData[bookmarksUseOfficialBpmEventsKey].AsBool;
		flag &= map.MajorVersion != 4;
		foreach (BaseBookmark bookmark in map.Bookmarks)
		{
			if (flag)
			{
				bookmark.JsonTime = map.SongBpmTimeToJsonTime(bookmark.JsonTime).Value;
			}
		}
	}

	public void RefreshBookmarkTimelinePositions()
	{
		foreach (BookmarkContainer bookmarkContainer in bookmarkContainers)
		{
			bookmarkContainer.RefreshPosition(timelineCanvas.sizeDelta.x + -20f);
		}
	}

	public void RefreshBookTooltips()
	{
		UpdateBookmarkTooltip(null);
	}

	private void UpdateBookmarkTooltip(object _)
	{
		foreach (BookmarkContainer bookmarkContainer in bookmarkContainers)
		{
			bookmarkContainer.UpdateUIText();
		}
	}

	private void UpdateBookmarkWidth(object _)
	{
		foreach (BookmarkContainer bookmarkContainer in bookmarkContainers)
		{
			bookmarkContainer.UpdateUIWidth();
		}
	}

	private void UpdateBookmarkBrightness(object _)
	{
		foreach (BookmarkContainer bookmarkContainer in bookmarkContainers)
		{
			bookmarkContainer.UpdateUIColor();
		}
	}

	private void LateUpdate()
	{
		if (previousCanvasWidth != timelineCanvas.sizeDelta.x)
		{
			previousCanvasWidth = timelineCanvas.sizeDelta.x;
			RefreshBookmarkTimelinePositions();
		}
	}

	public void OnCreateNewBookmark(InputAction.CallbackContext context)
	{
		if (!Atsc.IsPlaying && context.performed)
		{
			bookmarkColor.Value = Color.HSVToRGB((float)rng.NextDouble(), 0.75f, 1f);
			createBookmarkDialogBox.Open();
		}
	}

	public void OnNextBookmark(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnNextBookmark();
		}
	}

	public void OnPreviousBookmark(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnPreviousBookmark();
		}
	}

	internal void CreateNewBookmark(string name, Color? color = null)
	{
		if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
		{
			BaseBookmark baseBookmark = new BaseBookmark(Atsc.CurrentJsonTime, name);
			if (color.HasValue)
			{
				baseBookmark.Color = color.Value;
			}
			AddBookmark(baseBookmark);
		}
	}

	internal void AddBookmark(BaseBookmark bookmark, bool triggerEvent = true)
	{
		BookmarkContainer component = UnityEngine.Object.Instantiate(bookmarkContainerPrefab, base.transform).GetComponent<BookmarkContainer>();
		component.name = bookmark.Name;
		component.Init(this, bookmark);
		component.RefreshPosition(timelineCanvas.sizeDelta.x + -20f);
		bookmarkContainers = (from it in bookmarkContainers.Append(component)
			orderby it.Data.JsonTime
			select it).ToList();
		BeatSaberSongContainer.Instance.Map.Bookmarks = bookmarkContainers.Select((BookmarkContainer x) => x.Data).ToList();
		BookmarksUpdated();
		if (triggerEvent)
		{
			this.BookmarkAdded?.Invoke(bookmark);
		}
	}

	internal void DeleteBookmark(BookmarkContainer container, bool triggerEvent = true)
	{
		bookmarkContainers.Remove(container);
		BeatSaberSongContainer.Instance.Map.Bookmarks = bookmarkContainers.Select((BookmarkContainer x) => x.Data).ToList();
		BookmarksUpdated();
		if (triggerEvent)
		{
			this.BookmarkDeleted?.Invoke(container.Data);
		}
	}

	internal void DeleteBookmarkAtTime(float time, bool triggerEvent = true)
	{
		BookmarkContainer bookmarkContainer = bookmarkContainers.Find((BookmarkContainer it) => Mathf.Abs(it.Data.JsonTime - time) < 0.01f);
		if (bookmarkContainer != null)
		{
			DeleteBookmark(bookmarkContainer, triggerEvent);
		}
	}

	internal void OnNextBookmark()
	{
		BookmarkContainer bookmarkContainer = bookmarkContainers.Find((BookmarkContainer x) => x.Data.JsonTime > Atsc.CurrentJsonTime);
		if (bookmarkContainer != null)
		{
			MoveToBookmark(bookmarkContainer);
		}
	}

	internal void OnPreviousBookmark()
	{
		BookmarkContainer bookmarkContainer = bookmarkContainers.LastOrDefault((BookmarkContainer x) => x.Data.JsonTime < Atsc.CurrentJsonTime);
		if (bookmarkContainer != null)
		{
			MoveToBookmark(bookmarkContainer);
		}
	}

	private void MoveToBookmark(BookmarkContainer bookmark)
	{
		Tipc.PointerDown();
		Atsc.MoveToJsonTime(bookmark.Data.JsonTime);
		Tipc.PointerUp();
	}

	private void OnDestroy()
	{
		Settings.ClearSettingNotifications("BookmarkTimelineWidth");
		Settings.ClearSettingNotifications("BookmarkTooltipTimeInfo");
		Settings.ClearSettingNotifications("BookmarkTimelineBrightness");
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Remove(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(RefreshBookmarksFromLoadedDifficulty));
	}

	public void OnColorBookmarkModifier(InputAction.CallbackContext context)
	{
		ShiftContext = context;
	}
}
