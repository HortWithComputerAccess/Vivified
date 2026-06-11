using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine.InputSystem;

public class BeatmapBPMChangeInputController : BeatmapInputController<BpmEventContainer>, CMInput.IBPMChangeObjectsActions
{
	public void OnReplaceBPM(InputAction.CallbackContext context)
	{
		if (!context.performed || PersistentUI.Instance.InputBoxIsEnabled)
		{
			return;
		}
		RaycastFirstObject(out var containerToEdit);
		if (containerToEdit != null)
		{
			PersistentUI.Instance.ShowInputBox("Mapper", "bpm.dialog", delegate(string s)
			{
				ChangeBpm(containerToEdit, s);
			}, "", containerToEdit.BpmData.Bpm.ToString());
		}
	}

	public void OnTweakBPMValue(InputAction.CallbackContext context)
	{
		if (!context.performed)
		{
			return;
		}
		RaycastFirstObject(out var firstObject);
		if (!(firstObject != null))
		{
			return;
		}
		BaseObject baseObject = BeatmapFactory.Clone(firstObject.ObjectData);
		int num = ((context.ReadValue<float>() > 0f) ? 1 : (-1));
		firstObject.BpmData.Bpm += num;
		if (firstObject.BpmData.Bpm <= 0f)
		{
			firstObject.BpmData.Bpm = 1f;
		}
		firstObject.UpdateGridPosition();
		if (firstObject.BpmData.CompareTo(baseObject) != 0)
		{
			BPMChangeGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<BPMChangeGridContainer>(ObjectType.BpmChange);
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(firstObject.ObjectData, firstObject.ObjectData, baseObject, "Tweaked bpm", keepSelection: false, ActionMergeType.BPMValueTweak));
			BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(firstObject.BpmData.JsonTime);
			collectionForType.RefreshModifiedBeat();
			AudioTimeSyncController audioTimeSyncController = collectionForType.AudioTimeSyncController;
			if (firstObject.BpmData.JsonTime < audioTimeSyncController.CurrentJsonTime)
			{
				audioTimeSyncController.MoveToJsonTime(audioTimeSyncController.CurrentJsonTime);
			}
		}
	}

	internal static void ChangeBpm(BpmEventContainer containerToEdit, string obj)
	{
		if (string.IsNullOrEmpty(obj) || string.IsNullOrWhiteSpace(obj))
		{
			return;
		}
		if (float.TryParse(obj, out var result))
		{
			BaseObject originalData = BeatmapFactory.Clone(containerToEdit.ObjectData);
			containerToEdit.BpmData.Bpm = result;
			containerToEdit.UpdateGridPosition();
			BPMChangeGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<BPMChangeGridContainer>(ObjectType.BpmChange);
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(containerToEdit.ObjectData, containerToEdit.ObjectData, originalData, "Modified bpm"));
			BeatmapObjectContainerCollection.RefreshFutureObjectsPosition(containerToEdit.BpmData.JsonTime);
			collectionForType.RefreshModifiedBeat();
		}
		else
		{
			PersistentUI.Instance.ShowInputBox("Mapper", "bpm.dialog.invalid", delegate(string s)
			{
				ChangeBpm(containerToEdit, s);
			}, "", containerToEdit.BpmData.Bpm.ToString());
		}
	}
}
