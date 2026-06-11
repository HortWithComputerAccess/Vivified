using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class RecyclingListView : MonoBehaviour
{
	public delegate void ItemDelegate(RecyclingListViewItem item, int rowIndex);

	protected const int RowsAboveBelow = 1;

	[Tooltip("Prefab for all the child view objects in the list")]
	public RecyclingListViewItem ChildPrefab;

	[Tooltip("The amount of vertical padding to add between items")]
	public float RowPadding = 15f;

	[Tooltip("Minimum height to pre-allocate list items for. Use to prevent allocations on resizing.")]
	public float PreAllocHeight;

	[FormerlySerializedAs("topPadding")]
	public int TopPadding;

	[FormerlySerializedAs("bottomPadding")]
	public int BottomPadding;

	protected int ChildBufferStart;

	protected RecyclingListViewItem[] ChildItems;

	protected bool IgnoreScrollChange;

	public ItemDelegate ItemCallback;

	protected float PreviousBuildHeight;

	private int rowCount;

	protected ScrollRect ScrollRect;

	protected int SourceDataRowStart;

	public float VerticalNormalizedPosition
	{
		get
		{
			return ScrollRect.verticalNormalizedPosition;
		}
		set
		{
			ScrollRect.verticalNormalizedPosition = value;
		}
	}

	public int RowCount
	{
		get
		{
			return rowCount;
		}
		set
		{
			if (rowCount != value)
			{
				rowCount = value;
				IgnoreScrollChange = true;
				UpdateContentHeight();
				IgnoreScrollChange = false;
				ReorganiseContent(clearContents: true);
			}
		}
	}

	protected virtual void Awake()
	{
		ScrollRect = GetComponent<ScrollRect>();
	}

	protected virtual void OnEnable()
	{
		ScrollRect.onValueChanged.AddListener(OnScrollChanged);
		IgnoreScrollChange = false;
	}

	protected virtual void OnDisable()
	{
		ScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
	}

	public virtual void Refresh()
	{
		ReorganiseContent(clearContents: true);
	}

	public virtual void Refresh(int rowStart, int count)
	{
		int num = SourceDataRowStart + ChildItems.Length;
		for (int i = 0; i < count; i++)
		{
			int num2 = rowStart + i;
			if (num2 >= SourceDataRowStart && num2 < num)
			{
				int num3 = WrapChildIndex(ChildBufferStart + num2 - SourceDataRowStart);
				if (ChildItems[num3] != null)
				{
					UpdateChild(ChildItems[num3], num2);
				}
			}
		}
	}

	public virtual void Refresh(RecyclingListViewItem item)
	{
		for (int i = 0; i < ChildItems.Length; i++)
		{
			int num = WrapChildIndex(ChildBufferStart + i);
			if (ChildItems[num] != null && ChildItems[num] == item)
			{
				UpdateChild(ChildItems[i], SourceDataRowStart + i);
				break;
			}
		}
	}

	public virtual void Clear()
	{
		RowCount = 0;
	}

	public virtual void ScrollToRow(int row)
	{
		ScrollRect.verticalNormalizedPosition = GetRowScrollPosition(row);
	}

	public float GetRowScrollPosition(int row)
	{
		float num = ((float)row + 0.5f) * RowHeight();
		float num2 = ViewportHeight();
		float num3 = num2 * 0.5f;
		float num4 = Mathf.Max(0f, num - num3);
		float num5 = num4 + num2;
		float y = ScrollRect.content.sizeDelta.y;
		if (num5 > y)
		{
			num4 = Mathf.Max(0f, num4 - (num5 - y));
		}
		return Mathf.InverseLerp(y - num2, 0f, num4);
	}

	public RecyclingListViewItem GetRowItem(int row)
	{
		if (ChildItems != null && row >= SourceDataRowStart && row < SourceDataRowStart + ChildItems.Length && row < rowCount)
		{
			return ChildItems[WrapChildIndex(ChildBufferStart + row - SourceDataRowStart)];
		}
		return null;
	}

	protected virtual bool CheckChildItems()
	{
		float num = Mathf.Max(ViewportHeight(), PreAllocHeight);
		bool flag = ChildItems == null || num > PreviousBuildHeight;
		if (flag)
		{
			int num2 = Mathf.RoundToInt(0.5f + num / RowHeight());
			num2 += 2;
			if (ChildItems == null)
			{
				ChildItems = new RecyclingListViewItem[num2];
			}
			else if (num2 > ChildItems.Length)
			{
				Array.Resize(ref ChildItems, num2);
			}
			for (int i = 0; i < ChildItems.Length; i++)
			{
				if (ChildItems[i] == null)
				{
					ChildItems[i] = UnityEngine.Object.Instantiate(ChildPrefab);
				}
				ChildItems[i].RectTransform.SetParent(ScrollRect.content, worldPositionStays: false);
				ChildItems[i].gameObject.SetActive(value: false);
			}
			PreviousBuildHeight = num;
		}
		return flag;
	}

	protected virtual void OnScrollChanged(Vector2 normalisedPos)
	{
		if (!IgnoreScrollChange)
		{
			ReorganiseContent(clearContents: false);
		}
	}

	protected virtual void ReorganiseContent(bool clearContents)
	{
		if (clearContents)
		{
			ScrollRect.StopMovement();
			ScrollRect.verticalNormalizedPosition = 1f;
		}
		bool num = CheckChildItems() || clearContents;
		int num2 = (int)(ScrollRect.content.localPosition.y / RowHeight()) - 1;
		int num3 = num2 - SourceDataRowStart;
		if (num || Mathf.Abs(num3) >= ChildItems.Length)
		{
			SourceDataRowStart = num2;
			ChildBufferStart = 0;
			int num4 = num2;
			RecyclingListViewItem[] childItems = ChildItems;
			foreach (RecyclingListViewItem child in childItems)
			{
				UpdateChild(child, num4++);
			}
		}
		else
		{
			if (num3 == 0)
			{
				return;
			}
			int childBufferStart = (ChildBufferStart + num3) % ChildItems.Length;
			if (num3 < 0)
			{
				for (int j = 1; j <= -num3; j++)
				{
					int num5 = WrapChildIndex(ChildBufferStart - j);
					int rowIdx = SourceDataRowStart - j;
					UpdateChild(ChildItems[num5], rowIdx);
				}
			}
			else
			{
				int num6 = ChildBufferStart + ChildItems.Length - 1;
				int num7 = SourceDataRowStart + ChildItems.Length - 1;
				for (int k = 1; k <= num3; k++)
				{
					int num8 = WrapChildIndex(num6 + k);
					int rowIdx2 = num7 + k;
					UpdateChild(ChildItems[num8], rowIdx2);
				}
			}
			SourceDataRowStart = num2;
			ChildBufferStart = childBufferStart;
		}
	}

	private int WrapChildIndex(int idx)
	{
		while (idx < 0)
		{
			idx += ChildItems.Length;
		}
		return idx % ChildItems.Length;
	}

	private float RowHeight()
	{
		return RowPadding + ChildPrefab.RectTransform.rect.height;
	}

	private float ViewportHeight()
	{
		return ScrollRect.viewport.rect.height;
	}

	protected virtual void UpdateChild(RecyclingListViewItem child, int rowIdx)
	{
		if (rowIdx < 0 || rowIdx >= rowCount)
		{
			child.gameObject.SetActive(value: false);
			return;
		}
		if (ItemCallback == null)
		{
			Debug.Log("RecyclingListView is missing an ItemCallback, cannot function", this);
			return;
		}
		Rect rect = ChildPrefab.RectTransform.rect;
		Vector2 pivot = ChildPrefab.RectTransform.pivot;
		float num = RowHeight() * (float)rowIdx + (1f - pivot.y) * rect.height + (float)TopPadding;
		float x = 0f + pivot.x * rect.width;
		child.RectTransform.anchoredPosition = new Vector2(x, 0f - num);
		child.NotifyCurrentAssignment(this, rowIdx);
		child.gameObject.SetActive(value: true);
		ItemCallback(child, rowIdx);
	}

	protected virtual void UpdateContentHeight()
	{
		float y = ChildPrefab.RectTransform.rect.height * (float)rowCount + (float)(rowCount - 1) * RowPadding + (float)TopPadding + (float)BottomPadding;
		Vector2 sizeDelta = ScrollRect.content.sizeDelta;
		ScrollRect.content.sizeDelta = new Vector2(sizeDelta.x, y);
	}

	protected virtual void DisableAllChildren()
	{
		if (ChildItems != null)
		{
			for (int i = 0; i < ChildItems.Length; i++)
			{
				ChildItems[i].gameObject.SetActive(value: false);
			}
		}
	}
}
