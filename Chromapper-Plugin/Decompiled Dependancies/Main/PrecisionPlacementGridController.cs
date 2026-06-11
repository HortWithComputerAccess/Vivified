using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrecisionPlacementGridController : MonoBehaviour
{
	[SerializeField]
	private GameObject expandedGridParent;

	[SerializeField]
	private IntersectionCollider expandedGridBoxCollider;

	[SerializeField]
	private IntersectionCollider regularGridBoxCollider;

	private List<Material> allMaterialsInExpandedGrid = new List<Material>();

	private bool isEnabled = true;

	private Vector3 mousePosition;

	private static readonly int position = Shader.PropertyToID("_MousePosition");

	private void Start()
	{
		allMaterialsInExpandedGrid = (from x in expandedGridParent.GetComponentsInChildren<Renderer>()
			select x.material).ToList();
		TogglePrecisionPlacement(isVisible: false);
	}

	private void LateUpdate()
	{
		if (!isEnabled)
		{
			return;
		}
		foreach (Material item in allMaterialsInExpandedGrid)
		{
			item.SetVector(position, mousePosition);
		}
	}

	public void TogglePrecisionPlacement(bool isVisible)
	{
		if (isEnabled != isVisible)
		{
			isEnabled = isVisible;
			if (isVisible && Settings.Instance.PrecisionPlacementMode != PrecisionPlacementMode.Off)
			{
				expandedGridParent.SetActive(value: true);
				expandedGridBoxCollider.enabled = true;
				regularGridBoxCollider.enabled = false;
			}
			else
			{
				expandedGridParent.SetActive(value: false);
				expandedGridBoxCollider.enabled = false;
				regularGridBoxCollider.enabled = true;
			}
		}
	}

	public void UpdateMousePosition(Vector3 mousePosition)
	{
		this.mousePosition = mousePosition;
	}
}
