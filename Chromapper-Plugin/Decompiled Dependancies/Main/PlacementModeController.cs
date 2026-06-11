using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class PlacementModeController : MonoBehaviour
{
	public enum PlacementMode
	{
		[PickerChoice("Mapper", "place.note")]
		Note,
		[PickerChoice("Mapper", "place.bomb")]
		Bomb,
		[PickerChoice("Mapper", "place.wall")]
		Wall,
		[PickerChoice("Mapper", "place.chain")]
		Chain,
		[PickerChoice("Mapper", "place.arc")]
		Arc,
		[PickerChoice("Mapper", "place.delete")]
		Delete
	}

	[SerializeField]
	private NotePlacement notePlacement;

	[SerializeField]
	private BombPlacement bombPlacement;

	[SerializeField]
	private ObstaclePlacement obstaclePlacement;

	[SerializeField]
	private ArcPlacement arcPlacement;

	[SerializeField]
	private ChainPlacement chainPlacement;

	[SerializeField]
	private DeleteToolController deleteToolController;

	[SerializeField]
	private EnumPicker modePicker;

	private void Start()
	{
		modePicker.Initialize(typeof(PlacementMode));
		modePicker.OnClick += UpdateMode;
		UpdateMode(PlacementMode.Note);
	}

	public void SetMode(Enum placementMode)
	{
		modePicker.Select(placementMode);
		UpdateMode(placementMode);
	}

	private void UpdateMode(Enum placementMode)
	{
		PlacementMode placementMode2 = (PlacementMode)(object)placementMode;
		switch (placementMode2)
		{
		case PlacementMode.Arc:
			if (arcPlacement.SpawnArcsFromSelection() == 0)
			{
				string keybindFromAction2 = GetKeybindFromAction("SpawnArc");
				string message2 = string.Format(LocalizationSettings.StringDatabase.GetLocalizedString("Mapper", "place.arc.tutorial", null, FallbackBehavior.UseProjectSettings), keybindFromAction2);
				PersistentUI.Instance.ShowDialogBox(message2, null, PersistentUI.DialogBoxPresetType.Ok);
			}
			break;
		case PlacementMode.Chain:
			if (chainPlacement.SpawnChainFromSelection() == 0)
			{
				string keybindFromAction = GetKeybindFromAction("SpawnChain");
				string message = string.Format(LocalizationSettings.StringDatabase.GetLocalizedString("Mapper", "place.chain.tutorial", null, FallbackBehavior.UseProjectSettings), keybindFromAction);
				PersistentUI.Instance.ShowDialogBox(message, null, PersistentUI.DialogBoxPresetType.Ok);
			}
			break;
		}
		if (placementMode2 == PlacementMode.Arc || placementMode2 == PlacementMode.Chain)
		{
			placementMode2 = PlacementMode.Note;
			modePicker.Select(placementMode2);
		}
		notePlacement.IsActive = placementMode2 == PlacementMode.Note;
		bombPlacement.IsActive = placementMode2 == PlacementMode.Bomb;
		obstaclePlacement.IsActive = placementMode2 == PlacementMode.Wall;
		deleteToolController.UpdateDeletion(placementMode2 == PlacementMode.Delete);
	}

	private static string GetKeybindFromAction(string actionName)
	{
		IEnumerable<string> values = CMInputCallbackInstaller.InputInstance.asset.actionMaps.First((InputActionMap x) => x.actions.Any((InputAction y) => y.name == actionName)).SelectMany((InputAction x) => x.controls.Select((InputControl c) => c.displayName));
		return string.Join(" + ", values);
	}
}
