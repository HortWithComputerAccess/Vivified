using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrentSectionDisplay : MonoBehaviour
{
	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private BookmarkManager bookmarkManger;

	private readonly Stack<BookmarkContainer> upcomingBookmarks = new Stack<BookmarkContainer>();

	private bool isPlaying;

	private TextMeshProUGUI textMesh;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.TimeChanged = (Action)Delegate.Combine(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
		AudioTimeSyncController audioTimeSyncController2 = atsc;
		audioTimeSyncController2.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController2.PlayToggle, new Action<bool>(OnPlayToggle));
		BookmarkManager bookmarkManager = bookmarkManger;
		bookmarkManager.BookmarksUpdated = (Action)Delegate.Combine(bookmarkManager.BookmarksUpdated, new Action(OnTimeChanged));
	}

	private void OnDisable()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.TimeChanged = (Action)Delegate.Remove(audioTimeSyncController.TimeChanged, new Action(OnTimeChanged));
		AudioTimeSyncController audioTimeSyncController2 = atsc;
		audioTimeSyncController2.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController2.PlayToggle, new Action<bool>(OnPlayToggle));
		BookmarkManager bookmarkManager = bookmarkManger;
		bookmarkManager.BookmarksUpdated = (Action)Delegate.Remove(bookmarkManager.BookmarksUpdated, new Action(OnTimeChanged));
	}

	private void OnTimeChanged()
	{
		if (isPlaying)
		{
			if (upcomingBookmarks.Count != 0 && upcomingBookmarks.Peek().Data.JsonTime <= atsc.CurrentJsonTime)
			{
				textMesh.text = upcomingBookmarks.Pop().Data.Name;
			}
		}
		else
		{
			BookmarkContainer bookmarkContainer = bookmarkManger.bookmarkContainers.FindLast((BookmarkContainer x) => x.Data.JsonTime <= atsc.CurrentJsonTime);
			textMesh.text = ((bookmarkContainer != null) ? bookmarkContainer.Data.Name : "");
		}
	}

	private void OnPlayToggle(bool isPlaying)
	{
		this.isPlaying = isPlaying;
		upcomingBookmarks.Clear();
		foreach (BookmarkContainer item in from x in bookmarkManger.bookmarkContainers
			where x.Data.JsonTime > atsc.CurrentJsonTime
			orderby x.Data.JsonTime descending
			select x)
		{
			upcomingBookmarks.Push(item);
		}
	}
}
