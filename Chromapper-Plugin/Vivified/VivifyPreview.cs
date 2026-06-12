using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Vivified
{
    public static class VivifiedSettings
    {
        public static bool PreviewEnabled = true;
        /// <summary>Beat Saber world origin relative to ChroMapper's world.</summary>
        public static Vector3 WorldOffset = new Vector3(0f, -0.5f, -1.5f);
    }

    /// <summary>
    /// Lives in the editor scene. Loads the map's .vivify AssetBundle natively
    /// (ChroMapper is built-in render pipeline, so the real shaders render),
    /// spawns InstantiatePrefab prefabs over playback time, animates them with
    /// AnimateTrack / AssignTrackParent, and applies SetMaterialProperty /
    /// SetGlobalProperty values live.
    /// </summary>
    public class VivifyPreview : MonoBehaviour
    {
        public static VivifyPreview Instance { get; private set; }

        private AudioTimeSyncController atsc;
        private CustomEventGridContainer eventCollection;
        private AssetBundle bundle;
        private string bundlePath;
        private readonly Dictionary<string, string> assetNames = new Dictionary<string, string>(); // lowercase -> real
        private readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
        private readonly Dictionary<Material, Dictionary<string, object>> materialOriginals =
            new Dictionary<Material, Dictionary<string, object>>();
        private readonly Dictionary<string, object> globalOriginals = new Dictionary<string, object>();

        private readonly TrackEngine engine = new TrackEngine();
        private readonly Dictionary<PrefabSpawn, GameObject> instances = new Dictionary<PrefabSpawn, GameObject>();
        private readonly Dictionary<string, Transform> trackNodes = new Dictionary<string, Transform>();
        private Transform root;
        private bool rebuildQueued = true;
        private bool lastEnabled = true;

        public string Status { get; private set; } = "initializing";

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("VivifiedPreview");
            Instance = go.AddComponent<VivifyPreview>();
        }

        private void Start()
        {
            atsc = FindObjectOfType<AudioTimeSyncController>();
            var rootGo = new GameObject("VivifiedPreviewRoot");
            root = rootGo.transform;
            root.position = VivifiedSettings.WorldOffset;

            eventCollection = BeatmapObjectContainerCollection
                .GetCollectionForType(ObjectType.CustomEvent) as CustomEventGridContainer;
            if (eventCollection != null)
            {
                eventCollection.ObjectSpawnedEvent += OnEventsChanged;
                eventCollection.ObjectDeletedEvent += OnEventsChanged;
            }

            LoadBundle();
            QueueRebuild();
        }

        private void OnDestroy()
        {
            if (eventCollection != null)
            {
                eventCollection.ObjectSpawnedEvent -= OnEventsChanged;
                eventCollection.ObjectDeletedEvent -= OnEventsChanged;
            }
            RestoreMaterials();
            RestoreGlobals();
            ClearInstances();
            if (root != null) Destroy(root.gameObject);
            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
            if (Instance == this) Instance = null;
        }

        private void OnEventsChanged(BaseCustomEvent _) => QueueRebuild();

        public void QueueRebuild() => rebuildQueued = true;

        public void ReloadBundle()
        {
            RestoreMaterials();
            ClearInstances();
            prefabCache.Clear();
            materialCache.Clear();
            assetNames.Clear();
            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
            LoadBundle();
            QueueRebuild();
        }

        // --- bundle ------------------------------------------------------------

        private void LoadBundle()
        {
            var info = BeatSaberSongContainer.Instance != null ? BeatSaberSongContainer.Instance.Info : null;
            string dir = info != null ? info.Directory : null;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                Status = "no map directory";
                return;
            }

            // prefer the bundle built for the closest Unity version (ChroMapper
            // is on a modern Unity; newer bundles fail on older players, not
            // vice versa) - try 2021, then 2019, then anything else
            var candidates = Directory.GetFiles(dir)
                .Where(f =>
                {
                    var name = Path.GetFileName(f).ToLowerInvariant();
                    return name.EndsWith(".vivify") || (name.StartsWith("bundle") && !name.EndsWith(".dat"));
                })
                .OrderByDescending(f =>
                {
                    var n = Path.GetFileName(f).ToLowerInvariant();
                    if (n.Contains("2021")) return 2;
                    if (n.Contains("2019")) return 1;
                    return 0;
                })
                .ToList();

            foreach (var path in candidates)
            {
                try
                {
                    bundle = AssetBundle.LoadFromFile(path);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Vivified: failed to load bundle " + path + ": " + e.Message);
                    bundle = null;
                }
                if (bundle != null)
                {
                    bundlePath = path;
                    break;
                }
            }

            if (bundle == null)
            {
                Status = candidates.Count == 0
                    ? "no .vivify bundle in map folder"
                    : "bundle could not be loaded (incompatible Unity version?)";
                return;
            }

            foreach (var name in bundle.GetAllAssetNames())
                assetNames[name.ToLowerInvariant()] = name;
            Status = Path.GetFileName(bundlePath) + ": " + assetNames.Count + " assets";
            Debug.Log("Vivified: loaded " + bundlePath + " with " + assetNames.Count + " assets");
        }

        private GameObject GetPrefab(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || bundle == null) return null;
            GameObject cached;
            if (prefabCache.TryGetValue(assetPath, out cached)) return cached;
            GameObject prefab = null;
            string real;
            if (assetNames.TryGetValue(assetPath.ToLowerInvariant(), out real))
                prefab = bundle.LoadAsset<GameObject>(real);
            prefabCache[assetPath] = prefab;
            return prefab;
        }

        private Material GetMaterial(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || bundle == null) return null;
            Material cached;
            if (materialCache.TryGetValue(assetPath, out cached)) return cached;
            Material mat = null;
            string real;
            if (assetNames.TryGetValue(assetPath.ToLowerInvariant(), out real))
                mat = bundle.LoadAsset<Material>(real);
            materialCache[assetPath] = mat;
            return mat;
        }

        // --- per-frame ----------------------------------------------------------

        private void Update()
        {
            if (VivifiedSettings.PreviewEnabled != lastEnabled)
            {
                lastEnabled = VivifiedSettings.PreviewEnabled;
                if (!lastEnabled)
                {
                    ClearInstances();
                    RestoreMaterials();
                    RestoreGlobals();
                }
                else QueueRebuild();
            }
            if (!VivifiedSettings.PreviewEnabled || atsc == null) return;

            if (rebuildQueued)
            {
                rebuildQueued = false;
                RebuildEngine();
            }
            if (root != null) root.position = VivifiedSettings.WorldOffset;

            float beat = atsc.CurrentJsonTime;
            SyncInstances(beat);
            ApplyTransforms(beat);
            ApplyMaterialProperties(beat);
            ApplyGlobalProperties(beat);
        }

        private void RebuildEngine()
        {
            var container = BeatSaberSongContainer.Instance;
            if (container == null || container.Map == null) return;
            engine.Rebuild(container.Map.CustomEvents, container.Map.PointDefinitions);
            ClearInstances();
            Status = (bundle != null ? Path.GetFileName(bundlePath) + " · " : "") +
                     engine.Spawns.Count + " spawns · " +
                     engine.MaterialEvents.Count + " animated materials";
        }

        private void SyncInstances(float beat)
        {
            var active = new HashSet<PrefabSpawn>(engine.ActiveSpawns(beat));

            // remove dead instances
            var toRemove = new List<PrefabSpawn>();
            foreach (var pair in instances)
                if (!active.Contains(pair.Key))
                    toRemove.Add(pair.Key);
            foreach (var spawn in toRemove)
            {
                if (instances[spawn] != null) Destroy(instances[spawn]);
                instances.Remove(spawn);
            }

            // create new ones
            foreach (var spawn in active)
            {
                if (instances.ContainsKey(spawn)) continue;
                var prefab = GetPrefab(spawn.Asset);
                GameObject go;
                if (prefab != null)
                {
                    go = Instantiate(prefab);
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = Vector3.one * 0.5f;
                    var rend = go.GetComponent<Renderer>();
                    if (rend != null) rend.material.color = Color.magenta;
                }
                go.name = "Vivify:" + spawn.Id;
                go.transform.SetParent(ParentForSpawn(spawn), false);
                instances[spawn] = go;
            }
        }

        /// <summary>Track-parent chains become real transform hierarchies.</summary>
        private Transform ParentForSpawn(PrefabSpawn spawn)
        {
            string track = spawn.Tracks.Count > 0 ? spawn.Tracks[0] : null;
            if (string.IsNullOrEmpty(track)) return root;
            var chain = engine.ParentChain(track);
            Transform parent = root;
            for (int i = chain.Count - 1; i >= 0; i--)
                parent = GetTrackNode(chain[i], parent);
            return parent;
        }

        private Transform GetTrackNode(string track, Transform parent)
        {
            Transform node;
            if (trackNodes.TryGetValue(track, out node) && node != null)
                return node;
            var go = new GameObject("VivifyTrack:" + track);
            node = go.transform;
            node.SetParent(parent, false);
            trackNodes[track] = node;
            return node;
        }

        private void ApplyTransforms(float beat)
        {
            // animate parent track nodes
            foreach (var pair in trackNodes)
            {
                if (pair.Value == null) continue;
                var st = engine.Evaluate(pair.Key, beat);
                var pos = st.LocalPosition ?? st.Position ?? Vector3.zero;
                var rot = st.LocalRotation ?? st.Rotation ?? Vector3.zero;
                var scale = st.Scale ?? Vector3.one;
                pair.Value.localPosition = pos;
                pair.Value.localEulerAngles = rot;
                pair.Value.localScale = scale;
            }

            // animate instances: track properties override the spawn transform
            foreach (var pair in instances)
            {
                var spawn = pair.Key;
                var go = pair.Value;
                if (go == null) continue;

                Vector3 pos = spawn.Position;
                Vector3 rot = spawn.Rotation;
                Vector3 scale = spawn.Scale;
                float? dissolve = null;
                foreach (var track in spawn.Tracks)
                {
                    var st = engine.Evaluate(track, beat);
                    var p = st.LocalPosition ?? st.Position;
                    var r = st.LocalRotation ?? st.Rotation;
                    if (p.HasValue) pos = p.Value;
                    if (r.HasValue) rot = r.Value;
                    if (st.Scale.HasValue) scale = st.Scale.Value;
                    if (st.Dissolve.HasValue) dissolve = st.Dissolve;
                    if (p.HasValue || r.HasValue || st.Scale.HasValue || st.Dissolve.HasValue) break;
                }

                go.transform.localPosition = pos;
                go.transform.localEulerAngles = rot;
                go.transform.localScale = scale;

                // dissolve: most maps use it to hide/show objects; approximate
                // by toggling renderers near zero
                if (dissolve.HasValue)
                {
                    bool visible = dissolve.Value > 0.01f;
                    var renderers = go.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renderers.Length; i++)
                        if (renderers[i].enabled != visible)
                            renderers[i].enabled = visible;
                }
            }
        }

        // --- material / global properties ---------------------------------------

        private void ApplyMaterialProperties(float beat)
        {
            foreach (var assetPair in engine.MaterialEvents)
            {
                var mat = GetMaterial(assetPair.Key);
                if (mat == null) continue;
                foreach (var propPair in assetPair.Value)
                {
                    string id = propPair.Key;
                    var first = propPair.Value[0];
                    int dim = (first.Type == "Color" || first.Type == "Vector") ? 4 : 1;
                    var v = TrackEngine.CurrentValue(propPair.Value, beat, dim);
                    if (v == null)
                    {
                        RestoreMaterialProperty(mat, id, first.Type);
                        continue;
                    }
                    if (!mat.HasProperty(id)) continue;
                    SnapshotMaterialProperty(mat, id, first.Type);
                    switch (first.Type)
                    {
                        case "Color":
                            mat.SetColor(id, new Color(v[0], v.Length > 1 ? v[1] : 0f,
                                v.Length > 2 ? v[2] : 0f, v.Length > 3 ? v[3] : 1f));
                            break;
                        case "Vector":
                            mat.SetVector(id, new Vector4(v[0], v.Length > 1 ? v[1] : 0f,
                                v.Length > 2 ? v[2] : 0f, v.Length > 3 ? v[3] : 0f));
                            break;
                        case "Float":
                            mat.SetFloat(id, v[0]);
                            break;
                        case "Keyword":
                            if (v[0] > 0.5f) mat.EnableKeyword(id);
                            else mat.DisableKeyword(id);
                            break;
                    }
                }
            }
        }

        private void ApplyGlobalProperties(float beat)
        {
            foreach (var propPair in engine.GlobalEvents)
            {
                string id = propPair.Key;
                var first = propPair.Value[0];
                int dim = (first.Type == "Color" || first.Type == "Vector") ? 4 : 1;
                var v = TrackEngine.CurrentValue(propPair.Value, beat, dim);
                if (v == null) continue;
                switch (first.Type)
                {
                    case "Color":
                        SnapshotGlobal(id, first.Type);
                        Shader.SetGlobalColor(id, new Color(v[0], v.Length > 1 ? v[1] : 0f,
                            v.Length > 2 ? v[2] : 0f, v.Length > 3 ? v[3] : 1f));
                        break;
                    case "Vector":
                        SnapshotGlobal(id, first.Type);
                        Shader.SetGlobalVector(id, new Vector4(v[0], v.Length > 1 ? v[1] : 0f,
                            v.Length > 2 ? v[2] : 0f, v.Length > 3 ? v[3] : 0f));
                        break;
                    case "Float":
                        SnapshotGlobal(id, first.Type);
                        Shader.SetGlobalFloat(id, v[0]);
                        break;
                }
            }
        }

        private void SnapshotMaterialProperty(Material mat, string id, string type)
        {
            Dictionary<string, object> snap;
            if (!materialOriginals.TryGetValue(mat, out snap))
                materialOriginals[mat] = snap = new Dictionary<string, object>();
            if (snap.ContainsKey(id) || !mat.HasProperty(id)) return;
            switch (type)
            {
                case "Color": snap[id] = mat.GetColor(id); break;
                case "Vector": snap[id] = mat.GetVector(id); break;
                case "Float": snap[id] = mat.GetFloat(id); break;
                default: snap[id] = null; break;
            }
        }

        private void RestoreMaterialProperty(Material mat, string id, string type)
        {
            Dictionary<string, object> snap;
            object value;
            if (!materialOriginals.TryGetValue(mat, out snap) || !snap.TryGetValue(id, out value) || value == null)
                return;
            if (value is Color) mat.SetColor(id, (Color)value);
            else if (value is Vector4) mat.SetVector(id, (Vector4)value);
            else if (value is float) mat.SetFloat(id, (float)value);
        }

        private void RestoreMaterials()
        {
            foreach (var matPair in materialOriginals)
            {
                if (matPair.Key == null) continue;
                foreach (var snap in matPair.Value)
                {
                    if (snap.Value is Color) matPair.Key.SetColor(snap.Key, (Color)snap.Value);
                    else if (snap.Value is Vector4) matPair.Key.SetVector(snap.Key, (Vector4)snap.Value);
                    else if (snap.Value is float) matPair.Key.SetFloat(snap.Key, (float)snap.Value);
                }
            }
            materialOriginals.Clear();
        }

        private void SnapshotGlobal(string id, string type)
        {
            if (globalOriginals.ContainsKey(id)) return;
            switch (type)
            {
                case "Color": globalOriginals[id] = Shader.GetGlobalColor(id); break;
                case "Vector": globalOriginals[id] = Shader.GetGlobalVector(id); break;
                case "Float": globalOriginals[id] = Shader.GetGlobalFloat(id); break;
            }
        }

        private void RestoreGlobals()
        {
            foreach (var pair in globalOriginals)
            {
                if (pair.Value is Color) Shader.SetGlobalColor(pair.Key, (Color)pair.Value);
                else if (pair.Value is Vector4) Shader.SetGlobalVector(pair.Key, (Vector4)pair.Value);
                else if (pair.Value is float) Shader.SetGlobalFloat(pair.Key, (float)pair.Value);
            }
            globalOriginals.Clear();
        }

        private void ClearInstances()
        {
            foreach (var pair in instances)
                if (pair.Value != null)
                    Destroy(pair.Value);
            instances.Clear();
            foreach (var pair in trackNodes)
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            trackNodes.Clear();
        }
    }
}
