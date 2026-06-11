using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class CustomPlatformSettings
{
	private static CustomPlatformSettings instance;

	public Dictionary<string, PlatformInfo> CustomPlatformsDictionary = new Dictionary<string, PlatformInfo>();

	public static CustomPlatformSettings Instance => instance ?? (instance = Load());

	public GameObject[] LoadPlatform(string name)
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(CustomPlatformsDictionary[name].Info.FullName);
		GameObject[] array = assetBundle.LoadAssetWithSubAssets<GameObject>("_CustomPlatform");
		assetBundle.Unload(unloadAllLoadedObjects: false);
		Debug.Log("Load platform/s: " + name + " " + array.Length);
		return array;
	}

	private void LoadCustomEnvironments()
	{
		string customPlatformsFolder = Settings.Instance.CustomPlatformsFolder;
		if (!Directory.Exists(customPlatformsFolder))
		{
			return;
		}
		CustomPlatformsDictionary.Clear();
		string[] files = Directory.GetFiles(customPlatformsFolder);
		for (int i = 0; i < files.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(files[i]);
			if (!fileInfo.Extension.ToUpper().Contains("PLAT"))
			{
				continue;
			}
			string key = fileInfo.Name.Split('.')[0];
			if (CustomPlatformsDictionary.ContainsKey(key))
			{
				Debug.LogError(":hyperPepega: :mega: YOU HAVE TWO PLATFORMS WITH THE SAME FILE NAME");
				continue;
			}
			PlatformInfo value = new PlatformInfo
			{
				Info = fileInfo
			};
			using MD5 mD = MD5.Create();
			using Stream inputStream = File.OpenRead(fileInfo.FullName);
			byte[] array = mD.ComputeHash(inputStream);
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < array.Length; j++)
			{
				stringBuilder.Append(array[j].ToString("X2").ToLower());
			}
			value.Md5Hash = stringBuilder.ToString();
			CustomPlatformsDictionary.Add(key, value);
		}
	}

	private static CustomPlatformSettings Load()
	{
		CustomPlatformSettings customPlatformSettings = new CustomPlatformSettings();
		customPlatformSettings.LoadCustomEnvironments();
		return customPlatformSettings;
	}
}
