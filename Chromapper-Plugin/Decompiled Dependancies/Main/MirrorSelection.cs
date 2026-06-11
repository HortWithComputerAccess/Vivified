using System.Collections.Generic;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;
using SimpleJSON;
using UnityEngine;

public class MirrorSelection : MonoBehaviour
{
	[SerializeField]
	private NoteAppearanceSO noteAppearance;

	[SerializeField]
	private EventAppearanceSO eventAppearance;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private CreateEventTypeLabels labels;

	private readonly Dictionary<int, int> cutDirectionToMirrored = new Dictionary<int, int>
	{
		{ 6, 7 },
		{ 7, 6 },
		{ 4, 5 },
		{ 5, 4 },
		{ 3, 2 },
		{ 2, 3 }
	};

	public void MirrorTime()
	{
		if (!SelectionController.HasSelectedObjects())
		{
			PersistentUI.Instance.DisplayMessage("Mapper", "mirror.error", PersistentUI.DisplayMessageType.Bottom);
			return;
		}
		IOrderedEnumerable<BaseObject> source = SelectionController.SelectedObjects.OrderByDescending((BaseObject x) => x.JsonTime);
		IEnumerable<BaseObject> source2 = source.Where((BaseObject x) => x is BaseSlider);
		float num = Mathf.Max(b: source2.Any() ? source2.Max((BaseObject x) => (x as BaseSlider).TailJsonTime) : float.MinValue, a: source.First().JsonTime);
		float jsonTime = source.Last().JsonTime;
		List<BeatmapAction> list = new List<BeatmapAction>();
		foreach (BaseObject selectedObject in SelectionController.SelectedObjects)
		{
			BaseObject baseObject = BeatmapFactory.Clone(selectedObject);
			baseObject.JsonTime = jsonTime + (num - selectedObject.JsonTime);
			if (baseObject is BaseSlider baseSlider && selectedObject is BaseSlider baseSlider2)
			{
				baseSlider.TailJsonTime = jsonTime + (num - baseSlider2.TailJsonTime);
				baseSlider.SwapHeadAndTail();
			}
			list.Add(new BeatmapObjectModifiedAction(baseObject, selectedObject, selectedObject, "e", keepSelection: true));
		}
		BeatmapActionContainer.AddAction(new ActionCollectionAction(list, forceRefreshPool: true, clearsSelection: true, "Mirrored a selection of objects in time."), perform: true);
	}

	public void Mirror(bool moveNotes = true)
	{
		if (!SelectionController.HasSelectedObjects())
		{
			PersistentUI.Instance.DisplayMessage("Mapper", "mirror.error", PersistentUI.DisplayMessageType.Bottom);
			return;
		}
		EventGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<EventGridContainer>(ObjectType.Event);
		List<BaseObject> list = new List<BaseObject>();
		List<BaseObject> list2 = new List<BaseObject>();
		foreach (BaseObject selectedObject in SelectionController.SelectedObjects)
		{
			BaseObject baseObject = BeatmapFactory.Clone(selectedObject);
			BaseObstacle baseObstacle = baseObject as BaseObstacle;
			if (baseObstacle != null && moveNotes)
			{
				bool flag = baseObstacle.Width >= 1000;
				int posX = baseObstacle.PosX;
				if (baseObstacle.CustomCoordinate != null && baseObstacle.CustomCoordinate.IsArray)
				{
					Vector2 vector = baseObstacle.CustomCoordinate.ReadVector2();
					Vector2 vector2 = new Vector2(vector.x * -1f, vector.y);
					JSONNode customSize = baseObstacle.CustomSize;
					if (customSize != null && customSize.IsArray && customSize[0].IsNumber)
					{
						vector2.x -= customSize[0].AsFloat;
					}
					else
					{
						vector2.x -= baseObstacle.Width;
					}
					baseObstacle.CustomCoordinate = vector2;
				}
				if (baseObstacle.CustomLocalRotation != null)
				{
					if (baseObstacle.CustomLocalRotation.IsNumber)
					{
						baseObstacle.CustomLocalRotation = 0f - baseObstacle.CustomLocalRotation.AsFloat;
					}
					else if (baseObstacle.CustomLocalRotation is JSONArray jSONArray)
					{
						if (jSONArray.Count > 1)
						{
							jSONArray[1] = 0f - jSONArray[1].AsFloat;
						}
						if (jSONArray.Count > 2)
						{
							jSONArray[2] = 0f - jSONArray[2].AsFloat;
						}
					}
				}
				if (baseObstacle.CustomWorldRotation != null)
				{
					if (baseObstacle.CustomWorldRotation.IsNumber)
					{
						baseObstacle.CustomWorldRotation = 0f - baseObstacle.CustomWorldRotation.AsFloat;
					}
					else if (baseObstacle.CustomWorldRotation is JSONArray jSONArray2)
					{
						if (jSONArray2.Count > 1)
						{
							jSONArray2[1] = 0f - jSONArray2[1].AsFloat;
						}
						if (jSONArray2.Count > 2)
						{
							jSONArray2[2] = 0f - jSONArray2[2].AsFloat;
						}
					}
				}
				if (posX >= 1000 || posX <= -1000 || flag)
				{
					int num = posX;
					num = ((num <= -1000) ? (num + 1000) : ((num < 1000) ? (num * 1000) : (num - 1000)));
					num = (num - 2000) * -1 + 2000;
					int width = baseObstacle.Width;
					width = ((width >= 1000) ? (width - 1000) : (width * 1000));
					num -= width;
					num = ((num >= 0) ? (num + 1000) : (num - 1000));
					baseObstacle.PosX = num;
				}
				else
				{
					int num2 = (posX - 2) * -1 + 2;
					baseObstacle.PosX = num2 - baseObstacle.Width;
				}
			}
			else if (baseObject is BaseNote baseNote)
			{
				if (moveNotes)
				{
					baseNote.AngleOffset *= -1;
					if (baseNote.CustomCoordinate != null && baseNote.CustomCoordinate.IsArray)
					{
						Vector2 vector3 = baseNote.CustomCoordinate.ReadVector2();
						Vector2 vector4 = new Vector2((vector3.x + 0.5f) * -1f - 0.5f, vector3.y);
						baseNote.CustomCoordinate = vector4;
					}
					if (baseNote.CustomDirection.HasValue)
					{
						baseNote.CustomDirection *= -1f;
					}
					if (baseNote.CustomLocalRotation != null)
					{
						if (baseNote.CustomLocalRotation.IsNumber)
						{
							baseNote.CustomLocalRotation = 0f - baseNote.CustomLocalRotation.AsFloat;
						}
						else if (baseNote.CustomLocalRotation is JSONArray jSONArray3)
						{
							if (jSONArray3.Count > 1)
							{
								jSONArray3[1] = 0f - jSONArray3[1].AsFloat;
							}
							if (jSONArray3.Count > 2)
							{
								jSONArray3[2] = 0f - jSONArray3[2].AsFloat;
							}
						}
					}
					if (baseNote.CustomWorldRotation != null)
					{
						if (baseNote.CustomWorldRotation.IsNumber)
						{
							baseNote.CustomWorldRotation = 0f - baseNote.CustomWorldRotation.AsFloat;
						}
						else if (baseNote.CustomWorldRotation is JSONArray jSONArray4)
						{
							if (jSONArray4.Count > 1)
							{
								jSONArray4[1] = 0f - jSONArray4[1].AsFloat;
							}
							if (jSONArray4.Count > 2)
							{
								jSONArray4[2] = 0f - jSONArray4[2].AsFloat;
							}
						}
					}
					int posX2 = baseNote.PosX;
					if (posX2 > 3 || posX2 < 0)
					{
						int num3 = posX2;
						if (num3 <= -1000)
						{
							num3 += 1000;
						}
						else if (num3 >= 1000)
						{
							num3 -= 1000;
						}
						num3 = (num3 - 1500) * -1 + 1500;
						num3 = ((num3 >= 0) ? (num3 + 1000) : (num3 - 1000));
						baseNote.PosX = num3;
					}
					else
					{
						int posX3 = (int)(((float)posX2 - 1.5f) * -1f + 1.5f);
						baseNote.PosX = posX3;
					}
				}
				if (baseNote.Type != 3)
				{
					baseNote.Type = ((baseNote.Type == 0) ? 1 : 0);
					if (moveNotes && cutDirectionToMirrored.ContainsKey(baseNote.CutDirection))
					{
						baseNote.CutDirection = cutDirectionToMirrored[baseNote.CutDirection];
					}
				}
			}
			else if (baseObject is BaseEvent baseEvent)
			{
				if (baseEvent.IsLaneRotationEvent())
				{
					baseEvent.Rotation *= -1f;
					if (baseEvent.CustomLaneRotation.HasValue)
					{
						baseEvent.CustomLaneRotation *= -1f;
					}
					tracksManager.RefreshTracks();
				}
				else
				{
					if (baseEvent.CustomLightGradient != null)
					{
						ChromaLightGradient customLightGradient = baseEvent.CustomLightGradient;
						ChromaLightGradient customLightGradient2 = baseEvent.CustomLightGradient;
						Color endColor = baseEvent.CustomLightGradient.EndColor;
						Color startColor = baseEvent.CustomLightGradient.StartColor;
						customLightGradient.StartColor = endColor;
						customLightGradient2.EndColor = startColor;
					}
					if (!baseEvent.IsLightEvent())
					{
						continue;
					}
					if (moveNotes && baseEvent.IsPropagation && collectionForType.EventTypeToPropagate == baseEvent.Type && collectionForType.PropagationEditing == EventGridContainer.PropMode.Prop)
					{
						int propID = collectionForType.EventTypePropagationSize - baseEvent.CustomPropID - 1;
						baseEvent.CustomLightID = labels.PropIdToLightIds(baseEvent.Type, propID);
					}
					else if (moveNotes && baseEvent.CustomLightID != null && collectionForType.EventTypeToPropagate == baseEvent.Type && collectionForType.PropagationEditing == EventGridContainer.PropMode.Light)
					{
						int num4 = labels.LightIDToEditor(baseEvent.Type, baseEvent.CustomLightID[0]);
						int lightID = collectionForType.EventTypePropagationSize - num4 - 1;
						baseEvent.CustomLightID = new int[1] { labels.EditorToLightID(baseEvent.Type, lightID) };
					}
					if (moveNotes)
					{
						if (baseEvent.Value > 0 && baseEvent.Value <= 4)
						{
							baseEvent.Value += 4;
						}
						else if (baseEvent.Value > 4 && baseEvent.Value <= 8)
						{
							baseEvent.Value -= 4;
						}
					}
					else if (baseEvent.Value > 0 && baseEvent.Value <= 4)
					{
						baseEvent.Value += 4;
					}
					else if (baseEvent.Value > 4 && baseEvent.Value <= 8)
					{
						baseEvent.Value += 4;
					}
					else if (baseEvent.Value > 8 && baseEvent.Value <= 12)
					{
						baseEvent.Value -= 8;
					}
				}
			}
			else if (baseObject is BaseArc baseArc)
			{
				if (moveNotes)
				{
					if (baseArc.CustomCoordinate != null && baseArc.CustomCoordinate.IsArray)
					{
						Vector2 vector5 = baseArc.CustomCoordinate.ReadVector2();
						Vector2 vector6 = new Vector2((vector5.x + 0.5f) * -1f - 0.5f, vector5.y);
						baseArc.CustomCoordinate = vector6;
					}
					if (baseArc.CustomTailCoordinate != null && baseArc.CustomTailCoordinate.IsArray)
					{
						Vector2 vector7 = baseArc.CustomTailCoordinate.ReadVector2();
						Vector2 vector8 = new Vector2((vector7.x + 0.5f) * -1f - 0.5f, vector7.y);
						baseArc.CustomTailCoordinate = vector8;
					}
					baseArc.PosX = Mathf.RoundToInt(((float)baseArc.PosX - 1.5f) * -1f + 1.5f);
					if (cutDirectionToMirrored.ContainsKey(baseArc.CutDirection))
					{
						baseArc.CutDirection = cutDirectionToMirrored[baseArc.CutDirection];
					}
					baseArc.TailPosX = Mathf.RoundToInt(((float)baseArc.TailPosX - 1.5f) * -1f + 1.5f);
					if (cutDirectionToMirrored.ContainsKey(baseArc.TailCutDirection))
					{
						baseArc.TailCutDirection = cutDirectionToMirrored[baseArc.TailCutDirection];
					}
					if (baseArc.MidAnchorMode > 0 && baseArc.MidAnchorMode < 3)
					{
						baseArc.MidAnchorMode = ((baseArc.MidAnchorMode != 1) ? 1 : 2);
					}
				}
				baseArc.Color = ((baseArc.Color == 0) ? 1 : 0);
			}
			else if (baseObject is BaseChain baseChain)
			{
				if (moveNotes)
				{
					if (baseChain.CustomCoordinate != null && baseChain.CustomCoordinate.IsArray)
					{
						Vector2 vector9 = baseChain.CustomCoordinate.ReadVector2();
						Vector2 vector10 = new Vector2((vector9.x + 0.5f) * -1f - 0.5f, vector9.y);
						baseChain.CustomCoordinate = vector10;
					}
					if (baseChain.CustomTailCoordinate != null && baseChain.CustomTailCoordinate.IsArray)
					{
						Vector2 vector11 = baseChain.CustomTailCoordinate.ReadVector2();
						Vector2 vector12 = new Vector2((vector11.x + 0.5f) * -1f - 0.5f, vector11.y);
						baseChain.CustomTailCoordinate = vector12;
					}
					baseChain.PosX = Mathf.RoundToInt(((float)baseChain.PosX - 1.5f) * -1f + 1.5f);
					if (cutDirectionToMirrored.ContainsKey(baseChain.CutDirection))
					{
						baseChain.CutDirection = cutDirectionToMirrored[baseChain.CutDirection];
					}
					baseChain.TailPosX = Mathf.RoundToInt(((float)baseChain.TailPosX - 1.5f) * -1f + 1.5f);
				}
				baseChain.Color = ((baseChain.Color == 0) ? 1 : 0);
			}
			baseObject.SaveCustom();
			list2.Add(baseObject);
			list.Add(selectedObject);
		}
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedCollectionAction(list2, list, "Mirrored a selection of objects."), perform: true);
	}
}
