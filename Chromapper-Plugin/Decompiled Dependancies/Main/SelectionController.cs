using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionController : MonoBehaviour, CMInput.ISelectingActions, CMInput.IModifyingSelectionActions
{
	public static HashSet<BaseObject> SelectedObjects = new HashSet<BaseObject>();

	public static HashSet<BaseObject> CopiedObjects = new HashSet<BaseObject>();

	public static Action<BaseObject> ObjectWasSelectedEvent;

	public static Action SelectionChangedEvent;

	public static Action<IEnumerable<BaseObject>> SelectionPastedEvent;

	private static SelectionController instance;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private BPMChangeGridContainer bpmChangesContainer;

	[SerializeField]
	private Material selectionMaterial;

	[SerializeField]
	private Transform moveableGridTransform;

	[SerializeField]
	private Color selectedColor;

	[SerializeField]
	private Color copiedColor;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private EventPlacement eventPlacement;

	[SerializeField]
	private CreateEventTypeLabels labels;

	private bool shiftInPlace;

	private bool shiftInTime;

	public static Color SelectedColor => instance.selectedColor;

	public static Color CopiedColor => instance.copiedColor;

	private void Start()
	{
		instance = this;
		SelectedObjects.Clear();
	}

	public void OnPaste(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Paste();
		}
	}

	public void OnOverwritePaste(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Paste(triggersAction: true, overwriteSection: true);
		}
	}

	public void OnDeleteObjects(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Delete();
		}
	}

	public void OnCopy(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Copy();
		}
	}

	public void OnCut(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Copy(cut: true);
		}
	}

	public void OnShiftingMovement(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Vector2 vector = context.ReadValue<Vector2>();
			if (shiftInPlace)
			{
				ShiftSelection(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
			}
			if (shiftInTime)
			{
				MoveSelection(vector.y * (1f / (float)atsc.GridMeasureSnapping));
			}
		}
	}

	public void OnActivateShiftinTime(InputAction.CallbackContext context)
	{
		shiftInTime = context.performed;
	}

	public void OnActivateShiftinPlace(InputAction.CallbackContext context)
	{
		shiftInPlace = context.performed;
	}

	public void OnDeselectAll(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			DeselectAll();
		}
	}

	public static bool HasSelectedObjects()
	{
		return SelectedObjects.Count > 0;
	}

	public static bool HasCopiedObjects()
	{
		return CopiedObjects.Count > 0;
	}

	public static bool IsObjectSelected(BaseObject container)
	{
		return SelectedObjects.Contains(container);
	}

	public static void GetObjectTypes(IEnumerable<BaseObject> objects, out bool hasNoteOrObstacle, out bool hasEvent, out bool hasBpmChange, out bool hasNjsEvent)
	{
		hasNoteOrObstacle = false;
		hasEvent = false;
		hasBpmChange = false;
		hasNjsEvent = false;
		foreach (BaseObject @object in objects)
		{
			switch (@object.ObjectType)
			{
			case ObjectType.Note:
			case ObjectType.Obstacle:
			case ObjectType.CustomNote:
			case ObjectType.Arc:
			case ObjectType.Chain:
				hasNoteOrObstacle = true;
				break;
			case ObjectType.Event:
			case ObjectType.CustomEvent:
				hasEvent = true;
				break;
			case ObjectType.BpmChange:
				hasBpmChange = true;
				break;
			case ObjectType.NJSEvent:
				hasNjsEvent = true;
				break;
			}
		}
	}

	public static void ForEachObjectBetweenSongBpmTimeByGroup(float start, float end, bool hasNoteOrObstacle, bool hasEvent, bool hasBpmChange, bool hasNjsEvent, Action<BeatmapObjectContainerCollection, BaseObject> callback)
	{
		List<ObjectType> list = new List<ObjectType>();
		if (hasNoteOrObstacle)
		{
			list.AddRange(new ObjectType[5]
			{
				ObjectType.Note,
				ObjectType.Obstacle,
				ObjectType.CustomNote,
				ObjectType.Arc,
				ObjectType.Chain
			});
		}
		if (hasNoteOrObstacle && !hasEvent)
		{
			list.Add(ObjectType.Event);
		}
		if (hasEvent)
		{
			list.AddRange(new ObjectType[2]
			{
				ObjectType.Event,
				ObjectType.CustomEvent
			});
		}
		if (hasBpmChange)
		{
			list.Add(ObjectType.BpmChange);
		}
		if (hasNjsEvent)
		{
			list.Add(ObjectType.NJSEvent);
		}
		float epsilon = BeatmapObjectContainerCollection.Epsilon;
		foreach (ObjectType item in list)
		{
			BeatmapObjectContainerCollection collectionForType = BeatmapObjectContainerCollection.GetCollectionForType(item);
			if (collectionForType == null)
			{
				continue;
			}
			IEnumerable<BaseObject> enumerable = ((!(collectionForType is ArcGridContainer) && !(collectionForType is ChainGridContainer)) ? collectionForType.LoadedObjects.Where((BaseObject x) => start - epsilon < x.SongBpmTime && x.SongBpmTime < end + epsilon) : collectionForType.LoadedObjects.Where((BaseObject x) => (start - epsilon < x.SongBpmTime && x.SongBpmTime < end + epsilon) || (x.SongBpmTime < start + epsilon && start - epsilon < (x as BaseSlider).TailSongBpmTime)));
			foreach (BaseObject item2 in enumerable)
			{
				if (hasEvent || !(item2 is BaseEvent baseEvent) || baseEvent.IsLaneRotationEvent() || (hasBpmChange && baseEvent.IsBpmEvent()))
				{
					callback?.Invoke(collectionForType, item2);
				}
			}
		}
	}

	public static void Select(BaseObject obj, bool addsToSelection = false, bool automaticallyRefreshes = true, bool addActionEvent = true)
	{
		if (!addsToSelection)
		{
			DeselectAll();
		}
		BeatmapObjectContainerCollection collectionForType = BeatmapObjectContainerCollection.GetCollectionForType(obj.ObjectType);
		if (collectionForType.ContainsObject(obj))
		{
			SelectedObjects.Add(obj);
			if (collectionForType.LoadedContainers.TryGetValue(obj, out var value))
			{
				value.SetOutlineColor(instance.selectedColor);
			}
			if (addActionEvent)
			{
				ObjectWasSelectedEvent(obj);
				SelectionChangedEvent?.Invoke();
			}
		}
	}

	public static void SelectBetween(BaseObject first, BaseObject second, bool addsToSelection = false, bool addActionEvent = true)
	{
		if (!addsToSelection)
		{
			DeselectAll();
		}
		if (first.SongBpmTime > second.SongBpmTime)
		{
			BaseObject baseObject = second;
			BaseObject baseObject2 = first;
			first = baseObject;
			second = baseObject2;
		}
		GetObjectTypes(new BaseObject[2] { first, second }, out var hasNoteOrObstacle, out var hasEvent, out var hasBpmChange, out var hasNjsEvent);
		ForEachObjectBetweenSongBpmTimeByGroup(first.SongBpmTime, second.SongBpmTime, hasNoteOrObstacle, hasEvent, hasBpmChange, hasNjsEvent, delegate(BeatmapObjectContainerCollection collection, BaseObject beatmapObject)
		{
			if (!SelectedObjects.Contains(beatmapObject))
			{
				SelectedObjects.Add(beatmapObject);
				if (collection.LoadedContainers.TryGetValue(beatmapObject, out var value))
				{
					value.SetOutlineColor(instance.selectedColor);
				}
				if (addActionEvent)
				{
					ObjectWasSelectedEvent(beatmapObject);
				}
			}
		});
		if (addActionEvent)
		{
			SelectionChangedEvent?.Invoke();
		}
	}

	public static void Deselect(BaseObject obj, bool removeActionEvent = true)
	{
		SelectedObjects.Remove(obj);
		if (BeatmapObjectContainerCollection.GetCollectionForType(obj.ObjectType).LoadedContainers.TryGetValue(obj, out var value) && value != null)
		{
			value.OutlineVisible = false;
		}
		if (removeActionEvent)
		{
			SelectionChangedEvent?.Invoke();
		}
	}

	public static void DeselectAll(bool removeActionEvent = true)
	{
		BaseObject[] array = SelectedObjects.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Deselect(array[i], removeActionEvent: false);
		}
		if (removeActionEvent)
		{
			SelectionChangedEvent?.Invoke();
		}
	}

	internal static void RefreshSelectionMaterial(bool triggersAction = true)
	{
		foreach (BaseObject selectedObject in SelectedObjects)
		{
			if (BeatmapObjectContainerCollection.GetCollectionForType(selectedObject.ObjectType).LoadedContainers.TryGetValue(selectedObject, out var value))
			{
				value.OutlineVisible = true;
				value.SetOutlineColor(instance.selectedColor);
			}
		}
	}

	public void Delete(bool triggersAction = true)
	{
		IEnumerable<BaseObject> enumerable = SelectedObjects.ToArray();
		if (triggersAction)
		{
			BeatmapActionContainer.AddAction(new SelectionDeletedAction(enumerable));
		}
		DeselectAll();
		foreach (BaseObject item in enumerable)
		{
			BeatmapObjectContainerCollection.GetCollectionForType(item.ObjectType).DeleteObject(item, triggersAction: false, refreshesPool: false);
		}
	}

	public void Copy(bool cut = false)
	{
		if (!HasSelectedObjects())
		{
			return;
		}
		CopiedObjects.Clear();
		float jsonTime = SelectedObjects.OrderBy((BaseObject x) => x.JsonTime).First().JsonTime;
		foreach (BaseObject selectedObject in SelectedObjects)
		{
			if (BeatmapObjectContainerCollection.GetCollectionForType(selectedObject.ObjectType).LoadedContainers.TryGetValue(selectedObject, out var value))
			{
				value.SetOutlineColor(instance.copiedColor);
			}
			BaseObject baseObject = BeatmapFactory.Clone(selectedObject);
			baseObject.JsonTime -= jsonTime;
			if (baseObject is BaseSlider baseSlider)
			{
				baseSlider.TailJsonTime -= jsonTime;
			}
			CopiedObjects.Add(baseObject);
		}
		if (cut)
		{
			Delete();
		}
	}

	public void Paste(bool triggersAction = true, bool overwriteSection = false)
	{
		DeselectAll();
		List<BaseObject> pasted = new List<BaseObject>();
		Dictionary<ObjectType, BeatmapObjectContainerCollection> dictionary = new Dictionary<ObjectType, BeatmapObjectContainerCollection>();
		foreach (BaseObject copiedObject in CopiedObjects)
		{
			if (copiedObject != null)
			{
				float currentJsonTime = atsc.CurrentJsonTime;
				float jsonTime = currentJsonTime + copiedObject.JsonTime;
				BaseObject baseObject = BeatmapFactory.Clone(copiedObject);
				baseObject.JsonTime = jsonTime;
				if (baseObject is BaseSlider baseSlider)
				{
					baseSlider.TailJsonTime = currentJsonTime + baseSlider.TailJsonTime;
				}
				if (!dictionary.TryGetValue(baseObject.ObjectType, out var value))
				{
					value = BeatmapObjectContainerCollection.GetCollectionForType(baseObject.ObjectType);
					dictionary.Add(baseObject.ObjectType, value);
				}
				pasted.Add(baseObject);
			}
		}
		List<BaseObject> list = new List<BaseObject>();
		foreach (KeyValuePair<ObjectType, BeatmapObjectContainerCollection> kvp in dictionary)
		{
			kvp.Value.RemoveConflictingObjects(pasted.Where((BaseObject x) => x.ObjectType == kvp.Key), out var conflicting);
			list.AddRange(conflicting);
		}
		if (overwriteSection)
		{
			float songBpmTime = pasted.First().SongBpmTime;
			float songBpmTime2 = pasted.First().SongBpmTime;
			foreach (BaseObject item in pasted)
			{
				if (songBpmTime > item.SongBpmTime)
				{
					songBpmTime = item.SongBpmTime;
				}
				if (songBpmTime2 < item.SongBpmTime)
				{
					songBpmTime2 = item.SongBpmTime;
				}
			}
			GetObjectTypes(pasted, out var hasNoteOrObstacle, out var hasEvent, out var hasBpmChange, out var hasNjsEvent);
			List<(BeatmapObjectContainerCollection, BaseObject)> toRemove = new List<(BeatmapObjectContainerCollection, BaseObject)>();
			ForEachObjectBetweenSongBpmTimeByGroup(songBpmTime, songBpmTime2, hasNoteOrObstacle, hasEvent, hasBpmChange, hasNjsEvent, delegate(BeatmapObjectContainerCollection collection, BaseObject beatmapObject)
			{
				if (!pasted.Contains(beatmapObject))
				{
					toRemove.Add((collection, beatmapObject));
				}
			});
			foreach (var (beatmapObjectContainerCollection, baseObject2) in toRemove)
			{
				beatmapObjectContainerCollection.DeleteObject(baseObject2, triggersAction: false, refreshesPool: true, "No comment.", inCollectionOfDeletes: true);
				list.Add(baseObject2);
			}
		}
		foreach (BaseObject item2 in pasted)
		{
			dictionary[item2.ObjectType].SpawnObject(item2, removeConflicting: false, refreshesPool: false, inCollectionOfSpawns: true);
			Select(item2, addsToSelection: true, automaticallyRefreshes: false, addActionEvent: false);
		}
		RefreshMovedEventsAppearance(SelectedObjects.OfType<BaseEvent>());
		foreach (BeatmapObjectContainerCollection value2 in dictionary.Values)
		{
			value2.RefreshPool();
			if (value2 is BPMChangeGridContainer bPMChangeGridContainer)
			{
				bPMChangeGridContainer.RefreshModifiedBeat();
			}
		}
		if (CopiedObjects.Any((BaseObject x) => x is BaseEvent baseEvent && baseEvent.IsLaneRotationEvent()))
		{
			tracksManager.RefreshTracks();
		}
		if (triggersAction)
		{
			BeatmapActionContainer.AddAction(new SelectionPastedAction(pasted, list));
		}
		SelectionPastedEvent?.Invoke(pasted);
		SelectionChangedEvent?.Invoke();
		if (eventPlacement.objectContainerCollection.PropagationEditing != EventGridContainer.PropMode.Off)
		{
			eventPlacement.objectContainerCollection.PropagationEditing = eventPlacement.objectContainerCollection.PropagationEditing;
		}
		Debug.Log("Pasted!");
	}

	public void MoveSelection(float beats, bool snapObjects = false)
	{
		List<BaseObject> list = new List<BaseObject>();
		List<BaseObject> list2 = new List<BaseObject>();
		foreach (BaseObject selectedObject in SelectedObjects)
		{
			BaseObject baseObject = BeatmapFactory.Clone(selectedObject);
			baseObject.JsonTime += beats;
			if (snapObjects)
			{
				baseObject.JsonTime = Mathf.Round(beats / (1f / (float)atsc.GridMeasureSnapping)) * (1f / (float)atsc.GridMeasureSnapping);
			}
			if (baseObject is BaseSlider baseSlider)
			{
				baseSlider.TailJsonTime += beats;
				if (snapObjects)
				{
					baseSlider.TailJsonTime = Mathf.Round(beats / (1f / (float)atsc.GridMeasureSnapping)) * (1f / (float)atsc.GridMeasureSnapping);
				}
			}
			list2.Add(baseObject);
			list.Add(selectedObject);
		}
		RefreshMovedEventsAppearance(SelectedObjects.OfType<BaseEvent>());
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedCollectionAction(list2, list, "Shifted a selection of objects."), perform: true);
	}

	public void ShiftSelection(int leftRight, int upDown)
	{
		List<BaseObject> editedObjects = SelectedObjects.AsParallel().Select(delegate(BaseObject original)
		{
			BaseObject baseObject = BeatmapFactory.Clone(original);
			if (baseObject is BaseNote baseNote)
			{
				if (baseNote.CustomCoordinate != null && baseNote.CustomCoordinate.IsArray)
				{
					ShiftCustomCoordinates(baseNote, leftRight, upDown);
				}
				else
				{
					bool flag = false;
					if (baseNote.PosX >= 1000)
					{
						baseNote.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseNote.PosX < 1000)
						{
							baseNote.PosX = 1000;
						}
					}
					else if (baseNote.PosX <= -1000)
					{
						baseNote.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseNote.PosX > -1000)
						{
							baseNote.PosX = -1000;
						}
					}
					else
					{
						baseNote.PosX += leftRight;
						if (Settings.Instance.VanillaOnlyShift)
						{
							baseNote.PosX = Mathf.Clamp(baseNote.PosX, 0, 3);
						}
						else if (baseNote.PosX < 0 || baseNote.PosX > 3)
						{
							flag = true;
						}
					}
					baseNote.PosY += upDown;
					if (Settings.Instance.VanillaOnlyShift)
					{
						baseNote.PosY = Mathf.Clamp(baseNote.PosY, 0, 2);
					}
					else if (baseNote.PosY < 0 || baseNote.PosY > 2)
					{
						flag = true;
					}
					if (flag)
					{
						baseNote.CustomCoordinate = new Vector2((float)baseNote.PosX - 2f, baseNote.PosY);
						int posX = (baseNote.PosY = 0);
						baseNote.PosX = posX;
					}
				}
			}
			else if (baseObject is BaseObstacle baseObstacle)
			{
				if (baseObstacle.CustomCoordinate != null && baseObstacle.CustomCoordinate.IsArray)
				{
					ShiftCustomCoordinates(baseObstacle, leftRight, upDown);
				}
				else if (baseObstacle.PosX >= 1000)
				{
					baseObstacle.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
					if (baseObstacle.PosX < 1000)
					{
						baseObstacle.PosX = 1000;
					}
				}
				else if (baseObstacle.PosX <= -1000)
				{
					baseObstacle.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
					if (baseObstacle.PosX > -1000)
					{
						baseObstacle.PosX = -1000;
					}
				}
				else
				{
					baseObstacle.PosX += leftRight;
				}
			}
			else if (baseObject is BaseEvent baseEvent)
			{
				EventGridContainer objectContainerCollection = eventPlacement.objectContainerCollection;
				if (eventPlacement.objectContainerCollection.PropagationEditing == EventGridContainer.PropMode.Light)
				{
					int val = objectContainerCollection.platformDescriptor.LightingManagers[objectContainerCollection.EventTypeToPropagate].LightIDPlacementMap.Count - 1;
					int num2 = Math.Min(((baseEvent.CustomLightID != null) ? labels.LightIDToEditor(baseEvent.Type, baseEvent.CustomLightID[0]) : (-1)) + leftRight, val);
					if (num2 < 0)
					{
						baseEvent.CustomLightID = null;
					}
					else
					{
						int num3 = labels.EditorToLightID(baseEvent.Type, num2);
						baseEvent.CustomLightID = new int[1] { num3 };
					}
				}
				else if (eventPlacement.objectContainerCollection.PropagationEditing == EventGridContainer.PropMode.Prop)
				{
					int num4 = ((baseEvent.CustomLightID != null) ? labels.LightIdsToPropId(objectContainerCollection.EventTypeToPropagate, baseEvent.CustomLightID) : ((int?)null)) ?? (-1);
					int num5 = Math.Min(val2: objectContainerCollection.platformDescriptor.LightingManagers[objectContainerCollection.EventTypeToPropagate].LightsGroupedByZ.Length - 1, val1: num4 + leftRight);
					if (num5 < 0)
					{
						baseEvent.CustomLightID = null;
					}
					else
					{
						baseEvent.CustomLightID = labels.PropIdToLightIds(objectContainerCollection.EventTypeToPropagate, num5);
					}
				}
				else
				{
					int type = baseEvent.Type;
					int num6 = labels.EventTypeToLaneId(baseEvent.Type);
					num6 += leftRight;
					if (num6 < 0)
					{
						num6 = 0;
					}
					int num7 = labels.MaxLaneId();
					if (num6 > num7)
					{
						num6 = num7;
					}
					baseEvent.Type = labels.LaneIdToEventType(num6);
					if (baseEvent.CustomLightID != null)
					{
						int lightID = labels.LightIDToEditor(type, baseEvent.CustomLightID[0]);
						baseEvent.CustomLightID = new int[1] { labels.EditorToLightID(baseEvent.Type, lightID) };
					}
					int[] customLightID = baseEvent.CustomLightID;
					if (customLightID != null && customLightID.Length == 0)
					{
						baseEvent.CustomLightID = null;
					}
				}
				JSONNode customData = original.CustomData;
				if ((object)customData != null && customData.Count <= 0)
				{
					original.CustomData = null;
				}
			}
			else if (baseObject is BaseSlider baseSlider)
			{
				bool flag2 = false;
				if (baseSlider.CustomCoordinate != null && baseSlider.CustomCoordinate.IsArray)
				{
					ShiftCustomCoordinates(baseSlider, leftRight, upDown);
				}
				else
				{
					if (baseSlider.PosX >= 1000)
					{
						baseSlider.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseSlider.PosX < 1000)
						{
							baseSlider.PosX = 1000;
						}
					}
					else if (baseSlider.PosX <= -1000)
					{
						baseSlider.PosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseSlider.PosX > -1000)
						{
							baseSlider.PosX = -1000;
						}
					}
					else
					{
						baseSlider.PosX += leftRight;
						if (Settings.Instance.VanillaOnlyShift)
						{
							baseSlider.PosX = Mathf.Clamp(baseSlider.PosX, 0, 3);
						}
						else if (baseSlider.PosY < 0 || baseSlider.PosY > 2)
						{
							flag2 = true;
						}
					}
					baseSlider.PosY += upDown;
					if (Settings.Instance.VanillaOnlyShift)
					{
						baseSlider.PosY = Mathf.Clamp(baseSlider.PosY, 0, 2);
					}
					else if (baseSlider.PosY < 0 || baseSlider.PosY > 2)
					{
						flag2 = true;
					}
					if (flag2)
					{
						baseSlider.CustomCoordinate = new Vector2((float)baseSlider.PosX + 1f, baseSlider.PosY);
						int posX = (baseSlider.PosY = 0);
						baseSlider.PosX = posX;
					}
				}
				bool flag3 = false;
				if (baseSlider.CustomTailCoordinate != null && baseSlider.CustomTailCoordinate.IsArray)
				{
					ShiftCustomTailCoordinates(baseSlider, leftRight, upDown);
				}
				else
				{
					if (baseSlider.TailPosX >= 1000)
					{
						baseSlider.TailPosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseSlider.TailPosX < 1000)
						{
							baseSlider.TailPosX = 1000;
						}
					}
					else if (baseSlider.TailPosX <= -1000)
					{
						baseSlider.TailPosX += Mathf.RoundToInt(1f / (float)atsc.GridMeasureSnapping * 1000f * (float)leftRight);
						if (baseSlider.TailPosX > -1000)
						{
							baseSlider.TailPosX = -1000;
						}
					}
					else
					{
						baseSlider.TailPosX += leftRight;
						if (Settings.Instance.VanillaOnlyShift)
						{
							baseSlider.TailPosX = Mathf.Clamp(baseSlider.TailPosX, 0, 3);
						}
					}
					baseSlider.TailPosY += upDown;
					if (Settings.Instance.VanillaOnlyShift)
					{
						baseSlider.TailPosY = Mathf.Clamp(baseSlider.TailPosY, 0, 2);
					}
					else if (baseSlider.PosY < 0 || baseSlider.PosY > 2)
					{
						flag3 = true;
					}
					if (flag3)
					{
						baseSlider.CustomTailCoordinate = new Vector2((float)baseSlider.TailPosX + 1f, baseSlider.TailPosY);
						int posX = (baseSlider.TailPosY = 0);
						baseSlider.TailPosX = posX;
					}
				}
			}
			baseObject.SaveCustom();
			return baseObject;
		}).ToList();
		List<BaseObject> originalObjects = SelectedObjects.ToList();
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedCollectionAction(editedObjects, originalObjects, "Shifted a selection of objects."), perform: true);
		tracksManager.RefreshTracks();
	}

	private void ShiftCustomCoordinates(BaseGrid gridObject, int leftRight, int upDown)
	{
		Vector2 vector = new Vector2((float)gridObject.PosX - 2f, gridObject.PosY);
		if (gridObject.CustomCoordinate[0].IsNumber)
		{
			vector.x = gridObject.CustomCoordinate[0];
		}
		if (gridObject.CustomCoordinate[1].IsNumber)
		{
			vector.y = gridObject.CustomCoordinate[1];
		}
		gridObject.CustomCoordinate = new Vector2(vector.x + 1f / (float)atsc.GridMeasureSnapping * (float)leftRight, vector.y + 1f / (float)atsc.GridMeasureSnapping * (float)upDown);
	}

	private void ShiftCustomTailCoordinates(BaseSlider slider, int leftRight, int upDown)
	{
		Vector2 vector = new Vector2((float)slider.TailPosX - 2f, slider.TailPosY);
		if (slider.CustomTailCoordinate[0].IsNumber)
		{
			vector.x = slider.CustomTailCoordinate[0];
		}
		if (slider.CustomTailCoordinate[1].IsNumber)
		{
			vector.y = slider.CustomTailCoordinate[1];
		}
		slider.CustomTailCoordinate = new Vector2(vector.x + 1f / (float)atsc.GridMeasureSnapping * (float)leftRight, vector.y + 1f / (float)atsc.GridMeasureSnapping * (float)upDown);
	}

	private void RefreshMovedEventsAppearance(IEnumerable<BaseEvent> events)
	{
		if (events.Any())
		{
			EventGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<EventGridContainer>(ObjectType.Event);
			collectionForType.MarkEventsToBeRelinked(events);
			collectionForType.LinkAllLightEvents();
			collectionForType.RefreshEventsAppearance(events);
		}
	}
}
