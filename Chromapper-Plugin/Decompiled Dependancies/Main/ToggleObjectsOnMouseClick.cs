using UnityEngine;

public class ToggleObjectsOnMouseClick : MonoBehaviour
{
	[SerializeField]
	private GameObject[] shitToToggle;

	private bool objectsEnabled;

	private void Start()
	{
		GameObject[] array = shitToToggle;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	private void OnMouseDown()
	{
		objectsEnabled = !objectsEnabled;
		GameObject[] array = shitToToggle;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(objectsEnabled);
		}
	}
}
