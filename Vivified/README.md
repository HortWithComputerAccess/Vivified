# Vivified — ChroMapper plugin

Previews [Vivify](https://github.com/Aeroluna/Vivify) maps inside ChroMapper.
ChroMapper runs the same Unity built-in render pipeline as Beat Saber, so the
map's `.vivify` AssetBundle is loaded **natively** — prefabs render with their
real shaders, no recreation needed.

## What it does

- Loads the map folder's `.vivify` bundle on entering the editor
  (prefers `bundleWindows2021` — ChroMapper is Unity 2021.3 — falls back to 2019).
- Spawns **InstantiatePrefab** prefabs over playback/scrub time and removes
  them at **DestroyObject**, with spawn position/rotation/scale.
- Animates tracks: **AnimateTrack** position/localPosition/rotation/
  localRotation/scale/dissolve with full Heck point definitions (named refs,
  per-point easings, catmull-rom splines, repeat), and **AssignTrackParent**
  chains as real transform hierarchies.
- Applies **SetMaterialProperty** and **SetGlobalProperty**
  (Color/Float/Vector/Keyword, static and animated) to the actual bundle
  materials live — originals are restored when the preview is disabled or the
  editor exits.
- **Blit** post-processing applied to the editor camera with the real bundle
  materials (priority/pass ordering, per-event property animation).
- **SetRenderingSettings** (fog, ambient, quality), **SetCameraProperty**
  (clearFlags, backgroundColor, depthTextureMode), **CreateScreenTexture** and
  **CreateCamera** (render textures published as shader globals),
  **SetAnimatorProperty** (Bool/Float/Integer/Trigger on spawned prefabs),
  and Texture-type properties (bundle textures or screen texture ids).
- **Song-synced playback**: Animators, legacy Animations and particle systems
  on spawned prefabs are driven from song-time-since-spawn — scrubbing and
  rewinding work in both directions, and everything freezes when paused.
  Optionally the plugin also freezes engine time while paused ("Freeze shader
  time") so shaders animating on _Time stop too; this suspends itself while
  the camera moves or a dialog is open.
- **Authoring inside ChroMapper**: spawn any bundle prefab as an undoable
  InstantiatePrefab event at the playhead; Edit Mode lets you click-select a
  Vivify object in the 3D view and drag it to move it (writes back into the
  event, undoable); position/rotation/scale fields, duplicate and delete; and
  starter templates for every Vivify/Heck event type.
- Toolbar buttons (right-side panel):
  - **Vivified** dialog: all of the above (toggles, offsets, spawn/add/edit).
  - **Custom data editor**: edit the selected object's custom JSON with undo.

Not previewed: AssignObjectPrefab note/saber/wall skins (would require
replacing ChroMapper's own object visuals) and AssignPathAnimation note paths.
Both remain editable as events.

## Building

```
msbuild Vivified.csproj /p:Configuration=Release
```

Set `ChroMapperDir` (in the csproj or via `/p:ChroMapperDir=...`) to your
ChroMapper install — the folder containing `ChroMapper_Data`. The built DLL
auto-deploys to `<ChroMapper>\Plugins`.

Targets .NET Framework 4.8 (required: ChroMapper's Main.dll references
net4.8 assemblies).
