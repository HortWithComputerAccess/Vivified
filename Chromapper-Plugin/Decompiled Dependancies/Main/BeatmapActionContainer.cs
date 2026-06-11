using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeatmapActionContainer : MonoBehaviour, CMInput.IActionsActions
{
	public class BeatmapActionParams
	{
		public NodeEditorController NodeEditor;

		public SelectionController Selection;

		public TracksManager TracksManager;

		public BeatmapActionParams(BeatmapActionContainer container)
		{
			Selection = container.selection;
			NodeEditor = container.nodeEditor;
			TracksManager = container.tracksManager;
		}
	}

	private static BeatmapActionContainer instance;

	[SerializeField]
	private GameObject moveableGridTransform;

	[SerializeField]
	private SelectionController selection;

	[SerializeField]
	private NodeEditorController nodeEditor;

	[SerializeField]
	private TracksManager tracksManager;

	private readonly List<BeatmapAction> beatmapActions = new List<BeatmapAction>();

	private int activeActionsAfterSave;

	public bool ContainsUnsavedActions => activeActionsAfterSave != beatmapActions.Count((BeatmapAction x) => x.Active);

	public static event Action<BeatmapAction> ActionCreatedEvent;

	public static event Action<BeatmapAction> ActionUndoEvent;

	public static event Action<BeatmapAction> ActionRedoEvent;

	private void Start()
	{
		instance = this;
	}

	public void OnUndoMethod1(InputAction.CallbackContext context)
	{
		OnUndo(context);
	}

	public void OnUndoMethod2(InputAction.CallbackContext context)
	{
		OnUndo(context);
	}

	public void OnRedoMethod1(InputAction.CallbackContext context)
	{
		OnRedo(context);
	}

	public void OnRedoMethod2(InputAction.CallbackContext context)
	{
		OnRedo(context);
	}

	public static void AddAction(BeatmapAction action, bool perform = false)
	{
		if (!action.Networked)
		{
			instance.beatmapActions.RemoveAll((BeatmapAction x) => !x.Networked && !x.Active);
			BeatmapAction beatmapAction = instance.beatmapActions.LastOrDefault((BeatmapAction x) => !x.Networked);
			if (action is IMergeableAction mergeableAction && beatmapAction is IMergeableAction previous)
			{
				BeatmapAction beatmapAction2 = (BeatmapAction)mergeableAction.TryMerge(previous);
				if (beatmapAction2 != null)
				{
					instance.beatmapActions.Remove(beatmapAction);
					action = beatmapAction2;
				}
			}
		}
		instance.beatmapActions.Add(action);
		if (perform)
		{
			instance.DoRedo(action);
		}
		Debug.Log("Action of type " + action.GetType().Name + " added. (" + action.Comment + ")");
		if (!action.Networked)
		{
			BeatmapActionContainer.ActionCreatedEvent?.Invoke(action);
		}
	}

	public static void RemoveAllActionsOfType<T>() where T : BeatmapAction
	{
		instance.beatmapActions.RemoveAll((BeatmapAction x) => x is T);
	}

	public static void Undo(Guid actionGuid)
	{
		BeatmapAction beatmapAction = instance.beatmapActions.Find((BeatmapAction x) => x.Guid == actionGuid);
		if (beatmapAction != null)
		{
			Debug.Log("Undid a " + beatmapAction.GetType().Name + ". (" + beatmapAction.Comment + ")");
			instance.DoUndo(beatmapAction);
		}
	}

	public static void Redo(Guid actionGuid)
	{
		BeatmapAction beatmapAction = instance.beatmapActions.Find((BeatmapAction x) => x.Guid == actionGuid);
		if (beatmapAction != null)
		{
			Debug.Log("Redid a " + beatmapAction.GetType().Name + ". (" + beatmapAction.Comment + ")");
			instance.DoRedo(beatmapAction);
		}
	}

	public void Undo()
	{
		BeatmapAction beatmapAction = beatmapActions.FindLast((BeatmapAction x) => !x.Networked && x.Active);
		if (beatmapAction != null)
		{
			Debug.Log("Undid a " + beatmapAction.GetType().Name + ". (" + beatmapAction.Comment + ")");
			DoUndo(beatmapAction);
			BeatmapActionContainer.ActionUndoEvent?.Invoke(beatmapAction);
		}
	}

	public void Redo()
	{
		BeatmapAction beatmapAction = beatmapActions.Find((BeatmapAction x) => !x.Networked && !x.Active);
		if (beatmapAction != null)
		{
			Debug.Log("Redid a " + beatmapAction.GetType().Name + ". (" + beatmapAction.Comment + ")");
			DoRedo(beatmapAction);
			BeatmapActionContainer.ActionRedoEvent?.Invoke(beatmapAction);
		}
	}

	private void DoUndo(BeatmapAction action)
	{
		BeatmapActionParams param = new BeatmapActionParams(this);
		action.Undo(param);
		action.Active = false;
		nodeEditor.ObjectWasSelected();
	}

	private void DoRedo(BeatmapAction action)
	{
		BeatmapActionParams param = new BeatmapActionParams(this);
		action.Redo(param);
		action.Active = true;
		nodeEditor.ObjectWasSelected();
	}

	public void OnUndo(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Undo();
		}
	}

	public void OnRedo(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Redo();
		}
	}

	public void OnUserTrace(InputAction.CallbackContext context)
	{
		if (!context.performed || !Intersections.Raycast(Camera.main.ScreenPointToRay(KeybindsController.MousePosition), 9, out var hit))
		{
			return;
		}
		ObjectContainer componentInParent = hit.GameObject.GetComponentInParent<ObjectContainer>();
		if (!(componentInParent != null))
		{
			return;
		}
		BaseObject objectData = componentInParent.ObjectData;
		int num = beatmapActions.FindLastIndex((BeatmapAction it) => it.Active && it.DoesInvolveObject(objectData) != null);
		if (num == -1)
		{
			return;
		}
		BeatmapAction beatmapAction = beatmapActions[num];
		BeatmapAction beatmapAction2 = beatmapAction;
		for (int num2 = num; num2 >= 0; num2--)
		{
			BaseObject baseObject = beatmapActions[num2].DoesInvolveObject(objectData);
			if (baseObject != null)
			{
				objectData = baseObject;
				beatmapAction2 = beatmapActions[num2];
			}
		}
		if (beatmapAction2 != null && beatmapAction2.Identity != null)
		{
			DialogBox dialogBox = PersistentUI.Instance.CreateNewDialogBox().WithTitle("MultiMapping", "multi.trace");
			dialogBox.AddComponent<TextComponent>().WithInitialValue("MultiMapping", "multi.trace.first", beatmapAction2.Identity.Name);
			if (beatmapAction != beatmapAction2 && beatmapAction.Identity != null)
			{
				dialogBox.AddComponent<TextComponent>().WithInitialValue("MultiMapping", "multi.trace.last", beatmapAction.Identity.Name);
			}
			dialogBox.AddFooterButton(null, "PersistentUI", "ok");
			dialogBox.Open();
		}
	}

	public void UpdateActiveActionsAfterSave()
	{
		activeActionsAfterSave = beatmapActions.Count((BeatmapAction x) => x.Active);
	}

	public void ClearBeatmapActions()
	{
		activeActionsAfterSave = 0;
		beatmapActions.Clear();
	}
}
