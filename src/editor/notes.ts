import * as THREE from 'three';
import { V3Beatmap, CustomEvent } from '../map/types';
import { ThreeConverter } from '../unity/toThree';
import { TrackEngine } from '../anim/tracks';

interface NoteData {
  beat: number;
  x: number; // world x (unity)
  y: number; // world y
  color: 0 | 1 | 2; // 0 left, 1 right, 2 bomb
  dir: number; // cut direction 0-8
  angleOffset: number; // degrees
  track: string | null;
  isChainLink: boolean;
}

interface WallData {
  beat: number;
  duration: number; // beats
  cx: number; // center x (unity)
  baseY: number;
  width: number; // meters
  height: number;
  color: [number, number, number] | null;
  track: string | null;
}

/**
 * Z-rotation (degrees, CCW seen from the player) for each cut direction,
 * with the note model's arrow pointing DOWN at identity (Beat Saber's
 * convention: direction 1 "down" is the unrotated pose).
 */
const DIR_ROT_Z: Record<number, number> = {
  0: 180, // up
  1: 0, // down
  2: -90, // left
  3: 90, // right
  4: -135, // up-left
  5: 135, // up-right
  6: -45, // down-left
  7: 45, // down-right
  8: 0, // any (dot)
};

/**
 * Previews the playable map: notes/bombs/chains/walls flying at the player
 * with correct jump math, Heck track + path animations, AssignObjectPrefab
 * note skins, map color scheme, and sabers in POV mode.
 */
export class NotesPreview {
  group = new THREE.Group();
  saberGroup = new THREE.Group();
  enabled = true;
  private notes: NoteData[] = [];
  private walls: WallData[] = [];
  private hjd = 2;
  private dzPerBeat = 5;
  private leftColor = new THREE.Color(0.7843, 0.0784, 0.0784);
  private rightColor = new THREE.Color(0.1568, 0.5568, 0.8235);
  private trackSkins = new Map<string, string>();
  private saberAssets: { left: string | null; right: string | null } = { left: null, right: null };
  private pools = new Map<string, THREE.Object3D[]>();
  private active: THREE.Object3D[] = [];
  private converter: ThreeConverter | null = null;
  private sabersBuilt = false;

  constructor() {
    this.group.name = 'notes-preview';
    this.saberGroup.name = 'sabers';
    this.saberGroup.visible = false;
  }

  setConverter(converter: ThreeConverter | null): void {
    this.converter = converter;
    this.pools.clear();
    this.active = [];
    this.group.clear();
    this.sabersBuilt = false;
    this.saberGroup.clear();
  }

  configure(njs: number, startBeatOffset: number, bpm: number, colors?: { left?: any; right?: any }): void {
    const beatDur = 60 / Math.max(bpm, 1);
    let hjd = 4;
    while (njs * beatDur * hjd > 17.999) hjd /= 2;
    hjd += startBeatOffset;
    this.hjd = Math.max(hjd, 0.25);
    this.dzPerBeat = njs * beatDur;
    if (colors?.left) {
      this.leftColor.setRGB(colors.left.r ?? 1, colors.left.g ?? 0, colors.left.b ?? 0, THREE.SRGBColorSpace);
    }
    if (colors?.right) {
      this.rightColor.setRGB(colors.right.r ?? 0, colors.right.g ?? 0, colors.right.b ?? 1, THREE.SRGBColorSpace);
    }
    // colors changed: drop pools so notes rebuild with the right tint
    this.pools.clear();
    this.active = [];
    this.group.clear();
  }

  rebuild(map: V3Beatmap): void {
    this.notes = [];
    this.walls = [];

    const noteXY = (n: any): { x: number; y: number } => {
      const cd = n.customData ?? {};
      if (Array.isArray(cd.coordinates)) {
        return { x: (Number(cd.coordinates[0]) + 0.5) * 0.6, y: Number(cd.coordinates[1]) * 0.6 + 0.6 };
      }
      return { x: (Number(n.x ?? 0) - 1.5) * 0.6, y: Number(n.y ?? 0) * 0.6 + 0.6 };
    };
    const trackOf = (n: any): string | null => {
      const t = n.customData?.track;
      return typeof t === 'string' ? t : Array.isArray(t) ? t[0] : null;
    };

    const addNote = (n: any, color: 0 | 1 | 2) => {
      const { x, y } = noteXY(n);
      this.notes.push({
        beat: Number(n.b ?? 0),
        x,
        y,
        color,
        dir: Number(n.d ?? 8),
        angleOffset: Number(n.a ?? 0),
        track: trackOf(n),
        isChainLink: false,
      });
    };
    for (const n of map.colorNotes ?? []) addNote(n, (n.c ?? 0) as 0 | 1);
    for (const n of map.bombNotes ?? []) addNote(n, 2);
    for (const n of map.customData?.fakeColorNotes ?? []) addNote(n, (n.c ?? 0) as 0 | 1);
    for (const n of map.customData?.fakeBombNotes ?? []) addNote(n, 2);

    // chains: head is a real colorNote; links interpolate head -> tail
    for (const c of map.burstSliders ?? []) {
      const head = { ...c, customData: c.customData };
      const { x: hx, y: hy } = noteXY(head);
      const tx = (Number(c.tx ?? 0) - 1.5) * 0.6;
      const ty = Number(c.ty ?? 0) * 0.6 + 0.6;
      const slices = Math.max(Number(c.sc ?? 2), 2);
      const squish = Number(c.s ?? 1) || 1;
      for (let i = 1; i < slices; i++) {
        const t = (i / (slices - 1)) * squish;
        this.notes.push({
          beat: Number(c.b ?? 0) + (Number(c.tb ?? 0) - Number(c.b ?? 0)) * t,
          x: hx + (tx - hx) * t,
          y: hy + (ty - hy) * t,
          color: (c.c ?? 0) as 0 | 1,
          dir: 8,
          angleOffset: 0,
          track: trackOf(c),
          isChainLink: true,
        });
      }
    }
    this.notes.sort((a, b) => a.beat - b.beat);

    for (const o of map.obstacles ?? []) {
      const cd = o.customData ?? {};
      let cx: number;
      let baseY: number;
      let width: number;
      let height: number;
      if (Array.isArray(cd.coordinates) || Array.isArray(cd.size)) {
        const co = cd.coordinates ?? [Number(o.x ?? 0) - 2, Number(o.y ?? 0)];
        const size = cd.size ?? [Number(o.w ?? 1), Number(o.h ?? 1)];
        width = Number(size[0] ?? 1) * 0.6;
        height = Number(size[1] ?? 1) * 0.6;
        cx = (Number(co[0]) + 0.5) * 0.6 + width / 2 - 0.3;
        baseY = Number(co[1]) * 0.6;
      } else {
        const x = Number(o.x ?? 0);
        const w = Number(o.w ?? 1);
        width = w * 0.6;
        cx = (x - 2) * 0.6 + width / 2;
        baseY = Number(o.y ?? 0) * 0.6;
        height = Number(o.h ?? 1) * 0.6;
      }
      const col = Array.isArray(cd.color)
        ? ([Number(cd.color[0]), Number(cd.color[1]), Number(cd.color[2])] as [number, number, number])
        : null;
      const t = cd.track;
      this.walls.push({
        beat: Number(o.b ?? 0),
        duration: Number(o.d ?? 0),
        cx,
        baseY,
        width,
        height: Math.min(height, 5),
        color: col,
        track: typeof t === 'string' ? t : Array.isArray(t) ? t[0] : null,
      });
    }
    this.walls.sort((a, b) => a.beat - b.beat);
  }

  /** AssignObjectPrefab events define note/saber skins. */
  rebuildSkins(events: CustomEvent[]): void {
    this.trackSkins.clear();
    this.saberAssets = { left: null, right: null };
    for (const ev of events) {
      if (ev.t !== 'AssignObjectPrefab') continue;
      const cn = ev.d?.colorNotes;
      if (cn?.asset) {
        const tracks = typeof cn.track === 'string' ? [cn.track] : Array.isArray(cn.track) ? cn.track : [];
        for (const t of tracks) this.trackSkins.set(String(t), String(cn.asset));
      }
      const saber = ev.d?.saber;
      if (saber?.asset) {
        const type = String(saber.type ?? 'Both');
        if (type === 'Left' || type === 'Both') this.saberAssets.left = String(saber.asset);
        if (type === 'Right' || type === 'Both') this.saberAssets.right = String(saber.asset);
      }
    }
    this.sabersBuilt = false;
  }

  update(beat: number, engine: TrackEngine | null, pov: boolean): void {
    for (const obj of this.active) obj.visible = false;
    this.active = [];
    this.saberGroup.visible = pov && this.enabled;
    if (pov && !this.sabersBuilt) this.buildSabers();
    if (!this.enabled) return;

    this.updateNotes(beat, engine);
    this.updateWalls(beat, engine);
  }

  private updateNotes(beat: number, engine: TrackEngine | null): void {
    if (this.notes.length === 0) return;
    const minBeat = beat - 1;
    const maxBeat = beat + this.hjd;
    const lo = lowerBound(this.notes, minBeat);
    for (let i = lo; i < this.notes.length; i++) {
      const n = this.notes[i];
      if (n.beat > maxBeat) break;
      if (this.active.length > 300) break;

      let px = n.x;
      let py = n.y;
      let pz = (n.beat - beat) * this.dzPerBeat;
      let sx = 1, sy = 1, sz = 1;
      let opacity = 1;

      if (n.track && engine) {
        const lifeT = (beat - (n.beat - this.hjd)) / (2 * this.hjd);
        const st = engine.evaluate(n.track, beat);
        const off = st.offsetPosition ?? st.position;
        if (off) {
          px += off[0];
          py += off[1];
          pz += off[2];
        }
        if (st.scale) {
          sx = st.scale[0];
          sy = st.scale[1];
          sz = st.scale[2];
        }
        if (st.dissolve !== null) opacity *= st.dissolve;
        const pOff = engine.evaluatePath(n.track, 'offsetPosition', beat, lifeT);
        if (pOff) {
          px += pOff[0];
          py += pOff[1];
          pz += pOff[2];
        }
        const pDis = engine.evaluatePath(n.track, 'dissolve', beat, lifeT);
        if (pDis) opacity *= pDis[0];
      }
      if (opacity <= 0.004 || Math.abs(sx) < 1e-4) continue;

      // spawn rise during the first quarter of the jump
      const lifetime = (beat - (n.beat - this.hjd)) / this.hjd;
      if (lifetime < 0.25 && !n.track) {
        py *= Math.min(lifetime / 0.25, 1);
      }

      const obj = this.acquire(this.skinKey(n), n);
      obj.position.set(px, py, -pz);
      obj.scale.set(sx, sy, sz);
      const rotZ = (DIR_ROT_Z[n.dir] ?? 0) + n.angleOffset;
      obj.rotation.set(0, 0, (rotZ * Math.PI) / 180);
      obj.visible = true;
      setObjOpacity(obj, opacity);
      this.active.push(obj);
    }
  }

  private updateWalls(beat: number, engine: TrackEngine | null): void {
    for (const w of this.walls) {
      if (w.beat - this.hjd > beat + 0.01) break;
      if (w.beat + w.duration + 1 < beat) continue;
      if (this.active.length > 360) break;

      const frontZ = (w.beat - beat) * this.dzPerBeat;
      const len = Math.max(w.duration * this.dzPerBeat, 0.1);
      const key = w.color ? `wall:${w.color.join(',')}` : 'wall';
      const obj = this.acquire(key, null, w);
      let px = w.cx;
      let py = w.baseY + w.height / 2;
      let pz = frontZ + len / 2;
      if (w.track && engine) {
        const st = engine.evaluate(w.track, beat);
        const off = st.offsetPosition ?? st.position;
        if (off) {
          px += off[0];
          py += off[1];
          pz += off[2];
        }
      }
      obj.position.set(px, py, -pz);
      obj.scale.set(w.width, w.height, len);
      obj.visible = true;
      this.active.push(obj);
    }
  }

  private skinKey(n: NoteData): string {
    if (n.color === 2) return 'bomb';
    if (n.isChainLink) return `link:${n.color}`;
    const skin = n.track ? this.trackSkins.get(n.track) : null;
    return skin ? `skin:${skin}:${n.color}` : `note:${n.color}:${n.dir === 8 ? 'dot' : 'arrow'}`;
  }

  private acquire(key: string, n: NoteData | null, w?: WallData): THREE.Object3D {
    const pool = this.pools.get(key) ?? [];
    this.pools.set(key, pool);
    let obj = pool.find((o) => !o.visible);
    if (!obj) {
      obj = w ? this.createWallObject(w) : this.createNoteObject(key, n!);
      collectMaterials(obj);
      pool.push(obj);
      this.group.add(obj);
    }
    return obj;
  }

  private noteColor(c: 0 | 1): THREE.Color {
    return c === 0 ? this.leftColor.clone() : this.rightColor.clone();
  }

  private createNoteObject(key: string, n: NoteData): THREE.Object3D {
    if (key === 'bomb') {
      const geo = new THREE.SphereGeometry(0.21, 10, 8);
      return new THREE.Mesh(
        geo,
        new THREE.MeshStandardMaterial({ color: 0x18181c, roughness: 0.35, metalness: 0.3 })
      );
    }
    if (key.startsWith('link:')) {
      const c = this.noteColor(n.color as 0 | 1);
      const link = new THREE.Mesh(
        new THREE.BoxGeometry(0.4, 0.12, 0.3),
        new THREE.MeshStandardMaterial({ color: c, roughness: 0.35 })
      );
      return link;
    }
    if (key.startsWith('skin:') && this.converter) {
      const asset = key.slice(5, key.lastIndexOf(':'));
      const prefab = this.converter.prefabByPath(asset);
      if (prefab) {
        // Vivify tints note prefab materials with the note color (_Color);
        // clone materials per pool object so each color variant is distinct
        const tint = this.noteColor(n.color as 0 | 1);
        prefab.traverse((o) => {
          const mesh = o as THREE.Mesh;
          if (!mesh.isMesh) return;
          const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
          const cloned = mats.map((m) => {
            const cm = m.clone();
            const cc = (cm as THREE.MeshStandardMaterial).color;
            if (cc) cc.copy(tint);
            return cm;
          });
          mesh.material = Array.isArray(mesh.material) ? cloned : cloned[0];
        });
        const wrap = new THREE.Group();
        wrap.add(prefab);
        return wrap;
      }
    }
    // default note: cube + white arrow (pointing DOWN at identity) or dot
    const color = this.noteColor(n.color as 0 | 1);
    const group = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.BoxGeometry(0.45, 0.45, 0.45),
      new THREE.MeshStandardMaterial({ color, roughness: 0.32, metalness: 0.05 })
    );
    group.add(body);
    const faceMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
    if (n.dir !== 8) {
      const shape = new THREE.Shape();
      shape.moveTo(0, -0.17);
      shape.lineTo(0.14, 0.02);
      shape.lineTo(-0.14, 0.02);
      shape.closePath();
      const arrow = new THREE.Mesh(new THREE.ShapeGeometry(shape), faceMat);
      arrow.position.set(0, -0.015, 0.228);
      group.add(arrow);
    } else {
      const dot = new THREE.Mesh(new THREE.CircleGeometry(0.085, 16), faceMat);
      dot.position.set(0, 0, 0.228);
      group.add(dot);
    }
    return group;
  }

  private createWallObject(w: WallData): THREE.Object3D {
    const c = new THREE.Color();
    if (w.color) c.setRGB(w.color[0], w.color[1], w.color[2], THREE.SRGBColorSpace);
    else c.copy(this.leftColor);
    const group = new THREE.Group();
    const geo = new THREE.BoxGeometry(1, 1, 1);
    const fill = new THREE.Mesh(
      geo,
      new THREE.MeshBasicMaterial({
        color: c,
        transparent: true,
        opacity: 0.22,
        depthWrite: false,
        side: THREE.DoubleSide,
      })
    );
    const edges = new THREE.LineSegments(
      new THREE.EdgesGeometry(geo),
      new THREE.LineBasicMaterial({ color: c.clone().multiplyScalar(1.4), transparent: true, opacity: 0.8 })
    );
    group.add(fill, edges);
    return group;
  }

  // --- sabers (POV) ---------------------------------------------------------

  private buildSabers(): void {
    this.sabersBuilt = true;
    this.saberGroup.clear();
    const make = (side: 'left' | 'right'): THREE.Object3D => {
      const color = side === 'left' ? this.leftColor.clone() : this.rightColor.clone();
      let obj: THREE.Object3D | null = null;
      const asset = this.saberAssets[side];
      if (asset && this.converter) {
        obj = this.converter.prefabByPath(asset);
        if (obj) {
          obj.traverse((o) => {
            const mesh = o as THREE.Mesh;
            if (!mesh.isMesh) return;
            const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
            mesh.material = mats.map((m) => {
              const cm = m.clone();
              const cc = (cm as THREE.MeshStandardMaterial).color;
              if (cc && cc.r > 0.85 && cc.g > 0.85 && cc.b > 0.85) cc.copy(color);
              return cm;
            }) as any;
            if (!Array.isArray(mesh.material)) return;
            if ((mesh.material as any).length === 1) mesh.material = (mesh.material as any)[0];
          });
        }
      }
      if (!obj) {
        obj = new THREE.Group();
        const handle = new THREE.Mesh(
          new THREE.CylinderGeometry(0.022, 0.025, 0.24, 10),
          new THREE.MeshStandardMaterial({ color: 0x222228, roughness: 0.5 })
        );
        handle.rotation.x = Math.PI / 2;
        handle.position.z = 0.1;
        const blade = new THREE.Mesh(
          new THREE.BoxGeometry(0.035, 0.035, 1.0),
          new THREE.MeshBasicMaterial({ color })
        );
        blade.position.z = -0.52;
        const glow = new THREE.Mesh(
          new THREE.BoxGeometry(0.07, 0.07, 1.0),
          new THREE.MeshBasicMaterial({ color, transparent: true, opacity: 0.25, blending: THREE.AdditiveBlending, depthWrite: false })
        );
        glow.position.z = -0.52;
        obj.add(handle, blade, glow);
      }
      const holder = new THREE.Group();
      holder.add(obj);
      const sideSign = side === 'left' ? -1 : 1;
      // camera-relative resting pose (saberGroup is parented to the camera):
      // grips at the bottom of the view, tips angled up into frame
      holder.position.set(0.28 * sideSign, -0.38, -0.6);
      holder.rotation.set(-0.7, 0.12 * -sideSign, 0.08 * sideSign);
      return holder;
    };
    this.saberGroup.add(make('left'), make('right'));
  }
}

/** Cache each pooled object's materials for fast opacity updates. */
function collectMaterials(obj: THREE.Object3D): void {
  const mats: THREE.Material[] = [];
  obj.traverse((o) => {
    const mesh = o as THREE.Mesh;
    if (mesh.isMesh || (o as any).isLineSegments) {
      const m = (o as any).material;
      for (const mm of Array.isArray(m) ? m : [m]) {
        if (mm) {
          mm.userData.baseOpacity = mm.opacity;
          mm.userData.baseTransparent = mm.transparent;
          mats.push(mm);
        }
      }
    }
  });
  obj.userData.mats = mats;
}

function setObjOpacity(obj: THREE.Object3D, opacity: number): void {
  const mats = obj.userData.mats as THREE.Material[] | undefined;
  if (!mats) return;
  const o = Math.min(Math.max(opacity, 0), 1);
  for (const m of mats) {
    const base = (m.userData.baseOpacity as number) ?? 1;
    if (o >= 0.999) {
      m.opacity = base;
      m.transparent = m.userData.baseTransparent as boolean;
    } else {
      m.opacity = base * o;
      m.transparent = true;
    }
  }
}

function lowerBound(notes: { beat: number }[], beat: number): number {
  let lo = 0;
  let hi = notes.length;
  while (lo < hi) {
    const mid = (lo + hi) >> 1;
    if (notes[mid].beat < beat) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}
