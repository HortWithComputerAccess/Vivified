import './style.css';
import * as THREE from 'three';
import { AssetDB } from './unity/assets';
import { ThreeConverter } from './unity/toThree';
import { TrackEngine } from './anim/tracks';
import { BpmMap } from './map/bpm';
import {
  openMapFolder,
  loadMapFromFiles,
  loadDifficulty,
  saveDifficulty,
  songFile,
  bundleFiles,
  LoadedMap,
  LoadedDifficulty,
} from './map/mapio';
import { CustomEvent, ALL_EVENT_TYPES } from './map/types';
import { Viewport, GizmoMode } from './editor/viewport';
import { Timeline } from './editor/timeline';
import { EventListPanel, Inspector, AssetBrowser, makeEventFromTemplate } from './editor/panels';
import { SongPlayer } from './editor/audio';

const $ = <T extends HTMLElement = HTMLElement>(sel: string) => document.querySelector(sel) as T;

// --- state ---------------------------------------------------------------
let map: LoadedMap | null = null;
let diff: LoadedDifficulty | null = null;
let bpm = new BpmMap(120);
let engine: TrackEngine | null = null;
let db: AssetDB | null = null;
let beat = 0;
let dirty = false;
const undoStack: string[] = [];
const redoStack: string[] = [];

// --- components ----------------------------------------------------------
const viewport = new Viewport($('#viewport') as HTMLCanvasElement);
const timeline = new Timeline($('#timeline') as HTMLCanvasElement);
const eventList = new EventListPanel($('#event-list'), $('#event-filter') as HTMLInputElement);
const inspector = new Inspector($('#inspector'));
const assetBrowser = new AssetBrowser($('#asset-browser'));
const player = new SongPlayer();

const statusEl = $('#status');
const beatDisplay = $('#beat-display');
const beatInput = $('#beat-input') as HTMLInputElement;
const playBtn = $('#btn-play') as HTMLButtonElement;
const diffSelect = $('#difficulty-select') as HTMLSelectElement;
const hud = $('#viewport-hud');

function status(msg: string): void {
  statusEl.textContent = msg;
  console.log('[vivified]', msg);
}

function events(): CustomEvent[] {
  return diff?.data.customData?.customEvents ?? [];
}

// --- undo / redo -----------------------------------------------------------
function commit(): void {
  if (!diff) return;
  undoStack.push(JSON.stringify(events()));
  if (undoStack.length > 100) undoStack.shift();
  redoStack.length = 0;
  dirty = true;
}

function undo(): void {
  if (!diff || undoStack.length === 0) return;
  redoStack.push(JSON.stringify(events()));
  diff.data.customData!.customEvents = JSON.parse(undoStack.pop()!);
  afterEventsChanged(null);
  status('undo');
}

function redo(): void {
  if (!diff || redoStack.length === 0) return;
  undoStack.push(JSON.stringify(events()));
  diff.data.customData!.customEvents = JSON.parse(redoStack.pop()!);
  afterEventsChanged(null);
  status('redo');
}

// --- refresh pipeline ------------------------------------------------------
function afterEventsChanged(keepSelected: CustomEvent | null): void {
  if (!diff) return;
  engine = TrackEngine.fromBeatmap(diff.data);
  const endBeat = player.loaded ? bpm.beatAt(player.duration) : 64;
  timeline.setEvents(events(), endBeat);
  eventList.setEvents(events());
  if (keepSelected && events().includes(keepSelected)) {
    selectEvent(keepSelected, false);
  } else if (keepSelected === null) {
    // keep current selection if still valid
    const sel = inspector.event;
    if (sel && !events().includes(sel)) selectEvent(null, false);
    else {
      timeline.setSelected(sel);
      eventList.setSelected(sel);
    }
  }
  updateHud();
}

function selectEvent(ev: CustomEvent | null, scroll = true): void {
  inspector.show(ev);
  timeline.setSelected(ev);
  eventList.setSelected(ev);
  if (ev && ev.t === 'InstantiatePrefab' && engine) {
    const inst = engine.instances.find((i) => i.event === ev);
    viewport.selectInstanceById(inst ? inst.id : null);
  } else {
    viewport.selectInstanceById(null);
  }
  if (scroll) eventList.scrollToSelected();
}

function setBeat(b: number, fromTimeline = false): void {
  beat = Math.max(0, b);
  beatDisplay.textContent = `beat ${beat.toFixed(2)}`;
  if (document.activeElement !== beatInput) beatInput.value = beat.toFixed(2);
  if (!fromTimeline) timeline.setBeat(beat);
  if (player.playing) {
    player.play(bpm.secondsAt(beat));
  }
}

function updateHud(): void {
  const lines: string[] = [];
  if (map) lines.push(`${map.info._songName ?? 'map'} — ${diff?.ref.characteristic ?? ''} ${diff?.ref.difficulty ?? ''}`);
  if (engine) {
    const act = engine.activeInstances(beat).length;
    lines.push(`${events().length} custom events · ${engine.instances.length} prefab spawns · ${act} active`);
  }
  if (db) lines.push(`bundle: ${db.containers.length} assets`);
  hud.innerHTML = lines.map((l) => `<div>${l}</div>`).join('');
}

// --- loading ---------------------------------------------------------------
async function doOpenMap(): Promise<void> {
  try {
    const loaded = await openMapFolder();
    await afterMapOpened(loaded);
  } catch (e) {
    if ((e as Error).name !== 'AbortError') status(`open failed: ${(e as Error).message}`);
  }
}

async function afterMapOpened(loaded: LoadedMap): Promise<void> {
  {
    map = loaded;
    bpm = new BpmMap(loaded.info._beatsPerMinute ?? 120);
    status(`opened "${loaded.info._songName}" (${loaded.difficulties.length} difficulties)`);

    diffSelect.innerHTML = '';
    loaded.difficulties.forEach((d, i) => {
      const opt = document.createElement('option');
      opt.value = String(i);
      const viv = d.requirements.includes('Vivify') ? ' ★' : '';
      opt.textContent = `${d.characteristic} ${d.difficulty}${viv}`;
      diffSelect.appendChild(opt);
    });

    // prefer first difficulty that requires Vivify
    const idx = Math.max(0, loaded.difficulties.findIndex((d) => d.requirements.includes('Vivify')));
    diffSelect.value = String(idx);
    await doLoadDifficulty(idx);

    // audio
    const song = songFile(loaded);
    if (song) {
      try {
        await player.load(song);
        status(`audio loaded (${player.duration.toFixed(1)}s)`);
      } catch (e) {
        status(`audio failed to decode: ${(e as Error).message}`);
      }
    }

    // bundle
    const bundles = bundleFiles(loaded);
    if (bundles.length) {
      const pick = bundles.find((b) => /2019/.test(b.name)) ?? bundles[0];
      await doLoadBundle(pick);
    }
    afterEventsChanged(null);
  }
}

async function doLoadDifficulty(index: number): Promise<void> {
  if (!map) return;
  const ref = map.difficulties[index];
  if (!ref) return;
  try {
    diff = await loadDifficulty(map, ref);
    if (diff.data.bpmEvents?.length) {
      bpm = new BpmMap(map.info._beatsPerMinute ?? 120, diff.data.bpmEvents);
    }
    undoStack.length = 0;
    redoStack.length = 0;
    dirty = false;
    selectEvent(null, false);
    afterEventsChanged(null);
    status(`loaded ${ref.filename}: ${events().length} custom events`);
  } catch (e) {
    diff = null;
    engine = null;
    status((e as Error).message);
  }
}

async function doLoadBundle(file: File): Promise<void> {
  try {
    status(`parsing ${file.name}…`);
    const buf = await file.arrayBuffer();
    await new Promise((r) => setTimeout(r)); // let status paint
    db = AssetDB.fromBundle(buf);
    const converter = new ThreeConverter(db);
    viewport.setConverter(converter);
    assetBrowser.setDb(db);
    status(`${file.name}: ${db.containers.length} assets, ${db.objects.size} objects (unity ${db.unityRevision})`);
    updateHud();
  } catch (e) {
    status(`bundle parse failed: ${(e as Error).message}`);
    console.error(e);
  }
}

async function doSave(): Promise<void> {
  if (!map || !diff) return;
  try {
    const msg = await saveDifficulty(map, diff);
    dirty = false;
    status(msg);
  } catch (e) {
    status(`save failed: ${(e as Error).message}`);
  }
}

// --- wire up components ------------------------------------------------------
timeline.onScrub = (b) => {
  setBeat(b, true);
  updateHud();
};
timeline.onSelect = (ev) => selectEvent(ev);
timeline.onBeginMoveEvent = () => commit();
timeline.onMoveEvent = (ev, newBeat) => {
  ev.b = newBeat;
  afterEventsChanged(ev);
  inspector.show(ev);
};

eventList.onSelect = (ev) => selectEvent(ev, false);

inspector.onChange = () => {
  commit();
  afterEventsChanged(null);
};
inspector.onDelete = (ev) => {
  if (!diff) return;
  commit();
  const list = events();
  const i = list.indexOf(ev);
  if (i >= 0) list.splice(i, 1);
  selectEvent(null, false);
  afterEventsChanged(null);
  status(`deleted ${ev.t} @ ${ev.b}`);
};
inspector.onDuplicate = (ev) => {
  if (!diff) return;
  commit();
  const copy: CustomEvent = JSON.parse(JSON.stringify(ev));
  if (copy.d?.id) copy.d.id = `${copy.d.id}_copy`;
  events().push(copy);
  afterEventsChanged(copy);
  selectEvent(copy);
  status('duplicated event');
};
inspector.onJumpToBeat = (b) => setBeat(b);

viewport.onSelectInstance = (inst) => {
  if (inst) selectEvent(inst.event);
};
viewport.onGizmoChange = (inst, pos, rot, scale) => {
  commit();
  const d = (inst.event.d = inst.event.d ?? {});
  d.position = pos;
  d.rotation = rot;
  d.scale = scale;
  afterEventsChanged(inst.event);
  inspector.refreshData();
  status(`moved ${inst.id} → [${pos.join(', ')}]`);
};

assetBrowser.onSpawn = (assetPath) => {
  if (!diff) {
    status('open a map first to add events');
    return;
  }
  commit();
  const ev = makeEventFromTemplate('InstantiatePrefab', beat);
  ev.d.asset = assetPath;
  const base = assetPath.split('/').pop()!.replace(/\.prefab$/, '').replace(/\W+/g, '_');
  let n = 1;
  while (events().some((e) => e.d?.id === `${base}_${n}`)) n++;
  ev.d.id = `${base}_${n}`;
  ev.d.track = `${base}_${n}`;
  events().push(ev);
  afterEventsChanged(ev);
  selectEvent(ev);
  status(`spawned ${ev.d.id} at beat ${beat.toFixed(2)}`);
};
assetBrowser.onPreview = (assetPath) => viewport.showPreview(assetPath);

// --- toolbar -----------------------------------------------------------------
$('#btn-open-map').addEventListener('click', doOpenMap);
$('#btn-open-bundle').addEventListener('click', () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = '.vivify,.bundle,.assetbundle,*';
  input.onchange = () => {
    if (input.files?.[0]) void doLoadBundle(input.files[0]);
  };
  input.click();
});
diffSelect.addEventListener('change', () => void doLoadDifficulty(Number(diffSelect.value)));
$('#btn-save').addEventListener('click', () => void doSave());
$('#btn-undo').addEventListener('click', undo);
$('#btn-redo').addEventListener('click', redo);

function togglePlay(): void {
  if (!player.loaded) {
    status('no audio loaded');
    return;
  }
  if (player.playing) {
    player.stop();
    playBtn.textContent = '▶';
  } else {
    player.play(bpm.secondsAt(beat));
    playBtn.textContent = '⏸';
  }
}
playBtn.addEventListener('click', togglePlay);

beatInput.addEventListener('change', () => setBeat(Number(beatInput.value) || 0));

for (const mode of ['translate', 'rotate', 'scale'] as GizmoMode[]) {
  const btn = $(`#gizmo-${mode}`);
  btn.addEventListener('click', () => {
    viewport.setGizmoMode(mode);
    document.querySelectorAll('.gizmo-modes button').forEach((b) => b.classList.remove('active'));
    btn.classList.add('active');
  });
}

// add-event popup
$('#btn-add-event').addEventListener('click', (e) => {
  if (!diff) {
    status('open a map first');
    return;
  }
  const existing = document.getElementById('add-event-menu');
  if (existing) {
    existing.remove();
    return;
  }
  const menu = document.createElement('div');
  menu.id = 'add-event-menu';
  Object.assign(menu.style, {
    position: 'fixed',
    background: 'var(--bg3)',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    zIndex: '10',
    maxHeight: '320px',
    overflowY: 'auto',
    boxShadow: '0 4px 16px #000a',
  } as Partial<CSSStyleDeclaration>);
  const rect = (e.target as HTMLElement).getBoundingClientRect();
  menu.style.left = `${rect.left}px`;
  menu.style.top = `${rect.bottom + 4}px`;
  for (const t of ALL_EVENT_TYPES) {
    const item = document.createElement('div');
    item.textContent = t;
    Object.assign(item.style, { padding: '5px 12px', cursor: 'pointer' } as Partial<CSSStyleDeclaration>);
    item.addEventListener('mouseenter', () => (item.style.background = 'var(--accent)'));
    item.addEventListener('mouseleave', () => (item.style.background = ''));
    item.addEventListener('click', () => {
      menu.remove();
      commit();
      const ev = makeEventFromTemplate(t, beat);
      events().push(ev);
      afterEventsChanged(ev);
      selectEvent(ev);
      status(`added ${t} at beat ${beat.toFixed(2)}`);
    });
    menu.appendChild(item);
  }
  document.body.appendChild(menu);
  const close = (ev2: MouseEvent) => {
    if (!menu.contains(ev2.target as Node) && ev2.target !== e.target) {
      menu.remove();
      document.removeEventListener('mousedown', close);
    }
  };
  setTimeout(() => document.addEventListener('mousedown', close));
});

// panel tabs
document.querySelectorAll('.panel-tabs button').forEach((btn) => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.panel-tabs button').forEach((b) => b.classList.remove('active'));
    document.querySelectorAll('.tab-page').forEach((p) => p.classList.remove('active'));
    btn.classList.add('active');
    $(`#tab-${(btn as HTMLElement).dataset.tab}`).classList.add('active');
  });
});

// keyboard shortcuts
window.addEventListener('keydown', (e) => {
  const tag = (e.target as HTMLElement).tagName;
  const typing = tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT';
  if (e.code === 'Space' && !typing) {
    e.preventDefault();
    togglePlay();
    return;
  }
  if (typing) return;
  if (e.ctrlKey && e.code === 'KeyZ') {
    e.preventDefault();
    undo();
  } else if (e.ctrlKey && (e.code === 'KeyY' || (e.shiftKey && e.code === 'KeyZ'))) {
    e.preventDefault();
    redo();
  } else if (e.ctrlKey && e.code === 'KeyS') {
    e.preventDefault();
    void doSave();
  } else if (e.code === 'KeyW') {
    $('#gizmo-translate').click();
  } else if (e.code === 'KeyE') {
    $('#gizmo-rotate').click();
  } else if (e.code === 'KeyR') {
    $('#gizmo-scale').click();
  } else if (e.code === 'KeyF') {
    viewport.focusSelected();
  } else if (e.code === 'Delete') {
    const ev = inspector.event;
    if (ev) inspector.onDelete(ev);
  } else if (e.code === 'ArrowRight') {
    setBeat(beat + (e.shiftKey ? 4 : 1));
  } else if (e.code === 'ArrowLeft') {
    setBeat(Math.max(0, beat - (e.shiftKey ? 4 : 1)));
  }
});

window.addEventListener('beforeunload', (e) => {
  if (dirty) e.preventDefault();
});

// --- main loop -----------------------------------------------------------------
function frame(): void {
  requestAnimationFrame(frame);
  if (player.playing) {
    const b = bpm.beatAt(player.currentTime);
    beat = b;
    beatDisplay.textContent = `beat ${beat.toFixed(2)}`;
    timeline.setBeat(beat);
    if (document.activeElement !== beatInput) beatInput.value = beat.toFixed(2);
  } else if (playBtn.textContent !== '▶') {
    playBtn.textContent = '▶';
  }
  viewport.update(engine, beat);
  viewport.render();
}
frame();

status('ready — open a map folder (needs Info.dat) or a .vivify bundle');

(window as any).__vivifiedDebug = () => {
  const out: any = { instances: [] };
  const group = (viewport as any).instancesGroup;
  for (const root of group.children) {
    let meshes = 0;
    let verts = 0;
    root.traverse((o: any) => {
      if (o.isMesh) {
        meshes++;
        verts += o.geometry?.attributes?.position?.count ?? 0;
      }
    });
    const b = new THREE.Box3().setFromObject(root);
    const bbox = isFinite(b.min.x)
      ? { min: { x: b.min.x, y: b.min.y, z: b.min.z }, max: { x: b.max.x, y: b.max.y, z: b.max.z } }
      : null;
    out.instances.push({
      id: root.userData.instanceId,
      pos: { x: root.position.x, y: root.position.y, z: root.position.z },
      scale: { x: root.scale.x, y: root.scale.y, z: root.scale.z },
      meshes,
      verts,
      children: root.children.map((c: any) => c.name),
      bbox,
    });
  }
  return out;
};

// test/automation hooks (also used for drag-drop in the future)
(window as any).__vivified = {
  async loadBundleBytes(buf: ArrayBuffer, name = 'bundle.vivify'): Promise<void> {
    await doLoadBundle(new File([buf], name));
  },
  async loadMapFiles(files: Record<string, ArrayBuffer | string>): Promise<void> {
    const fileMap = new Map<string, File>();
    for (const [name, data] of Object.entries(files)) {
      fileMap.set(name.toLowerCase(), new File([data as any], name));
    }
    await afterMapOpened(await loadMapFromFiles(fileMap));
  },
  setBeat,
  selectInstance(id: string): void {
    const inst = engine?.instances.find((i) => i.id === id);
    if (inst) selectEvent(inst.event);
  },
  getState() {
    return {
      events: events().length,
      instances: engine?.instances.length ?? 0,
      active: engine ? engine.activeInstances(beat).map((i) => i.id) : [],
      assets: db?.containers.length ?? 0,
      beat,
      status: statusEl.textContent,
    };
  },
};
