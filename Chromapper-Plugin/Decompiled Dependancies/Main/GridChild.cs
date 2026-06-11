using UnityEngine;

public class GridChild : MonoBehaviour
{
	public bool RegisterChildOnStart = true;

	[SerializeField]
	private int order;

	[SerializeField]
	private Vector3 localOffset = Vector3.zero;

	[SerializeField]
	private int size;

	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			order = value;
			GridOrderController.MarkDirty();
		}
	}

	public Vector3 LocalOffset
	{
		get
		{
			return localOffset;
		}
		set
		{
			localOffset = value;
			GridOrderController.MarkDirty();
		}
	}

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
			GridOrderController.MarkDirty();
		}
	}

	private void OnEnable()
	{
		if (RegisterChildOnStart)
		{
			GridOrderController.RegisterChild(this);
		}
	}

	private void OnDisable()
	{
		GridOrderController.DeregisterChild(this);
	}
}
