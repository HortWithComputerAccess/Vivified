import { CustomEvent, V3Beatmap } from '../map/types';
import { ease } from './easings';
import { parsePointDef, samplePointDef, sampleRotation, PointDef } from './points';

export interface TransformState {
  position: [number, number, number] | null;
  localPosition: [number, number, number] | null;
  rotation: [number, number, number] | null; // euler degrees (unity)
  localRotation: [number, number, number] | null;
  scale: [number, number, number] | null;
  dissolve: number | null;
}

export interface PrefabInstance {
  /** the InstantiatePrefab event that creates this instance */
  event: CustomEvent;
  id: string;
  asset: string;
  tracks: string[];
  spawnBeat: number;
  destroyBeat: number | null;
  /** static spawn transform from the event data (unity space) */
  spawnPosition: [number, number, number];
  spawnRotation: [number, number, number];
  spawnScale: [number, number, number];
}

interface PropTimelineEntry {
  beat: number;
  duration: number;
  easing?: string;
  repeat: number;
  def: PointDef;
}

const TRANSFORM_PROPS = ['position', 'localPosition', 'rotation', 'localRotation', 'scale', 'dissolve'] as const;
type TransformProp = (typeof TRANSFORM_PROPS)[number];
const PROP_DIMS: Record<TransformProp, number> = {
  position: 3,
  localPosition: 3,
  rotation: 3,
  localRotation: 3,
  scale: 3,
  dissolve: 1,
};

/**
 * Evaluates Heck/Vivify state at a beat: which prefab instances exist, what
 * their track-driven transforms are, and track parenting.
 */
export class TrackEngine {
  instances: PrefabInstance[] = [];
  /** track -> property -> sorted AnimateTrack segments */
  private timelines = new Map<string, Map<TransformProp, PropTimelineEntry[]>>();
  /** child track -> parent track */
  parents = new Map<string, string>();
  /** tracks assigned to the player via AssignPlayerToTrack */
  playerTracks: string[] = [];
  pointDefinitions: Record<string, any[]> = {};

  static fromBeatmap(map: V3Beatmap): TrackEngine {
    const engine = new TrackEngine();
    const events = map.customData?.customEvents ?? [];
    engine.pointDefinitions = map.customData?.pointDefinitions ?? {};
    engine.rebuild(events);
    return engine;
  }

  rebuild(events: CustomEvent[]): void {
    this.instances = [];
    this.timelines.clear();
    this.parents.clear();
    this.playerTracks = [];

    const sorted = [...events].sort((a, b) => (a.b ?? 0) - (b.b ?? 0));
    let autoId = 0;

    for (const ev of sorted) {
      const d = ev.d ?? {};
      switch (ev.t) {
        case 'InstantiatePrefab': {
          const tracks = asStringArray(d.track);
          this.instances.push({
            event: ev,
            id: typeof d.id === 'string' && d.id ? d.id : `__auto_${autoId++}`,
            asset: String(d.asset ?? ''),
            tracks,
            spawnBeat: ev.b ?? 0,
            destroyBeat: null,
            spawnPosition: vec3(d.localPosition ?? d.position, 0),
            spawnRotation: vec3(d.localRotation ?? d.rotation, 0),
            spawnScale: vec3(d.scale, 1),
          });
          break;
        }
        case 'DestroyObject': {
          const ids = asStringArray(d.id);
          for (const id of ids) {
            for (const inst of this.instances) {
              if (inst.id === id && inst.destroyBeat === null && inst.spawnBeat <= (ev.b ?? 0)) {
                inst.destroyBeat = ev.b ?? 0;
              }
            }
          }
          break;
        }
        case 'AnimateTrack': {
          const tracks = asStringArray(d.track);
          const duration = num(d.duration, 0);
          const repeat = Math.max(0, Math.floor(num(d.repeat, 0)));
          const easing = typeof d.easing === 'string' ? d.easing : undefined;
          for (const prop of TRANSFORM_PROPS) {
            if (!(prop in d)) continue;
            const def = parsePointDef(d[prop], PROP_DIMS[prop], this.pointDefinitions);
            if (!def) continue;
            for (const track of tracks) {
              let perTrack = this.timelines.get(track);
              if (!perTrack) {
                perTrack = new Map();
                this.timelines.set(track, perTrack);
              }
              let list = perTrack.get(prop);
              if (!list) {
                list = [];
                perTrack.set(prop, list);
              }
              list.push({ beat: ev.b ?? 0, duration, easing, repeat, def });
            }
          }
          break;
        }
        case 'AssignTrackParent': {
          const parent = String(d.parentTrack ?? '');
          for (const child of asStringArray(d.childrenTracks)) {
            if (parent) this.parents.set(child, parent);
          }
          break;
        }
        case 'AssignPlayerToTrack': {
          for (const t of asStringArray(d.track)) {
            if (!this.playerTracks.includes(t)) this.playerTracks.push(t);
          }
          break;
        }
      }
    }
    for (const perTrack of this.timelines.values()) {
      for (const list of perTrack.values()) {
        list.sort((a, b) => a.beat - b.beat);
      }
    }
  }

  /** Prefab instances alive at the given beat. */
  activeInstances(beat: number): PrefabInstance[] {
    return this.instances.filter(
      (i) => i.spawnBeat <= beat && (i.destroyBeat === null || i.destroyBeat > beat)
    );
  }

  trackNames(): string[] {
    const names = new Set<string>(this.timelines.keys());
    for (const i of this.instances) for (const t of i.tracks) names.add(t);
    for (const [c, p] of this.parents) {
      names.add(c);
      names.add(p);
    }
    return [...names].sort();
  }

  /** Evaluate the animated transform state of a track at a beat. */
  evaluate(track: string, beat: number): TransformState {
    const state: TransformState = {
      position: null,
      localPosition: null,
      rotation: null,
      localRotation: null,
      scale: null,
      dissolve: null,
    };
    const perTrack = this.timelines.get(track);
    if (!perTrack) return state;

    for (const prop of TRANSFORM_PROPS) {
      const list = perTrack.get(prop);
      if (!list || list.length === 0) continue;
      // last event that has started
      let entry: PropTimelineEntry | null = null;
      for (const e of list) {
        if (e.beat <= beat) entry = e;
        else break;
      }
      if (!entry) continue;
      const totalDur = entry.duration * (entry.repeat + 1);
      let s: number;
      if (entry.duration <= 0) {
        s = 1;
      } else if (beat >= entry.beat + totalDur) {
        s = 1;
      } else {
        const elapsed = (beat - entry.beat) % entry.duration;
        const cycle = Math.floor((beat - entry.beat) / entry.duration);
        // final repeat holds at its end; intermediate repeats wrap
        s = elapsed / entry.duration;
        if (cycle > entry.repeat) s = 1;
      }
      s = ease(entry.easing, Math.min(Math.max(s, 0), 1));

      if (prop === 'rotation' || prop === 'localRotation') {
        const v = sampleRotation(entry.def, s);
        if (v) state[prop] = v;
      } else if (prop === 'dissolve') {
        const v = samplePointDef(entry.def, s);
        if (v) state.dissolve = v[0];
      } else {
        const v = samplePointDef(entry.def, s);
        if (v) state[prop] = [v[0], v[1], v[2]];
      }
    }
    return state;
  }

  /** Resolve full parent chain for a track (closest first). */
  parentChain(track: string): string[] {
    const chain: string[] = [];
    let cur = this.parents.get(track);
    const seen = new Set<string>();
    while (cur && !seen.has(cur)) {
      chain.push(cur);
      seen.add(cur);
      cur = this.parents.get(cur);
    }
    return chain;
  }
}

function asStringArray(v: any): string[] {
  if (typeof v === 'string') return v ? [v] : [];
  if (Array.isArray(v)) return v.filter((x) => typeof x === 'string');
  return [];
}

function vec3(v: any, def: number): [number, number, number] {
  if (Array.isArray(v) && v.length >= 3) {
    return [num(v[0], def), num(v[1], def), num(v[2], def)];
  }
  return [def, def, def];
}

function num(v: any, def: number): number {
  return typeof v === 'number' && isFinite(v) ? v : def;
}
