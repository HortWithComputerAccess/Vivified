using System.Collections.Generic;
using System.Globalization;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;

public class NJSEventPlacement : PlacementController<BaseNJSEvent, NJSEventContainer, NJSEventGridContainer>
{
	private List<string> beatSaberMapFormatEasings = new List<string>
	{
		"None", "Linear", "InQuad", "OutQuad", "InOutQuad", "InCirc", "OutCirc", "InOutCirc", "InBack", "OutBack",
		"InOutBack", "InElastic", "OutElastic", "InOutElastic", "InBounce", "OutBounce", "InOutBounce", "BeatSaberInOutBack", "BeatSaberInOutElastic", "BeatSaberInOutBounce"
	};

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, $"Placed a NJS Event at time {spawned.JsonTime}");
	}

	public override BaseNJSEvent GenerateOriginalData()
	{
		return new BaseNJSEvent();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit _, Vector3 __)
	{
		instantiatedContainer.transform.localPosition = new Vector3(0.5f, 0.5f, instantiatedContainer.transform.localPosition.z);
	}

	public override void TransferQueuedToDraggedObject(ref BaseNJSEvent dragged, BaseNJSEvent queued)
	{
		dragged.JsonTime = queued.JsonTime;
	}

	internal override void ApplyToMap()
	{
		CreateAndOpenNJSDialogue(isInitialPlacement: true);
	}

	internal override void Start()
	{
		base.gameObject.SetActive(BeatSaberSongContainer.Instance.Info.MajorVersion != 2);
		base.Start();
	}

	private void AttemptPlaceNJSChange(string njsInput, int easingDropdownValue, bool extend)
	{
		if (string.IsNullOrEmpty(njsInput) || string.IsNullOrWhiteSpace(njsInput))
		{
			return;
		}
		if (float.TryParse(njsInput, out var result))
		{
			if (result <= 0f)
			{
				CreateAndOpenNJSDialogue(isInitialPlacement: false);
				return;
			}
			float relativeNJS = result - BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
			queuedData.Easing = MapTMPDropdownValueToEasing(easingDropdownValue);
			queuedData.RelativeNJS = relativeNJS;
			queuedData.UsePrevious = (extend ? 1 : 0);
			base.ApplyToMap();
		}
		else
		{
			CreateAndOpenNJSDialogue(isInitialPlacement: false);
		}
	}

	private void CreateAndOpenNJSDialogue(bool isInitialPlacement)
	{
		DialogBox dialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Mapper", "njs.dialog");
		if (!isInitialPlacement)
		{
			dialogBox.AddComponent<TextComponent>().WithInitialValue("Mapper", "njs.dialogue.invalidnumber");
		}
		float noteJumpSpeed = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
		TextBoxComponent njsTextInput = dialogBox.AddComponent<TextBoxComponent>().WithLabel("Mapper", "njs").WithInitialValue(noteJumpSpeed.ToString(CultureInfo.InvariantCulture));
		DropdownComponent easingDropdown = dialogBox.AddComponent<DropdownComponent>().WithLabel("Mapper", "easing").WithOptions(beatSaberMapFormatEasings)
			.WithInitialValue(1);
		ToggleComponent extendToggle = dialogBox.AddComponent<ToggleComponent>().WithLabel("Mapper", "njs.dialogue.useprevious").WithInitialValue(initialValue: false);
		dialogBox.OnQuickSubmit(delegate
		{
			AttemptPlaceNJSChange(njsTextInput.Value, easingDropdown.Value, extendToggle.Value);
		});
		dialogBox.AddFooterButton(null, "PersistentUI", "cancel");
		dialogBox.AddFooterButton(delegate
		{
			AttemptPlaceNJSChange(njsTextInput.Value, easingDropdown.Value, extendToggle.Value);
		}, "PersistentUI", "ok");
		dialogBox.Open();
		easingDropdown.Value = 1;
	}

	private static int MapTMPDropdownValueToEasing(int dropdownEasing)
	{
		if (dropdownEasing < 17)
		{
			if (dropdownEasing >= 5)
			{
				return dropdownEasing + 14;
			}
			return dropdownEasing - 1;
		}
		return dropdownEasing + 83;
	}
}
