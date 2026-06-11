import { CustomEvent } from '../map/types';

/**
 * Visual keyframe authoring: convert a gizmo pose at a beat into Heck
 * AnimateTrack keyframes. Keys are stored in a single AnimateTrack event per
 * track+property (created on demand); the event's beat/duration window is
 * re-derived from the key range and keyframe times are normalized 0..1.
 */
export interface TransformKey {
  position?: [number, number, number];
  rotation?: [number, number, number];
  scale?: [number, number, number];
}

const KEYABLE = ['position', 'rotation', 'scale'] as const;

export function writeTransformKeys(
  events: CustomEvent[],
  track: string,
  beat: number,
  key: TransformKey
): CustomEvent[] {
  const touched: CustomEvent[] = [];
  for (const prop of KEYABLE) {
    const value = key[prop];
    if (!value) continue;
    const ev = findOrCreateKeyEvent(events, track, prop, beat);
    upsertKey(ev, prop, beat, value);
    touched.push(ev);
  }
  return touched;
}

/** Remove the keyframe nearest `beat` (within tolerance) for all properties. */
export function deleteTransformKeys(
  events: CustomEvent[],
  track: string,
  beat: number,
  tolerance = 0.1
): CustomEvent[] {
  const touched: CustomEvent[] = [];
  for (const prop of KEYABLE) {
    const ev = findKeyEvent(events, track, prop);
    if (!ev) continue;
    const keys = absoluteKeys(ev, prop);
    const idx = keys.findIndex((k) => Math.abs(k.beat - beat) <= tolerance);
    if (idx === -1) continue;
    keys.splice(idx, 1);
    if (keys.length === 0) {
      delete ev.d[prop];
      // drop the event entirely if it animates nothing anymore
      if (!KEYABLE.some((p) => p in ev.d)) {
        const i = events.indexOf(ev);
        if (i >= 0) events.splice(i, 1);
      }
    } else {
      writeBackKeys(ev, prop, keys);
    }
    touched.push(ev);
  }
  return touched;
}

/** All keyframes (absolute beats) for a track property, for timeline display. */
export function listKeys(
  events: CustomEvent[],
  track: string
): { beat: number; prop: string; event: CustomEvent }[] {
  const out: { beat: number; prop: string; event: CustomEvent }[] = [];
  for (const ev of events) {
    if (ev.t !== 'AnimateTrack' || !matchesTrack(ev, track)) continue;
    for (const prop of KEYABLE) {
      if (!(prop in (ev.d ?? {}))) continue;
      for (const k of absoluteKeys(ev, prop)) {
        out.push({ beat: k.beat, prop, event: ev });
      }
    }
  }
  return out.sort((a, b) => a.beat - b.beat);
}

// ---------------------------------------------------------------------------

function matchesTrack(ev: CustomEvent, track: string): boolean {
  const t = ev.d?.track;
  return t === track || (Array.isArray(t) && t.includes(track));
}

function isKeyframeArray(v: any): boolean {
  return Array.isArray(v) && v.length > 0 && v.every((k) => Array.isArray(k));
}

function findKeyEvent(events: CustomEvent[], track: string, prop: string): CustomEvent | null {
  // prefer an event we can safely edit: keyframe-array value, plain track
  let best: CustomEvent | null = null;
  for (const ev of events) {
    if (ev.t !== 'AnimateTrack' || !matchesTrack(ev, track)) continue;
    const v = ev.d?.[prop];
    if (v === undefined) continue;
    if (isKeyframeArray(v) || isFlatVector(v)) best = ev;
  }
  return best;
}

function findOrCreateKeyEvent(
  events: CustomEvent[],
  track: string,
  prop: string,
  beat: number
): CustomEvent {
  const existing = findKeyEvent(events, track, prop);
  if (existing) return existing;
  // keep a pose together: reuse an event that already keys another property
  for (const other of KEYABLE) {
    if (other === prop) continue;
    const sibling = findKeyEvent(events, track, other);
    if (sibling) return sibling;
  }
  const ev: CustomEvent = { b: beat, t: 'AnimateTrack', d: { track, duration: 0 } };
  events.push(ev);
  return ev;
}

function isFlatVector(v: any): boolean {
  return Array.isArray(v) && v.length > 0 && v.every((x) => typeof x === 'number');
}

interface AbsKey {
  beat: number;
  values: number[];
  flags: (string | boolean)[]; // easing/spline strings preserved
}

function absoluteKeys(ev: CustomEvent, prop: string): AbsKey[] {
  const v = ev.d?.[prop];
  const b = ev.b ?? 0;
  const dur = typeof ev.d?.duration === 'number' ? ev.d.duration : 0;
  if (isFlatVector(v)) {
    return [{ beat: b, values: (v as number[]).slice(), flags: [] }];
  }
  if (!isKeyframeArray(v)) return [];
  const out: AbsKey[] = [];
  for (const entry of v as any[][]) {
    const nums = entry.filter((x) => typeof x === 'number') as number[];
    const flags = entry.filter((x) => typeof x !== 'number') as string[];
    if (nums.length === 0) continue;
    const t = nums.pop()!;
    out.push({ beat: b + t * dur, values: nums, flags });
  }
  return out.sort((a, c) => a.beat - c.beat);
}

function upsertKey(ev: CustomEvent, prop: string, beat: number, values: number[]): void {
  const keys = absoluteKeys(ev, prop);
  const idx = keys.findIndex((k) => Math.abs(k.beat - beat) < 1e-3);
  if (idx >= 0) {
    keys[idx] = { ...keys[idx], values: values.slice() };
  } else {
    keys.push({ beat, values: values.slice(), flags: [] });
    keys.sort((a, c) => a.beat - c.beat);
  }
  writeBackKeys(ev, prop, keys);
}

/**
 * Re-derive event window from key range, renormalize times. Other animated
 * properties on the same event are re-timed too so they stay correct.
 */
function writeBackKeys(ev: CustomEvent, prop: string, keys: AbsKey[]): void {
  const d = (ev.d = ev.d ?? {});
  const oldB = ev.b ?? 0;
  const oldDur = typeof d.duration === 'number' ? d.duration : 0;

  // collect absolute keys of OTHER keyable props before changing the window
  const others: Record<string, AbsKey[]> = {};
  for (const p of KEYABLE) {
    if (p === prop || !(p in d)) continue;
    others[p] = absoluteKeys(ev, p);
  }

  let newB = Math.min(...keys.map((k) => k.beat));
  let newEnd = Math.max(...keys.map((k) => k.beat));
  for (const list of Object.values(others)) {
    for (const k of list) {
      newB = Math.min(newB, k.beat);
      newEnd = Math.max(newEnd, k.beat);
    }
  }
  const newDur = Math.max(newEnd - newB, 0);

  ev.b = round(newB);
  d.duration = round(newDur);

  const serialize = (list: AbsKey[]): any => {
    if (list.length === 1 && list[0].flags.length === 0) {
      // single key: flat vector form
      return list[0].values.map(round);
    }
    return list.map((k) => {
      const t = newDur > 0 ? (k.beat - newB) / newDur : 0;
      return [...k.values.map(round), round(t), ...k.flags];
    });
  };

  d[prop] = serialize(keys);
  for (const [p, list] of Object.entries(others)) {
    if (list.length) d[p] = serialize(list);
  }
  void oldB;
  void oldDur;
}

function round(v: number): number {
  return Math.round(v * 10000) / 10000;
}
