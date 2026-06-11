using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Containers;
using SimpleJSON;
using TMPro;
using UnityEngine;

public class CreateEventTypeLabels : MonoBehaviour
{
	private static readonly int[] modifiedToEventArray = new int[16]
	{
		14, 15, 0, 1, 2, 3, 4, 8, 9, 12,
		13, 5, 6, 7, 10, 11
	};

	private static readonly int[] eventToModifiedArray = new int[16]
	{
		2, 3, 4, 5, 6, 11, 12, 13, 7, 8,
		14, 15, 9, 10, 0, 1
	};

	private static readonly int[] eventToModifiedArrayInterscope = new int[18]
	{
		5, 2, 4, 3, 6, 13, 7, 8, 9, 10,
		16, 17, 11, 12, 0, 1, 14, 15
	};

	private static readonly int[] eventToModifiedArrayGaga = new int[20]
	{
		9, 10, 5, 6, 2, 11, 4, 7, 18, 19,
		3, 8, 14, 15, 0, 1, 13, 16, 12, 17
	};

	public TMP_FontAsset AvailableAsset;

	public TMP_FontAsset UtilityAsset;

	public TMP_FontAsset RedAsset;

	public GameObject LayerInstantiate;

	public Transform[] EventGrid;

	[SerializeField]
	private DarkThemeSO darkTheme;

	public RotationCallbackController RotationCallback;

	private readonly List<LaneInfo> laneObjs = new List<LaneInfo>();

	private LightsManager[] lightingManagers;

	private bool loadedWithRotationEvents;

	[HideInInspector]
	public int NoRotationLaneOffset
	{
		get
		{
			if (!loadedWithRotationEvents && !RotationCallback.IsActive)
			{
				return -2;
			}
			return 0;
		}
	}

	private void Start()
	{
		loadedWithRotationEvents = BeatSaberSongContainer.Instance.Map.Events.Any((BaseEvent i) => i.IsLaneRotationEvent());
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	private void OnDestroy()
	{
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(PlatformLoaded));
	}

	public void UpdateLabels(EventGridContainer.PropMode propMode, int eventType, int lanes = 16)
	{
		foreach (Transform item in LayerInstantiate.transform.parent.transform)
		{
			if (item.gameObject.activeSelf)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		laneObjs.Clear();
		for (int i = 0; i < lanes; i++)
		{
			int num = ((propMode == EventGridContainer.PropMode.Off) ? EventTypeToModifiedType(i) : i) + NoRotationLaneOffset;
			if (num < 0 && propMode == EventGridContainer.PropMode.Off)
			{
				continue;
			}
			LaneInfo laneInfo = new LaneInfo(i, (propMode != EventGridContainer.PropMode.Off) ? i : num);
			GameObject gameObject = UnityEngine.Object.Instantiate(LayerInstantiate, LayerInstantiate.transform.parent);
			gameObject.SetActive(value: true);
			gameObject.transform.localPosition = new Vector3((propMode != EventGridContainer.PropMode.Off) ? i : num, 0f, 0f);
			laneObjs.Add(laneInfo);
			try
			{
				TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if (propMode != EventGridContainer.PropMode.Off)
				{
					componentInChildren.font = UtilityAsset;
					if (i == 0)
					{
						componentInChildren.text = "All Lights";
						componentInChildren.font = RedAsset;
					}
					else
					{
						componentInChildren.text = $"{lightingManagers[eventType].name} ID {EditorToLightID(eventType, i - 1)}";
						if (i % 2 == 0)
						{
							componentInChildren.font = UtilityAsset;
						}
						else
						{
							componentInChildren.font = AvailableAsset;
						}
					}
				}
				else
				{
					int environmentNameIndex = BeatSaberSongContainer.Instance.MapDifficultyInfo.EnvironmentNameIndex;
					bool flag = BeatSaberSongContainer.Instance.Info.EnvironmentNames[environmentNameIndex] == "GagaEnvironment";
					switch (i)
					{
					case 8:
						componentInChildren.font = UtilityAsset;
						componentInChildren.text = "Ring Rotation";
						break;
					case 9:
						componentInChildren.font = UtilityAsset;
						componentInChildren.text = "Ring Zoom";
						break;
					case 12:
						componentInChildren.text = ((!flag) ? "Left Laser Speed" : "Tower 3 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 13:
						componentInChildren.text = ((!flag) ? "Right Laser Speed" : "Tower 4 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 14:
						componentInChildren.text = "Rotation (Include)";
						componentInChildren.font = UtilityAsset;
						break;
					case 15:
						componentInChildren.text = "Rotation (Exclude)";
						componentInChildren.font = UtilityAsset;
						break;
					case 5:
						componentInChildren.text = "Boost Lights";
						componentInChildren.font = UtilityAsset;
						break;
					case 16:
						componentInChildren.text = ((!flag) ? "Utility Event 0" : "Tower 2 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 17:
						componentInChildren.text = ((!flag) ? "Utility Event 1" : "Tower 5 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 18:
						componentInChildren.text = ((!flag) ? "Utility Event 2" : "Tower 1 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 19:
						componentInChildren.text = ((!flag) ? "Utility Event 3" : "Tower 6 Height");
						componentInChildren.font = UtilityAsset;
						break;
					case 40:
						componentInChildren.text = "Special Event 0";
						componentInChildren.font = UtilityAsset;
						break;
					case 41:
						componentInChildren.text = "Special Event 1";
						componentInChildren.font = UtilityAsset;
						break;
					case 42:
						componentInChildren.text = "Special Event 2";
						componentInChildren.font = UtilityAsset;
						break;
					case 43:
						componentInChildren.text = "Special Event 3";
						componentInChildren.font = UtilityAsset;
						break;
					default:
						if (lightingManagers.Length > i)
						{
							LightsManager lightsManager = lightingManagers[i];
							if (lightsManager != null)
							{
								componentInChildren.text = lightsManager.name;
								componentInChildren.font = AvailableAsset;
							}
						}
						else
						{
							UnityEngine.Object.Destroy(componentInChildren);
							laneObjs.Remove(laneInfo);
						}
						break;
					}
					if (Settings.Instance.DarkTheme)
					{
						componentInChildren.font = darkTheme.TekoReplacement;
					}
				}
				laneInfo.Name = componentInChildren.text;
			}
			catch
			{
			}
		}
		laneObjs.Sort();
	}

	private void PlatformLoaded(PlatformDescriptor descriptor)
	{
		lightingManagers = descriptor.LightingManagers;
	}

	public int LaneIdToEventType(int laneId)
	{
		return laneObjs[laneId].Type;
	}

	public int EventTypeToLaneId(int eventType)
	{
		return laneObjs.FindIndex((LaneInfo it) => it.Type == eventType);
	}

	public int? LightIdsToPropId(int type, int[] lightID)
	{
		if (type >= lightingManagers.Length)
		{
			return null;
		}
		LightingEvent lightingEvent = lightingManagers[type].ControllingLights.Find((LightingEvent x) => Array.IndexOf(lightID, x.LightID) > -1);
		if (!(lightingEvent != null))
		{
			return null;
		}
		return lightingEvent.PropGroup;
	}

	public int[] PropIdToLightIds(int type, int propID)
	{
		if (type >= lightingManagers.Length)
		{
			return new int[0];
		}
		return (from x in lightingManagers[type].ControllingLights
			where x.PropGroup == propID
			select x.LightID into x
			orderby x
			select x).Distinct().ToArray();
	}

	public JSONArray PropIdToLightIdsJ(int type, int propID)
	{
		JSONArray jSONArray = new JSONArray();
		int[] array = PropIdToLightIds(type, propID);
		foreach (int num in array)
		{
			jSONArray.Add(num);
		}
		return jSONArray;
	}

	public int EditorToLightID(int type, int lightID)
	{
		return lightingManagers[type].LightIDPlacementMap[lightID];
	}

	public int LightIDToEditor(int type, int lightID)
	{
		if (lightingManagers[type].LightIDPlacementMapReverse.ContainsKey(lightID))
		{
			return lightingManagers[type].LightIDPlacementMapReverse[lightID];
		}
		return -1;
	}

	public static int EventTypeToModifiedType(int eventType)
	{
		if (EventContainer.ModifyTypeMode == -1)
		{
			return eventType;
		}
		if (EventContainer.ModifyTypeMode == 0)
		{
			if (!Enumerable.Contains(eventToModifiedArray, eventType))
			{
				Debug.LogWarning($"Event Type {eventType} does not have a modified type");
				return eventType;
			}
			return eventToModifiedArray[eventType];
		}
		if (EventContainer.ModifyTypeMode == 1)
		{
			return eventType switch
			{
				5 => 1, 
				1 => 2, 
				6 => 3, 
				2 => 4, 
				7 => 5, 
				3 => 6, 
				10 => 7, 
				4 => 8, 
				11 => 9, 
				8 => 10, 
				9 => 11, 
				_ => eventType, 
			};
		}
		if (EventContainer.ModifyTypeMode == 2)
		{
			return eventToModifiedArrayInterscope[eventType];
		}
		if (EventContainer.ModifyTypeMode == 3)
		{
			return eventToModifiedArrayGaga[eventType];
		}
		return -1;
	}

	public static int ModifiedTypeToEventType(int modifiedType)
	{
		if (EventContainer.ModifyTypeMode == -1)
		{
			return modifiedType;
		}
		if (EventContainer.ModifyTypeMode == 0)
		{
			if (!Enumerable.Contains(modifiedToEventArray, modifiedType))
			{
				Debug.LogWarning($"Event Type {modifiedType} does not have a valid event type! WTF!?!?");
				return modifiedType;
			}
			return modifiedToEventArray[modifiedType];
		}
		if (EventContainer.ModifyTypeMode == 1)
		{
			return modifiedType switch
			{
				1 => 5, 
				2 => 1, 
				3 => 6, 
				4 => 2, 
				5 => 7, 
				6 => 3, 
				7 => 10, 
				8 => 4, 
				9 => 11, 
				10 => 8, 
				11 => 9, 
				_ => modifiedType, 
			};
		}
		return -1;
	}

	public int MaxLaneId()
	{
		return laneObjs.Count - 1;
	}
}
