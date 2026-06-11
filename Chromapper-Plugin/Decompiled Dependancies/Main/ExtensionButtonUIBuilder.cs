using UnityEngine;

public class ExtensionButtonUIBuilder : MonoBehaviour
{
	[SerializeField]
	private ExtensionButtonUI buttonPrefab;

	private void Awake()
	{
		ExtensionButtons.ForEachButton(BuildButton);
	}

	private void BuildButton(ExtensionButton button)
	{
		Object.Instantiate(buttonPrefab, base.transform).Init(button);
	}
}
