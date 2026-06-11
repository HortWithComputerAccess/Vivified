using UnityEngine;

public class SpectrogramSideSwapper : MonoBehaviour
{
	[SerializeField]
	private GridChild spectrogramGridChild;

	[SerializeField]
	private GridChild spectrogramChunksChild;

	public bool IsNoteSide { get; set; } = true;

	public void SwapSides()
	{
		IsNoteSide = !IsNoteSide;
		int num = (IsNoteSide ? (-1) : 3);
		float num2 = (IsNoteSide ? 3.5f : 2.5f);
		GridOrderController.DeregisterChild(spectrogramChunksChild);
		GridOrderController.DeregisterChild(spectrogramGridChild);
		GridChild gridChild = spectrogramChunksChild;
		int order = (spectrogramGridChild.Order = num);
		gridChild.Order = order;
		spectrogramGridChild.LocalOffset = new Vector3(num2, 0f, 0f);
		spectrogramChunksChild.LocalOffset = new Vector3(num2 - 2f, 0f, 0f);
		GridOrderController.RegisterChild(spectrogramChunksChild);
		GridOrderController.RegisterChild(spectrogramGridChild);
	}
}
