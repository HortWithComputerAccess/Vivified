using System;
using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class NoteGridContainer : BeatmapObjectContainerCollection<BaseNote>
{
	[SerializeField]
	private GameObject notePrefab;

	[SerializeField]
	private GameObject bombPrefab;

	[FormerlySerializedAs("noteAppearanceSO")]
	[SerializeField]
	private NoteAppearanceSO noteAppearanceSo;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private CountersPlusController countersPlus;

	private readonly List<ObjectContainer> objectsAtSameTime = new List<ObjectContainer>();

	public static bool ShowArcVisualizer { get; private set; }

	public override ObjectType ContainerType => ObjectType.Note;

	internal override void SubscribeToCallbacks()
	{
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(spawnCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveNoteCheckFinished = (Action<bool, int>)Delegate.Combine(spawnCallbackController2.RecursiveNoteCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(despawnCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Combine(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		UIMode.PreviewModeSwitched = (Action)Delegate.Combine(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.NotifyBySettingName("NoteColorMultiplier", AppearanceChanged);
		Settings.NotifyBySettingName("ArrowColorMultiplier", AppearanceChanged);
		Settings.NotifyBySettingName("ArrowColorWhiteBlend", AppearanceChanged);
		Settings.NotifyBySettingName("AccurateNoteSize", AppearanceChanged);
	}

	internal override void UnsubscribeToCallbacks()
	{
		BeatmapObjectCallbackController spawnCallbackController = SpawnCallbackController;
		spawnCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(spawnCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(SpawnCallback));
		BeatmapObjectCallbackController spawnCallbackController2 = SpawnCallbackController;
		spawnCallbackController2.RecursiveNoteCheckFinished = (Action<bool, int>)Delegate.Remove(spawnCallbackController2.RecursiveNoteCheckFinished, new Action<bool, int>(RecursiveCheckFinished));
		BeatmapObjectCallbackController despawnCallbackController = DespawnCallbackController;
		despawnCallbackController.NotePassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(despawnCallbackController.NotePassedThreshold, new Action<bool, int, BaseObject>(DespawnCallback));
		AudioTimeSyncController audioTimeSyncController = AudioTimeSyncController;
		audioTimeSyncController.PlayToggle = (Action<bool>)Delegate.Remove(audioTimeSyncController.PlayToggle, new Action<bool>(OnPlayToggle));
		UIMode.PreviewModeSwitched = (Action)Delegate.Remove(UIMode.PreviewModeSwitched, new Action(OnUIPreviewModeSwitch));
		Settings.ClearSettingNotifications("NoteColorMultiplier");
		Settings.ClearSettingNotifications("ArrowColorMultiplier");
		Settings.ClearSettingNotifications("ArrowColorWhiteBlend");
		Settings.ClearSettingNotifications("AccurateNoteSize");
	}

	private void OnPlayToggle(bool isPlaying)
	{
		if (!isPlaying)
		{
			RefreshPool();
		}
	}

	private void OnUIPreviewModeSwitch()
	{
		RefreshPool(forceRefresh: true);
	}

	private void AppearanceChanged(object _)
	{
		RefreshPool(forceRefresh: true);
	}

	private void SpawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (!LoadedContainers.ContainsKey(objectData))
		{
			CreateContainerFromPool(objectData);
		}
	}

	private void DespawnCallback(bool initial, int index, BaseObject objectData)
	{
		if (LoadedContainers.ContainsKey(objectData))
		{
			if (!LoadedContainers[objectData].Animator.AnimatedLife)
			{
				RecycleContainer(objectData);
			}
			else
			{
				LoadedContainers[objectData].Animator.ShouldRecycle = true;
			}
		}
	}

	private void RecursiveCheckFinished(bool natural, int lastPassedIndex)
	{
		RefreshPool();
	}

	public void UpdateColor(Color red, Color blue)
	{
		noteAppearanceSo.UpdateColor(red, blue);
	}

	public override ObjectContainer CreateContainer()
	{
		NoteContainer noteContainer = NoteContainer.SpawnBeatmapNote(null, ref notePrefab);
		noteContainer.Animator.Atsc = AudioTimeSyncController;
		noteContainer.Animator.TracksManager = tracksManager;
		return noteContainer;
	}

	protected override void UpdateContainerData(ObjectContainer con, BaseObject obj)
	{
		NoteContainer noteContainer = con as NoteContainer;
		BaseNote noteData = obj as BaseNote;
		noteAppearanceSo.SetNoteAppearance(noteContainer);
		noteContainer.Setup();
		noteContainer.DirectionTargetEuler = NoteContainer.Directionalize(noteData);
		if (!noteContainer.Animator.AnimatedTrack)
		{
			tracksManager.GetTrackAtTime(obj.SongBpmTime).AttachContainer(con);
		}
	}

	protected override void OnObjectSpawned(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Notes);
	}

	protected override void OnObjectDelete(BaseObject _, bool __ = false)
	{
		countersPlus.UpdateStatistic(CountersPlusStatistic.Notes);
	}

	protected override void OnContainerSpawn(ObjectContainer container, BaseObject obj)
	{
		RefreshSpecialAngles(obj, objectWasSpawned: true, AudioTimeSyncController.IsPlaying);
	}

	protected override void OnContainerDespawn(ObjectContainer container, BaseObject obj)
	{
		RefreshSpecialAngles(obj, objectWasSpawned: false, AudioTimeSyncController.IsPlaying);
	}

	public void RefreshSpecialAngles(BaseObject obj, bool objectWasSpawned, bool isNatural)
	{
		if (!objectWasSpawned && isNatural)
		{
			return;
		}
		BaseNote baseNote = obj as BaseNote;
		if (baseNote.Type == 3 || baseNote.CustomFake)
		{
			return;
		}
		PopulateObjectsAtSameTime(baseNote);
		if (objectsAtSameTime.Count != 2)
		{
			ClearSpecialAnglesFromObjectsAtSameTime();
			return;
		}
		BaseNote baseNote2 = objectsAtSameTime[0].ObjectData as BaseNote;
		BaseNote baseNote3 = objectsAtSameTime[^1].ObjectData as BaseNote;
		NoteContainer noteContainer = objectsAtSameTime[0] as NoteContainer;
		NoteContainer noteContainer2 = objectsAtSameTime[^1] as NoteContainer;
		bool flag = baseNote2.CustomCoordinate != null || baseNote3.CustomCoordinate != null;
		bool flag2 = baseNote2.CutDirection >= 1000 || baseNote2.CutDirection <= -1000 || baseNote3.CutDirection >= 1000 || baseNote3.CutDirection <= -1000;
		if (baseNote2.CutDirection != baseNote3.CutDirection && baseNote2.CutDirection != 8 && baseNote3.CutDirection != 8 && !flag2 && !flag)
		{
			Vector3 directionTargetEuler = NoteContainer.Directionalize(baseNote2);
			Vector3 directionTargetEuler2 = NoteContainer.Directionalize(baseNote3);
			noteContainer.DirectionTarget.localEulerAngles = (noteContainer.DirectionTargetEuler = directionTargetEuler);
			noteContainer2.DirectionTarget.localEulerAngles = (noteContainer2.DirectionTargetEuler = directionTargetEuler2);
			return;
		}
		if (baseNote2.CutDirection == 8)
		{
			BaseNote baseNote4 = baseNote3;
			BaseNote baseNote5 = baseNote2;
			baseNote2 = baseNote4;
			baseNote3 = baseNote5;
			NoteContainer noteContainer3 = noteContainer2;
			NoteContainer noteContainer4 = noteContainer;
			noteContainer = noteContainer3;
			noteContainer2 = noteContainer4;
		}
		Vector2 position = baseNote2.GetPosition();
		Vector2 position2 = baseNote3.GetPosition();
		Vector2 vec = ((baseNote2.CutDirection == 8) ? Vector2.up : Direction(baseNote2));
		Vector2 line = position - position2;
		float num = SignedAngleToLine(vec, line);
		if (baseNote2.CutDirection == 8 && baseNote3.CutDirection == 8)
		{
			noteContainer.DirectionTargetEuler = Vector3.forward * num;
			noteContainer2.DirectionTargetEuler = Vector3.forward * num;
		}
		else if (Mathf.Abs(num) <= 40f)
		{
			Vector3 vector = NoteContainer.Directionalize(baseNote2) + new Vector3(0f, 0f, -baseNote2.AngleOffset);
			Vector3 vector2 = NoteContainer.Directionalize(baseNote3) + new Vector3(0f, 0f, -baseNote3.AngleOffset);
			noteContainer.DirectionTargetEuler = vector + Vector3.forward * num;
			if (baseNote3.CutDirection == 8 && !baseNote2.IsMainDirection)
			{
				noteContainer2.DirectionTargetEuler = vector2 + Vector3.forward * (num + 45f);
			}
			else
			{
				noteContainer2.DirectionTargetEuler = vector2 + Vector3.forward * num;
			}
		}
		else
		{
			Vector3 directionTargetEuler3 = NoteContainer.Directionalize(baseNote2);
			Vector3 directionTargetEuler4 = NoteContainer.Directionalize(baseNote3);
			noteContainer.DirectionTargetEuler = directionTargetEuler3;
			noteContainer2.DirectionTargetEuler = directionTargetEuler4;
		}
		noteContainer.DirectionTarget.localEulerAngles = noteContainer.DirectionTargetEuler;
		noteContainer2.DirectionTarget.localEulerAngles = noteContainer2.DirectionTargetEuler;
	}

	public void ClearSpecialAngles(BaseObject obj)
	{
		BaseNote note = obj as BaseNote;
		PopulateObjectsAtSameTime(note);
		ClearSpecialAnglesFromObjectsAtSameTime();
	}

	private void PopulateObjectsAtSameTime(BaseNote note)
	{
		objectsAtSameTime.Clear();
		foreach (KeyValuePair<BaseObject, ObjectContainer> loadedContainer in LoadedContainers)
		{
			if (!note.CustomFake && loadedContainer.Key.JsonTime - BeatmapObjectContainerCollection.Epsilon <= note.JsonTime && loadedContainer.Key.JsonTime + BeatmapObjectContainerCollection.Epsilon >= note.JsonTime && (loadedContainer.Key as BaseNote).Type == note.Type)
			{
				objectsAtSameTime.Add(loadedContainer.Value);
			}
		}
	}

	private void ClearSpecialAnglesFromObjectsAtSameTime()
	{
		foreach (ObjectContainer item in objectsAtSameTime)
		{
			Vector3 vector = NoteContainer.Directionalize(item.ObjectData as BaseNote);
			(item as NoteContainer).DirectionTarget.localEulerAngles = vector;
			(item as NoteContainer).DirectionTargetEuler = vector;
		}
	}

	public static Vector2 Direction(BaseNote obj)
	{
		return obj.CutDirection switch
		{
			0 => new Vector2(0f, 1f), 
			1 => new Vector2(0f, -1f), 
			2 => new Vector2(-1f, 0f), 
			3 => new Vector2(1f, 0f), 
			4 => new Vector2(-0.7071f, 0.7071f), 
			5 => new Vector2(0.7071f, 0.7071f), 
			6 => new Vector2(-0.7071f, -0.7071f), 
			7 => new Vector2(0.7071f, -0.7071f), 
			_ => new Vector2(0f, 0f), 
		};
	}

	private float SignedAngleToLine(Vector2 vec, Vector2 line)
	{
		float num = Vector2.SignedAngle(vec, line);
		float num2 = Vector2.SignedAngle(vec, -line);
		if (Mathf.Abs(num) >= Mathf.Abs(num2))
		{
			return num2;
		}
		return num;
	}
}
