using UnityEngine;
using UnityEngine.InputSystem;

public class BeatmapVersionSwitchInputController : MonoBehaviour, CMInput.ISwitchVersionActions
{
	[SerializeField]
	private PauseManager pauseManager;

	public void OnSwitchingVersion(InputAction.CallbackContext context)
	{
		if (!context.performed && !context.canceled)
		{
			PromptSwitchVersion();
		}
	}

	private void OnChangeVersion(int version)
	{
		switch (version)
		{
		case 2:
		{
			int mapVersion = Settings.Instance.MapVersion;
			if (mapVersion == 3 || mapVersion == 4)
			{
				BeatSaberSongContainer.Instance.Map.ConvertCustomDataVersion(Settings.Instance.MapVersion, 2);
			}
			Settings.Instance.MapVersion = 2;
			break;
		}
		case 3:
			if (Settings.Instance.MapVersion == 2)
			{
				BeatSaberSongContainer.Instance.Map.ConvertCustomDataVersion(2, 3);
			}
			Settings.Instance.MapVersion = 3;
			break;
		case 4:
			if (Settings.Instance.MapVersion == 2)
			{
				BeatSaberSongContainer.Instance.Map.ConvertCustomDataVersion(2, 4);
			}
			Settings.Instance.MapVersion = 4;
			break;
		}
	}

	public void PromptSwitchVersion()
	{
		DialogBox dialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Mapper", "change.beatmap.version");
		dialogBox.AddComponent<TextComponent>().WithInitialValue("Mapper", "change.beatmap.version.warning");
		dialogBox.AddFooterButton(null, "PersistentUI", "cancel");
		dialogBox.AddFooterButton(delegate
		{
			OnChangeVersion(2);
		}, "v2");
		dialogBox.AddFooterButton(delegate
		{
			OnChangeVersion(3);
		}, "v3");
		if (BeatSaberSongContainer.Instance.Info.MajorVersion == 4)
		{
			dialogBox.AddFooterButton(delegate
			{
				OnChangeVersion(4);
			}, "v4");
		}
		dialogBox.Open();
	}
}
