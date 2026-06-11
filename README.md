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

The viewport evaluates the Heck animation engine over time: `AnimateTrack`
(position/rotation/scale/dissolve with point definitions, easings, splines, repeat),
`InstantiatePrefab`/`DestroyObject` lifetimes, and `AssignTrackParent` chains.

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
