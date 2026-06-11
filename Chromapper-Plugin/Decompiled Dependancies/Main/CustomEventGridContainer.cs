using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Beatmap.Animations;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomEventGridContainer : BeatmapObjectContainerCollection<BaseCustomEvent>, CMInput.ICustomEventsContainerActions
{
	[SerializeField]
	private GameObject customEventPrefab;

	[SerializeField]
	private TextMeshProUGUI customEventLabelPrefab;

	[SerializeField]
	private Transform customEventLabelTransform;

	[SerializeField]
	private Transform[] customEventScalingOffsets;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private CameraController playerCamera;

	private List<string> customEventTypes = new List<string>();

	public Dictionary<string, List<BaseCustomEvent>> EventsByTrack;

	public override ObjectType ContainerType => ObjectType.CustomEvent;

	public ReadOnlyCollection<string> CustomEventTypes => customEventTypes.AsReadOnly();

	private void Start()
	{
		RefreshTrack();
		if (!Settings.Instance.AdvancedShit)
		{
			Debug.LogWarning("Disabling some objects since an Advanced setting is not enabled...");
			Transform[] array = customEventScalingOffsets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void LoadAll()
	{
		EventsByTrack = new Dictionary<string, List<BaseCustomEvent>>();
		Span<BaseCustomEvent> span = MapObjects.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			BaseCustomEvent ev = span[i];
			AddCustomEvent(ev);
		}
	}

	public void OnAssignObjectstoTrack(InputAction.CallbackContext context)
	{
		if (Settings.Instance.AdvancedShit && context.performed && !PersistentUI.Instance.InputBoxIsEnabled)
		{
			PersistentUI.Instance.ShowInputBox("Assign the selected objects to a track ID.\n\nIf you dont know what you're doing, turn back now.", HandleTrackAssign);
		}
	}

	public void OnSetTrackFilter(InputAction.CallbackContext context)
	{
		if (Settings.Instance.AdvancedShit && context.performed && !PersistentUI.Instance.InputBoxIsEnabled)
		{
			SetTrackFilter();
		}
	}

	public void OnCreateNewEventType(InputAction.CallbackContext context)
	{
		if (Settings.Instance.AdvancedShit && context.performed && !PersistentUI.Instance.InputBoxIsEnabled)
		{
			CreateNewType();
		}
	}

	protected override void OnObjectSpawned(BaseObject obj, bool inCollection = false)
	{
		BaseCustomEvent baseCustomEvent = obj as BaseCustomEvent;
		if (!customEventTypes.Contains(baseCustomEvent.Type))
		{
			customEventTypes.Add(baseCustomEvent.Type);
			RefreshTrack();
		}
		AddCustomEvent(baseCustomEvent);
	}

	protected override void OnObjectDelete(BaseObject obj, bool inCollection = false)
	{
		BaseCustomEvent baseCustomEvent = obj as BaseCustomEvent;
		JSONNode customTrack = baseCustomEvent.CustomTrack;
		List<string> list = ((customTrack is JSONString jSONString) ? new List<string> { jSONString } : ((!(customTrack is JSONArray jSONArray)) ? new List<string>() : new List<string>(jSONArray.Children.Select((Func<JSONNode, string>)((JSONNode c) => c)))));
		foreach (string item in list)
		{
			EventsByTrack[item].Remove(baseCustomEvent);
			if (EventsByTrack[item].Count == 0)
			{
				EventsByTrack.Remove(item);
			}
			if (baseCustomEvent.Type == "AnimateTrack")
			{
				tracksManager.GetAnimationTrack(item).RemoveEvent(baseCustomEvent);
			}
		}
	}

	private void AddCustomEvent(BaseCustomEvent ev)
	{
		JSONNode customTrack = ev.CustomTrack;
		List<string> list = ((customTrack is JSONString jSONString) ? new List<string> { jSONString } : ((!(customTrack is JSONArray jSONArray)) ? new List<string>() : new List<string>(jSONArray.Children.Select((Func<JSONNode, string>)((JSONNode c) => c)))));
		foreach (string item in list)
		{
			if (!EventsByTrack.ContainsKey(item))
			{
				EventsByTrack[item] = new List<BaseCustomEvent>();
			}
			EventsByTrack[item].Add(ev);
			if (ev.Type == "AnimateTrack")
			{
				tracksManager.GetAnimationTrack(item).AddEvent(ev);
			}
		}
		string type = ev.Type;
		if (!(type == "AssignTrackParent"))
		{
			if (type == "AssignPlayerToTrack" && !(ev.CustomTrack == null))
			{
				playerCamera.gameObject.SetActive(value: true);
				TrackAnimator animationTrack = tracksManager.GetAnimationTrack(ev.CustomTrack);
				playerCamera.AddPlayerTrack(ev.JsonTime, animationTrack);
			}
		}
		else
		{
			if (ev.DataParentTrack == null)
			{
				return;
			}
			TrackAnimator animationTrack2 = tracksManager.GetAnimationTrack(ev.DataParentTrack);
			customTrack = ev.DataChildrenTracks;
			JSONArray jSONArray2 = ((customTrack is JSONArray jSONArray3) ? jSONArray3 : ((!(customTrack is JSONString arg)) ? new JSONArray() : JSONNode.Parse($"[{arg}]").AsArray));
			JSONNode.Enumerator enumerator2 = jSONArray2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, JSONNode> current2 = enumerator2.Current;
				TrackAnimator animationTrack3 = tracksManager.GetAnimationTrack(current2.Value);
				animationTrack3.Track.transform.SetParent(animationTrack2.Track.ObjectParentTransform, ev.DataWorldPositionStays == true);
				if (animationTrack3.Animator == null)
				{
					animationTrack3.Animator = animationTrack3.gameObject.AddComponent<ObjectAnimator>();
					animationTrack3.Animator.Atsc = AudioTimeSyncController;
					animationTrack3.Animator.AttachToTrack(animationTrack3.Track, current2.Value);
				}
				if (!animationTrack2.Children.Contains(animationTrack3.Animator))
				{
					animationTrack2.Children.Add(animationTrack3.Animator);
					animationTrack3.Parents.Add(animationTrack2);
					animationTrack3.OnChildrenChanged();
				}
			}
		}
	}

	private void OnUIPreviewModeSwitch()
	{
		RefreshPool(true);
	}

	public override void RefreshPool(bool force)
	{
		if (UIMode.AnimationMode)
		{
			while (ObjectsWithContainers.Count > 0)
			{
				RecycleContainer(ObjectsWithContainers[0]);
			}
		}
		else
		{
			base.RefreshPool(force);
		}
	}

	private void RefreshTrack()
	{
		Transform[] array = customEventScalingOffsets;
		foreach (Transform transform in array)
		{
			Vector3 localScale = transform.localScale;
			if (customEventTypes.Count == 0)
			{
				transform.gameObject.SetActive(value: false);
				continue;
			}
			transform.gameObject.SetActive(value: true);
			transform.localScale = new Vector3((float)customEventTypes.Count / 10f + 0.01f, localScale.y, localScale.z);
		}
		for (int j = 0; j < customEventLabelTransform.childCount; j++)
		{
			UnityEngine.Object.Destroy(customEventLabelTransform.GetChild(j).gameObject);
		}
		foreach (string customEventType in customEventTypes)
		{
			TextMeshProUGUI component = UnityEngine.Object.Instantiate(customEventLabelPrefab.gameObject, customEventLabelTransform).GetComponent<TextMeshProUGUI>();
			component.rectTransform.localPosition = new Vector3(customEventTypes.IndexOf(customEventType), 0.25f, 0f);
			component.text = customEventType;
		}
		foreach (ObjectContainer value in LoadedContainers.Values)
		{
			value.UpdateGridPosition();
		}
	}

	internal override void SubscribeToCallbacks()
	{
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(SetInitialTracks));
		UIMode.PreviewModeSwitched = (Action)Delegate.Combine(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
	}

	private void SetInitialTracks()
	{
		Span<BaseCustomEvent> span = MapObjects.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			BaseCustomEvent baseCustomEvent = span[i];
			if (!customEventTypes.Contains(baseCustomEvent.Type))
			{
				customEventTypes.Add(baseCustomEvent.Type);
				RefreshTrack();
			}
		}
	}

	internal override void UnsubscribeToCallbacks()
	{
		LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(SetInitialTracks));
		UIMode.PreviewModeSwitched = (Action)Delegate.Remove(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
	}

	private void CreateNewType()
	{
		if (!PersistentUI.Instance.InputBoxIsEnabled)
		{
			PersistentUI.Instance.ShowInputBox("A new custom event type, I see?\n\nCustom event types are for the advanced of advanced users. Node Editor and JSON knowledge are required for these babies.\n\nIf you dont know what these do, or don't have the documentation for them, turn back now.\n\nBut if you do, what would you like to name this new event type?", HandleNewTypeCreation, "NewCustomEventType");
		}
	}

	private void HandleNewTypeCreation(string res)
	{
		if (!string.IsNullOrEmpty(res) && !string.IsNullOrWhiteSpace(res))
		{
			customEventTypes.Add(res);
			customEventTypes = customEventTypes.OrderBy((string x) => x).ToList();
			RefreshTrack();
		}
	}

	private void HandleTrackAssign(string res)
	{
		if (res == null)
		{
			return;
		}
		List<BaseObject> list = new List<BaseObject>();
		string text = ((res == "") ? null : res);
		foreach (BaseObject selectedObject in SelectionController.SelectedObjects)
		{
			BaseObject baseObject = BeatmapFactory.Clone(selectedObject);
			list.Add(baseObject);
			baseObject.CustomTrack = text;
			baseObject.WriteCustom();
		}
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedCollectionAction(list, SelectionController.SelectedObjects.ToList(), $"Assigned track to ({SelectionController.SelectedObjects.Count}) objects."), perform: true);
	}

	public override ObjectContainer CreateContainer()
	{
		return CustomEventContainer.SpawnCustomEvent(null, this, ref customEventPrefab);
	}
}
