using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyRow
{
	public Transform Obj { get; }

	public Image Background { get; }

	public string Name { get; }

	public Toggle Toggle { get; }

	public Button Button { get; }

	public TMP_InputField NameInput { get; }

	public Button Copy { get; }

	public Image CopyImage { get; }

	public Button Save { get; }

	public Button Revert { get; }

	public Button Paste { get; }

	public DifficultyRow(Transform obj)
	{
		Obj = obj;
		Name = obj.name;
		Background = obj.GetComponent<Image>();
		Toggle = obj.Find("Button/Toggle").GetComponent<Toggle>();
		Button = obj.Find("Button").GetComponent<Button>();
		NameInput = obj.Find("Button/Name").GetComponent<TMP_InputField>();
		Copy = obj.Find("Copy").GetComponent<Button>();
		CopyImage = obj.Find("Copy").GetComponent<Image>();
		Save = obj.Find("Warning").GetComponent<Button>();
		Revert = obj.Find("Revert").GetComponent<Button>();
		Paste = obj.Find("Paste").GetComponent<Button>();
	}

	public void SetInteractable(bool val)
	{
		TMP_InputField nameInput = NameInput;
		Button button = Button;
		bool flag = (Toggle.isOn = val);
		bool interactable = (button.interactable = flag);
		nameInput.interactable = interactable;
	}

	public void ShowDirtyObjects(DifficultySettings difficultySettings)
	{
		ShowDirtyObjects(difficultySettings.IsDirty(), !difficultySettings.IsDirty());
	}

	public void ShowDirtyObjects(bool show, bool copy)
	{
		Copy.gameObject.SetActive(copy);
		Save.gameObject.SetActive(show);
		Revert.gameObject.SetActive(show);
	}
}
