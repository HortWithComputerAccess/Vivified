using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using LiteNetLib.Utils;

public class ActionCollectionAction : BeatmapAction, IMergeableAction
{
	private IEnumerable<BeatmapAction> actions;

	private bool clearSelection;

	private bool forceRefreshesPool;

	public ActionMergeType MergeType { get; set; }

	public int MergeCount { get; set; }

	public ActionCollectionAction()
	{
	}

	public ActionCollectionAction(IEnumerable<BeatmapAction> beatmapActions, bool forceRefreshPool = false, bool clearsSelection = true, string comment = "No comment.", ActionMergeType mergeType = ActionMergeType.None)
		: base(beatmapActions.SelectMany((BeatmapAction x) => x.Data), comment)
	{
		foreach (BeatmapAction beatmapAction in beatmapActions)
		{
			beatmapAction.inCollection = true;
			affectsSeveralObjects = true;
		}
		actions = beatmapActions;
		clearSelection = clearsSelection;
		forceRefreshesPool = forceRefreshPool;
		MergeType = mergeType;
	}

	public IMergeableAction TryMerge(IMergeableAction previous)
	{
		if (!CanMerge(previous))
		{
			return null;
		}
		return DoMerge(previous);
	}

	public bool CanMerge(IMergeableAction previous)
	{
		if (!(previous is ActionCollectionAction actionCollectionAction))
		{
			return false;
		}
		if (MergeType == ActionMergeType.None || actionCollectionAction.MergeType != MergeType)
		{
			return false;
		}
		foreach (BeatmapAction action in actions)
		{
			if (!(action is IMergeableAction))
			{
				return false;
			}
		}
		foreach (BeatmapAction action2 in actionCollectionAction.actions)
		{
			if (!(action2 is IMergeableAction))
			{
				return false;
			}
		}
		if (actions.Count() != actionCollectionAction.actions.Count())
		{
			return false;
		}
		return true;
	}

	public IMergeableAction DoMerge(IMergeableAction previous)
	{
		if (!(previous is ActionCollectionAction actionCollectionAction))
		{
			return null;
		}
		Dictionary<IMergeableAction, IMergeableAction> dictionary = new Dictionary<IMergeableAction, IMergeableAction>();
		foreach (BeatmapAction action in actions)
		{
			IMergeableAction modifiedAction = (IMergeableAction)action;
			IMergeableAction mergeableAction = (IMergeableAction)actionCollectionAction.actions.FirstOrDefault((BeatmapAction x) => modifiedAction.CanMerge((IMergeableAction)x));
			if (mergeableAction == null)
			{
				return null;
			}
			dictionary[modifiedAction] = mergeableAction;
		}
		List<BeatmapAction> list = new List<BeatmapAction>();
		foreach (var (mergeableAction4, previous2) in dictionary)
		{
			list.Add((BeatmapAction)mergeableAction4.DoMerge(previous2));
		}
		ActionCollectionAction actionCollectionAction2 = new ActionCollectionAction(list, forceRefreshesPool, clearSelection, Comment, MergeType);
		actionCollectionAction2.MergeCount = actionCollectionAction.MergeCount + 1;
		actionCollectionAction2.Comment += $" ({actionCollectionAction2.MergeCount}x merged)";
		return actionCollectionAction2;
	}

	public override BaseObject DoesInvolveObject(BaseObject obj)
	{
		foreach (BeatmapAction action in actions)
		{
			BaseObject baseObject = action.DoesInvolveObject(obj);
			if (baseObject != null)
			{
				return baseObject;
			}
		}
		return null;
	}

	public override void Redo(BeatmapActionContainer.BeatmapActionParams param)
	{
		if (clearSelection && !Networked)
		{
			SelectionController.DeselectAll();
		}
		foreach (BeatmapAction action in actions)
		{
			action.Redo(param);
		}
		if (forceRefreshesPool)
		{
			RefreshPools(Data);
		}
		RefreshEventAppearance();
	}

	public override void Undo(BeatmapActionContainer.BeatmapActionParams param)
	{
		if (clearSelection && !Networked)
		{
			SelectionController.DeselectAll();
		}
		foreach (BeatmapAction action in actions)
		{
			action.Undo(param);
		}
		if (forceRefreshesPool)
		{
			RefreshPools(Data);
		}
		RefreshEventAppearance();
	}

	protected override void RefreshEventAppearance()
	{
		List<BaseEvent> list = actions.SelectMany((BeatmapAction x) => x.Data).OfType<BaseEvent>().ToList();
		if (!list.Any())
		{
			return;
		}
		EventGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<EventGridContainer>(ObjectType.Event);
		collectionForType.MarkEventsToBeRelinked(list);
		collectionForType.LinkAllLightEvents();
		foreach (BaseEvent item in list)
		{
			if (item.Prev != null && collectionForType.LoadedContainers.TryGetValue(item.Prev, out var value))
			{
				(value as EventContainer).RefreshAppearance();
			}
			if (collectionForType.LoadedContainers.TryGetValue(item, out var value2))
			{
				(value2 as EventContainer).RefreshAppearance();
			}
		}
	}

	public override void Serialize(NetDataWriter writer)
	{
		writer.Put(clearSelection);
		writer.Put(forceRefreshesPool);
		writer.Put(actions.Count());
		foreach (BeatmapAction action in actions)
		{
			writer.PutBeatmapAction(action);
		}
	}

	public override void Deserialize(NetDataReader reader)
	{
		clearSelection = reader.GetBool();
		forceRefreshesPool = reader.GetBool();
		int num = reader.GetInt();
		List<BeatmapAction> list = new List<BeatmapAction>(num);
		for (int i = 0; i < num; i++)
		{
			BeatmapAction beatmapAction = reader.GetBeatmapAction(Identity);
			beatmapAction.inCollection = true;
			list.Add(beatmapAction);
		}
		actions = list;
		Data = actions.Where((BeatmapAction x) => x != null && x.Data != null).SelectMany((BeatmapAction x) => x.Data);
	}
}
