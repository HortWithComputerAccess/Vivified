public interface IMergeableAction
{
	ActionMergeType MergeType { get; set; }

	int MergeCount { get; set; }

	IMergeableAction TryMerge(IMergeableAction previous);

	bool CanMerge(IMergeableAction previous);

	IMergeableAction DoMerge(IMergeableAction previous);
}
