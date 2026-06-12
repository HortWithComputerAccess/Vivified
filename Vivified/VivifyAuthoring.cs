using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Helper;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vivified
{
    /// <summary>
    /// In-editor authoring: spawn bundle prefabs as InstantiatePrefab events,
    /// click-select and drag-move them in the 3D view (undoable), delete and
    /// duplicate spawns, and insert templates for every Vivify event type.
    /// </summary>
    public partial class VivifyPreview
    {
        private BaseCustomEvent selectedEvent;
        private bool dragging;
        private Plane dragPlane;
        private Vector3 dragGrabWorld;
        private Vector3 dragStartWorld;
        private Transform dragTransform;

        public static readonly string[] EventTemplateTypes =
        {
            "InstantiatePrefab", "DestroyObject", "AnimateTrack", "AssignPathAnimation",
            "AssignTrackParent", "AssignPlayerToTrack", "SetMaterialProperty", "SetGlobalProperty",
            "Blit", "CreateCamera", "CreateScreenTexture", "SetCameraProperty",
            "SetAnimatorProperty", "SetRenderingSettings", "AssignObjectPrefab",
        };

        /// <summary>Bundle prefab asset paths, for the spawn dropdown.</summary>
        public List<string> PrefabPaths()
        {
            var list = assetNames.Keys.Where(n => n.EndsWith(".prefab")).ToList();
            list.Sort();
            return list;
        }

        public string SelectedDescription()
        {
            if (selectedEvent == null) return "Nothing selected. Enable Edit Mode and click a Vivify object.";
            var d = selectedEvent.Data;
            return string.Format("{0} @ {1:0.###}  asset: {2}",
                selectedEvent.Type, selectedEvent.JsonTime,
                d != null && d.HasKey("asset") ? d["asset"].Value.Split('/').Last() : "?");
        }

        public bool HasSelection => selectedEvent != null;

        // --- creating events --------------------------------------------------------

        private static bool IsV2 => Settings.Instance.MapVersion == 2;

        private static JSONNode EventNode(float beat, string type, JSONNode data)
        {
            var node = new JSONObject();
            node[IsV2 ? "_time" : "b"] = beat;
            node[IsV2 ? "_type" : "t"] = type;
            node[IsV2 ? "_data" : "d"] = data;
            return node;
        }

        /// <summary>Insert a custom event into the map, undoably.</summary>
        public BaseCustomEvent InsertEvent(float beat, string type, JSONNode data, string comment)
        {
            var ev = BeatmapFactory.CustomEvent(EventNode(beat, type, data));
            if (eventCollection != null)
            {
                eventCollection.SpawnObject(ev, true, true, false);
                BeatmapActionContainer.AddAction(
                    new BeatmapObjectPlacementAction(ev, new List<BaseObject>(), comment));
            }
            else
            {
                BeatSaberSongContainer.Instance.Map.CustomEvents.Add(ev);
            }
            QueueRebuild();
            return ev;
        }

        /// <summary>Spawn a prefab from the bundle at the current beat.</summary>
        public void SpawnPrefab(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || atsc == null) return;
            string baseName = assetPath.Split('/').Last().Replace(".prefab", "");
            var safe = new string(baseName.Where(char.IsLetterOrDigit).ToArray());
            int n = 1;
            var events = BeatSaberSongContainer.Instance.Map.CustomEvents;
            while (events.Any(e => e.Data != null && e.Data["id"].Value == safe + "_" + n)) n++;
            string id = safe + "_" + n;

            var data = new JSONObject();
            data["asset"] = assetPath;
            data["id"] = id;
            data["track"] = id;
            var posArr = new JSONArray();
            posArr.Add(0f); posArr.Add(0f); posArr.Add(0f);
            data["position"] = posArr;

            var ev = InsertEvent(atsc.CurrentJsonTime, "InstantiatePrefab", data, "Vivified: spawn " + id);
            selectedEvent = ev;
            Status = "spawned " + id + " @ " + atsc.CurrentJsonTime.ToString("0.##");
        }

        /// <summary>Insert a starter template for any Vivify/Heck event type.</summary>
        public void InsertTemplate(string type)
        {
            if (atsc == null) return;
            var d = new JSONObject();
            switch (type)
            {
                case "InstantiatePrefab": d["asset"] = ""; d["id"] = "myPrefab"; d["track"] = "myPrefab"; break;
                case "DestroyObject": d["id"] = "myPrefab"; break;
                case "AnimateTrack":
                {
                    d["track"] = "myPrefab";
                    d["duration"] = 4f;
                    var keys = new JSONArray();
                    var k0 = new JSONArray(); k0.Add(0f); k0.Add(0f); k0.Add(0f); k0.Add(0f);
                    var k1 = new JSONArray(); k1.Add(0f); k1.Add(5f); k1.Add(0f); k1.Add(1f); k1.Add("easeOutQuad");
                    keys.Add(k0); keys.Add(k1);
                    d["position"] = keys;
                    break;
                }
                case "AssignPathAnimation": d["track"] = "noteTrack"; break;
                case "AssignTrackParent":
                {
                    var children = new JSONArray(); children.Add("childTrack");
                    d["childrenTracks"] = children; d["parentTrack"] = "parentTrack";
                    break;
                }
                case "AssignPlayerToTrack": d["track"] = "player"; break;
                case "SetMaterialProperty":
                case "SetGlobalProperty":
                {
                    if (type == "SetMaterialProperty") d["asset"] = "";
                    var props = new JSONArray();
                    var p = new JSONObject();
                    p["id"] = "_Color"; p["type"] = "Color";
                    var col = new JSONArray(); col.Add(1f); col.Add(1f); col.Add(1f); col.Add(1f);
                    p["value"] = col;
                    props.Add(p);
                    d["properties"] = props;
                    break;
                }
                case "Blit": d["asset"] = ""; d["duration"] = 4f; d["priority"] = 0; break;
                case "CreateCamera": d["id"] = "camera1"; d["texture"] = "_CameraTexture"; break;
                case "CreateScreenTexture": d["id"] = "_ScreenTexture"; break;
                case "SetCameraProperty":
                {
                    var props = new JSONObject();
                    var modes = new JSONArray(); modes.Add("Depth");
                    props["depthTextureMode"] = modes;
                    d["properties"] = props;
                    break;
                }
                case "SetAnimatorProperty":
                {
                    d["id"] = "myPrefab";
                    var props = new JSONArray();
                    var p = new JSONObject();
                    p["id"] = "MyParam"; p["type"] = "Float"; p["value"] = 1f;
                    props.Add(p);
                    d["properties"] = props;
                    break;
                }
                case "SetRenderingSettings":
                {
                    var rs = new JSONObject(); rs["fog"] = true;
                    d["renderSettings"] = rs;
                    break;
                }
                case "AssignObjectPrefab":
                {
                    d["loadMode"] = "Single";
                    var cn = new JSONObject(); cn["track"] = "noteTrack"; cn["asset"] = "";
                    d["colorNotes"] = cn;
                    break;
                }
            }
            InsertEvent(atsc.CurrentJsonTime, type, d, "Vivified: add " + type);
            Status = "added " + type + " @ " + atsc.CurrentJsonTime.ToString("0.##");
        }

        // --- selection editing ---------------------------------------------------------

        public void DeleteSelected()
        {
            if (selectedEvent == null || eventCollection == null) return;
            eventCollection.DeleteObject(selectedEvent, true, true, "Vivified: delete spawn", false, true);
            selectedEvent = null;
            QueueRebuild();
        }

        public void DuplicateSelected()
        {
            if (selectedEvent == null || atsc == null) return;
            var data = selectedEvent.Data != null
                ? JSON.Parse(selectedEvent.Data.ToString())
                : new JSONObject() as JSONNode;
            if (data.HasKey("id"))
            {
                data["id"] = data["id"].Value + "_copy";
                if (data.HasKey("track")) data["track"] = data["id"].Value;
            }
            var ev = InsertEvent(atsc.CurrentJsonTime, selectedEvent.Type, data, "Vivified: duplicate");
            selectedEvent = ev;
        }

        public string GetSelectedField(string key, string fallback)
        {
            if (selectedEvent == null || selectedEvent.Data == null) return fallback;
            var arr = selectedEvent.Data[key].AsArray;
            if (arr == null || arr.Count < 3) return fallback;
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}",
                arr[0].AsFloat, arr[1].AsFloat, arr[2].AsFloat);
        }

        /// <summary>Apply "x, y, z" text into a vector field of the selected event.</summary>
        public void ApplySelectedField(string key, string text)
        {
            if (selectedEvent == null) return;
            var parts = text.Split(',');
            if (parts.Length < 3) return;
            float x, y, z;
            if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
                !float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
                !float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                return;
            WriteSelectedVector(key, new Vector3(x, y, z), "Vivified: edit " + key);
        }

        private void WriteSelectedVector(string key, Vector3 v, string comment)
        {
            if (selectedEvent == null) return;
            var original = BeatmapFactory.Clone(selectedEvent);
            var data = selectedEvent.Data ?? new JSONObject();
            var arr = new JSONArray();
            arr.Add(Round3(v.x)); arr.Add(Round3(v.y)); arr.Add(Round3(v.z));
            data[key] = arr;
            selectedEvent.Data = data;
            BeatmapActionContainer.AddAction(
                new BeatmapObjectModifiedAction(selectedEvent, selectedEvent, original, comment));
            QueueRebuild();
        }

        private static float Round3(float v) => Mathf.Round(v * 1000f) / 1000f;

        // --- click select + drag move ------------------------------------------------

        private void UpdateEditing()
        {
            if (highlight != null)
            {
                Bounds? target = null;
                var selected = FindSelectedState();
                if (selected != null && selected.Go != null)
                {
                    var renderers = selected.Go.GetComponentsInChildren<Renderer>(false);
                    if (renderers.Length > 0)
                    {
                        var b = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                        target = b;
                    }
                    else
                    {
                        target = new Bounds(selected.Go.transform.position, Vector3.one * 0.5f);
                    }
                }
                highlight.Target = target;
            }

            if (!VivifiedSettings.EditMode)
            {
                dragging = false;
                return;
            }
            var cam = Camera.main;
            if (cam == null) return;

            if (Input.GetKeyDown(KeyCode.Escape)) selectedEvent = null;

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                var hit = PickInstance(ray);
                if (hit != null)
                {
                    selectedEvent = hit.Item1.Event;
                    dragTransform = hit.Item2.Go.transform;
                    dragStartWorld = dragTransform.position;
                    dragGrabWorld = ray.GetPoint(hit.Item3);
                    dragPlane = new Plane(-cam.transform.forward, dragGrabWorld);
                    dragging = true;
                }
            }

            if (dragging && dragTransform != null && Input.GetMouseButton(0))
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                float enter;
                if (dragPlane.Raycast(ray, out enter))
                {
                    var delta = ray.GetPoint(enter) - dragGrabWorld;
                    dragTransform.position = dragStartWorld + delta;
                }
            }

            if (dragging && Input.GetMouseButtonUp(0))
            {
                dragging = false;
                if (dragTransform != null && selectedEvent != null)
                {
                    string key = selectedEvent.Data != null && selectedEvent.Data.HasKey("localPosition")
                        ? "localPosition"
                        : "position";
                    WriteSelectedVector(key, dragTransform.localPosition, "Vivified: move prefab");
                }
                dragTransform = null;
            }
        }

        /// <summary>Instances are dragged live; skip engine transforms for them.</summary>
        private bool IsDraggingInstance(PrefabSpawn spawn) =>
            dragging && selectedEvent != null && spawn.Event == selectedEvent;

        private InstanceState FindSelectedState()
        {
            if (selectedEvent == null) return null;
            foreach (var pair in instances)
                if (pair.Key.Event == selectedEvent)
                    return pair.Value;
            return null;
        }

        private Tuple<PrefabSpawn, InstanceState, float> PickInstance(Ray ray)
        {
            PrefabSpawn bestSpawn = null;
            InstanceState bestState = null;
            float bestDist = float.MaxValue;
            foreach (var pair in instances)
            {
                if (pair.Value.Go == null) continue;
                var renderers = pair.Value.Go.GetComponentsInChildren<Renderer>(false);
                for (int i = 0; i < renderers.Length; i++)
                {
                    float dist;
                    if (renderers[i].bounds.IntersectRay(ray, out dist) && dist < bestDist)
                    {
                        bestDist = dist;
                        bestSpawn = pair.Key;
                        bestState = pair.Value;
                    }
                }
            }
            return bestSpawn == null ? null : Tuple.Create(bestSpawn, bestState, bestDist);
        }

        private static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
