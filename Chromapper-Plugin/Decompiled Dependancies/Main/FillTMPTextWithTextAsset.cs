using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FillTMPTextWithTextAsset : MonoBehaviour
{
	[SerializeField]
	private TextAsset textAsset;

	private void Start()
	{
		GetComponent<TextMeshProUGUI>().text = textAsset.text;
	}
}
