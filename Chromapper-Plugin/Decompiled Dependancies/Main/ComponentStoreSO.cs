using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CMUI/Component Store")]
public class ComponentStoreSO : ScriptableObject
{
	public static ComponentStoreSO Instance;

	[SerializeField]
	private List<CMUIComponentBase> components;

	private void OnEnable()
	{
		if (Instance != null)
		{
			Debug.LogError("Component Store instance has already been assigned.");
			UnityEngine.Object.DestroyImmediate(this);
		}
		Instance = this;
	}

	public CMUIComponent<T> InstantiateCMUIComponentForHandledType<T>(Transform parent)
	{
		return InstantiateCMUIComponentForHandledType(parent, typeof(T)) as CMUIComponent<T>;
	}

	public CMUIComponentBase InstantiateCMUIComponentForHandledType(Transform parent, Type handledType)
	{
		CMUIComponentBase cMUIComponentBase = components.Find((CMUIComponentBase x) => x.GetType().BaseType.IsGenericType && x.GetType().BaseType.GenericTypeArguments[0] == handledType);
		if (!(cMUIComponentBase == null))
		{
			return UnityEngine.Object.Instantiate(cMUIComponentBase, parent);
		}
		throw new MissingReferenceException("No registered CMUI Component that handles type " + handledType.Name + ".");
	}

	public T InstantiateCMUIComponentForComponentType<T>(Transform parent) where T : CMUIComponentBase
	{
		return InstantiateCMUIComponentForComponentType(parent, typeof(T)) as T;
	}

	public CMUIComponentBase InstantiateCMUIComponentForComponentType(Transform parent, Type componentType)
	{
		CMUIComponentBase cMUIComponentBase = components.Find((CMUIComponentBase x) => x.GetType() == componentType);
		if (!(cMUIComponentBase == null))
		{
			return UnityEngine.Object.Instantiate(cMUIComponentBase, parent);
		}
		throw new MissingReferenceException("CMUI Component type " + componentType.Name + " is not registered.");
	}
}
