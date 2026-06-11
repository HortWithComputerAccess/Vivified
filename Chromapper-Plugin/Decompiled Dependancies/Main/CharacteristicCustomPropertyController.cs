using System.Collections;
using System.Collections.Generic;
using System.Linq;
using __Scripts.UI.SongEditMenu;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CharacteristicCustomPropertyController : MonoBehaviour
{
	[SerializeField]
	private GameObject EditDialog;

	[SerializeField]
	private Button EditButton;

	[SerializeField]
	private Image StandardIcon;

	[SerializeField]
	private Image OneSaberIcon;

	[SerializeField]
	private Image NoArrowsIcon;

	[SerializeField]
	private Image ThreeSixtyDegreesIcon;

	[SerializeField]
	private Image NinetyDegreesIcon;

	[SerializeField]
	private Image LegacyIcon;

	[SerializeField]
	private Image LightshowIcon;

	[SerializeField]
	private Image LawlessIcon;

	[SerializeField]
	private GameObject listContainer;

	[SerializeField]
	private GameObject CustomPropertyItemPrefab;

	public ImageBrowser ImageBrowser;

	private Dictionary<string, CharacteristicCustomPropertyItem> characteristicToCustomPropertyItem = new Dictionary<string, CharacteristicCustomPropertyItem>();

	private Dictionary<string, Image> characteristicToIcon = new Dictionary<string, Image>();

	private IEnumerator Start()
	{
		if (BeatSaberSongContainer.Instance.Info == null)
		{
			yield break;
		}
		characteristicToIcon = new Dictionary<string, Image>
		{
			{ "Standard", StandardIcon },
			{ "OneSaber", OneSaberIcon },
			{ "NoArrows", NoArrowsIcon },
			{ "360Degree", ThreeSixtyDegreesIcon },
			{ "90Degree", NinetyDegreesIcon },
			{ "Legacy", LegacyIcon },
			{ "Lightshow", LightshowIcon },
			{ "Lawless", LawlessIcon }
		};
		foreach (KeyValuePair<string, Image> item2 in characteristicToIcon)
		{
			item2.Deconstruct(out var key, out var value);
			string characteristic = key;
			Image image = value;
			CharacteristicCustomPropertyItem item = Object.Instantiate(CustomPropertyItemPrefab, listContainer.transform).GetComponent<CharacteristicCustomPropertyItem>();
			yield return item.Setup(this, characteristic, image.sprite);
			characteristicToCustomPropertyItem[characteristic] = item;
			ReplaceCharacteristicIcon(characteristic);
			ReplaceCharacteristicTooltip(characteristic);
		}
	}

	public void ReplaceCharacteristicIcon(string characteristic)
	{
		if (characteristicToIcon.ContainsKey(characteristic))
		{
			CharacteristicCustomPropertyItem characteristicCustomPropertyItem = characteristicToCustomPropertyItem[characteristic];
			characteristicToIcon[characteristic].overrideSprite = characteristicCustomPropertyItem.Image.overrideSprite;
		}
	}

	public void ReplaceCharacteristicTooltip(string characteristic)
	{
		if (characteristicToIcon.ContainsKey(characteristic))
		{
			CharacteristicCustomPropertyItem characteristicCustomPropertyItem = characteristicToCustomPropertyItem[characteristic];
			characteristicToIcon[characteristic].GetComponent<Tooltip>().TooltipOverride = characteristicCustomPropertyItem.CustomNameField.text;
		}
	}

	public void OpenEditDialog()
	{
		EditDialog.SetActive(!EditDialog.activeSelf);
	}

	public void CommitToInfo()
	{
		foreach (KeyValuePair<string, CharacteristicCustomPropertyItem> item in characteristicToCustomPropertyItem)
		{
			item.Deconstruct(out var _, out var value);
			value.CommitToInfo();
		}
	}

	public void UndoChanges()
	{
		foreach (var (characteristic, characteristicCustomPropertyItem2) in characteristicToCustomPropertyItem)
		{
			characteristicCustomPropertyItem2.UndoChanges();
			ReplaceCharacteristicTooltip(characteristic);
			StartCoroutine(characteristicCustomPropertyItem2.ReplaceCharacteristicIcons());
		}
	}

	public bool IsDirty()
	{
		return characteristicToCustomPropertyItem.Any((KeyValuePair<string, CharacteristicCustomPropertyItem> x) => x.Value.IsDirty());
	}
}
