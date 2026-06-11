using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

internal class PluginLoader : MonoBehaviour
{
	private const string pluginDir = "Plugins";

	private const bool loadPluginsInEditor = false;

	private static readonly List<Plugin> plugins = new List<Plugin>();

	public static Action<Plugin[]> PluginsLoadedEvent;

	public static IReadOnlyList<Plugin> LoadedPlugins => plugins.AsReadOnly();

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Application.isEditor)
		{
			LoadAssemblies();
		}
	}

	private void OnDestroy()
	{
		BroadcastEvent<ExitAttribute>();
	}

	private void LoadAssemblies()
	{
		if (!Directory.Exists("Plugins"))
		{
			Directory.CreateDirectory("Plugins");
		}
		string[] files = Directory.GetFiles("Plugins", "*.dll", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			Assembly assembly = Assembly.LoadFile(Path.GetFullPath(files[i]));
			Type[] exportedTypes = assembly.GetExportedTypes();
			foreach (Type type in exportedTypes)
			{
				PluginAttribute pluginAttribute = null;
				try
				{
					pluginAttribute = type.GetCustomAttribute<PluginAttribute>();
				}
				catch (Exception)
				{
				}
				if (pluginAttribute != null)
				{
					try
					{
						Plugin item = new Plugin(pluginAttribute.Name, assembly.GetName(), Activator.CreateInstance(type));
						plugins.Add(item);
					}
					catch (Exception exception)
					{
						Debug.LogError("Incompatible plugin " + pluginAttribute.Name + ", please check for an update or remove it!");
						Debug.LogException(exception);
					}
				}
			}
		}
		foreach (Plugin plugin in plugins)
		{
			plugin.Init();
		}
		PluginsLoadedEvent(plugins.ToArray());
	}

	public static void BroadcastEvent<T>() where T : Attribute
	{
		foreach (Plugin plugin in plugins)
		{
			plugin.CallMethod<T>();
		}
	}

	public static void BroadcastEvent<T, TS>(TS obj) where T : Attribute
	{
		foreach (Plugin plugin in plugins)
		{
			plugin.CallMethod<T, TS>(obj);
		}
	}
}
