using Beatmap.Info;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacteristicSelect : MonoBehaviour
{
	[SerializeField]
	private Color selectedColor;

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private DifficultySelect difficultySelect;

	private Transform selected;

	private static BaseInfo MapInfo
	{
		get
		{
			if (!(BeatSaberSongContainer.Instance != null))
			{
				return null;
			}
			return BeatSaberSongContainer.Instance.Info;
		}
	}

	public void Start()
	{
		foreach (Transform child in base.transform)
		{
			Recalculate(child);
			child.GetComponent<Button>().onClick.AddListener(delegate
			{
				OnClick(child);
			});
			if (selected == null || (Settings.Instance.LastLoadedMap.Equals(MapInfo.Directory) && Settings.Instance.LastLoadedChar.Equals(child.name)))
			{
				OnClick(child, firstLoad: true);
			}
		}
	}

	private void OnClick(Transform obj, bool firstLoad = false)
	{
		if (selected != null)
		{
			selected.GetComponent<Image>().color = normalColor;
		}
		selected = obj;
		selected.GetComponent<Image>().color = selectedColor;
		difficultySelect.SetCharacteristic(obj.name, firstLoad);
	}

	private void Recalculate(Transform transform)
	{
		difficultySelect.Characteristics.TryGetValue(transform.name, out var value);
		int num = value?.Count ?? 0;
		transform.Find("Difficulty Count").GetComponent<TMP_Text>().text = num.ToString();
	}

	public void Recalculate()
	{
		Recalculate(selected);
	}
}
