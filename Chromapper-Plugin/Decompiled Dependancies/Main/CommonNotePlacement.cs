using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using UnityEngine;

public static class CommonNotePlacement
{
	public static void UpdateAttachedSlidersDirection(BaseNote noteData, ICollection<BeatmapAction> actions)
	{
		float epsilon = BeatmapObjectContainerCollection.Epsilon;
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer in BeatmapObjectContainerCollection.GetCollectionForType<ArcGridContainer>(ObjectType.Arc).LoadedContainers)
		{
			BaseArc baseArc = loadedContainer.Key as BaseArc;
			bool num = Mathf.Abs(baseArc.JsonTime - noteData.JsonTime) < epsilon && baseArc.GetPosition() == noteData.GetPosition();
			bool flag = Mathf.Abs(baseArc.TailJsonTime - noteData.JsonTime) < epsilon && baseArc.GetTailPosition() == noteData.GetPosition();
			if (num)
			{
				BaseArc originalData = BeatmapFactory.Clone(baseArc);
				baseArc.CutDirection = noteData.CutDirection;
				(loadedContainer.Value as ArcContainer).NotifySplineChanged();
				actions.Add(new BeatmapObjectModifiedAction(baseArc, baseArc, originalData, "No comment.", keepSelection: true, ActionMergeType.NoteDirectionChange));
			}
			else if (flag)
			{
				BaseArc originalData2 = BeatmapFactory.Clone(baseArc);
				baseArc.TailCutDirection = noteData.CutDirection;
				(loadedContainer.Value as ArcContainer).NotifySplineChanged();
				actions.Add(new BeatmapObjectModifiedAction(baseArc, baseArc, originalData2, "No comment.", keepSelection: true, ActionMergeType.NoteDirectionChange));
			}
		}
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer2 in BeatmapObjectContainerCollection.GetCollectionForType<ChainGridContainer>(ObjectType.Chain).LoadedContainers)
		{
			BaseChain baseChain = loadedContainer2.Key as BaseChain;
			if (Mathf.Abs(baseChain.JsonTime - noteData.JsonTime) < epsilon && baseChain.GetPosition() == noteData.GetPosition())
			{
				BaseChain originalData3 = BeatmapFactory.Clone(baseChain);
				baseChain.CutDirection = noteData.CutDirection;
				(loadedContainer2.Value as ChainContainer).GenerateChain();
				actions.Add(new BeatmapObjectModifiedAction(baseChain, baseChain, originalData3, "No comment.", keepSelection: true, ActionMergeType.NoteDirectionChange));
			}
		}
	}
}
