using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateSupporterList : MonoBehaviour
{
	[SerializeField]
	private PatreonSupporters supporters;

	[SerializeField]
	private TextMeshProUGUI prefab;

	private void Start()
	{
		foreach (string allSupporter in supporters.GetAllSupporters())
		{
			TextMeshProUGUI component = Object.Instantiate(prefab.gameObject, base.transform).GetComponent<TextMeshProUGUI>();
			component.text = allSupporter;
			if (supporters.HighTierPatrons.Contains(allSupporter))
			{
				component.color = Color.cyan;
			}
			if (allSupporter.Contains("Zyxi"))
			{
				SneakButton(component.gameObject, 1);
			}
		}
		prefab.gameObject.SetActive(value: false);
	}

	private void SneakButton(GameObject gameObject, int bongoId)
	{
		GameObject gameObject2 = new GameObject("button", typeof(RectTransform));
		gameObject2.transform.SetParent(gameObject.transform);
		RectTransform obj = gameObject2.transform as RectTransform;
		obj.localScale = Vector3.one;
		obj.anchoredPosition = Vector2.zero;
		obj.anchorMin = Vector2.zero;
		obj.anchorMax = Vector2.one;
		gameObject2.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
		Button button = gameObject2.AddComponent<Button>();
		Navigation navigation = button.navigation;
		navigation.mode = Navigation.Mode.None;
		button.navigation = navigation;
		button.onClick.AddListener(delegate
		{
			button.gameObject.GetComponentInParent<OptionsController>().ToggleBongo(bongoId);
		});
	}
}
