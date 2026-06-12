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
- Toolbar buttons (right-side panel):
  - **Vivified** dialog: preview toggle, world offset, reload bundle, rebuild.
  - **Custom data editor**: edit the selected object's custom JSON with undo.

## Building

```
msbuild Vivified.csproj /p:Configuration=Release
```

Set `ChroMapperDir` (in the csproj or via `/p:ChroMapperDir=...`) to your
ChroMapper install — the folder containing `ChroMapper_Data`. The built DLL
auto-deploys to `<ChroMapper>\Plugins`.

Targets .NET Framework 4.8 (required: ChroMapper's Main.dll references
net4.8 assemblies).
