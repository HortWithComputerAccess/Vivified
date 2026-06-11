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
  /** transform path ("Ring/Inner") when known from editor curves */
  path: string;
  /** CRC32 hash of the path (release/muscle clips bind by hash) */
  pathHash: number | null;
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

  // release builds bake curves into the streamed "muscle clip" format
  const muscle = parseMuscleClip(clip, name);
  if (muscle) return muscle;

  const bindings = new Map<string, ClipBinding>();
  const get = (path: string): ClipBinding => {
    let b = bindings.get(path);
    if (!b) {
      b = { path, pathHash: null };
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

// ---------------------------------------------------------------------------
// Muscle clip (release format): StreamedClip + DenseClip + ConstantClip with
// curve slots mapped to transforms via m_ClipBindingConstant path hashes.
// ---------------------------------------------------------------------------

interface CurveSamples {
  times: number[];
  values: number[];
}

function parseMuscleClip(clip: any, name: string): ParsedClip | null {
  const mc = clip.m_MuscleClip;
  const data = mc?.m_Clip?.data ?? mc?.m_Clip;
  if (!data) return null;
  const streamed = data.m_StreamedClip;
  const dense = data.m_DenseClip;
  const constant = data.m_ConstantClip;
  const genericBindings: any[] = clip.m_ClipBindingConstant?.genericBindings ?? [];
  if (genericBindings.length === 0) return null;

  const streamedCount = Number(streamed?.curveCount ?? 0);
  const denseCount = Number(dense?.m_CurveCount ?? 0);
  const constantData: number[] = Array.isArray(constant?.data) ? constant.data : [];
  const stopTime = Number(mc.m_StopTime ?? 0) || Number(clip.m_AnimationClipSettings?.m_StopTime ?? 0);

  // decode streamed frames into per-curve keyframe lists
  const curves = new Map<number, CurveSamples>();
  if (streamedCount > 0 && Array.isArray(streamed?.data) && streamed.data.length) {
    const words: number[] = streamed.data;
    const bytes = new ArrayBuffer(words.length * 4);
    const view = new DataView(bytes);
    for (let i = 0; i < words.length; i++) view.setUint32(i * 4, words[i] >>> 0, true);
    let pos = 0;
    const total = words.length * 4;
    while (pos + 8 <= total) {
      const time = view.getFloat32(pos, true);
      const keyCount = view.getInt32(pos + 4, true);
      pos += 8;
      if (keyCount < 0 || pos + keyCount * 20 > total) break;
      for (let k = 0; k < keyCount; k++) {
        const index = view.getInt32(pos, true);
        const value = view.getFloat32(pos + 16, true); // coeff[3] is the value
        pos += 20;
        if (!isFinite(time)) {
          // sentinel frames at ±infinity carry initial values; pin to t=0
          if (time < 0) addSample(curves, index, 0, value);
          continue;
        }
        addSample(curves, index, time, value);
      }
    }
  }
  // dense samples
  if (denseCount > 0 && Array.isArray(dense?.m_SampleArray)) {
    const frames = Number(dense.m_FrameCount ?? 0);
    const rate = Number(dense.m_SampleRate ?? 60) || 60;
    const begin = Number(dense.m_BeginTime ?? 0);
    const arr: number[] = dense.m_SampleArray;
    for (let f = 0; f < frames; f++) {
      const t = begin + f / rate;
      for (let c = 0; c < denseCount; c++) {
        addSample(curves, streamedCount + c, t, Number(arr[f * denseCount + c] ?? 0));
      }
    }
  }
  // constants
  for (let c = 0; c < constantData.length; c++) {
    addSample(curves, streamedCount + denseCount + c, 0, Number(constantData[c] ?? 0));
  }

  // walk bindings over the curve slot space
  const bindings: ClipBinding[] = [];
  let slot = 0;
  let length = 0;
  for (const gb of genericBindings) {
    const classID = Number(gb.typeID ?? gb.classID ?? 0);
    const attr = Number(gb.attribute ?? 0);
    const isTransform = classID === 4;
    const width = !isTransform ? 1 : attr === 2 ? 4 : 3;
    if (isTransform && (attr === 1 || attr === 3 || attr === 4 || attr === 2)) {
      const comp: CurveSamples[] = [];
      for (let i = 0; i < width; i++) comp.push(curves.get(slot + i) ?? { times: [0], values: [defaultFor(attr, i)] });
      const times = mergeTimes(comp);
      for (const t of times) length = Math.max(length, t);
      const binding: ClipBinding = { path: '', pathHash: Number(gb.path ?? 0) >>> 0 };
      if (attr === 2) {
        binding.rotation = {
          times,
          values: times.map((t) => [
            sampleCurve(comp[0], t),
            sampleCurve(comp[1], t),
            sampleCurve(comp[2], t),
            sampleCurve(comp[3], t),
          ]),
        };
      } else {
        const keys: ClipKeys<[number, number, number]> = {
          times,
          values: times.map((t) => [sampleCurve(comp[0], t), sampleCurve(comp[1], t), sampleCurve(comp[2], t)]),
        };
        if (attr === 1) binding.position = keys;
        else if (attr === 3) binding.scale = keys;
        else binding.euler = keys;
      }
      bindings.push(binding);
    }
    slot += width;
  }
  if (bindings.length === 0) return null;
  const len = stopTime > 0 ? stopTime : length;
  if (len <= 0) return null;
  return { name, length: len, loop: mc.m_LoopTime === true || mc.m_LoopTime === 1, bindings };
}

function addSample(curves: Map<number, CurveSamples>, index: number, time: number, value: number): void {
  let c = curves.get(index);
  if (!c) {
    c = { times: [], values: [] };
    curves.set(index, c);
  }
  const n = c.times.length;
  if (n > 0 && time <= c.times[n - 1]) {
    if (time === c.times[n - 1]) c.values[n - 1] = value;
    return;
  }
  c.times.push(time);
  c.values.push(value);
}

function sampleCurve(c: CurveSamples, t: number): number {
  const { times, values } = c;
  if (times.length === 0) return 0;
  if (t <= times[0]) return values[0];
  if (t >= times[times.length - 1]) return values[values.length - 1];
  let lo = 0;
  let hi = times.length - 1;
  while (hi - lo > 1) {
    const mid = (lo + hi) >> 1;
    if (times[mid] <= t) lo = mid;
    else hi = mid;
  }
  const s = (t - times[lo]) / (times[hi] - times[lo] || 1);
  return values[lo] + (values[hi] - values[lo]) * s;
}

function mergeTimes(comps: CurveSamples[]): number[] {
  const set = new Set<number>();
  for (const c of comps) for (const t of c.times) set.add(t);
  const out = [...set].filter((t) => isFinite(t)).sort((a, b) => a - b);
  return out.length ? out : [0];
}

function defaultFor(attr: number, component: number): number {
  if (attr === 3) return 1; // scale
  if (attr === 2 && component === 3) return 1; // quaternion w
  return 0;
}

/** Standard CRC32 (IEEE), as Unity uses for animation binding paths. */
const CRC_TABLE = (() => {
  const table = new Uint32Array(256);
  for (let i = 0; i < 256; i++) {
    let c = i;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    table[i] = c >>> 0;
  }
  return table;
})();

export function crc32(str: string): number {
  const bytes = new TextEncoder().encode(str);
  let crc = 0xffffffff;
  for (const b of bytes) crc = CRC_TABLE[(crc ^ b) & 0xff] ^ (crc >>> 8);
  return (crc ^ 0xffffffff) >>> 0;
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
