using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CMInputCallbackInstaller : MonoBehaviour
{
	private class EventHandler
	{
		public readonly Dictionary<Type, int> Blockers = new Dictionary<Type, int>();

		public readonly EventInfo EventInfo;

		public readonly object EventObject;

		public readonly Delegate Handler;

		public readonly Type InterfaceType;

		public bool IsDisabled;

		public EventHandler(EventInfo eventInfo, object eventObject, Delegate handler, Type interfaceType)
		{
			EventInfo = eventInfo;
			EventObject = eventObject;
			Handler = handler;
			InterfaceType = interfaceType;
			EventInfo.AddEventHandler(EventObject, new Action<InputAction.CallbackContext>(ReleaseListenerFunc));
		}

		public void EnableEventHandler()
		{
			EventInfo.AddEventHandler(EventObject, Handler);
			IsDisabled = false;
		}

		public void DisableEventHandler(bool fully = false)
		{
			if (fully)
			{
				EventInfo.RemoveEventHandler(EventObject, new Action<InputAction.CallbackContext>(ReleaseListenerFunc));
			}
			EventInfo.RemoveEventHandler(EventObject, Handler);
			IsDisabled = true;
		}

		private void ReleaseListenerFunc(InputAction.CallbackContext context)
		{
			if (IsDisabled && context.canceled)
			{
				Handler.DynamicInvoke(context);
			}
		}

		public override int GetHashCode()
		{
			return InterfaceType.GetHashCode();
		}
	}

	private class QueueInfo
	{
		public readonly Type Owner;

		public readonly IEnumerable<Type> ToChange;

		public QueueInfo(Type owner, IEnumerable<Type> toChange)
		{
			Owner = owner;
			ToChange = toChange;
		}
	}

	public static bool TestMode = false;

	public static CMInput InputInstance;

	private static CMInputCallbackInstaller instance;

	private static readonly List<EventHandler> allEventHandlers = new List<EventHandler>();

	private static readonly BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;

	private static readonly List<EventHandler> disabledEventHandlers = new List<EventHandler>();

	private static readonly Dictionary<string, object> interfaceNameToReference = new Dictionary<string, object>();

	private static readonly Dictionary<string, Type> interfaceNameToType = new Dictionary<string, Type>();

	private static readonly List<Transform> persistentObjects = new List<Transform>();

	private static readonly List<QueueInfo> queuedToDisable = new List<QueueInfo>();

	private static readonly List<QueueInfo> queuedToEnable = new List<QueueInfo>();

	private CMInput input;

	private void Awake()
	{
		Debug.Log("Using Harmony Patch - Disable Unity Input Consumption");
		InputSystem.settings.shortcutKeysConsumeInput = false;
	}

	private void Start()
	{
		SendMessage("InputObjectCreated", input);
		Type[] nestedTypes = typeof(CMInput).GetNestedTypes();
		foreach (Type type in nestedTypes)
		{
			if (type.IsInterface)
			{
				interfaceNameToType.Add(type.Name, null);
			}
		}
		foreach (string item in new List<string>(interfaceNameToType.Keys))
		{
			Type nestedType = typeof(CMInput).GetNestedType(item.Substring(1));
			if (nestedType != null)
			{
				interfaceNameToType[item] = nestedType;
			}
		}
		PropertyInfo[] properties = typeof(CMInput).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			base.name = propertyInfo.PropertyType.Name;
			if (interfaceNameToType.ContainsKey("I" + base.name))
			{
				interfaceNameToReference.Add("I" + base.name, propertyInfo.GetValue(input));
			}
		}
	}

	private void Update()
	{
		if (queuedToDisable.Any())
		{
			foreach (QueueInfo item in queuedToDisable)
			{
				foreach (Type interfaceType in item.ToChange)
				{
					foreach (EventHandler item2 in allEventHandlers.Where((EventHandler x) => x.InterfaceType == interfaceType))
					{
						if (item2.Blockers.TryGetValue(item.Owner, out var value))
						{
							item2.Blockers[item.Owner] = value + 1;
						}
						else
						{
							item2.Blockers[item.Owner] = 1;
						}
						if (!item2.IsDisabled)
						{
							item2.DisableEventHandler();
							disabledEventHandlers.Add(item2);
						}
					}
				}
			}
			queuedToDisable.Clear();
		}
		if (!queuedToEnable.Any())
		{
			return;
		}
		foreach (QueueInfo item3 in queuedToEnable)
		{
			foreach (Type interfaceType2 in item3.ToChange)
			{
				foreach (EventHandler item4 in allEventHandlers.Where((EventHandler x) => x.InterfaceType == interfaceType2 && x.IsDisabled))
				{
					if (item4.Blockers.TryGetValue(item3.Owner, out var value2))
					{
						value2--;
						item4.Blockers[item3.Owner] = value2;
						if (value2 <= 0)
						{
							item4.Blockers.Remove(item3.Owner);
						}
					}
					if (item4.Blockers.Count <= 0)
					{
						item4.EnableEventHandler();
						disabledEventHandlers.Remove(item4);
					}
				}
			}
		}
		queuedToEnable.Clear();
	}

	private void OnEnable()
	{
		instance = this;
		input = new CMInput();
		input.Enable();
		InputInstance = input;
		SceneManager.sceneLoaded += SceneLoaded;
		Application.wantsToQuit += WantsToQuit;
	}

	private void OnDisable()
	{
		instance = null;
		input.Disable();
		ClearAllEvents();
		SceneManager.sceneLoaded -= SceneLoaded;
		Application.wantsToQuit -= WantsToQuit;
	}

	public static void DisableActionMaps(Type you, IEnumerable<Type> interfaceTypesToDisable)
	{
		queuedToDisable.Add(new QueueInfo(you, interfaceTypesToDisable));
	}

	public static void ClearDisabledActionMaps(Type you, IEnumerable<Type> interfaceTypesToEnable)
	{
		queuedToEnable.Add(new QueueInfo(you, interfaceTypesToEnable));
	}

	public static bool IsActionMapDisabled(Type actionMap)
	{
		return disabledEventHandlers.Any((EventHandler x) => x.InterfaceType == actionMap);
	}

	private void SceneLoaded(Scene scene, LoadSceneMode sceneMode)
	{
		if (sceneMode == LoadSceneMode.Single)
		{
			ClearAllEvents();
		}
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			FindAndInstallCallbacksRecursive(rootGameObjects[i].transform);
		}
		foreach (Transform persistentObject in persistentObjects)
		{
			FindAndInstallCallbacksRecursive(persistentObject);
		}
		StartCoroutine(WaitThenReenableInputs());
	}

	private IEnumerator WaitThenReenableInputs()
	{
		yield return new WaitUntil(() => !SceneTransitionManager.IsLoading);
		input.Enable();
		SceneTransitionManager.Instance.AddLoadRoutine(DisableInputs());
	}

	private IEnumerator DisableInputs()
	{
		if (!TestMode)
		{
			yield return new WaitForEndOfFrame();
		}
		input.Disable();
	}

	private bool WantsToQuit()
	{
		PropertyInfo[] properties = typeof(CMInput).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (interfaceNameToType.ContainsKey(propertyInfo.PropertyType.Name))
			{
				interfaceNameToType[propertyInfo.PropertyType.Name].InvokeMember("SetCallbacks", bindingFlags, Type.DefaultBinder, propertyInfo.GetValue(input), new object[1]);
			}
		}
		input.Disable();
		return true;
	}

	public static void PersistentObject(Transform obj)
	{
		persistentObjects.Add(obj);
	}

	public static void FindAndInstallCallbacksRecursive(Transform obj)
	{
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if ((object)monoBehaviour == null || (object)monoBehaviour.GetType() == null)
			{
				continue;
			}
			Type[] interfaces = monoBehaviour.GetType().GetInterfaces();
			foreach (Type type in interfaces)
			{
				if (!interfaceNameToType.ContainsKey(type.Name))
				{
					continue;
				}
				Debug.Log("Found " + type.Name + " in " + monoBehaviour.name);
				PropertyInfo[] properties = interfaceNameToReference[type.Name].GetType().GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (propertyInfo.PropertyType == typeof(InputAction))
					{
						InputAction eventObject = (InputAction)propertyInfo.GetValue(interfaceNameToReference[type.Name]);
						EventInfo[] events = propertyInfo.PropertyType.GetEvents();
						for (int l = 0; l < events.Length; l++)
						{
							AddEventHandler(events[l], eventObject, monoBehaviour, type.GetMethod("On" + propertyInfo.Name), type);
						}
					}
				}
			}
		}
		foreach (Transform item in obj)
		{
			FindAndInstallCallbacksRecursive(item);
		}
	}

	public static void FindAndRemoveCallbacksRecursive(Transform obj)
	{
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if ((object)monoBehaviour == null || (object)monoBehaviour.GetType() == null)
			{
				continue;
			}
			Type[] interfaces = monoBehaviour.GetType().GetInterfaces();
			foreach (Type interfaceType in interfaces)
			{
				foreach (EventHandler item in allEventHandlers.FindAll((EventHandler it) => it.InterfaceType == interfaceType))
				{
					item.DisableEventHandler(fully: true);
					allEventHandlers.Remove(item);
				}
			}
		}
		foreach (Transform item2 in obj)
		{
			FindAndInstallCallbacksRecursive(item2);
		}
	}

	private void ClearAllEvents()
	{
		foreach (EventHandler allEventHandler in allEventHandlers)
		{
			allEventHandler.DisableEventHandler(fully: true);
		}
		allEventHandlers.Clear();
		disabledEventHandlers.Clear();
	}

	private static void AddEventHandler(EventInfo eventInfo, object eventObject, object item, MethodInfo action, Type interfaceType)
	{
		ParameterExpression[] array = (from parameter in eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters()
			select Expression.Parameter(parameter.ParameterType)).ToArray();
		Delegate handler = Expression.Lambda(eventInfo.EventHandlerType, Expression.Call(Expression.Constant(item), action, array[0]), array).Compile();
		eventInfo.AddEventHandler(eventObject, handler);
		EventHandler item2 = new EventHandler(eventInfo, eventObject, handler, interfaceType);
		allEventHandlers.Add(item2);
	}
}
