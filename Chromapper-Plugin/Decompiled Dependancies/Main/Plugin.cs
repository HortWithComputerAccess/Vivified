using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Plugin
{
	private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private readonly List<Type> attributes = new List<Type>
	{
		typeof(InitAttribute),
		typeof(ObjectLoadedAttribute),
		typeof(EventPassedThresholdAttribute),
		typeof(NotePassedThresholdAttribute),
		typeof(ExitAttribute)
	};

	private readonly Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();

	private readonly object pluginInstance;

	public readonly AssemblyName AssemblyName;

	public string Name { get; }

	public Version Version => AssemblyName.Version;

	public Plugin(string name, AssemblyName assemblyName, object pluginInstance)
	{
		Name = name;
		AssemblyName = assemblyName;
		this.pluginInstance = pluginInstance;
		MethodInfo[] array = pluginInstance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in array)
		{
			foreach (Type attribute in attributes)
			{
				if (methodInfo.GetCustomAttribute(attribute) != null)
				{
					methods.Add(attribute, methodInfo);
				}
			}
		}
	}

	public bool CallMethod<T>()
	{
		methods.TryGetValue(typeof(T), out var value);
		try
		{
			value?.Invoke(pluginInstance, new object[0]);
			return true;
		}
		catch (TargetInvocationException ex)
		{
			Debug.LogException(ex.InnerException);
		}
		return false;
	}

	public bool CallMethod<T, TS>(TS obj)
	{
		methods.TryGetValue(typeof(T), out var value);
		try
		{
			value?.Invoke(pluginInstance, new object[1] { obj });
			return true;
		}
		catch (TargetInvocationException ex)
		{
			Debug.LogException(ex.InnerException);
		}
		return false;
	}

	public void Init()
	{
		if (CallMethod<InitAttribute>())
		{
			Debug.Log($"Loaded Plugin: {Name} - v{Version}");
		}
		else
		{
			Debug.LogError($"Error Loading Plugin: {Name} - v{Version}");
		}
	}
}
