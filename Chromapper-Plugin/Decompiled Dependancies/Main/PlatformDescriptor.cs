using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlatformDescriptor : MonoBehaviour
{
	private class Gradient
	{
		public BaseEvent GradientEvent;

		public Coroutine Routine;
	}

	[Header("Rings")]
	[Tooltip("Leave null if you do not want small rings.")]
	public TrackLaneRingsManager SmallRingManager;

	[Tooltip("Leave null if you do not want big rings.")]
	public TrackLaneRingsManagerBase BigRingManager;

	[Tooltip("Leave null if you do not want gaga environment disks.")]
	public GagaDiskManager DiskManager;

	[Header("Lighting Groups")]
	[Tooltip("Manually map an Event ID (Index) to a group of lights (LightingManagers)")]
	public LightsManager[] LightingManagers = new LightsManager[0];

	[Tooltip("If you want a thing to rotate around a 360 level with the track, place it here.")]
	public GridRotationController RotationController;

	[FormerlySerializedAs("colors")]
	[HideInInspector]
	public PlatformColors Colors;

	[FormerlySerializedAs("defaultColors")]
	public PlatformColors DefaultColors = new PlatformColors();

	[Tooltip("-1 = No Sorting | 0 = Default Sorting | 1 = Collider Platform Special | 2 = New lanes 6/7 + 16/17 | 3 = Gaga Lanes")]
	public int SortMode;

	[Tooltip("Objects to disable through the L keybind, like lights and static objects in 360 environments.")]
	public GameObject[] DisablableObjects;

	[Tooltip("Change scale of normal map for shiny objects.")]
	public float NormalMapScale = 2f;

	private readonly Dictionary<LightsManager, Color> chromaCustomColors = new Dictionary<LightsManager, Color>();

	private readonly Dictionary<LightsManager, Gradient> chromaGradients = new Dictionary<LightsManager, Gradient>();

	private readonly Dictionary<int, List<PlatformEventHandler>> platformEventHandlers = new Dictionary<int, List<PlatformEventHandler>>();

	private AudioTimeSyncController atsc;

	private BeatmapObjectCallbackController callbackController;

	private RotationCallbackController rotationCallback;

	private static readonly int baseMap = Shader.PropertyToID("_BaseMap");

	public bool SoloAnEventType { get; private set; }

	public int SoloEventType { get; private set; }

	public bool ColorBoost { get; private set; }

	private void Awake()
	{
		if (SceneManager.GetActiveScene().name != "999_PrefabBuilding")
		{
			LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Combine(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoaded));
		}
	}

	private void Start()
	{
		PlatformEventHandler[] componentsInChildren = GetComponentsInChildren<PlatformEventHandler>();
		foreach (PlatformEventHandler platformEventHandler in componentsInChildren)
		{
			int[] listeningEventTypes = platformEventHandler.ListeningEventTypes;
			foreach (int key in listeningEventTypes)
			{
				if (!platformEventHandlers.TryGetValue(key, out var value))
				{
					value = new List<PlatformEventHandler>();
					platformEventHandlers.Add(key, value);
				}
				value.Add(platformEventHandler);
			}
		}
		UpdateShinyMaterialSettings();
	}

	private void OnDestroy()
	{
		if (callbackController != null)
		{
			BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
			beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Remove(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassed));
		}
		if (SceneManager.GetActiveScene().name != "999_PrefabBuilding")
		{
			LoadInitialMap.LevelLoadedEvent = (Action)Delegate.Remove(LoadInitialMap.LevelLoadedEvent, new Action(LevelLoaded));
		}
	}

	public void UpdateShinyMaterialSettings()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.sharedMaterial.name.Contains("Shiny Ass Black"))
			{
				Vector3 lossyScale = renderer.gameObject.transform.lossyScale;
				Vector2 value = new Vector2(lossyScale.x, lossyScale.z) / NormalMapScale;
				renderer.material.SetTextureScale(baseMap, value);
				renderer.material.SetTextureOffset(baseMap, Vector2.zero);
			}
		}
	}

	private void LevelLoaded()
	{
		callbackController = GameObject.Find("Vertical Grid Callback").GetComponent<BeatmapObjectCallbackController>();
		rotationCallback = Resources.FindObjectsOfTypeAll<RotationCallbackController>().First();
		atsc = rotationCallback.Atsc;
		if (RotationController != null)
		{
			RotationController.RotationCallback = rotationCallback;
			RotationController.Init();
		}
		BeatmapObjectCallbackController beatmapObjectCallbackController = callbackController;
		beatmapObjectCallbackController.EventPassedThreshold = (Action<bool, int, BaseObject>)Delegate.Combine(beatmapObjectCallbackController.EventPassedThreshold, new Action<bool, int, BaseObject>(EventPassed));
		RefreshLightingManagers();
		if (Settings.Instance.HideDisablableObjectsOnLoad)
		{
			ToggleDisablableObjects();
		}
	}

	public void RefreshLightingManagers()
	{
		LightsManager[] lightingManagers = LightingManagers;
		foreach (LightsManager lightsManager in lightingManagers)
		{
			if ((object)lightsManager != null)
			{
				IEnumerable<LightingEvent> controllingLights = lightsManager.ControllingLights;
				IEnumerable<LightingEvent> lights = controllingLights.Where((LightingEvent x) => !x.UseInvertedPlatformColors);
				IEnumerable<LightingEvent> lights2 = controllingLights.Where((LightingEvent x) => x.UseInvertedPlatformColors);
				lightsManager.ChangeColor(Colors.BlueColor, 0f, lights);
				lightsManager.ChangeColor(Colors.RedColor, 0f, lights2);
				lightsManager.ChangeAlpha(0f, 0f, controllingLights);
			}
		}
	}

	public void UpdateSoloEventType(bool solo, int soloTypeID)
	{
		SoloAnEventType = solo;
		SoloEventType = soloTypeID;
	}

	public void ToggleDisablableObjects()
	{
		GameObject[] disablableObjects = DisablableObjects;
		foreach (GameObject obj in disablableObjects)
		{
			obj.SetActive(!obj.activeInHierarchy);
		}
	}

	public void KillLights()
	{
		LightsManager[] lightingManagers = LightingManagers;
		foreach (LightsManager lightsManager in lightingManagers)
		{
			if (lightsManager != null)
			{
				lightsManager.ChangeAlpha(0f, 1f, lightsManager.ControllingLights);
			}
		}
	}

	public void KillChromaLights()
	{
		chromaCustomColors.Clear();
		foreach (KeyValuePair<LightsManager, Gradient> chromaGradient in chromaGradients)
		{
			StopCoroutine(chromaGradient.Value.Routine);
			chromaGradient.Key.ChangeMultiplierAlpha(1f, chromaGradient.Key.ControllingLights);
		}
		chromaGradients.Clear();
	}

	public void EventPassed(bool isPlaying, int index, BaseObject obj)
	{
		BaseEvent baseEvent = obj as BaseEvent;
		UnityEngine.Random.InitState(Mathf.RoundToInt(obj.JsonTime * 100f));
		switch (baseEvent.Type)
		{
		case 8:
			if (baseEvent.CustomNameFilter != null)
			{
				string customNameFilter = baseEvent.CustomNameFilter;
				if (customNameFilter.Contains("Big") || customNameFilter.Contains("Large"))
				{
					if (BigRingManager != null)
					{
						BigRingManager.HandleRotationEvent(baseEvent);
					}
					break;
				}
				if (customNameFilter.Contains("Small") || customNameFilter.Contains("Panels") || customNameFilter.Contains("Triangle"))
				{
					if (SmallRingManager != null)
					{
						SmallRingManager.HandleRotationEvent(baseEvent);
					}
					break;
				}
				if (BigRingManager != null)
				{
					BigRingManager.HandleRotationEvent(baseEvent);
				}
				if (SmallRingManager != null)
				{
					SmallRingManager.HandleRotationEvent(baseEvent);
				}
			}
			else
			{
				if (BigRingManager != null)
				{
					BigRingManager.HandleRotationEvent(baseEvent);
				}
				if (SmallRingManager != null)
				{
					SmallRingManager.HandleRotationEvent(baseEvent);
				}
			}
			break;
		case 9:
			if (BigRingManager != null)
			{
				BigRingManager.HandlePositionEvent(baseEvent);
			}
			if (SmallRingManager != null)
			{
				SmallRingManager.HandlePositionEvent(baseEvent);
			}
			break;
		case 12:
			if (HandleGagaHeightEvent(baseEvent))
			{
				return;
			}
			foreach (int item in new List<int> { 2, 10, 6 }.Where((int eventType) => LightingManagers.Length >= eventType))
			{
				foreach (RotatingLightsBase rotatingLight in LightingManagers[item].RotatingLights)
				{
					rotatingLight.UpdateOffset(isLeftEvent: true, baseEvent);
				}
			}
			break;
		case 13:
			if (HandleGagaHeightEvent(baseEvent))
			{
				return;
			}
			foreach (int item2 in new List<int> { 3, 11, 7 }.Where((int eventType) => LightingManagers.Length >= eventType))
			{
				foreach (RotatingLightsBase rotatingLight2 in LightingManagers[item2].RotatingLights)
				{
					rotatingLight2.UpdateOffset(isLeftEvent: true, baseEvent);
				}
			}
			break;
		case 5:
		{
			ColorBoost = baseEvent.Value == 1;
			LightsManager[] lightingManagers = LightingManagers;
			foreach (LightsManager lightsManager in lightingManagers)
			{
				if (!(lightsManager == null))
				{
					lightsManager.Boost(ColorBoost, ColorBoost ? Colors.RedBoostColor : Colors.RedColor, ColorBoost ? Colors.BlueBoostColor : Colors.BlueColor, ColorBoost ? Colors.WhiteBoostColor : Colors.WhiteColor);
				}
			}
			break;
		}
		case 16:
		case 17:
		case 18:
		case 19:
			if (HandleGagaHeightEvent(baseEvent))
			{
				return;
			}
			break;
		default:
			if (baseEvent.Type < LightingManagers.Length && LightingManagers[baseEvent.Type] != null)
			{
				HandleLights(LightingManagers[baseEvent.Type], baseEvent.Value, baseEvent);
			}
			break;
		}
		if (!(atsc != null) || !atsc.IsPlaying || !platformEventHandlers.TryGetValue(baseEvent.Type, out var value))
		{
			return;
		}
		foreach (PlatformEventHandler item3 in value)
		{
			item3.OnEventTrigger(baseEvent.Type, baseEvent);
		}
	}

	private void HandleLights(LightsManager group, int value, BaseEvent e)
	{
		Color color = Color.white;
		Color color2 = Color.white;
		if ((object)group == null)
		{
			return;
		}
		if (value >= 2000000000 && Settings.Instance.EmulateChromaLite)
		{
			if (chromaCustomColors.ContainsKey(group))
			{
				chromaCustomColors[group] = ColourManager.ColourFromInt(value);
			}
			else
			{
				chromaCustomColors.Add(group, ColourManager.ColourFromInt(value));
			}
			return;
		}
		if (value == 1900000001 && Settings.Instance.EmulateChromaLite && chromaCustomColors.ContainsKey(group))
		{
			chromaCustomColors.Remove(group);
		}
		if (chromaGradients.ContainsKey(group))
		{
			BaseEvent gradientEvent = chromaGradients[group].GradientEvent;
			if (atsc.CurrentJsonTime >= gradientEvent.CustomLightGradient.Duration + gradientEvent.JsonTime || !Settings.Instance.EmulateChromaLite)
			{
				StopCoroutine(chromaGradients[group].Routine);
				chromaGradients.Remove(group);
				chromaCustomColors.Remove(group);
			}
		}
		if (e.CustomLightGradient != null && Settings.Instance.EmulateChromaLite)
		{
			if (chromaGradients.ContainsKey(group))
			{
				StopCoroutine(chromaGradients[group].Routine);
				chromaGradients.Remove(group);
			}
			Gradient gradient = new Gradient
			{
				GradientEvent = e,
				Routine = StartCoroutine(GradientRoutine(e, group))
			};
			if (gradient.Routine != null)
			{
				chromaGradients.Add(group, gradient);
			}
		}
		if (value <= 4)
		{
			color = (ColorBoost ? Colors.BlueBoostColor : Colors.BlueColor);
			color2 = (ColorBoost ? Colors.RedBoostColor : Colors.RedColor);
		}
		else if (value <= 8)
		{
			color = (ColorBoost ? Colors.RedBoostColor : Colors.RedColor);
			color2 = (ColorBoost ? Colors.BlueBoostColor : Colors.BlueColor);
		}
		else if (value <= 12)
		{
			color = (color2 = (ColorBoost ? Colors.WhiteBoostColor : Colors.WhiteColor));
		}
		if (e.CustomColor.HasValue && Settings.Instance.EmulateChromaLite && !e.IsWhite)
		{
			color = (color2 = e.CustomColor.Value);
			chromaCustomColors.Remove(group);
			if (chromaGradients.ContainsKey(group))
			{
				StopCoroutine(chromaGradients[group].Routine);
				chromaGradients.Remove(group);
			}
		}
		if (chromaCustomColors.ContainsKey(group) && Settings.Instance.EmulateChromaLite)
		{
			color = (color2 = chromaCustomColors[group]);
			group.ChangeMultiplierAlpha(color.a, group.ControllingLights);
		}
		if (SoloAnEventType && e.Type != SoloEventType)
		{
			color = (color2 = Color.black.WithAlpha(0f));
		}
		IEnumerable<LightingEvent> enumerable = group.ControllingLights;
		if (e.CustomLightID != null && Settings.Instance.EmulateChromaAdvanced)
		{
			int[] customLightID = e.CustomLightID;
			List<LightingEvent> list = new List<LightingEvent>(customLightID.Length);
			int[] array = customLightID;
			foreach (int key in array)
			{
				if (group.LightIDMap != null && group.LightIDMap.TryGetValue(key, out var value2) && (object)value2 != null)
				{
					list.Add(value2);
				}
			}
			enumerable = list;
		}
		foreach (LightingEvent item in enumerable)
		{
			Color color3 = (item.UseInvertedPlatformColors ? color2 : color);
			float floatValue = e.FloatValue;
			item.UpdateMultiplyAlpha();
			switch (value)
			{
			case 0:
				if (item.CanBeTurnedOff)
				{
					item.UpdateTargetAlpha(0f, 0f);
				}
				else
				{
					item.UpdateTargetAlpha(color3.a * floatValue * (2f / 3f), 0f);
				}
				TrySetTransition(item, e);
				break;
			case 1:
			case 4:
			case 5:
			case 8:
			case 9:
			case 12:
				item.UpdateTargetColor(color3.Multiply(LightsManager.HDRIntensity), 0f);
				item.UpdateTargetAlpha(color3.a * floatValue, 0f);
				item.UpdateEasing(Easing.Linear);
				TrySetTransition(item, e);
				break;
			case 2:
			case 6:
			case 10:
				item.UpdateTargetAlpha(color3.a * floatValue, 0f);
				item.UpdateTargetColor(color3.Multiply(LightsManager.HDRFlashIntensity), 0f);
				item.UpdateTargetColor(color3.Multiply(LightsManager.HDRIntensity), LightsManager.FlashTime);
				item.UpdateEasing(Easing.Cubic.Out);
				break;
			case 3:
			case 7:
			case 11:
				item.UpdateTargetAlpha(color3.a * floatValue, 0f);
				item.UpdateTargetColor(color3.Multiply(LightsManager.HDRFlashIntensity), 0f);
				item.UpdateEasing(Easing.Exponential.Out);
				if (item.CanBeTurnedOff)
				{
					item.UpdateTargetAlpha(0f, LightsManager.FadeTime);
					item.UpdateTargetColor(Color.black, LightsManager.FadeTime);
				}
				else
				{
					item.UpdateTargetColor(color3.Multiply(LightsManager.HDRIntensity), LightsManager.FadeTime);
				}
				break;
			}
		}
		group.SetValue(value);
	}

	private bool TryGetNextTransitionNote(in BaseEvent e, out BaseEvent transitionEvent)
	{
		transitionEvent = null;
		BaseEvent next = e.Next;
		if (next != null && next.IsTransition)
		{
			transitionEvent = e.Next;
			return true;
		}
		return false;
	}

	private Color InferColorFromValue(bool useInvertedPlatformColors, int value)
	{
		if (value <= 4)
		{
			if (!useInvertedPlatformColors)
			{
				if (!ColorBoost)
				{
					return Colors.BlueColor;
				}
				return Colors.BlueBoostColor;
			}
			if (!ColorBoost)
			{
				return Colors.RedColor;
			}
			return Colors.RedBoostColor;
		}
		if (value <= 8)
		{
			if (!useInvertedPlatformColors)
			{
				if (!ColorBoost)
				{
					return Colors.RedColor;
				}
				return Colors.RedBoostColor;
			}
			if (!ColorBoost)
			{
				return Colors.BlueColor;
			}
			return Colors.BlueBoostColor;
		}
		if (value <= 12)
		{
			if (!ColorBoost)
			{
				return Colors.WhiteColor;
			}
			return Colors.WhiteBoostColor;
		}
		return Color.white;
	}

	private void TrySetTransition(LightingEvent light, BaseEvent e)
	{
		if (TryGetNextTransitionNote(in e, out var transitionEvent))
		{
			Color? color = transitionEvent.CustomColor;
			if (e.IsWhite)
			{
				color = null;
			}
			Color color2 = color ?? InferColorFromValue(light.UseInvertedPlatformColors, transitionEvent.Value);
			float num = transitionEvent.FloatValue;
			if (color.HasValue)
			{
				num *= color.Value.a;
			}
			float secondsFromBeat = atsc.GetSecondsFromBeat(transitionEvent.SongBpmTime - e.SongBpmTime);
			if (e.IsOff)
			{
				light.UpdateTargetAlpha(0f, 0f);
			}
			light.UpdateTargetColor(color2.Multiply(LightsManager.HDRIntensity), secondsFromBeat);
			light.UpdateTargetAlpha(num, secondsFromBeat);
			light.UpdateEasing(e.CustomEasing ?? "easeLinear");
		}
	}

	private IEnumerator GradientRoutine(BaseEvent gradientEvent, LightsManager group)
	{
		ChromaLightGradient gradient = gradientEvent.CustomLightGradient;
		Func<float, float> easingFunc = Easing.ByName[gradient.EasingType];
		float arg;
		while ((arg = (atsc.CurrentJsonTime - gradientEvent.JsonTime) / gradient.Duration) < 1f)
		{
			Color color = Color.LerpUnclamped(gradient.StartColor, gradient.EndColor, easingFunc(arg));
			if (!SoloAnEventType || gradientEvent.Type == SoloEventType)
			{
				chromaCustomColors[group] = color;
				group.ChangeColor(color.WithAlpha(1f), 0f, group.ControllingLights);
				group.ChangeMultiplierAlpha(color.a, group.ControllingLights);
			}
			yield return new WaitForEndOfFrame();
		}
		chromaCustomColors[group] = gradient.EndColor;
		group.ChangeColor(chromaCustomColors[group].WithAlpha(1f), 0f, group.ControllingLights);
		group.ChangeMultiplierAlpha(chromaCustomColors[group].a, group.ControllingLights);
	}

	private bool HandleGagaHeightEvent(BaseEvent evt)
	{
		if (DiskManager != null)
		{
			DiskManager.HandlePositionEvent(evt);
			return true;
		}
		return false;
	}
}
