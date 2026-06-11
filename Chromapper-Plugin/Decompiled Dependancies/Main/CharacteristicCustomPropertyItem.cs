using System.Collections;
using Beatmap.Info;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacteristicCustomPropertyItem : MonoBehaviour
{
	private CharacteristicCustomPropertyController controller;

	private string characteristic;

	[SerializeField]
	public Image Image;

	[SerializeField]
	public TMP_InputField CustomNameField;

	private string iconImageFileName = "";

	private string initialCustomName;

	private string initialImageFileName;

	public IEnumerator Setup(CharacteristicCustomPropertyController controller, string characteristic, Sprite originalIconSprite)
	{
		this.controller = controller;
		this.characteristic = characteristic;
		CustomNameField.placeholder.GetComponent<TMP_Text>().text = characteristic;
		Image.sprite = originalIconSprite;
		InfoDifficultySet infoDifficultySet = BeatSaberSongContainer.Instance.Info.DifficultySets.Find((InfoDifficultySet x) => x.Characteristic == this.characteristic);
		CharacteristicCustomPropertyItem characteristicCustomPropertyItem = this;
		TMP_InputField customNameField = CustomNameField;
		string obj = infoDifficultySet?.CustomCharacteristicLabel ?? "";
		string text = obj;
		customNameField.text = obj;
		characteristicCustomPropertyItem.initialCustomName = text;
		initialImageFileName = (iconImageFileName = infoDifficultySet?.CustomCharacteristicIconImageFileName ?? "");
		CustomNameField.onEndEdit.AddListener(delegate
		{
			controller.ReplaceCharacteristicTooltip(characteristic);
		});
		if (!string.IsNullOrEmpty(initialImageFileName))
		{
			yield return controller.ImageBrowser.LoadImageIntoSprite(initialImageFileName, Image, isOverride: true);
		}
	}

	public void OnDestroy()
	{
		CustomNameField.onEndEdit.RemoveAllListeners();
	}

	public bool IsDirty()
	{
		if (!(CustomNameField.text != initialCustomName))
		{
			return iconImageFileName != initialImageFileName;
		}
		return true;
	}

	public void CommitToInfo()
	{
		InfoDifficultySet infoDifficultySet = BeatSaberSongContainer.Instance.Info.DifficultySets.Find((InfoDifficultySet x) => x.Characteristic == characteristic);
		if (infoDifficultySet == null)
		{
			infoDifficultySet = new InfoDifficultySet
			{
				Characteristic = characteristic
			};
			BeatSaberSongContainer.Instance.Info.DifficultySets.Add(infoDifficultySet);
		}
		string text = (infoDifficultySet.CustomCharacteristicLabel = CustomNameField.text);
		initialCustomName = text;
		text = (infoDifficultySet.CustomCharacteristicIconImageFileName = iconImageFileName);
		initialImageFileName = text;
	}

	public void UndoChanges()
	{
		iconImageFileName = initialImageFileName;
		CustomNameField.text = initialCustomName;
	}

	public void Clear()
	{
		CustomNameField.text = null;
		iconImageFileName = "";
		Image.overrideSprite = null;
		controller.ReplaceCharacteristicIcon(characteristic);
		controller.ReplaceCharacteristicTooltip(characteristic);
	}

	private void LoadImageCallback(string imageFileName)
	{
		iconImageFileName = imageFileName;
		StartCoroutine(ReplaceCharacteristicIcons());
	}

	public IEnumerator ReplaceCharacteristicIcons()
	{
		if (!string.IsNullOrEmpty(iconImageFileName))
		{
			yield return controller.ImageBrowser.LoadImageIntoSprite(iconImageFileName, Image, isOverride: true);
		}
		else
		{
			Image.overrideSprite = null;
		}
		controller.ReplaceCharacteristicIcon(characteristic);
	}

	public void BrowseForImage()
	{
		controller.ImageBrowser.BrowseForImage(LoadImageCallback);
	}
}
