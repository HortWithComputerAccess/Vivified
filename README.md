# Vivified

A browser-based 3D editor for [Vivify](https://github.com/Aeroluna/Vivify) (Beat Saber mod) maps:
load Unity AssetBundles (`.vivify`), inspect prefabs in 3D, and edit custom events —
`InstantiatePrefab` spawns, `AnimateTrack` movement, `DestroyObject`, material/camera events and more —
with a beat timeline synced to the song.

Everything runs client-side. The AssetBundle parser (UnityFS, LZMA/LZ4, typetrees, meshes,
materials, DXT/BC7 textures) is implemented in TypeScript — no Unity install needed.

## Run it

```sh
npm install
npm run dev      # open the printed localhost URL in Chrome/Edge
```

Use **Chrome or Edge** — saving in place uses the File System Access API
(other browsers fall back to downloading the edited `.dat`).

## Usage

| Action | How |
|---|---|
| Open a map | **Open Map** → pick the map folder (must contain `Info.dat`). Audio, difficulty and `.vivify` bundle load automatically. |
| Open just a bundle | **Open Bundle** → pick a `.vivify` / AssetBundle file to browse and preview prefabs. |
| Scrub time | Click/drag the timeline, arrow keys, or the beat box. **Space** plays the song. |
| Spawn a prefab | Assets tab → `+` on a prefab → creates an `InstantiatePrefab` event at the current beat. |
| Move/rotate/scale a spawn | Click the object in the viewport, use the gizmo (**W/E/R** modes). Transform writes back into the event data. |
| Edit any event | Events tab or timeline marker → inspector shows beat, type, and raw JSON (validated live). |
| Move an event in time | Drag its marker on the timeline (snaps to 1/4 beat; hold **Alt** for free). |
| Add / delete / duplicate | **+ Add** button, inspector **Delete**/**Duplicate**, or **Del** key. |
| Focus selection | **F** frames the selected object (environments can be huge). |
| Undo / redo / save | **Ctrl+Z / Ctrl+Y / Ctrl+S** (writes the difficulty `.dat` in place). |
| **Animate visually** | Toggle **●Key** (auto-key): every gizmo edit writes an `AnimateTrack` keyframe at the current beat. Or press **K**/**+Key** to key the current pose. Keys appear as diamonds on the timeline; the event window and normalized keyframe times are managed for you. |
| Edit materials | Click a material in the Assets tab: shows the shader, how it was recreated (additive/alpha/opaque), and live-editable colors/floats. **+ SetMaterialProperty @ beat** turns your edits into an event. |
| Inspect textures | Click a texture in the Assets tab for a full preview (DXT1/5, BC4/5, BC7 and uncompressed formats decode in-browser). |
| Notes preview | **Notes** toggles notes, bombs, chains and walls flying at the player with correct jump math, map color scheme, Noodle coordinates, and `AssignObjectPrefab` note skins tinted per color. Note tracks follow `AssignPathAnimation` offsetPosition and `AnimateTrack` offset/dissolve/scale. |
| Player POV | **POV** switches to the player's first-person view, following `AssignPlayerToTrack` animation, with sabers (custom saber prefabs when assigned). |
| View modes | **1 Rendered** recreates the game look (shader recreation, particles, glow). **2 Indexed** renders flat unlit with one distinct color per material — ideal for telling parts apart. **3 Unshaded** uses a plain lit material everywhere for clean geometry inspection. |

The viewport evaluates the Heck animation engine over time: `AnimateTrack`
(position/rotation/scale/dissolve with point definitions, easings, splines, repeat),
`InstantiatePrefab`/`DestroyObject` lifetimes, `AssignTrackParent` chains, and
`SetMaterialProperty` color/float animations applied live to the converted materials.

**Shader recreation:** the editor parses each shader's serialized render state
(blend factors — including material-property-driven ones — culling, depth write,
lighting flag, render queue) and rebuilds the equivalent Three.js material:
additive glow, alpha blending, multiply, unlit full-bright for non-lit/VFX/HDR
shaders, or a standard lit fallback. Compiled HLSL can't run in the browser, so
custom vertex animation and post-processing still need the game for an exact
look — active `Blit`/camera effects are listed in the viewport HUD instead.

**Also recreated in the browser view:**
- Unity **AnimationClips**: prefabs with Animators play their controller's
  clip — including the release-build "muscle clip" format (StreamedClip/
  DenseClip/ConstantClip decoded, bindings resolved by CRC32 path hashes),
  starting at the prefab's spawn beat and synced to song time.
- Unity's **built-in primitive meshes** (Quad/Sphere/Plane/Cube/Cylinder/
  Capsule from `unity default resources`) are substituted with three.js
  geometry, so billboards and primitive-based children render.
- **Particle systems**: each emitter renders an approximated particle cloud
  (shape, rate, lifetime, speed, size, start color, gravity, renderer
  material/texture and blending) animated with song time.
- **SetRenderingSettings** fog distances and ambient intensity applied to the
  scene live, with duration/easing.
- **Lighting events** (types 0–5) tint the scene lights using the difficulty's
  environment colors and Chroma per-event colors, with flash/fade decay.

## What it can't do (yet)

- Custom shaders, `Blit` post-processing, and particle systems are approximated
  (standard materials / placeholders) — accuracy requires the game itself.
- `AssignObjectPrefab` note/saber skins and note path animations aren't visualized
  (the events are still fully editable).
- Beatmap **v3** only for editing. BC6H (HDR) textures show as placeholders.
- Bundles must include typetrees (Unity builds them in by default).

## Development

```sh
npm run test:bundle   # parse the reference .vivify bundles end-to-end
npm run test:map      # animation engine against the reference map
npm run build         # typecheck + production build
npx tsx scripts/verify-browser.ts   # headless browser E2E + screenshots in .verify/
```

`src/unity/` — UnityFS/SerializedFile/typetree parser, mesh/material/texture decoding, Three.js conversion.
`src/anim/` — Heck point definitions, easings, track evaluation.
`src/map/` — beatmap v3 + Info.dat IO, BPM mapping.
`src/editor/` — viewport, timeline, panels, audio.
