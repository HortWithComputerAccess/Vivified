import { AssetDB, ClassID } from './assets';

/**
 * Parsed Unity AnimationClip: transform curves bound by relative path
 * ("Ring/Inner"). Sampled with linear interpolation, which is visually fine
 * for baked clips; quaternion curves use nlerp.
 */
export interface ClipKeys<T> {
  times: number[];
  values: T[];
}

export interface ClipBinding {
  path: string;
  position?: ClipKeys<[number, number, number]>;
  euler?: ClipKeys<[number, number, number]>;
  rotation?: ClipKeys<[number, number, number, number]>;
  scale?: ClipKeys<[number, number, number]>;
}

export interface ParsedClip {
  name: string;
  length: number;
  loop: boolean;
  bindings: ClipBinding[];
}

const clipCache = new WeakMap<AssetDB, Map<number, ParsedClip | null>>();

export function parseAnimationClip(db: AssetDB, clipPathID: number): ParsedClip | null {
  let perDb = clipCache.get(db);
  if (!perDb) {
    perDb = new Map();
    clipCache.set(db, perDb);
  }
  if (perDb.has(clipPathID)) return perDb.get(clipPathID)!;
  let clip: ParsedClip | null = null;
  try {
    clip = parseClipUncached(db, clipPathID);
  } catch (e) {
    console.warn(`AnimationClip ${clipPathID} parse failed`, e);
  }
  perDb.set(clipPathID, clip);
  return clip;
}

function parseClipUncached(db: AssetDB, clipPathID: number): ParsedClip | null {
  const clip = db.read(clipPathID) as any;
  if (!clip) return null;
  const name = String(clip.m_Name ?? 'Clip');
  const bindings = new Map<string, ClipBinding>();
  const get = (path: string): ClipBinding => {
    let b = bindings.get(path);
    if (!b) {
      b = { path };
      bindings.set(path, b);
    }
    return b;
  };

  let length = 0;
  const track = (t: number) => {
    if (t > length) length = t;
  };

  for (const c of clip.m_PositionCurves ?? []) {
    const keys = vecKeys(c?.curve?.m_Curve);
    if (!keys) continue;
    keys.times.forEach(track);
    get(String(c.path ?? '')).position = keys;
  }
  for (const c of clip.m_EulerCurves ?? []) {
    const keys = vecKeys(c?.curve?.m_Curve);
    if (!keys) continue;
    keys.times.forEach(track);
    get(String(c.path ?? '')).euler = keys;
  }
  for (const c of clip.m_RotationCurves ?? []) {
    const curve = c?.curve?.m_Curve;
    if (!Array.isArray(curve) || curve.length === 0) continue;
    const times: number[] = [];
    const values: [number, number, number, number][] = [];
    for (const k of curve) {
      const v = k?.value;
      if (!v) continue;
      times.push(Number(k.time ?? 0));
      values.push([num(v.x), num(v.y), num(v.z), num(v.w, 1)]);
    }
    if (times.length) {
      times.forEach(track);
      get(String(c.path ?? '')).rotation = { times, values };
    }
  }
  for (const c of clip.m_ScaleCurves ?? []) {
    const keys = vecKeys(c?.curve?.m_Curve);
    if (!keys) continue;
    keys.times.forEach(track);
    get(String(c.path ?? '')).scale = keys;
  }

  const settings = clip.m_AnimationClipSettings ?? {};
  const stop = Number(settings.m_StopTime ?? 0);
  if (stop > length) length = stop;
  const loop = settings.m_LoopTime === true || settings.m_LoopTime === 1 || clip.m_WrapMode === 2;

  if (bindings.size === 0 || length <= 0) return null;
  return { name, length, loop, bindings: [...bindings.values()] };
}

/** Resolve an Animator's controller to its first animation clip. */
export function clipForController(db: AssetDB, controllerPathID: number): ParsedClip | null {
  const ctrl = db.read(controllerPathID) as any;
  if (!ctrl) return null;
  for (const ptr of ctrl.m_AnimationClips ?? []) {
    if (ptr?.m_PathID && db.classOf(ptr.m_PathID) === ClassID.AnimationClip) {
      const clip = parseAnimationClip(db, ptr.m_PathID);
      if (clip) return clip;
    }
  }
  return null;
}

// --- sampling ---------------------------------------------------------------

export function sampleVec(
  keys: ClipKeys<[number, number, number]>,
  t: number
): [number, number, number] {
  const { times, values } = keys;
  if (t <= times[0]) return values[0];
  if (t >= times[times.length - 1]) return values[values.length - 1];
  let i = 1;
  while (times[i] < t) i++;
  const s = (t - times[i - 1]) / (times[i] - times[i - 1] || 1);
  const a = values[i - 1];
  const b = values[i];
  return [a[0] + (b[0] - a[0]) * s, a[1] + (b[1] - a[1]) * s, a[2] + (b[2] - a[2]) * s];
}

export function sampleQuat(
  keys: ClipKeys<[number, number, number, number]>,
  t: number
): [number, number, number, number] {
  const { times, values } = keys;
  if (t <= times[0]) return values[0];
  if (t >= times[times.length - 1]) return values[values.length - 1];
  let i = 1;
  while (times[i] < t) i++;
  let s = (t - times[i - 1]) / (times[i] - times[i - 1] || 1);
  const a = values[i - 1];
  let b = values[i];
  // nlerp with hemisphere correction
  const dot = a[0] * b[0] + a[1] * b[1] + a[2] * b[2] + a[3] * b[3];
  if (dot < 0) b = [-b[0], -b[1], -b[2], -b[3]];
  const out: [number, number, number, number] = [
    a[0] + (b[0] - a[0]) * s,
    a[1] + (b[1] - a[1]) * s,
    a[2] + (b[2] - a[2]) * s,
    a[3] + (b[3] - a[3]) * s,
  ];
  const len = Math.hypot(out[0], out[1], out[2], out[3]) || 1;
  return [out[0] / len, out[1] / len, out[2] / len, out[3] / len];
}

function vecKeys(curve: any): ClipKeys<[number, number, number]> | null {
  if (!Array.isArray(curve) || curve.length === 0) return null;
  const times: number[] = [];
  const values: [number, number, number][] = [];
  for (const k of curve) {
    const v = k?.value;
    if (!v) continue;
    times.push(Number(k.time ?? 0));
    values.push([num(v.x), num(v.y), num(v.z)]);
  }
  return times.length ? { times, values } : null;
}

function num(v: any, def = 0): number {
  return typeof v === 'number' && isFinite(v) ? v : def;
}
