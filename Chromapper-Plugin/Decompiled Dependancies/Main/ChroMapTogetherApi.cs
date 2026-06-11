using System;
using System.Collections;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public static class ChroMapTogetherApi
{
	public static void TryRoomCode(string code, Action<string, int> onSuccess, Action<int, string> onFail)
	{
		PersistentUI.Instance.StartCoroutine(AttemptRoomCode(code, onSuccess, onFail));
	}

	private static IEnumerator AttemptRoomCode(string code, Action<string, int> onSuccess, Action<int, string> onFail)
	{
		string uri = Path.Combine(Settings.Instance.MultiSettings.ChroMapTogetherServerUrl, "JoinServer?code=" + code).Replace('\\', '/');
		using UnityWebRequest request = UnityWebRequest.Get(uri);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			JSONNode jSONNode = JSONNode.Parse(request.downloadHandler.text);
			onSuccess?.Invoke(jSONNode["ip"], jSONNode["port"]);
		}
		else
		{
			onFail?.Invoke((int)request.responseCode, request.error);
		}
	}

	public static void TryHost(Action<Guid, int, string> onSuccess, Action<int, string> onFail)
	{
		PersistentUI.Instance.StartCoroutine(AttemptHost(onSuccess, onFail));
	}

	private static IEnumerator AttemptHost(Action<Guid, int, string> onSuccess, Action<int, string> onFail)
	{
		string uri = Path.Combine(Settings.Instance.MultiSettings.ChroMapTogetherServerUrl, "CreateServer").Replace('\\', '/');
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("appVersion", Application.version);
		using UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			JSONNode jSONNode = JSONNode.Parse(request.downloadHandler.text);
			onSuccess?.Invoke(Guid.Parse(jSONNode["guid"]), jSONNode["port"], jSONNode["code"]);
		}
		else
		{
			onFail?.Invoke((int)request.responseCode, request.error);
		}
	}

	public static void TryKeepAlive(Guid guid, Action<int, string> onFail)
	{
		PersistentUI.Instance.StartCoroutine(AttemptKeepAlive(guid, onFail));
	}

	private static IEnumerator AttemptKeepAlive(Guid guid, Action<int, string> onFail)
	{
		string uri = Path.Combine(Settings.Instance.MultiSettings.ChroMapTogetherServerUrl, $"KeepServerAlive?guid={guid}").Replace('\\', '/');
		using UnityWebRequest request = UnityWebRequest.Put(uri, string.Empty);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			onFail?.Invoke((int)request.responseCode, request.error);
		}
	}
}
