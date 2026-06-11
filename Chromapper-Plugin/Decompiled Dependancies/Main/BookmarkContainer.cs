using System;
using Beatmap.Base.Customs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class BookmarkContainer : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	private BookmarkManager manager;

	public BaseBookmark Data { get; private set; }

	public void Init(BookmarkManager manager, BaseBookmark data)
	{
		if (Data == null)
		{
			Data = data;
			this.manager = manager;
			UpdateUI();
		}
	}

	public void UpdateUI()
	{
		UpdateUIText();
		UpdateUIColor();
		UpdateUIWidth();
	}

	public void UpdateUIText()
	{
		string text = Data.Name.StripTMPTags();
		if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
		{
			text = "<i>(This Bookmark has no name)</i>";
		}
		if (Settings.Instance.BookmarkTooltipTimeInfo)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(manager.Atsc.GetSecondsFromBeat(Data.SongBpmTime));
			text += $" [{Math.Round(Data.JsonTime, 2)} | {timeSpan:mm':'ss}]";
		}
		GetComponent<Tooltip>().TooltipOverride = text;
	}

	public void UpdateUIColor()
	{
		Color color = Data.Color.Multiply(Settings.Instance.BookmarkTimelineBrightness).WithAlpha(Data.Color.a);
		GetComponent<Image>().color = color;
	}

	public void UpdateUIWidth()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		rectTransform.sizeDelta = new Vector2(Settings.Instance.BookmarkTimelineWidth, rectTransform.sizeDelta.y);
	}

	public void RefreshPosition(float width)
	{
		float num = width / manager.Atsc.GetBeatFromSeconds(BeatSaberSongContainer.Instance.LoadedSong.length);
		((RectTransform)base.transform).anchoredPosition = new Vector2(num * Data.SongBpmTime, 50f);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Middle:
			PersistentUI.Instance.ShowDialogBox("Mapper", "bookmark.delete", HandleDeleteBookmark, PersistentUI.DialogBoxPresetType.YesNo);
			break;
		case PointerEventData.InputButton.Right:
			DisplayBookmarkEditUI();
			break;
		case PointerEventData.InputButton.Left:
			break;
		}
	}

	private void DisplayBookmarkEditUI()
	{
		string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString("Mapper", "bookmark.update.dialog", null, FallbackBehavior.UseProjectSettings);
		DialogBox dialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle(localizedString);
		TextBoxComponent textBox = dialogBox.AddComponent<TextBoxComponent>().WithInitialValue(Data.Name).WithLabel("Mapper", "bookmark.dialog.name");
		NestedColorPickerComponent colorPicker = dialogBox.AddComponent<NestedColorPickerComponent>().WithInitialValue(Data.Color).WithLabel("Mapper", "bookmark.dialog.color");
		Action action = delegate
		{
			if (!string.IsNullOrWhiteSpace(textBox.Value))
			{
				Data.Name = textBox.Value;
			}
			_ = colorPicker.Value;
			Data.Color = colorPicker.Value;
			manager.BookmarksUpdated();
			UpdateUIText();
			UpdateUIColor();
		};
		dialogBox.AddFooterButton(delegate
		{
		}, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "cancel", null, FallbackBehavior.UseProjectSettings));
		dialogBox.AddFooterButton(action, LocalizationSettings.StringDatabase.GetLocalizedString("PersistentUI", "submit", null, FallbackBehavior.UseProjectSettings));
		dialogBox.OnQuickSubmit(action);
		dialogBox.Open();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			manager.Tipc.PointerDown();
			manager.Atsc.MoveToJsonTime(Data.JsonTime);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			manager.Tipc.PointerUp();
		}
	}

	internal void HandleDeleteBookmark(int res)
	{
		if (res == 0)
		{
			manager.DeleteBookmark(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
