using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine.InputSystem;

public class BeatmapNJSEventInputController : BeatmapInputController<NJSEventContainer>, CMInput.INJSEventObjectsActions
{
	public void OnTweakNJSValue(InputAction.CallbackContext context)
	{
		if (!context.performed)
		{
			return;
		}
		RaycastFirstObject(out var firstObject);
		if (firstObject != null && firstObject.NJSData.UsePrevious != 1)
		{
			BaseObject baseObject = BeatmapFactory.Clone(firstObject.ObjectData);
			float num = ((context.ReadValue<float>() > 0f) ? 0.5f : (-0.5f));
			firstObject.NJSData.RelativeNJS += num;
			if (firstObject.NJSData.RelativeNJS <= 0f - BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed)
			{
				firstObject.NJSData.RelativeNJS = 0.5f - BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed;
			}
			if (firstObject.NJSData.CompareTo(baseObject) != 0)
			{
				firstObject.UpdateNJSText();
				BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(firstObject.ObjectData, firstObject.ObjectData, baseObject, "Tweaked NJS", keepSelection: false, ActionMergeType.NJSValueTweak));
				BeatmapObjectContainerCollection.GetCollectionForType<NJSEventGridContainer>(ObjectType.NJSEvent).UpdateHJDLine();
			}
		}
	}
}
