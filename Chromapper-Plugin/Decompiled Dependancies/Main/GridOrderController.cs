using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridOrderController : MonoBehaviour
{
	private static Dictionary<int, List<GridChild>> allChilds = new Dictionary<int, List<GridChild>>();

	private static bool dirty;

	[SerializeField]
	private GridRotationController gridRotationController;

	private void Start()
	{
		GridRotationController obj = gridRotationController;
		obj.ObjectRotationChangedEvent = (Action)Delegate.Combine(obj.ObjectRotationChangedEvent, new Action(MarkDirty));
	}

	private void LateUpdate()
	{
		if (!dirty)
		{
			return;
		}
		dirty = false;
		float num = 0f;
		if (allChilds.Any((KeyValuePair<int, List<GridChild>> x) => x.Key < 0))
		{
			if (allChilds.TryGetValue(0, out var value))
			{
				num -= (float)value.Max((GridChild x) => x.Size);
			}
			num -= 1f;
			foreach (KeyValuePair<int, List<GridChild>> item in allChilds.Where((KeyValuePair<int, List<GridChild>> x) => x.Key < 0))
			{
				num -= Mathf.Ceil(item.Value.Max((GridChild x) => x.Size));
				num -= 1f;
			}
		}
		foreach (KeyValuePair<int, List<GridChild>> allChild in allChilds)
		{
			if (allChild.Key == 0 || (allChild.Key > 0 && num < 0f))
			{
				num = 0f;
			}
			allChild.Value.RemoveAll((GridChild x) => x == null);
			foreach (GridChild item2 in allChild.Value)
			{
				item2.transform.eulerAngles = new Vector3(item2.transform.eulerAngles.x, base.transform.eulerAngles.y, item2.transform.eulerAngles.z);
				float num2 = num + item2.LocalOffset.x;
				Vector3 vector = base.transform.right.normalized * num2;
				Vector3 vector2 = base.transform.up.normalized * item2.LocalOffset.y;
				Vector3 vector3 = base.transform.forward.normalized * item2.LocalOffset.z;
				Vector3 vector4 = vector + vector2 + vector3;
				item2.transform.position = base.transform.position + vector4;
			}
			num += Mathf.Ceil(allChild.Value.Any() ? (allChild.Value.Max((GridChild x) => x.Size) + 1) : 0);
		}
	}

	private void OnDestroy()
	{
		GridRotationController obj = gridRotationController;
		obj.ObjectRotationChangedEvent = (Action)Delegate.Remove(obj.ObjectRotationChangedEvent, new Action(MarkDirty));
	}

	public static int GetSizeForOrder(int order)
	{
		if (allChilds.TryGetValue(order, out var value))
		{
			return Mathf.CeilToInt(value.Any() ? value.Max((GridChild x) => x.Size) : 0);
		}
		return 0;
	}

	public static void RegisterChild(GridChild child)
	{
		if (allChilds.TryGetValue(child.Order, out var value))
		{
			value.Add(child);
			return;
		}
		allChilds[child.Order] = new List<GridChild> { child };
		RefreshChildDictionary();
	}

	public static void DeregisterChild(GridChild child)
	{
		if (allChilds.TryGetValue(child.Order, out var value))
		{
			value.Remove(child);
			if (value.Count == 0)
			{
				allChilds.Remove(child.Order);
				RefreshChildDictionary();
			}
		}
	}

	public static void MarkDirty()
	{
		dirty = true;
	}

	public static void RefreshChildDictionary()
	{
		allChilds = allChilds.OrderBy((KeyValuePair<int, List<GridChild>> x) => x.Key).ToDictionary((KeyValuePair<int, List<GridChild>> x) => x.Key, (KeyValuePair<int, List<GridChild>> y) => y.Value);
		MarkDirty();
	}
}
