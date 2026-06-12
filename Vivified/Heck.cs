using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base.Customs;
using SimpleJSON;
using UnityEngine;

namespace Vivified
{
    /// <summary>Heck easing functions (easings.net naming).</summary>
    public static class Easing
    {
        public static float Interpolate(string name, float t)
        {
            if (string.IsNullOrEmpty(name)) return t;
            switch (name)
            {
                case "easeLinear": return t;
                case "easeStep": return t >= 1f ? 1f : 0f;
                case "easeInQuad": return t * t;
                case "easeOutQuad": return 1f - (1f - t) * (1f - t);
                case "easeInOutQuad": return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                case "easeInCubic": return t * t * t;
                case "easeOutCubic": return 1f - Mathf.Pow(1f - t, 3f);
                case "easeInOutCubic": return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                case "easeInQuart": return t * t * t * t;
                case "easeOutQuart": return 1f - Mathf.Pow(1f - t, 4f);
                case "easeInOutQuart": return t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;
                case "easeInQuint": return t * t * t * t * t;
                case "easeOutQuint": return 1f - Mathf.Pow(1f - t, 5f);
                case "easeInOutQuint": return t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;
                case "easeInSine": return 1f - Mathf.Cos(t * Mathf.PI / 2f);
                case "easeOutSine": return Mathf.Sin(t * Mathf.PI / 2f);
                case "easeInOutSine": return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
                case "easeInExpo": return t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
                case "easeOutExpo": return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
                case "easeInOutExpo":
                    if (t == 0f || t == 1f) return t;
                    return t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
                case "easeInCirc": return 1f - Mathf.Sqrt(1f - t * t);
                case "easeOutCirc": return Mathf.Sqrt(1f - (t - 1f) * (t - 1f));
                case "easeInOutCirc":
                    return t < 0.5f
                        ? (1f - Mathf.Sqrt(1f - 4f * t * t)) / 2f
                        : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
                case "easeInBack": return 2.70158f * t * t * t - 1.70158f * t * t;
                case "easeOutBack": return 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
                case "easeInOutBack":
                {
                    const float c2 = 1.70158f * 1.525f;
                    return t < 0.5f
                        ? Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2) / 2f
                        : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
                }
                case "easeInElastic":
                    if (t == 0f || t == 1f) return t;
                    return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * (2f * Mathf.PI / 3f));
                case "easeOutElastic":
                    if (t == 0f || t == 1f) return t;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
                case "easeInOutElastic":
                    if (t == 0f || t == 1f) return t;
                    return t < 0.5f
                        ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * (2f * Mathf.PI / 4.5f))) / 2f
                        : Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * (2f * Mathf.PI / 4.5f)) / 2f + 1f;
                case "easeInBounce": return 1f - BounceOut(1f - t);
                case "easeOutBounce": return BounceOut(t);
                case "easeInOutBounce":
                    return t < 0.5f ? (1f - BounceOut(1f - 2f * t)) / 2f : (1f + BounceOut(2f * t - 1f)) / 2f;
                default: return t;
            }
        }

        private static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }

    /// <summary>
    /// A Heck point definition: keyframes of N values + time, with optional
    /// per-point easing and catmull-rom spline flags. Named references are
    /// resolved against the map's pointDefinitions.
    /// </summary>
    public class PointDefinition
    {
        private struct Key
        {
            public float[] Values;
            public float Time;
            public string EasingName;
            public bool Spline;
        }

        private readonly List<Key> keys = new List<Key>();
        private readonly int dimension;
        public bool Unsupported { get; private set; }

        public static PointDefinition Parse(JSONNode raw, int dimension, Dictionary<string, JSONArray> pointDefinitions)
        {
            if (raw == null) return null;
            if (raw.IsString)
            {
                JSONArray named;
                if (pointDefinitions != null && pointDefinitions.TryGetValue(raw.Value, out named))
                    return Parse(named, dimension, pointDefinitions);
                var missing = new PointDefinition(dimension);
                missing.Unsupported = true;
                return missing;
            }
            if (raw.IsNumber)
            {
                var single = new PointDefinition(dimension);
                single.keys.Add(new Key { Values = new[] { raw.AsFloat }, Time = 0f });
                return single;
            }
            if (!raw.IsArray) return null;

            var def = new PointDefinition(dimension);
            var arr = raw.AsArray;

            // flat constant form: [x, y, z]
            bool flat = arr.Count > 0;
            for (int i = 0; i < arr.Count; i++)
                if (arr[i].IsArray) { flat = false; break; }
            if (flat)
            {
                var values = new List<float>();
                for (int i = 0; i < arr.Count; i++)
                    if (arr[i].IsNumber) values.Add(arr[i].AsFloat);
                if (values.Count == 0) { def.Unsupported = true; return def; }
                def.keys.Add(new Key { Values = values.ToArray(), Time = 0f });
                return def;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                var entry = arr[i];
                if (!entry.IsArray) continue;
                var nums = new List<float>();
                string easing = null;
                bool spline = false;
                var e = entry.AsArray;
                for (int j = 0; j < e.Count; j++)
                {
                    var item = e[j];
                    if (item.IsNumber) nums.Add(item.AsFloat);
                    else if (item.IsString)
                    {
                        var s = item.Value;
                        if (s.StartsWith("ease")) easing = s;
                        else if (s == "splineCatmullRom") spline = true;
                        else if (s != "lerpHSV") def.Unsupported = true; // base providers / modifiers
                    }
                    else if (item.IsArray) def.Unsupported = true;
                }
                if (nums.Count == 0) { def.Unsupported = true; continue; }
                float time = nums[nums.Count - 1];
                nums.RemoveAt(nums.Count - 1);
                def.keys.Add(new Key { Values = nums.ToArray(), Time = time, EasingName = easing, Spline = spline });
            }
            def.keys.Sort((a, b) => a.Time.CompareTo(b.Time));
            return def;
        }

        private PointDefinition(int dimension) => this.dimension = dimension;

        public bool HasKeys => keys.Count > 0;

        public float[] Sample(float t)
        {
            if (keys.Count == 0) return null;
            if (keys.Count == 1 || t <= keys[0].Time) return Pad(keys[0].Values);
            var last = keys[keys.Count - 1];
            if (t >= last.Time) return Pad(last.Values);

            int i = 1;
            while (i < keys.Count && keys[i].Time < t) i++;
            var k0 = keys[i - 1];
            var k1 = keys[i];
            float span = k1.Time - k0.Time;
            float s = span > 0f ? (t - k0.Time) / span : 1f;
            s = Easing.Interpolate(k1.EasingName, s);

            var a = Pad(k0.Values);
            var b = Pad(k1.Values);
            if (k1.Spline && keys.Count > 2)
            {
                var p0 = Pad(keys[Mathf.Max(0, i - 2)].Values);
                var p3 = Pad(keys[Mathf.Min(keys.Count - 1, i + 1)].Values);
                var outv = new float[dimension];
                for (int d = 0; d < dimension; d++)
                {
                    float t2 = s * s;
                    float t3 = t2 * s;
                    outv[d] = 0.5f * (2f * a[d] + (-p0[d] + b[d]) * s +
                        (2f * p0[d] - 5f * a[d] + 4f * b[d] - p3[d]) * t2 +
                        (-p0[d] + 3f * a[d] - 3f * b[d] + p3[d]) * t3);
                }
                return outv;
            }
            var result = new float[dimension];
            for (int d = 0; d < dimension; d++) result[d] = a[d] + (b[d] - a[d]) * s;
            return result;
        }

        private float[] Pad(float[] v)
        {
            if (v.Length >= dimension) return v;
            var outv = new float[dimension];
            Array.Copy(v, outv, v.Length);
            return outv;
        }
    }

    public class TransformState
    {
        public Vector3? Position;
        public Vector3? LocalPosition;
        public Vector3? Rotation; // euler degrees
        public Vector3? LocalRotation;
        public Vector3? Scale;
        public float? Dissolve;
    }

    public class PrefabSpawn
    {
        public BaseCustomEvent Event;
        public string Id;
        public string Asset;
        public List<string> Tracks = new List<string>();
        public float SpawnBeat;
        public float? DestroyBeat;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }

    public class MaterialPropertyEvent
    {
        public float Beat;
        public float Duration;
        public string EasingName;
        public string Id;
        public string Type; // Color | Float | Vector | Texture | Keyword
        public JSONNode StaticValue;
        public PointDefinition Def;
    }

    public class BlitEvent
    {
        public float Beat;
        public float Duration;
        public int Priority;
        public int Pass = -1;
        public string Asset;
    }

    public class ScreenTextureDef
    {
        public float Beat;
        public string Id;
        public float XRatio = 1f;
        public float YRatio = 1f;
        public int Width;
        public int Height;
    }

    public class CameraDef
    {
        public float Beat;
        public string Id;
        public string Texture;
        public string DepthTexture;
    }

    public class CameraPropEvent
    {
        public float Beat;
        public string ClearFlags;
        public float[] BackgroundColor;
    }

    public class AnimatorPropEvent
    {
        public float Beat;
        public float Duration;
        public string EasingName;
        public string TargetId;
        public string Id;
        public string Type; // Bool | Float | Integer | Trigger
        public JSONNode StaticValue;
        public PointDefinition Def;
    }

    /// <summary>
    /// Evaluates Vivify/Heck state over beats: prefab spawn lifetimes,
    /// AnimateTrack transforms, parent chains, and material/global properties.
    /// Mirrors the web editor's engine, in JsonTime space.
    /// </summary>
    public class TrackEngine
    {
        private static readonly string[] TransformProps =
            { "position", "localPosition", "rotation", "localRotation", "scale", "dissolve" };

        private class TimelineEntry
        {
            public float Beat;
            public float Duration;
            public int Repeat;
            public string EasingName;
            public PointDefinition Def;
        }

        public readonly List<PrefabSpawn> Spawns = new List<PrefabSpawn>();
        public readonly Dictionary<string, string> Parents = new Dictionary<string, string>();
        /// <summary>material asset path -> property id -> events sorted by beat</summary>
        public readonly Dictionary<string, Dictionary<string, List<MaterialPropertyEvent>>> MaterialEvents =
            new Dictionary<string, Dictionary<string, List<MaterialPropertyEvent>>>();
        public readonly Dictionary<string, List<MaterialPropertyEvent>> GlobalEvents =
            new Dictionary<string, List<MaterialPropertyEvent>>();
        public readonly List<BlitEvent> Blits = new List<BlitEvent>();
        /// <summary>"category.field" -> events, e.g. "renderSettings.fogColor"</summary>
        public readonly Dictionary<string, List<MaterialPropertyEvent>> RenderSettingEvents =
            new Dictionary<string, List<MaterialPropertyEvent>>();
        public readonly List<ScreenTextureDef> ScreenTextures = new List<ScreenTextureDef>();
        public readonly List<CameraDef> Cameras = new List<CameraDef>();
        public readonly List<CameraPropEvent> CameraProps = new List<CameraPropEvent>();
        public readonly List<AnimatorPropEvent> AnimatorProps = new List<AnimatorPropEvent>();

        private readonly Dictionary<string, Dictionary<string, List<TimelineEntry>>> timelines =
            new Dictionary<string, Dictionary<string, List<TimelineEntry>>>();

        public void Rebuild(IEnumerable<BaseCustomEvent> events, Dictionary<string, JSONArray> pointDefinitions)
        {
            Spawns.Clear();
            Parents.Clear();
            timelines.Clear();
            MaterialEvents.Clear();
            GlobalEvents.Clear();
            Blits.Clear();
            RenderSettingEvents.Clear();
            ScreenTextures.Clear();
            Cameras.Clear();
            CameraProps.Clear();
            AnimatorProps.Clear();

            int autoId = 0;
            foreach (var ev in events.OrderBy(e => e.JsonTime))
            {
                var d = ev.Data;
                if (d == null) continue;
                switch (ev.Type)
                {
                    case "InstantiatePrefab":
                    {
                        var spawn = new PrefabSpawn
                        {
                            Event = ev,
                            Asset = d["asset"].Value,
                            Id = d.HasKey("id") && !string.IsNullOrEmpty(d["id"].Value)
                                ? d["id"].Value
                                : "__auto_" + autoId++,
                            SpawnBeat = ev.JsonTime,
                            Position = ReadVec3(d, "localPosition", ReadVec3(d, "position", Vector3.zero)),
                            Rotation = ReadVec3(d, "localRotation", ReadVec3(d, "rotation", Vector3.zero)),
                            Scale = ReadVec3(d, "scale", Vector3.one),
                        };
                        ReadStringList(d["track"], spawn.Tracks);
                        Spawns.Add(spawn);
                        break;
                    }
                    case "DestroyObject":
                    {
                        var ids = new List<string>();
                        ReadStringList(d["id"], ids);
                        foreach (var id in ids)
                            foreach (var s in Spawns)
                                if (s.Id == id && !s.DestroyBeat.HasValue && s.SpawnBeat <= ev.JsonTime)
                                    s.DestroyBeat = ev.JsonTime;
                        break;
                    }
                    case "AnimateTrack":
                    {
                        var tracks = new List<string>();
                        ReadStringList(d["track"], tracks);
                        float duration = d.HasKey("duration") ? d["duration"].AsFloat : 0f;
                        int repeat = d.HasKey("repeat") ? d["repeat"].AsInt : 0;
                        string easing = d.HasKey("easing") ? d["easing"].Value : null;
                        foreach (var prop in TransformProps)
                        {
                            if (!d.HasKey(prop)) continue;
                            var def = PointDefinition.Parse(d[prop], prop == "dissolve" ? 1 : 3, pointDefinitions);
                            if (def == null || !def.HasKeys) continue;
                            foreach (var track in tracks)
                            {
                                Dictionary<string, List<TimelineEntry>> perTrack;
                                if (!timelines.TryGetValue(track, out perTrack))
                                    timelines[track] = perTrack = new Dictionary<string, List<TimelineEntry>>();
                                List<TimelineEntry> list;
                                if (!perTrack.TryGetValue(prop, out list))
                                    perTrack[prop] = list = new List<TimelineEntry>();
                                list.Add(new TimelineEntry
                                {
                                    Beat = ev.JsonTime,
                                    Duration = duration,
                                    Repeat = Mathf.Max(repeat, 0),
                                    EasingName = easing,
                                    Def = def,
                                });
                            }
                        }
                        break;
                    }
                    case "AssignTrackParent":
                    {
                        var children = new List<string>();
                        ReadStringList(d["childrenTracks"], children);
                        string parent = d["parentTrack"].Value;
                        if (!string.IsNullOrEmpty(parent))
                            foreach (var c in children)
                                Parents[c] = parent;
                        break;
                    }
                    case "SetMaterialProperty":
                    {
                        string asset = d["asset"].Value;
                        if (string.IsNullOrEmpty(asset) || !d.HasKey("properties")) break;
                        Dictionary<string, List<MaterialPropertyEvent>> perProp;
                        if (!MaterialEvents.TryGetValue(asset, out perProp))
                            MaterialEvents[asset] = perProp = new Dictionary<string, List<MaterialPropertyEvent>>();
                        AddPropertyEvents(perProp, ev, d, pointDefinitions);
                        break;
                    }
                    case "SetGlobalProperty":
                    {
                        if (!d.HasKey("properties")) break;
                        AddPropertyEvents(GlobalEvents, ev, d, pointDefinitions);
                        break;
                    }
                    case "SetRenderingSettings":
                    {
                        // categories: renderSettings / qualitySettings / xrSettings
                        foreach (var category in d.Keys)
                        {
                            if (category == "duration" || category == "easing") continue;
                            var cat = d[category];
                            if (cat == null || !cat.IsObject) continue;
                            foreach (var field in cat.Keys)
                            {
                                string key = category + "." + field;
                                var value = cat[field];
                                bool isColor = field.EndsWith("Color") || field == "ambientLight";
                                int dim = isColor ? 4 : 1;
                                PointDefinition def = null;
                                if (value != null && (value.IsArray || value.IsString))
                                    def = PointDefinition.Parse(value, dim, pointDefinitions);
                                List<MaterialPropertyEvent> list;
                                if (!RenderSettingEvents.TryGetValue(key, out list))
                                    RenderSettingEvents[key] = list = new List<MaterialPropertyEvent>();
                                list.Add(new MaterialPropertyEvent
                                {
                                    Beat = ev.JsonTime,
                                    Duration = d.HasKey("duration") ? d["duration"].AsFloat : 0f,
                                    EasingName = d.HasKey("easing") ? d["easing"].Value : null,
                                    Id = key,
                                    Type = isColor ? "Color" : "Float",
                                    StaticValue = value,
                                    Def = def,
                                });
                            }
                        }
                        break;
                    }
                    case "CreateScreenTexture":
                    {
                        ScreenTextures.Add(new ScreenTextureDef
                        {
                            Beat = ev.JsonTime,
                            Id = d["id"].Value,
                            XRatio = d.HasKey("xRatio") ? d["xRatio"].AsFloat : 1f,
                            YRatio = d.HasKey("yRatio") ? d["yRatio"].AsFloat : 1f,
                            Width = d.HasKey("width") ? d["width"].AsInt : 0,
                            Height = d.HasKey("height") ? d["height"].AsInt : 0,
                        });
                        break;
                    }
                    case "CreateCamera":
                    {
                        Cameras.Add(new CameraDef
                        {
                            Beat = ev.JsonTime,
                            Id = d["id"].Value,
                            Texture = d.HasKey("texture") ? d["texture"].Value : null,
                            DepthTexture = d.HasKey("depthTexture") ? d["depthTexture"].Value : null,
                        });
                        break;
                    }
                    case "SetCameraProperty":
                    {
                        var props = d["properties"];
                        if (props == null || !props.IsObject) break;
                        var cp = new CameraPropEvent { Beat = ev.JsonTime };
                        if (props.HasKey("clearFlags")) cp.ClearFlags = props["clearFlags"].Value;
                        if (props.HasKey("backgroundColor"))
                        {
                            var arr = props["backgroundColor"].AsArray;
                            if (arr != null && arr.Count >= 3)
                                cp.BackgroundColor = new[]
                                {
                                    arr[0].AsFloat, arr[1].AsFloat, arr[2].AsFloat,
                                    arr.Count > 3 ? arr[3].AsFloat : 1f,
                                };
                        }
                        if (cp.ClearFlags != null || cp.BackgroundColor != null) CameraProps.Add(cp);
                        break;
                    }
                    case "SetAnimatorProperty":
                    {
                        string targetId = d["id"].Value;
                        if (string.IsNullOrEmpty(targetId) || !d.HasKey("properties")) break;
                        var props = d["properties"].AsArray;
                        if (props == null) break;
                        for (int i = 0; i < props.Count; i++)
                        {
                            var p = props[i];
                            if (string.IsNullOrEmpty(p["id"].Value)) continue;
                            string type = p.HasKey("type") ? p["type"].Value : "Float";
                            PointDefinition def = null;
                            var value = p["value"];
                            if (value != null && (value.IsArray || value.IsString))
                                def = PointDefinition.Parse(value, 1, pointDefinitions);
                            AnimatorProps.Add(new AnimatorPropEvent
                            {
                                Beat = ev.JsonTime,
                                Duration = d.HasKey("duration") ? d["duration"].AsFloat : 0f,
                                EasingName = d.HasKey("easing") ? d["easing"].Value : null,
                                TargetId = targetId,
                                Id = p["id"].Value,
                                Type = type,
                                StaticValue = value,
                                Def = def,
                            });
                        }
                        break;
                    }
                    case "Blit":
                    {
                        string asset = d["asset"].Value;
                        if (string.IsNullOrEmpty(asset)) break;
                        Blits.Add(new BlitEvent
                        {
                            Beat = ev.JsonTime,
                            Duration = d.HasKey("duration") ? d["duration"].AsFloat : 0f,
                            Priority = d.HasKey("priority") ? d["priority"].AsInt : 0,
                            Pass = d.HasKey("pass") ? d["pass"].AsInt : -1,
                            Asset = asset,
                        });
                        // Blit events can animate their material's properties too
                        if (d.HasKey("properties"))
                        {
                            Dictionary<string, List<MaterialPropertyEvent>> perProp;
                            if (!MaterialEvents.TryGetValue(asset, out perProp))
                                MaterialEvents[asset] = perProp = new Dictionary<string, List<MaterialPropertyEvent>>();
                            AddPropertyEvents(perProp, ev, d, pointDefinitions);
                        }
                        break;
                    }
                }
            }
            foreach (var perTrack in timelines.Values)
                foreach (var list in perTrack.Values)
                    list.Sort((a, b) => a.Beat.CompareTo(b.Beat));
        }

        private static void AddPropertyEvents(
            Dictionary<string, List<MaterialPropertyEvent>> perProp,
            BaseCustomEvent ev, JSONNode d, Dictionary<string, JSONArray> pointDefinitions)
        {
            var props = d["properties"].AsArray;
            if (props == null) return;
            for (int i = 0; i < props.Count; i++)
            {
                var p = props[i];
                string id = p["id"].Value;
                if (string.IsNullOrEmpty(id)) continue;
                string type = p.HasKey("type") ? p["type"].Value : "Float";
                int dim = (type == "Color" || type == "Vector") ? 4 : 1;
                PointDefinition def = null;
                var value = p["value"];
                if (value != null && (value.IsArray || value.IsString))
                    def = PointDefinition.Parse(value, dim, pointDefinitions);
                List<MaterialPropertyEvent> list;
                if (!perProp.TryGetValue(id, out list)) perProp[id] = list = new List<MaterialPropertyEvent>();
                list.Add(new MaterialPropertyEvent
                {
                    Beat = ev.JsonTime,
                    Duration = d.HasKey("duration") ? d["duration"].AsFloat : 0f,
                    EasingName = d.HasKey("easing") ? d["easing"].Value : null,
                    Id = id,
                    Type = type,
                    StaticValue = value,
                    Def = def,
                });
            }
        }

        public List<PrefabSpawn> ActiveSpawns(float beat)
        {
            var outList = new List<PrefabSpawn>();
            foreach (var s in Spawns)
                if (s.SpawnBeat <= beat && (!s.DestroyBeat.HasValue || s.DestroyBeat.Value > beat))
                    outList.Add(s);
            return outList;
        }

        public TransformState Evaluate(string track, float beat)
        {
            var state = new TransformState();
            Dictionary<string, List<TimelineEntry>> perTrack;
            if (!timelines.TryGetValue(track, out perTrack)) return state;

            foreach (var pair in perTrack)
            {
                TimelineEntry entry = null;
                foreach (var e in pair.Value)
                {
                    if (e.Beat <= beat) entry = e;
                    else break;
                }
                if (entry == null) continue;

                float s;
                float totalDur = entry.Duration * (entry.Repeat + 1);
                if (entry.Duration <= 0f || beat >= entry.Beat + totalDur) s = 1f;
                else
                {
                    float elapsed = (beat - entry.Beat) % entry.Duration;
                    int cycle = (int)((beat - entry.Beat) / entry.Duration);
                    s = cycle > entry.Repeat ? 1f : elapsed / entry.Duration;
                }
                s = Easing.Interpolate(entry.EasingName, Mathf.Clamp01(s));

                var v = entry.Def.Sample(s);
                if (v == null) continue;
                switch (pair.Key)
                {
                    case "position": state.Position = new Vector3(v[0], v[1], v[2]); break;
                    case "localPosition": state.LocalPosition = new Vector3(v[0], v[1], v[2]); break;
                    case "rotation": state.Rotation = new Vector3(v[0], v[1], v[2]); break;
                    case "localRotation": state.LocalRotation = new Vector3(v[0], v[1], v[2]); break;
                    case "scale": state.Scale = new Vector3(v[0], v[1], v[2]); break;
                    case "dissolve": state.Dissolve = v[0]; break;
                }
            }
            return state;
        }

        public List<string> ParentChain(string track)
        {
            var chain = new List<string>();
            var seen = new HashSet<string>();
            string cur;
            Parents.TryGetValue(track, out cur);
            while (!string.IsNullOrEmpty(cur) && !seen.Contains(cur))
            {
                chain.Add(cur);
                seen.Add(cur);
                string next;
                Parents.TryGetValue(cur, out next);
                cur = next;
            }
            return chain;
        }

        /// <summary>Latest property event at or before the beat.</summary>
        public static MaterialPropertyEvent CurrentEvent(List<MaterialPropertyEvent> list, float beat)
        {
            MaterialPropertyEvent entry = null;
            foreach (var e in list)
            {
                if (e.Beat <= beat) entry = e;
                else break;
            }
            return entry;
        }

        /// <summary>Current value of a property event list at a beat (eased over duration).</summary>
        public static float[] CurrentValue(List<MaterialPropertyEvent> list, float beat, int dim)
        {
            MaterialPropertyEvent entry = null;
            foreach (var e in list)
            {
                if (e.Beat <= beat) entry = e;
                else break;
            }
            if (entry == null) return null;
            float s = entry.Duration > 0f ? Mathf.Clamp01((beat - entry.Beat) / entry.Duration) : 1f;
            s = Easing.Interpolate(entry.EasingName, s);
            if (entry.Def != null && entry.Def.HasKeys) return entry.Def.Sample(s);
            if (entry.StaticValue != null && entry.StaticValue.IsNumber) return new[] { entry.StaticValue.AsFloat };
            return null;
        }

        private static Vector3 ReadVec3(JSONNode d, string key, Vector3 fallback)
        {
            if (d == null || !d.HasKey(key)) return fallback;
            var arr = d[key].AsArray;
            if (arr == null || arr.Count < 3) return fallback;
            return new Vector3(arr[0].AsFloat, arr[1].AsFloat, arr[2].AsFloat);
        }

        private static void ReadStringList(JSONNode node, List<string> outList)
        {
            if (node == null) return;
            if (node.IsString)
            {
                if (!string.IsNullOrEmpty(node.Value)) outList.Add(node.Value);
            }
            else if (node.IsArray)
            {
                var arr = node.AsArray;
                for (int i = 0; i < arr.Count; i++)
                    if (arr[i].IsString) outList.Add(arr[i].Value);
            }
        }
    }
}
