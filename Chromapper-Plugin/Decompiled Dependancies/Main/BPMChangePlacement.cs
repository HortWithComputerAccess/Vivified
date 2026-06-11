using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;

public class BPMChangePlacement : PlacementController<BaseBpmEvent, BpmEventContainer, BPMChangeGridContainer>
{
	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, $"Placed a BPM Event at time {spawned.JsonTime}");
	}

	public override BaseBpmEvent GenerateOriginalData()
	{
		return new BaseBpmEvent(0f, 100f);
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit _, Vector3 __)
	{
		instantiatedContainer.transform.localPosition = new Vector3(0.5f, 0.5f, instantiatedContainer.transform.localPosition.z);
	}

	public override void TransferQueuedToDraggedObject(ref BaseBpmEvent dragged, BaseBpmEvent queued)
	{
		dragged.JsonTime = queued.JsonTime;
		objectContainerCollection.RefreshModifiedBeat();
	}

	public override void ClickAndDragFinished()
	{
		objectContainerCollection.RefreshModifiedBeat();
	}

	internal override void ApplyToMap()
	{
		CreateAndOpenBpmDialogue(isInitialPlacement: true);
	}

	private void AttemptPlaceBpmChange(string obj, bool willResetGrid)
	{
		if (string.IsNullOrEmpty(obj) || string.IsNullOrWhiteSpace(obj))
		{
			return;
		}
		if (float.TryParse(obj, out var result))
		{
			if (result <= 0f)
			{
				CreateAndOpenBpmDialogue(isInitialPlacement: false);
			}
			else if (willResetGrid && Mathf.Abs(queuedData.JsonTime - Mathf.Round(queuedData.JsonTime)) > BeatmapObjectContainerCollection.Epsilon)
			{
				float num = BeatSaberSongContainer.Instance.Map.BpmAtSongBpmTime(base.SongBpmTime).Value;
				float num2 = Mathf.Floor(queuedData.JsonTime);
				float jsonTime = Mathf.Ceil(queuedData.JsonTime);
				float bpm = num / (queuedData.JsonTime - num2);
				BaseBpmEvent baseBpmEvent = new BaseBpmEvent(num2, bpm);
				objectContainerCollection.SpawnObject(baseBpmEvent, out var conflicting);
				BaseBpmEvent baseBpmEvent2 = new BaseBpmEvent(jsonTime, result);
				objectContainerCollection.SpawnObject(baseBpmEvent2, out var conflicting2);
				BeatmapActionContainer.AddAction(new ActionCollectionAction(new List<BeatmapAction>
				{
					GenerateAction(baseBpmEvent, conflicting),
					GenerateAction(baseBpmEvent2, conflicting2)
				}));
			}
			else
			{
				queuedData.Bpm = result;
				base.ApplyToMap();
			}
		}
		else
		{
			CreateAndOpenBpmDialogue(isInitialPlacement: false);
		}
	}

	private void CreateAndOpenBpmDialogue(bool isInitialPlacement)
	{
		DialogBox dialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Mapper", "bpm.dialog");
		if (!isInitialPlacement)
		{
			dialogBox.AddComponent<TextComponent>().WithInitialValue("Mapper", "bpm.dialogue.invalidnumber");
		}
		float num = BeatSaberSongContainer.Instance.Map.BpmAtSongBpmTime(base.SongBpmTime).Value;
		TextBoxComponent bpmTextInput = dialogBox.AddComponent<TextBoxComponent>().WithLabel("Mapper", "bpm.dialogue.beatsperminute").WithInitialValue(num.ToString());
		ToggleComponent resetBeatToggle = dialogBox.AddComponent<ToggleComponent>().WithLabel("Mapper", "bpm.dialogue.resetbeat").WithInitialValue(initialValue: false);
		dialogBox.OnQuickSubmit(delegate
		{
			AttemptPlaceBpmChange(bpmTextInput.Value, resetBeatToggle.Value);
		});
		dialogBox.AddFooterButton(null, "PersistentUI", "cancel");
		dialogBox.AddFooterButton(delegate
		{
			AttemptPlaceBpmChange(bpmTextInput.Value, resetBeatToggle.Value);
		}, "PersistentUI", "ok");
		dialogBox.Open();
	}
}
