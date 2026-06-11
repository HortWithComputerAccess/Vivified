using System;
using System.Collections.Generic;
using System.Linq;
using CustomFloorPlugin;
using UnityEngine;

public class CustomPlatformsLoader : MonoBehaviour
{
	private static CustomPlatformsLoader instance;

	private readonly CustomPlatformSettings customPlatformSettings = CustomPlatformSettings.Instance;

	private readonly List<string> environmentsOnly = new List<string>();

	private readonly List<string> platformsOnly = new List<string>();

	private Material lightMaterial;

	private PlatformDescriptor platformDescriptor;

	private Material useThisBlack;

	private static readonly int baseColor = Shader.PropertyToID("_BaseColor");

	private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");

	public static CustomPlatformsLoader Instance
	{
		get
		{
			if (!(instance != null))
			{
				return instance = Load();
			}
			return instance;
		}
	}

	private void Awake()
	{
		lightMaterial = Resources.Load("ControllableLight", typeof(Material)) as Material;
		useThisBlack = new Material(Resources.Load("Basic Black", typeof(Material)) as Material);
	}

	public void Init()
	{
	}

	private static CustomPlatformsLoader Load()
	{
		CustomPlatformsLoader customPlatformsLoader = new GameObject("Custom Platforms Loader").AddComponent<CustomPlatformsLoader>();
		UnityEngine.Object.DontDestroyOnLoad(customPlatformsLoader.gameObject);
		foreach (string allEnvironmentId in customPlatformsLoader.GetAllEnvironmentIds())
		{
			GameObject gameObject = customPlatformsLoader.LoadPlatform(allEnvironmentId);
			CustomPlatform customPlatform = customPlatformsLoader.FindCustomPlatformScript(gameObject);
			if (customPlatform != null)
			{
				if (!customPlatform.hideHighway && !customPlatform.hideTowers && customPlatform.hideDefaultPlatform && !customPlatform.hideEQVisualizer && !customPlatform.hideSmallRings && !customPlatform.hideBigRings && !customPlatform.hideBackColumns && !customPlatform.hideBackLasers && !customPlatform.hideDoubleLasers && !customPlatform.hideDoubleColorLasers && !customPlatform.hideRotatingLasers && !customPlatform.hideTrackLights)
				{
					customPlatformsLoader.platformsOnly.Add(allEnvironmentId);
				}
				else
				{
					customPlatformsLoader.environmentsOnly.Add(allEnvironmentId);
				}
			}
			UnityEngine.Object.DestroyImmediate(gameObject, allowDestroyingAssets: true);
		}
		return customPlatformsLoader;
	}

	public GameObject LoadPlatform(string customEnvironmentString, GameObject defaultEnvironment = null, string customPlatformString = null)
	{
		try
		{
			GameObject gameObject = null;
			if (defaultEnvironment != null)
			{
				gameObject = UnityEngine.Object.Instantiate(defaultEnvironment, LoadInitialMap.PlatformOffset, Quaternion.identity);
				platformDescriptor = gameObject.GetComponentInParent<PlatformDescriptor>();
			}
			GameObject[] array = customPlatformSettings.LoadPlatform(customEnvironmentString);
			GameObject gameObject2 = null;
			CustomPlatform customPlatform = null;
			GameObject[] array2 = array;
			foreach (GameObject gameObject3 in array2)
			{
				customPlatform = FindCustomPlatformScript(gameObject3);
				if (customPlatform != null)
				{
					gameObject2 = gameObject3;
					RemoveHiddenElementsFromEnvironment(gameObject, customPlatform);
					break;
				}
			}
			GameObject[] array3 = null;
			GameObject gameObject4 = null;
			if (customPlatformString != null)
			{
				array3 = customPlatformSettings.LoadPlatform(customPlatformString);
				CustomPlatform customPlatform2 = null;
				array2 = array3;
				foreach (GameObject gameObject5 in array2)
				{
					customPlatform2 = FindCustomPlatformScript(gameObject5);
					if (customPlatform2 != null)
					{
						gameObject4 = gameObject5;
						RemoveHiddenElementsFromEnvironment(gameObject, customPlatform2);
						break;
					}
				}
				GameObject gameObject6 = UnityEngine.Object.Instantiate(gameObject4, LoadInitialMap.PlatformOffset, Quaternion.identity);
				DisableElementsFromEnvironmentRecursive(gameObject6, "Camera");
				gameObject6.transform.SetParent(gameObject.transform);
				array2 = array3;
				foreach (GameObject gameObject7 in array2)
				{
					if (gameObject7 != gameObject4)
					{
						UnityEngine.Object.Instantiate(gameObject7, LoadInitialMap.PlatformOffset, Quaternion.identity).transform.SetParent(gameObject6.transform);
					}
				}
			}
			if (defaultEnvironment != null)
			{
				GameObject gameObject8 = UnityEngine.Object.Instantiate(gameObject2, LoadInitialMap.PlatformOffset, Quaternion.identity);
				DisableElementsFromEnvironmentRecursive(gameObject8, "Camera");
				gameObject8.transform.SetParent(gameObject.transform);
				array2 = array;
				foreach (GameObject gameObject9 in array2)
				{
					if (gameObject9 != gameObject2)
					{
						UnityEngine.Object.Instantiate(gameObject9, LoadInitialMap.PlatformOffset, Quaternion.identity).transform.SetParent(gameObject8.transform);
					}
				}
				ReplaceBetterBlack(gameObject);
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer shadersCorrectly in componentsInChildren)
				{
					SetShadersCorrectly(shadersCorrectly);
				}
				SetLightsManagerSize(gameObject);
				platformDescriptor.RefreshLightingManagers();
				int num = 0;
				TrackRings[] componentsInChildren2 = gameObject.GetComponentsInChildren<TrackRings>();
				foreach (TrackRings trackRings in componentsInChildren2)
				{
					SetRings(trackRings.gameObject, trackRings, num);
					num++;
				}
				SetLightingEventsForTubeLights(gameObject);
				return gameObject;
			}
			return gameObject2;
		}
		catch
		{
			return UnityEngine.Object.Instantiate(defaultEnvironment, LoadInitialMap.PlatformOffset, Quaternion.identity);
		}
	}

	private void SetLightsManagerSize(GameObject gameObject)
	{
		TubeLight[] componentsInChildren = gameObject.GetComponentsInChildren<TubeLight>();
		int num = platformDescriptor.LightingManagers.Length;
		TubeLight[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i].lightsID)
			{
			case TubeLight.LightsID.Unused5:
				num = Math.Max(num, 6);
				break;
			case TubeLight.LightsID.Unused6:
				num = Math.Max(num, 7);
				break;
			case TubeLight.LightsID.Unused7:
				num = Math.Max(num, 8);
				break;
			case TubeLight.LightsID.Unused10:
				num = Math.Max(num, 11);
				break;
			case TubeLight.LightsID.Unused11:
				num = Math.Max(num, 12);
				break;
			}
		}
		if (num != platformDescriptor.LightingManagers.Length)
		{
			Array.Resize(ref platformDescriptor.LightingManagers, num);
		}
	}

	private void SetLightingEventsForTubeLights(GameObject gameObject)
	{
		TubeLight[] componentsInChildren = gameObject.GetComponentsInChildren<TubeLight>();
		foreach (TubeLight tubeLight in componentsInChildren)
		{
			if (tubeLight.gameObject.GetComponent<LightingEvent>() != null)
			{
				continue;
			}
			int num = 0;
			switch (tubeLight.lightsID)
			{
			case TubeLight.LightsID.Static:
				num = 0;
				break;
			case TubeLight.LightsID.BackLights:
				num = 0;
				break;
			case TubeLight.LightsID.BigRingLights:
				num = 1;
				break;
			case TubeLight.LightsID.LeftLasers:
				num = 2;
				break;
			case TubeLight.LightsID.RightLasers:
				num = 3;
				break;
			case TubeLight.LightsID.TrackAndBottom:
				num = 4;
				break;
			case TubeLight.LightsID.Unused5:
				num = 5;
				break;
			case TubeLight.LightsID.Unused6:
				num = 6;
				break;
			case TubeLight.LightsID.Unused7:
				num = 7;
				break;
			case TubeLight.LightsID.Unused10:
				num = 10;
				break;
			case TubeLight.LightsID.Unused11:
				num = 11;
				break;
			default:
				Debug.Log("Custom LightsID " + tubeLight.lightsID);
				break;
			case TubeLight.LightsID.RingsRotationEffect:
			case TubeLight.LightsID.RingsStepEffect:
			case TubeLight.LightsID.RingSpeedLeft:
			case TubeLight.LightsID.RingSpeedRight:
				break;
			}
			LightsManager lightsManager = platformDescriptor.LightingManagers[num];
			if (lightsManager == null)
			{
				lightsManager = tubeLight.transform.parent.gameObject.AddComponent<LightsManager>();
				lightsManager.DisableCustomInitialization = true;
				platformDescriptor.LightingManagers[num] = lightsManager;
			}
			MeshRenderer[] componentsInChildren2 = tubeLight.gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in componentsInChildren2)
			{
				SetRendererMaterials(renderer, lightsManager, tubeLight.width);
			}
			if (tubeLight.gameObject.GetComponent<MeshFilter>() != null && tubeLight.gameObject.GetComponent<MeshFilter>().sharedMesh == null)
			{
				Mesh mesh = new Mesh
				{
					name = "ScriptGenerated"
				};
				Vector3[] array = new Vector3[8]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(tubeLight.width, 0f, 0f),
					new Vector3(tubeLight.width, tubeLight.length, 0f),
					new Vector3(0f, tubeLight.length, 0f),
					new Vector3(0f, tubeLight.length, tubeLight.width),
					new Vector3(tubeLight.width, tubeLight.length, tubeLight.width),
					new Vector3(tubeLight.width, 0f, tubeLight.width),
					new Vector3(0f, 0f, tubeLight.width)
				};
				int[] triangles = new int[36]
				{
					0, 2, 1, 0, 3, 2, 2, 3, 4, 2,
					4, 5, 1, 2, 5, 1, 5, 6, 0, 7,
					4, 0, 4, 3, 5, 4, 7, 5, 7, 6,
					0, 6, 7, 0, 1, 6
				};
				mesh.vertices = array;
				mesh.triangles = triangles;
				Color[] array2 = new Color[array.Length];
				for (int k = 0; k < array.Length; k++)
				{
					array2[k] = tubeLight.color;
				}
				mesh.colors = array2;
				Vector3 vector = tubeLight.transform.position - tubeLight.transform.TransformPoint(mesh.bounds.center);
				tubeLight.transform.position = tubeLight.transform.position + vector;
				tubeLight.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			}
		}
		SongEventHandler[] componentsInChildren3 = gameObject.GetComponentsInChildren<SongEventHandler>();
		foreach (SongEventHandler songEventHandler in componentsInChildren3)
		{
			if (!(songEventHandler.gameObject.GetComponent<LightingEvent>() != null))
			{
				int eventType = (int)songEventHandler.eventType;
				LightsManager lightsManager2 = platformDescriptor.LightingManagers[eventType];
				if (lightsManager2 == null)
				{
					lightsManager2 = songEventHandler.transform.parent.gameObject.AddComponent<LightsManager>();
					lightsManager2.DisableCustomInitialization = true;
					platformDescriptor.LightingManagers[eventType] = lightsManager2;
				}
				Renderer[] componentsInChildren4 = songEventHandler.gameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer2 in componentsInChildren4)
				{
					SetRendererMaterials(renderer2, lightsManager2);
				}
			}
		}
	}

	private void SetShadersCorrectly(Renderer renderer)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		if (sharedMaterials.Length >= 1)
		{
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if (!(material == null) && !(material.shader == null))
				{
					if (material.shader.name.Contains("BeatSaber/Standard") || material.shader.name.Equals("Standard"))
					{
						material.shader = Shader.Find("Universal Render Pipeline/Simple Lit");
					}
					if (material.name.ToUpper().Contains("GLOW_BLUE"))
					{
						material = new Material(lightMaterial);
						material.SetColor(baseColor, Color.white);
						material.EnableKeyword("_EMISSION");
						material.SetColor(emissionColor, DefaultColors.Right * LightsManager.HDRIntensity);
					}
					if (material.name.ToUpper().Contains("GLOW_RED"))
					{
						material = new Material(lightMaterial);
						material.SetColor(baseColor, Color.white);
						material.EnableKeyword("_EMISSION");
						material.SetColor(emissionColor, DefaultColors.Left * LightsManager.HDRIntensity);
					}
					sharedMaterials[i] = material;
				}
			}
		}
		renderer.sharedMaterials = sharedMaterials;
	}

	private void SetRendererMaterials(Renderer renderer, LightsManager lightsManager = null, float width = 1f)
	{
		Material[] array = renderer.sharedMaterials;
		if (array.Length < 1 || !(array[0] != null))
		{
			array = new Material[1]
			{
				new Material(lightMaterial)
			};
		}
		else
		{
			if (array[0] != null && width >= 0.5f)
			{
				Array.Resize(ref array, array.Length + 1);
			}
			Material material = new Material(lightMaterial);
			for (int i = 0; i < array.Length; i++)
			{
				Material material2 = array[i];
				if (!(material2 == null))
				{
					_ = material2.color;
					if (material2.shader.name.Equals("Unlit/Color") && (material2.color.r != 1f || material2.color.g != 1f || material2.color.b != 1f))
					{
						material2 = useThisBlack;
					}
					array[i] = material;
					material = material2;
				}
			}
		}
		renderer.sharedMaterials = array;
		if (lightsManager != null)
		{
			LightingEvent item = renderer.gameObject.AddComponent<LightingEvent>();
			lightsManager.ControllingLights.Add(item);
		}
	}

	private void RemoveHiddenElementsFromEnvironment(GameObject environment, CustomPlatform customPlatform)
	{
		if (customPlatform.hideHighway)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Floor", new List<string> { "PlayersPlace" });
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Legs");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Top");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Platform");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Mirror");
		}
		if (customPlatform.hideTowers)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "RocketCar");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "RocketCar (1)");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Pillars Object");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "BG");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "RocketArena");
		}
		if (customPlatform.hideDefaultPlatform)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "PlayersPlace");
		}
		_ = customPlatform.hideEQVisualizer;
		if (customPlatform.hideSmallRings)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Small Rings");
		}
		if (customPlatform.hideBigRings)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Big Ring Lights");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Big Rings");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Platform Rings");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Big Ring Neons");
		}
		_ = customPlatform.hideBackColumns;
		if (customPlatform.hideBackLasers)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Back Lights");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Back Lasers");
		}
		_ = customPlatform.hideDoubleLasers;
		_ = customPlatform.hideDoubleColorLasers;
		if (customPlatform.hideRotatingLasers)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Left Rotating Lasers");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Right Rotating Lasers");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Left Rotating Lights");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Right Rotating Lights");
		}
		if (customPlatform.hideTrackLights)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Center Lights");
			RemoveHiddenElementsFromEnvironmentRecursive(environment, "Road Lights");
		}
	}

	private void ReplaceBetterBlack(GameObject gameObject)
	{
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			if (sharedMaterials == null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (sharedMaterials[j] != null && sharedMaterials[j].name != null && (sharedMaterials[j].name.StartsWith("BetterBlack") || sharedMaterials[j].name.StartsWith("_dark_replace")))
				{
					sharedMaterials[j] = useThisBlack;
					flag = true;
				}
			}
			if (flag)
			{
				renderer.gameObject.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
			}
		}
	}

	private void RemoveHiddenElementsFromEnvironmentRecursive(GameObject gameObject, string name, List<string> keepIfChildren = null)
	{
		if (gameObject == null)
		{
			return;
		}
		if (gameObject.name.Equals(name))
		{
			if (keepIfChildren != null)
			{
				bool flag = false;
				foreach (Transform item in gameObject.transform)
				{
					if (keepIfChildren.Contains(item.gameObject.name))
					{
						flag = true;
					}
				}
				if (flag)
				{
					foreach (Transform item2 in gameObject.transform)
					{
						if (!keepIfChildren.Contains(item2.gameObject.name))
						{
							HideGameObjectRecursive(item2.gameObject);
						}
					}
					return;
				}
				HideGameObjectRecursive(gameObject);
			}
			else
			{
				HideGameObjectRecursive(gameObject);
			}
			return;
		}
		foreach (Transform item3 in gameObject.transform)
		{
			RemoveHiddenElementsFromEnvironmentRecursive(item3.gameObject, name);
		}
	}

	private void DisableElementsFromEnvironmentRecursive(GameObject gameObject, string name)
	{
		if (gameObject == null)
		{
			return;
		}
		if (gameObject.name.Equals(name))
		{
			gameObject.SetActive(value: false);
			return;
		}
		foreach (Transform item in gameObject.transform)
		{
			DisableElementsFromEnvironmentRecursive(item.gameObject, name);
		}
	}

	private void HideGameObjectRecursive(GameObject gameObject)
	{
		Renderer component = gameObject.GetComponent<Renderer>();
		if (component != null)
		{
			component.enabled = false;
			return;
		}
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	public Dictionary<string, PlatformInfo> GetAllEnvironments()
	{
		return customPlatformSettings.CustomPlatformsDictionary;
	}

	public List<string> GetAllEnvironmentIds()
	{
		return CustomPlatformSettings.Instance.CustomPlatformsDictionary.Keys.ToList();
	}

	public int GetEnvironmentIdByPlatform(string platform)
	{
		return CustomPlatformSettings.Instance.CustomPlatformsDictionary.Keys.ToList().IndexOf(platform);
	}

	private CustomPlatform FindCustomPlatformScript(GameObject prefab)
	{
		return prefab?.GetComponentInChildren<CustomPlatform>();
	}

	private void SetRings(GameObject gameObject, TrackRings trackRings, int ringCount)
	{
		TrackLaneRingsManager trackLaneRingsManager;
		if (gameObject.name.ToLower().Contains("big") || gameObject.name.ToLower().Contains("outer") || gameObject.name.ToLower().Equals("rings"))
		{
			if (platformDescriptor.BigRingManager != null)
			{
				UnityEngine.Object[] toDestroy = platformDescriptor.BigRingManager.GetToDestroy();
				for (int i = 0; i < toDestroy.Length; i++)
				{
					UnityEngine.Object.Destroy(toDestroy[i]);
				}
			}
			platformDescriptor.BigRingManager = gameObject.AddComponent<TrackLaneRingsManager>();
			if (platformDescriptor.RotationController == null)
			{
				platformDescriptor.RotationController = gameObject.AddComponent<GridRotationController>();
			}
			trackLaneRingsManager = ((!(platformDescriptor.BigRingManager is TrackLaneRingsManager trackLaneRingsManager2)) ? null : trackLaneRingsManager2);
		}
		else
		{
			if (platformDescriptor.SmallRingManager != null)
			{
				UnityEngine.Object.Destroy(platformDescriptor.SmallRingManager.RotationEffect);
				UnityEngine.Object.Destroy(platformDescriptor.SmallRingManager);
			}
			platformDescriptor.SmallRingManager = gameObject.AddComponent<TrackLaneRingsManager>();
			if (platformDescriptor.RotationController == null)
			{
				platformDescriptor.RotationController = gameObject.AddComponent<GridRotationController>();
			}
			trackLaneRingsManager = platformDescriptor.SmallRingManager;
		}
		if (trackLaneRingsManager == null)
		{
			return;
		}
		TubeLight[] componentsInChildren = trackRings.trackLaneRingPrefab.GetComponentsInChildren<TubeLight>();
		TubeLight[] array = componentsInChildren;
		foreach (TubeLight obj in array)
		{
			int num = -1;
			switch (obj.lightsID)
			{
			case TubeLight.LightsID.Static:
				num = 0;
				break;
			case TubeLight.LightsID.BackLights:
				num = 0;
				break;
			case TubeLight.LightsID.BigRingLights:
				num = 1;
				break;
			case TubeLight.LightsID.LeftLasers:
				num = 2;
				break;
			case TubeLight.LightsID.RightLasers:
				num = 3;
				break;
			case TubeLight.LightsID.TrackAndBottom:
				num = 4;
				break;
			case TubeLight.LightsID.Unused5:
				num = 5;
				break;
			case TubeLight.LightsID.Unused6:
				num = 6;
				break;
			case TubeLight.LightsID.Unused7:
				num = 7;
				break;
			case TubeLight.LightsID.Unused10:
				num = 10;
				break;
			case TubeLight.LightsID.Unused11:
				num = 11;
				break;
			}
			if (num > 0)
			{
				LightsManager lightsManager = platformDescriptor.LightingManagers[num];
				LightsManager lightsManager2 = gameObject.AddComponent<LightsManager>();
				lightsManager2.ControllingLights = lightsManager.ControllingLights;
				lightsManager2.RotatingLights = lightsManager.RotatingLights;
				lightsManager2.GroupLightsBasedOnZ();
				UnityEngine.Object.Destroy(lightsManager);
				platformDescriptor.LightingManagers[num] = lightsManager2;
				break;
			}
		}
		if (componentsInChildren.Length == 0)
		{
			LightsManager lightsManager3 = platformDescriptor.LightingManagers[1];
			MeshRenderer[] componentsInChildren2 = trackRings.trackLaneRingPrefab.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in componentsInChildren2)
			{
				SetRendererMaterials(renderer, lightsManager3);
			}
			LightsManager lightsManager4 = gameObject.AddComponent<LightsManager>();
			lightsManager4.ControllingLights = lightsManager3.ControllingLights;
			lightsManager4.RotatingLights = lightsManager3.RotatingLights;
			lightsManager4.GroupLightsBasedOnZ();
			UnityEngine.Object.Destroy(lightsManager3);
			platformDescriptor.LightingManagers[1] = lightsManager4;
		}
		ReplaceBetterBlack(trackRings.trackLaneRingPrefab);
		SetLightingEventsForTubeLights(trackRings.trackLaneRingPrefab);
		TrackLaneRing prefab = trackRings.trackLaneRingPrefab.AddComponent<TrackLaneRing>();
		trackLaneRingsManager.Prefab = prefab;
		trackLaneRingsManager.RingCount = trackRings.ringCount;
		if (trackRings.useStepEffect)
		{
			trackLaneRingsManager.MINPositionStep = trackRings.minPositionStep;
			trackLaneRingsManager.MAXPositionStep = trackRings.maxPositionStep;
		}
		else
		{
			trackLaneRingsManager.MINPositionStep = (trackLaneRingsManager.MAXPositionStep = trackRings.ringPositionStep);
		}
		trackLaneRingsManager.MoveSpeed = trackRings.moveSpeed;
		trackLaneRingsManager.RotationStep = trackRings.rotationStep;
		trackLaneRingsManager.PropagationSpeed = Mathf.RoundToInt(trackRings.rotationPropagationSpeed);
		trackLaneRingsManager.FlexySpeed = trackRings.rotationFlexySpeed;
		if (trackRings.useRotationEffect)
		{
			TrackLaneRingsRotationEffect trackLaneRingsRotationEffect = (trackLaneRingsManager.RotationEffect = gameObject.AddComponent<TrackLaneRingsRotationEffect>());
			trackLaneRingsRotationEffect.Manager = trackLaneRingsManager;
			trackLaneRingsRotationEffect.StartupRotationAngle = trackRings.startupRotationAngle;
			trackLaneRingsRotationEffect.StartupRotationStep = trackRings.startupRotationStep;
			trackLaneRingsRotationEffect.StartupRotationPropagationSpeed = Mathf.RoundToInt(trackRings.startupRotationPropagationSpeed);
			trackLaneRingsRotationEffect.StartupRotationFlexySpeed = trackRings.startupRotationFlexySpeed;
		}
	}
}
