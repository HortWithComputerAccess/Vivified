import * as THREE from 'three';
import { V3Beatmap, CustomEvent } from '../map/types';
import { ThreeConverter } from '../unity/toThree';

interface NoteData {
  beat: number;
  x: number; // world x (unity)
  y: number; // world y
  color: 0 | 1 | 2; // 0 left, 1 right, 2 bomb
  dir: number; // cut direction 0-8
  track: string | null;
  fake: boolean;
}

const CUT_ROT_Z: Record<number, number> = {
  0: 180, // up
  1: 0, // down
  2: -90, // left
  3: 90, // right
  4: 135, // up-left? (approx)
  5: -135,
  6: 45,
  7: -45,
  8: 0, // any
};

/**
 * Previews colorNotes/bombNotes flying at the player with correct
 * jump-duration math, optionally skinned via AssignObjectPrefab events.
 */
export class NotesPreview {
  group = new THREE.Group();
  enabled = true;
  private notes: NoteData[] = [];
  private hjd = 2; // half jump duration in beats
  private dzPerBeat = 5; // unity units per beat
  private leftColor = new THREE.Color(0.93, 0.61, 1);
  private rightColor = new THREE.Color(0, 0.8, 1);
  /** track -> note prefab asset (from AssignObjectPrefab colorNotes) */
  private trackSkins = new Map<string, string>();
  private pools = new Map<string, THREE.Object3D[]>();
  private active: { obj: THREE.Object3D; key: string }[] = [];
  private converter: ThreeConverter | null = null;

  constructor() {
    this.group.name = 'notes-preview';
  }

  setConverter(converter: ThreeConverter | null): void {
    this.converter = converter;
    this.pools.clear();
    this.releaseAll();
  }

  configure(njs: number, startBeatOffset: number, bpm: number, colors?: { left?: any; right?: any }): void {
    const beatDur = 60 / Math.max(bpm, 1);
    let hjd = 4;
    while (njs * beatDur * hjd > 17.999) hjd /= 2;
    hjd += startBeatOffset;
    this.hjd = Math.max(hjd, 0.25);
    this.dzPerBeat = njs * beatDur;
    if (colors?.left) this.leftColor.setRGB(colors.left.r ?? 1, colors.left.g ?? 0, colors.left.b ?? 0);
    if (colors?.right) this.rightColor.setRGB(colors.right.r ?? 0, colors.right.g ?? 0, colors.right.b ?? 1);
  }

  rebuild(map: V3Beatmap): void {
    this.notes = [];
    const add = (n: any, color: 0 | 1 | 2, fake: boolean) => {
      const cd = n.customData ?? {};
      let wx: number;
      let wy: number;
      if (Array.isArray(cd.coordinates)) {
        wx = (Number(cd.coordinates[0]) + 0.5) * 0.6;
        wy = Number(cd.coordinates[1]) * 0.6 + 0.6;
      } else {
        wx = (Number(n.x ?? 0) - 1.5) * 0.6;
        wy = Number(n.y ?? 0) * 0.6 + 0.6;
      }
      const track = typeof cd.track === 'string' ? cd.track : Array.isArray(cd.track) ? cd.track[0] : null;
      this.notes.push({
        beat: Number(n.b ?? 0),
        x: wx,
        y: wy,
        color,
        dir: Number(n.d ?? 8),
        track,
        fake,
      });
    };
    for (const n of map.colorNotes ?? []) add(n, (n.c ?? 0) as 0 | 1, false);
    for (const n of map.bombNotes ?? []) add(n, 2, false);
    for (const n of map.customData?.fakeColorNotes ?? []) add(n, (n.c ?? 0) as 0 | 1, true);
    this.notes.sort((a, b) => a.beat - b.beat);
  }

  /** AssignObjectPrefab events define note skins per track. */
  rebuildSkins(events: CustomEvent[]): void {
    this.trackSkins.clear();
    for (const ev of events) {
      if (ev.t !== 'AssignObjectPrefab') continue;
      const cn = ev.d?.colorNotes;
      if (!cn?.asset) continue;
      const tracks = typeof cn.track === 'string' ? [cn.track] : Array.isArray(cn.track) ? cn.track : [];
      for (const t of tracks) this.trackSkins.set(String(t), String(cn.asset));
    }
  }

  update(beat: number): void {
    this.releaseAll();
    if (!this.enabled || this.notes.length === 0) return;

    // visible window: spawned (b - hjd) and not yet 1 beat past the player
    const minBeat = beat - 1;
    const maxBeat = beat + this.hjd;
    let lo = lowerBound(this.notes, minBeat);
    for (let i = lo; i < this.notes.length; i++) {
      const n = this.notes[i];
      if (n.beat > maxBeat) break;
      if (this.active.length > 220) break; // sanity cap

      const z = (n.beat - beat) * this.dzPerBeat; // unity z (forward)
      const key = this.skinKey(n);
      const obj = this.acquire(key, n);
      obj.position.set(n.x, n.y, -z);
      obj.visible = true;
      // spawn rise animation in the first quarter of the jump
      const lifetime = (beat - (n.beat - this.hjd)) / this.hjd;
      if (lifetime < 0.25) {
        obj.position.y = n.y * Math.min(lifetime / 0.25, 1);
      }
    }
  }

  private skinKey(n: NoteData): string {
    if (n.color === 2) return 'bomb';
    const skin = n.track ? this.trackSkins.get(n.track) : null;
    return skin ? `skin:${skin}:${n.color}` : `default:${n.color}:${n.dir}`;
  }

  private acquire(key: string, n: NoteData): THREE.Object3D {
    const pool = this.pools.get(key) ?? [];
    this.pools.set(key, pool);
    let obj = pool.find((o) => !o.visible);
    if (!obj) {
      obj = this.createNoteObject(key, n);
      pool.push(obj);
      this.group.add(obj);
    }
    this.active.push({ obj, key });
    return obj;
  }

  private releaseAll(): void {
    for (const { obj } of this.active) obj.visible = false;
    this.active = [];
  }

  private createNoteObject(key: string, n: NoteData): THREE.Object3D {
    if (key === 'bomb') {
      const bomb = new THREE.Mesh(
        new THREE.SphereGeometry(0.22, 12, 12),
        new THREE.MeshStandardMaterial({ color: 0x202020, roughness: 0.4 })
      );
      return bomb;
    }
    if (key.startsWith('skin:') && this.converter) {
      const asset = key.slice(5, key.lastIndexOf(':'));
      const prefab = this.converter.prefabByPath(asset);
      if (prefab) {
        const wrap = new THREE.Group();
        wrap.add(prefab);
        return wrap;
      }
    }
    // default note: rounded-ish cube + direction indicator
    const color = n.color === 0 ? this.leftColor : this.rightColor;
    const group = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.BoxGeometry(0.45, 0.45, 0.45),
      new THREE.MeshStandardMaterial({ color, roughness: 0.35 })
    );
    group.add(body);
    if (n.dir !== 8) {
      const arrow = new THREE.Mesh(
        new THREE.ConeGeometry(0.12, 0.2, 4),
        new THREE.MeshBasicMaterial({ color: 0xffffff })
      );
      // arrow on the front face pointing along the cut direction
      arrow.position.set(0, -0.12, 0.24);
      arrow.rotation.z = ((CUT_ROT_Z[n.dir] ?? 0) * Math.PI) / 180;
      const holder = new THREE.Group();
      holder.add(arrow);
      holder.rotation.z = ((CUT_ROT_Z[n.dir] ?? 0) * Math.PI) / 180;
      arrow.position.set(0, -0.14, 0.235);
      arrow.rotation.set(0, 0, Math.PI);
      group.add(holder);
    } else {
      const dot = new THREE.Mesh(
        new THREE.CircleGeometry(0.09, 16),
        new THREE.MeshBasicMaterial({ color: 0xffffff })
      );
      dot.position.set(0, 0, 0.235);
      group.add(dot);
    }
    return group;
  }
}

function lowerBound(notes: NoteData[], beat: number): number {
  let lo = 0;
  let hi = notes.length;
  while (lo < hi) {
    const mid = (lo + hi) >> 1;
    if (notes[mid].beat < beat) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}
