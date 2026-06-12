using System.Collections.Generic;
using UnityEngine;

namespace Vivified
{
    /// <summary>
    /// Blit post-processing, SetRenderingSettings, camera state/textures,
    /// animator properties, texture properties, and shader time freezing.
    /// </summary>
    public partial class VivifyPreview
    {
        private VivifyBlitRenderer blitRenderer;
        private VivifySelectionHighlight highlight;
        private CameraController cameraController;

        private readonly Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();
        private readonly Dictionary<string, RenderTexture> screenTextures = new Dictionary<string, RenderTexture>();
        private readonly Dictionary<string, Camera> extraCameras = new Dictionary<string, Camera>();

        // render settings / camera snapshots for restore
        private bool renderSnapTaken;
        private bool snapFog;
        private Color snapFogColor;
        private float snapFogDensity, snapFogStart, snapFogEnd;
        private FogMode snapFogMode;
        private float snapAmbientIntensity;
        private Color snapAmbientLight, snapAmbientSky, snapAmbientEquator, snapAmbientGround;
        private UnityEngine.Rendering.AmbientMode snapAmbientMode;
        private float snapReflectionIntensity;
        private bool camSnapTaken;
        private CameraClearFlags snapClearFlags;
        private Color snapBackground;
        private bool timeScaleChanged;

        private void EnsureCameraComponents()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (blitRenderer == null)
            {
                blitRenderer = cam.GetComponent<VivifyBlitRenderer>();
                if (blitRenderer == null) blitRenderer = cam.gameObject.AddComponent<VivifyBlitRenderer>();
                blitRenderer.enabled = false;
            }
            if (highlight == null)
            {
                highlight = cam.GetComponent<VivifySelectionHighlight>();
                if (highlight == null) highlight = cam.gameObject.AddComponent<VivifySelectionHighlight>();
            }
            if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        }

        // --- Blit ----------------------------------------------------------------

        private void UpdateBlits(float beat)
        {
            EnsureCameraComponents();
            if (blitRenderer == null) return;
            blitRenderer.Active.Clear();

            // active = started, still inside duration window; ordered by priority
            var active = new List<BlitEvent>();
            foreach (var blit in engine.Blits)
            {
                if (blit.Beat > beat) continue;
                if (blit.Duration <= 0f) continue; // instantaneous: nothing to hold
                if (beat < blit.Beat + blit.Duration) active.Add(blit);
            }
            active.Sort((a, b) => a.Priority != b.Priority
                ? a.Priority.CompareTo(b.Priority)
                : a.Beat.CompareTo(b.Beat));

            foreach (var blit in active)
            {
                var mat = GetMaterial(blit.Asset);
                if (mat == null) continue;
                blitRenderer.Active.Add(new VivifyBlitRenderer.Entry { Material = mat, Pass = blit.Pass });
            }
            blitRenderer.enabled = blitRenderer.Active.Count > 0;
        }

        // --- SetRenderingSettings ---------------------------------------------------

        private void SnapshotRenderSettings()
        {
            if (renderSnapTaken) return;
            renderSnapTaken = true;
            snapFog = RenderSettings.fog;
            snapFogColor = RenderSettings.fogColor;
            snapFogDensity = RenderSettings.fogDensity;
            snapFogStart = RenderSettings.fogStartDistance;
            snapFogEnd = RenderSettings.fogEndDistance;
            snapFogMode = RenderSettings.fogMode;
            snapAmbientIntensity = RenderSettings.ambientIntensity;
            snapAmbientLight = RenderSettings.ambientLight;
            snapAmbientSky = RenderSettings.ambientSkyColor;
            snapAmbientEquator = RenderSettings.ambientEquatorColor;
            snapAmbientGround = RenderSettings.ambientGroundColor;
            snapAmbientMode = RenderSettings.ambientMode;
            snapReflectionIntensity = RenderSettings.reflectionIntensity;
        }

        private void RestoreRenderSettings()
        {
            if (!renderSnapTaken) return;
            RenderSettings.fog = snapFog;
            RenderSettings.fogColor = snapFogColor;
            RenderSettings.fogDensity = snapFogDensity;
            RenderSettings.fogStartDistance = snapFogStart;
            RenderSettings.fogEndDistance = snapFogEnd;
            RenderSettings.fogMode = snapFogMode;
            RenderSettings.ambientIntensity = snapAmbientIntensity;
            RenderSettings.ambientLight = snapAmbientLight;
            RenderSettings.ambientSkyColor = snapAmbientSky;
            RenderSettings.ambientEquatorColor = snapAmbientEquator;
            RenderSettings.ambientGroundColor = snapAmbientGround;
            RenderSettings.ambientMode = snapAmbientMode;
            RenderSettings.reflectionIntensity = snapReflectionIntensity;
            renderSnapTaken = false;
        }

        private void ApplyRenderSettingsNow(float beat)
        {
            if (engine.RenderSettingEvents.Count == 0) return;
            foreach (var pair in engine.RenderSettingEvents)
            {
                var first = pair.Value[0];
                int dim = first.Type == "Color" ? 4 : 1;
                var v = TrackEngine.CurrentValue(pair.Value, beat, dim);
                if (v == null)
                {
                    continue; // before first event: snapshot state still applies
                }
                SnapshotRenderSettings();
                Color color = dim == 4
                    ? new Color(v[0], v.Length > 1 ? v[1] : 0f, v.Length > 2 ? v[2] : 0f,
                        v.Length > 3 ? v[3] : 1f)
                    : Color.black;
                float f = v[0];
                switch (pair.Key)
                {
                    case "renderSettings.fog": RenderSettings.fog = f > 0.5f; break;
                    case "renderSettings.fogColor": RenderSettings.fogColor = color; break;
                    case "renderSettings.fogDensity": RenderSettings.fogDensity = f; break;
                    case "renderSettings.fogStartDistance": RenderSettings.fogStartDistance = f; break;
                    case "renderSettings.fogEndDistance": RenderSettings.fogEndDistance = f; break;
                    case "renderSettings.fogMode": RenderSettings.fogMode = (FogMode)(int)f; break;
                    case "renderSettings.ambientIntensity": RenderSettings.ambientIntensity = f; break;
                    case "renderSettings.ambientLight": RenderSettings.ambientLight = color; break;
                    case "renderSettings.ambientSkyColor": RenderSettings.ambientSkyColor = color; break;
                    case "renderSettings.ambientEquatorColor": RenderSettings.ambientEquatorColor = color; break;
                    case "renderSettings.ambientGroundColor": RenderSettings.ambientGroundColor = color; break;
                    case "renderSettings.ambientMode":
                        RenderSettings.ambientMode = (UnityEngine.Rendering.AmbientMode)(int)f;
                        break;
                    case "renderSettings.reflectionIntensity": RenderSettings.reflectionIntensity = f; break;
                    case "qualitySettings.shadowDistance": QualitySettings.shadowDistance = f; break;
                    case "qualitySettings.antiAliasing": QualitySettings.antiAliasing = (int)f; break;
                    // other fields have no editor analogue; ignore
                }
            }
        }

        // --- camera state, screen textures, extra cameras -------------------------

        private void ApplyCameraState(float beat)
        {
            var cam = Camera.main;
            if (cam == null) return;

            // clearFlags / backgroundColor from SetCameraProperty
            CameraPropEvent current = null;
            foreach (var cp in engine.CameraProps)
            {
                if (cp.Beat <= beat) current = cp;
                else break;
            }
            if (current != null)
            {
                if (!camSnapTaken)
                {
                    camSnapTaken = true;
                    snapClearFlags = cam.clearFlags;
                    snapBackground = cam.backgroundColor;
                }
                if (current.ClearFlags != null)
                {
                    switch (current.ClearFlags)
                    {
                        case "Skybox": cam.clearFlags = CameraClearFlags.Skybox; break;
                        case "SolidColor": cam.clearFlags = CameraClearFlags.SolidColor; break;
                        case "Depth": cam.clearFlags = CameraClearFlags.Depth; break;
                        case "Nothing": cam.clearFlags = CameraClearFlags.Nothing; break;
                    }
                }
                if (current.BackgroundColor != null)
                {
                    cam.backgroundColor = new Color(current.BackgroundColor[0], current.BackgroundColor[1],
                        current.BackgroundColor[2], current.BackgroundColor[3]);
                }
            }
            else if (camSnapTaken)
            {
                cam.clearFlags = snapClearFlags;
                cam.backgroundColor = snapBackground;
                camSnapTaken = false;
            }

            // screen textures: allocate + publish globally once defined
            foreach (var def in engine.ScreenTextures)
            {
                if (def.Beat > beat || string.IsNullOrEmpty(def.Id) || screenTextures.ContainsKey(def.Id))
                    continue;
                int w = def.Width > 0 ? def.Width : Mathf.Max(1, (int)(Screen.width * def.XRatio));
                int h = def.Height > 0 ? def.Height : Mathf.Max(1, (int)(Screen.height * def.YRatio));
                var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
                rt.name = "Vivify_" + def.Id;
                rt.Create();
                screenTextures[def.Id] = rt;
                Shader.SetGlobalTexture(def.Id, rt);
            }

            // extra cameras rendering into global textures
            foreach (var def in engine.Cameras)
            {
                if (def.Beat > beat || string.IsNullOrEmpty(def.Id) || extraCameras.ContainsKey(def.Id))
                    continue;
                var go = new GameObject("VivifyCamera_" + def.Id);
                go.transform.SetParent(cam.transform, false);
                var extra = go.AddComponent<Camera>();
                extra.CopyFrom(cam);
                extra.depth = cam.depth - 1f;
                if (!string.IsNullOrEmpty(def.Texture))
                {
                    RenderTexture rt;
                    if (!screenTextures.TryGetValue(def.Texture, out rt))
                    {
                        rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf);
                        rt.name = "Vivify_" + def.Texture;
                        rt.Create();
                        screenTextures[def.Texture] = rt;
                    }
                    extra.targetTexture = rt;
                    Shader.SetGlobalTexture(def.Texture, rt);
                }
                if (!string.IsNullOrEmpty(def.DepthTexture))
                    extra.depthTextureMode |= DepthTextureMode.Depth;
                extraCameras[def.Id] = extra;
            }
        }

        // --- SetAnimatorProperty -----------------------------------------------------

        private void ApplyAnimatorProps(float beat)
        {
            if (engine.AnimatorProps.Count == 0) return;
            // latest value per (target, property)
            var latest = new Dictionary<string, AnimatorPropEvent>();
            foreach (var ap in engine.AnimatorProps)
            {
                if (ap.Beat > beat) break;
                latest[ap.TargetId + "\n" + ap.Id] = ap;
            }
            if (latest.Count == 0) return;

            foreach (var pair in instances)
            {
                var spawn = pair.Key;
                var state = pair.Value;
                if (state.Go == null || state.Animators == null || state.Animators.Length == 0) continue;
                foreach (var entry in latest.Values)
                {
                    if (entry.TargetId != spawn.Id) continue;
                    float s = entry.Duration > 0f
                        ? Mathf.Clamp01((beat - entry.Beat) / entry.Duration)
                        : 1f;
                    s = Easing.Interpolate(entry.EasingName, s);
                    float value = 0f;
                    if (entry.Def != null && entry.Def.HasKeys)
                    {
                        var v = entry.Def.Sample(s);
                        if (v != null) value = v[0];
                    }
                    else if (entry.StaticValue != null)
                    {
                        value = entry.StaticValue.IsBoolean ? (entry.StaticValue.AsBool ? 1f : 0f)
                            : entry.StaticValue.AsFloat;
                    }
                    for (int i = 0; i < state.Animators.Length; i++)
                    {
                        var animator = state.Animators[i];
                        if (animator == null) continue;
                        switch (entry.Type)
                        {
                            case "Bool": animator.SetBool(entry.Id, value > 0.5f); break;
                            case "Integer": animator.SetInteger(entry.Id, (int)value); break;
                            case "Trigger":
                                // triggers only fire near their event so scrubbing
                                // far past them doesn't re-fire constantly
                                if (beat - entry.Beat < 0.25f) animator.SetTrigger(entry.Id);
                                break;
                            default: animator.SetFloat(entry.Id, value); break;
                        }
                    }
                }
            }
        }

        // --- texture lookups -----------------------------------------------------------

        /// <summary>Texture-type property values: bundle asset path or screen texture id.</summary>
        private Texture GetTextureValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            RenderTexture rt;
            if (screenTextures.TryGetValue(value, out rt)) return rt;
            Texture cached;
            if (textureCache.TryGetValue(value, out cached)) return cached;
            Texture tex = null;
            string real;
            if (bundle != null && assetNames.TryGetValue(value.ToLowerInvariant(), out real))
                tex = bundle.LoadAsset<Texture>(real);
            textureCache[value] = tex;
            return tex;
        }

        // --- shader time freeze ------------------------------------------------------

        /// <summary>
        /// Shaders animate on the engine-managed _Time, which cannot be
        /// overridden directly. Freezing Time.timeScale while paused is the one
        /// lever that exists - suspended whenever ChroMapper itself needs scaled
        /// time (camera movement, dialogs).
        /// </summary>
        private void UpdateTimeFreeze()
        {
            // Freezing while ChroMapper needs scaled time soft-locks the editor
            // (scene-transition fades, dialogs, camera). Every exemption here is
            // load-bearing; when in doubt, run at normal speed.
            bool freeze = VivifiedSettings.FreezeShaderTime &&
                          VivifiedSettings.PreviewEnabled &&
                          atsc != null && atsc.Initialized && !atsc.IsPlaying &&
                          !SceneTransitionManager.IsLoading &&
                          (cameraController == null || !cameraController.MovingCamera) &&
                          (PersistentUI.Instance == null || !PersistentUI.Instance.DialogBoxIsEnabled);
            if (freeze)
            {
                timeScaleChanged = true;
                Time.timeScale = 0f;
            }
            else if (timeScaleChanged || Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
                timeScaleChanged = false;
            }
        }

        private void CleanupExtras()
        {
            if (timeScaleChanged)
            {
                Time.timeScale = 1f;
                timeScaleChanged = false;
            }
            RestoreRenderSettings();
            if (camSnapTaken && Camera.main != null)
            {
                Camera.main.clearFlags = snapClearFlags;
                Camera.main.backgroundColor = snapBackground;
                camSnapTaken = false;
            }
            foreach (var pair in extraCameras)
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            extraCameras.Clear();
            foreach (var pair in screenTextures)
                if (pair.Value != null)
                {
                    pair.Value.Release();
                    Destroy(pair.Value);
                }
            screenTextures.Clear();
            if (blitRenderer != null) Destroy(blitRenderer);
            if (highlight != null) Destroy(highlight);
        }
    }
}
