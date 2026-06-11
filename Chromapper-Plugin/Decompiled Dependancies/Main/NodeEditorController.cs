using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Beatmap.Base;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NodeEditorController : MonoBehaviour, CMInput.INodeEditorActions
{
	public static bool IsActive;

	[SerializeField]
	private TMP_InputField nodeEditorInputField;

	[SerializeField]
	private TextMeshProUGUI labelTextMesh;

	[SerializeField]
	private Button closeButton;

	private readonly Type[] actionMapsEnabledWhenNodeEditing = new Type[5]
	{
		typeof(CMInput.ICameraActions),
		typeof(CMInput.IBeatmapObjectsActions),
		typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions),
		typeof(CMInput.ITimelineActions)
	};

	private JSONNode editingNode;

	private IEnumerable<BaseObject> editingObjects;

	private bool firstActive = true;

	private int height = 205;

	private bool isEditing;

	private bool queuedUpdate;

	private Type[] ActionMapsDisabled => (from x in typeof(CMInput).GetNestedTypes()
		where x.IsInterface && !Enumerable.Contains(actionMapsEnabledWhenNodeEditing, x)
		select x).ToArray();

	private void Start()
	{
		SelectionController.SelectionChangedEvent = (Action)Delegate.Combine(SelectionController.SelectionChangedEvent, new Action(ObjectWasSelected));
	}

	private void Update()
	{
		if (UIMode.SelectedMode == UIModeType.Normal && !SelectionController.HasSelectedObjects() && IsActive)
		{
			if (!Settings.Instance.NodeEditor_UseKeybind)
			{
				StopAllCoroutines();
				Close();
			}
			labelTextMesh.text = "Nothing Selected";
			nodeEditorInputField.text = "Please select an object to use Node Editor.";
		}
	}

	private void OnDestroy()
	{
		SelectionController.SelectionChangedEvent = (Action)Delegate.Remove(SelectionController.SelectionChangedEvent, new Action(ObjectWasSelected));
	}

	public void OnToggleNodeEditor(InputAction.CallbackContext context)
	{
		if (!nodeEditorInputField.isFocused && !UIMode.PreviewMode && Settings.Instance.NodeEditor_UseKeybind && context.performed && !PersistentUI.Instance.InputBoxIsEnabled)
		{
			StopAllCoroutines();
			if (IsActive)
			{
				CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), new Type[1] { typeof(CMInput.INodeEditorActions) });
				CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), ActionMapsDisabled);
				BeatmapActionContainer.RemoveAllActionsOfType<NodeEditorTextChangedAction>();
			}
			else
			{
				closeButton.gameObject.SetActive(value: true);
				CMInputCallbackInstaller.DisableActionMaps(typeof(NodeEditorController), ActionMapsDisabled);
			}
			StartCoroutine(UpdateGroup(!IsActive, base.transform as RectTransform));
		}
	}

	private IEnumerator UpdateGroup(bool enabled, RectTransform group)
	{
		IsActive = enabled;
		if (enabled)
		{
			if (queuedUpdate)
			{
				ObjectWasSelected();
			}
			height = Mathf.FloorToInt((float)Settings.Instance.NodeEditorSize * 20.5f);
			GetComponent<RectTransform>().sizeDelta = new Vector2(300f, height);
			nodeEditorInputField.pointSize = Settings.Instance.NodeEditorTextSize;
		}
		float dest = (enabled ? (-5) : (-height));
		float og = group.anchoredPosition.y;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			group.anchoredPosition = new Vector2(group.anchoredPosition.x, Mathf.Lerp(og, dest, t));
			og = group.anchoredPosition.y;
			yield return new WaitForEndOfFrame();
		}
		group.anchoredPosition = new Vector2(group.anchoredPosition.x, dest);
	}

	public void ObjectWasSelected()
	{
		queuedUpdate = !IsActive;
		if (queuedUpdate)
		{
			return;
		}
		if (!SelectionController.HasSelectedObjects())
		{
			isEditing = false;
			return;
		}
		BeatmapActionContainer.RemoveAllActionsOfType<NodeEditorTextChangedAction>();
		isEditing = true;
		if (!Settings.Instance.NodeEditor_UseKeybind)
		{
			StopAllCoroutines();
			closeButton.gameObject.SetActive(value: false);
			StartCoroutine(UpdateGroup(enabled: true, base.transform as RectTransform));
			if (firstActive)
			{
				firstActive = false;
				PersistentUI.Instance.DisplayMessage("Mapper", "node.warning", PersistentUI.DisplayMessageType.Bottom);
			}
		}
		UpdateJson();
	}

	private void UpdateJson()
	{
		editingObjects = SelectionController.SelectedObjects.Select((BaseObject it) => it);
		editingNode = GetSharedJson(editingObjects.Select((BaseObject it) => it.ToJson().Clone()));
		nodeEditorInputField.text = string.Join("", editingNode.ToString(2).Split('\r'));
		if (editingObjects.Count() == 1)
		{
			string[] array = editingObjects.First().ObjectType.ToString().Split('_');
			List<string> list = new List<string>(array.Length);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Substring(0, 1);
				text2 += text.ToLower().Substring(1);
				list.Add(text2);
			}
			string text3 = string.Join(" ", list);
			labelTextMesh.text = "Editing " + text3;
			nodeEditorInputField.text = string.Join("", editingNode.ToString(2).Split('\r'));
		}
		else
		{
			labelTextMesh.text = $"Editing ({editingObjects.Count()}) objects";
		}
	}

	public void NodeEditor_StartEdit(string content)
	{
		if (IsActive && !CMInputCallbackInstaller.IsActionMapDisabled(ActionMapsDisabled[0]))
		{
			CMInputCallbackInstaller.DisableActionMaps(typeof(NodeEditorController), new Type[1] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.DisableActionMaps(typeof(NodeEditorController), ActionMapsDisabled);
		}
	}

	public void NodeEditor_EndEdit(string nodeText)
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), new Type[1] { typeof(CMInput.INodeEditorActions) });
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), ActionMapsDisabled);
		try
		{
			if (isEditing && IsActive)
			{
				JSONNode jSONNode = JSON.Parse(Regex.Replace(nodeText, "\\p{C}+", string.Empty));
				if (string.IsNullOrEmpty(jSONNode.ToString()))
				{
					throw new Exception("Node cannot be empty.");
				}
				Dictionary<BaseObject, JSONNode> dictionary = editingObjects.ToDictionary((BaseObject it) => it, (BaseObject it) => it.ToJson().Clone());
				ApplyJson(editingNode.AsObject, jSONNode.AsObject, dictionary);
				BeatmapActionContainer.AddAction(new ActionCollectionAction(dictionary.Select((KeyValuePair<BaseObject, JSONNode> entry) => new BeatmapObjectModifiedAction(Activator.CreateInstance(entry.Key.GetType(), new object[1] { entry.Value }) as BaseObject, entry.Key, entry.Key, $"Edited a {entry.Key.ObjectType} with Node Editor.", keepSelection: true)).ToList(), forceRefreshPool: true, clearsSelection: true, $"Edited ({editingObjects.Count()}) objects with Node Editor."), perform: true);
				UpdateJson();
			}
		}
		catch (Exception ex)
		{
			string message = ex.Message;
			if (!(ex is JSONParseException ex2))
			{
				if (ex is TargetInvocationException ex3)
				{
					message = ex3.InnerException.Message;
				}
				else
				{
					Debug.LogError(ex);
				}
			}
			else
			{
				message = ex2.ToUIFriendlyString();
			}
			PersistentUI.Instance.ShowDialogBox(message, null, PersistentUI.DialogBoxPresetType.Ok);
		}
	}

	public void Close()
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), new Type[1] { typeof(CMInput.INodeEditorActions) });
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(NodeEditorController), ActionMapsDisabled);
		StartCoroutine(UpdateGroup(enabled: false, base.transform as RectTransform));
	}

	private int TypeToInt(JSONNode node)
	{
		if (node.IsObject)
		{
			return 0;
		}
		if (node.IsArray)
		{
			return 1;
		}
		return 2;
	}

	private int? GetAllType(string key, IEnumerable<JSONNode> nodes)
	{
		int num = -1;
		int num2 = -1;
		foreach (JSONNode node in nodes)
		{
			if (!node.HasKey(key))
			{
				return null;
			}
			int num3 = TypeToInt(node[key]);
			if ((num3 != num && num >= 0) || (num == 1 && node[key].AsArray.Count != num2))
			{
				return null;
			}
			if (node[key].IsArray && num == -1)
			{
				num2 = node[key].AsArray.Count;
			}
			num = num3;
		}
		return num;
	}

	private JSONNode GetSharedJson(IEnumerable<JSONNode> nodes)
	{
		JSONNode first = nodes.First();
		JSONObject jSONObject = new JSONObject();
		JSONNode.KeyEnumerator enumerator = first.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current;
			int? allType = GetAllType(key, nodes);
			if (!allType.HasValue)
			{
				continue;
			}
			if (allType == 0)
			{
				jSONObject[key] = GetSharedJson(nodes.Select((JSONNode it) => it[key]));
			}
			else if (allType == 2)
			{
				jSONObject[key] = (nodes.All((JSONNode it) => it[key].Value == first[key].Value) ? first[key] : new JSONDash());
			}
			else if (allType == 1)
			{
				jSONObject[key] = GetSharedJson(nodes.Select((JSONNode it) => it[key].AsArray));
			}
			else
			{
				jSONObject[key] = new JSONDash();
			}
		}
		return jSONObject;
	}

	private int? GetAllType(int idx, IEnumerable<JSONArray> nodes)
	{
		int num = -1;
		int num2 = -1;
		foreach (JSONArray node in nodes)
		{
			int num3 = TypeToInt(node[idx]);
			if ((num3 != num && num >= 0) || (num == 1 && node.AsArray.Count != num2))
			{
				return null;
			}
			if (node.IsArray && num == -1)
			{
				num2 = node.AsArray.Count;
			}
			num = num3;
		}
		return num;
	}

	private JSONNode GetSharedJson(IEnumerable<JSONArray> nodes)
	{
		JSONArray first = nodes.First();
		JSONArray jSONArray = new JSONArray();
		int key;
		for (key = 0; key < first.Count; key++)
		{
			int? allType = GetAllType(key, nodes);
			if (!allType.HasValue)
			{
				continue;
			}
			if (allType == 0)
			{
				jSONArray[key] = GetSharedJson(nodes.Select((JSONArray it) => it[key]));
			}
			else if (allType == 2)
			{
				jSONArray[key] = (nodes.All((JSONArray it) => it[key] == first[key]) ? first[key] : new JSONDash());
			}
			else if (allType == 1)
			{
				jSONArray[key] = GetSharedJson(nodes.Select((JSONArray it) => it[key].AsArray));
			}
			else
			{
				jSONArray[key] = new JSONDash();
			}
		}
		return jSONArray;
	}

	private void ApplyJson(JSONObject old, JSONObject updated, Dictionary<BaseObject, JSONNode> objects)
	{
		JSONNode.KeyEnumerator enumerator = old.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (updated.HasKey(current))
			{
				continue;
			}
			foreach (KeyValuePair<BaseObject, JSONNode> @object in objects)
			{
				@object.Value.Remove(current);
			}
		}
		enumerator = updated.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current;
			if (updated[key] == (object)"-")
			{
				continue;
			}
			if (updated[key].IsObject && old[key].IsObject)
			{
				ApplyJson(old[key].AsObject, updated[key].AsObject, objects.ToDictionary((KeyValuePair<BaseObject, JSONNode> it) => it.Key, (KeyValuePair<BaseObject, JSONNode> it) => it.Value[key]));
				continue;
			}
			if (updated[key].IsArray && old[key].IsArray)
			{
				ApplyJson(old[key].AsArray, updated[key].AsArray, objects.ToDictionary((KeyValuePair<BaseObject, JSONNode> it) => it.Key, (KeyValuePair<BaseObject, JSONNode> it) => it.Value[key].AsArray));
				continue;
			}
			foreach (KeyValuePair<BaseObject, JSONNode> object2 in objects)
			{
				object2.Value[key] = updated[key];
			}
		}
	}

	private void ApplyJson(JSONArray old, JSONArray updated, Dictionary<BaseObject, JSONArray> objects)
	{
		foreach (KeyValuePair<BaseObject, JSONArray> @object in objects)
		{
			for (int num = @object.Value.Count - 1; num >= updated.Count; num--)
			{
				@object.Value.Remove(num);
			}
		}
		int i;
		for (i = 0; i < updated.Count; i++)
		{
			if (updated[i] == (object)"-")
			{
				continue;
			}
			if (updated[i].IsObject && old[i].IsObject)
			{
				ApplyJson(old[i].AsObject, updated[i].AsObject, objects.ToDictionary((KeyValuePair<BaseObject, JSONArray> it) => it.Key, (KeyValuePair<BaseObject, JSONArray> it) => it.Value[i]));
				continue;
			}
			if (updated[i].IsArray && old[i].IsArray)
			{
				ApplyJson(old[i].AsArray, updated[i].AsArray, objects.ToDictionary((KeyValuePair<BaseObject, JSONArray> it) => it.Key, (KeyValuePair<BaseObject, JSONArray> it) => it.Value[i].AsArray));
				continue;
			}
			foreach (KeyValuePair<BaseObject, JSONArray> object2 in objects)
			{
				object2.Value[i] = updated[i];
			}
		}
	}
}
