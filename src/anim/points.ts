import { ease } from './easings';

/**
 * Heck point definitions.
 *
 * Forms:
 *   42                                  -> constant float
 *   [x, y, z]                           -> constant vec
 *   [[x, y, z, t], [x, y, z, t, "easeOutQuad"], ...] -> keyframes
 *   "namedPointDef"                     -> reference into pointDefinitions
 *
 * Flags after the time value: easing name, "splineCatmullRom", "lerpHSV".
 * Base-provider strings ("baseHeadPosition" etc.) and modifier ops are not
 * evaluated in the editor; such points resolve to null.
 */

export interface Keyframe {
  values: number[];
  time: number;
  easing?: string;
  spline?: boolean;
  hsv?: boolean;
}

export interface PointDef {
  dim: number;
  frames: Keyframe[];
  unsupported?: boolean;
}

export function parsePointDef(
  raw: any,
  dim: number,
  pointDefinitions: Record<string, any[]> = {}
): PointDef | null {
  if (raw === undefined || raw === null) return null;
  if (typeof raw === 'string') {
    const named = pointDefinitions[raw];
    if (!named) return { dim, frames: [], unsupported: true };
    return parsePointDef(named, dim, pointDefinitions);
  }
  if (typeof raw === 'number') {
    return { dim, frames: [{ values: [raw], time: 0 }] };
  }
  if (!Array.isArray(raw)) return null;

  // flat constant: [x, y, z] (no nested arrays)
  if (raw.length > 0 && !raw.some((v) => Array.isArray(v))) {
    const values = raw.filter((v) => typeof v === 'number');
    if (values.length === 0) return { dim, frames: [], unsupported: true };
    return { dim, frames: [{ values, time: 0 }] };
  }

  const frames: Keyframe[] = [];
  let unsupported = false;
  for (const entry of raw) {
    if (!Array.isArray(entry)) continue;
    const values: number[] = [];
    let time = 0;
    let easing: string | undefined;
    let spline = false;
    let hsv = false;
    let numCount = 0;
    for (const item of entry) {
      if (typeof item === 'number') {
        values.push(item);
        numCount++;
      } else if (typeof item === 'string') {
        if (item.startsWith('ease')) easing = item;
        else if (item === 'splineCatmullRom') spline = true;
        else if (item === 'lerpHSV') hsv = true;
        else unsupported = true; // base provider / modifier
      } else if (Array.isArray(item)) {
        unsupported = true; // modifier operations
      }
    }
    if (numCount === 0) {
      unsupported = true;
      continue;
    }
    // last number is the time
    time = values.pop()!;
    frames.push({ values, time, easing, spline, hsv });
  }
  frames.sort((a, b) => a.time - b.time);
  return { dim, frames, unsupported };
}

/** Sample a point definition at normalized time t (0..1). */
export function samplePointDef(def: PointDef, t: number): number[] | null {
  const frames = def.frames;
  if (frames.length === 0) return null;
  if (frames.length === 1 || t <= frames[0].time) return pad(frames[0].values, def.dim);
  const last = frames[frames.length - 1];
  if (t >= last.time) return pad(last.values, def.dim);

  let i = 1;
  while (i < frames.length && frames[i].time < t) i++;
  const f0 = frames[i - 1];
  const f1 = frames[i];
  const span = f1.time - f0.time;
  let s = span > 0 ? (t - f0.time) / span : 1;
  s = ease(f1.easing, s);

  const a = pad(f0.values, def.dim);
  const b = pad(f1.values, def.dim);

  if (f1.spline) {
    const p0 = pad(frames[Math.max(0, i - 2)].values, def.dim);
    const p3 = pad(frames[Math.min(frames.length - 1, i + 1)].values, def.dim);
    return catmullRom(p0, a, b, p3, s);
  }
  if (f1.hsv && def.dim === 4) {
    return lerpHSV(a, b, s);
  }
  return a.map((v, k) => v + (b[k] - v) * s);
}

/** Quaternion sampling for rotation properties (values are euler degrees). */
export function sampleRotation(def: PointDef, t: number): [number, number, number] | null {
  // For the editor we lerp euler angles per-component with shortest-path wrap,
  // which matches visual expectations for typical keyframes.
  const frames = def.frames;
  if (frames.length === 0) return null;
  if (frames.length === 1 || t <= frames[0].time) return euler3(frames[0].values);
  const last = frames[frames.length - 1];
  if (t >= last.time) return euler3(last.values);
  let i = 1;
  while (i < frames.length && frames[i].time < t) i++;
  const f0 = frames[i - 1];
  const f1 = frames[i];
  const span = f1.time - f0.time;
  let s = span > 0 ? (t - f0.time) / span : 1;
  s = ease(f1.easing, s);
  const a = euler3(f0.values);
  const b = euler3(f1.values);
  const out: [number, number, number] = [0, 0, 0];
  for (let k = 0; k < 3; k++) {
    let delta = b[k] - a[k];
    while (delta > 180) delta -= 360;
    while (delta < -180) delta += 360;
    out[k] = a[k] + delta * s;
  }
  return out;
}

function euler3(v: number[]): [number, number, number] {
  return [v[0] ?? 0, v[1] ?? 0, v[2] ?? 0];
}

function pad(v: number[], dim: number): number[] {
  if (v.length >= dim) return v.slice(0, dim);
  const out = v.slice();
  while (out.length < dim) out.push(0);
  return out;
}

function catmullRom(p0: number[], p1: number[], p2: number[], p3: number[], t: number): number[] {
  const t2 = t * t;
  const t3 = t2 * t;
  return p1.map((_, i) => {
    return 0.5 * (
      2 * p1[i] +
      (-p0[i] + p2[i]) * t +
      (2 * p0[i] - 5 * p1[i] + 4 * p2[i] - p3[i]) * t2 +
      (-p0[i] + 3 * p1[i] - 3 * p2[i] + p3[i]) * t3
    );
  });
}

function lerpHSV(a: number[], b: number[], t: number): number[] {
  const ha = rgbToHsv(a[0], a[1], a[2]);
  const hb = rgbToHsv(b[0], b[1], b[2]);
  let dh = hb[0] - ha[0];
  if (dh > 0.5) dh -= 1;
  if (dh < -0.5) dh += 1;
  const h = (ha[0] + dh * t + 1) % 1;
  const s = ha[1] + (hb[1] - ha[1]) * t;
  const v = ha[2] + (hb[2] - ha[2]) * t;
  const rgb = hsvToRgb(h, s, v);
  return [rgb[0], rgb[1], rgb[2], a[3] + (b[3] - a[3]) * t];
}

function rgbToHsv(r: number, g: number, b: number): [number, number, number] {
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  const d = max - min;
  let h = 0;
  if (d !== 0) {
    if (max === r) h = ((g - b) / d) % 6;
    else if (max === g) h = (b - r) / d + 2;
    else h = (r - g) / d + 4;
    h /= 6;
    if (h < 0) h += 1;
  }
  return [h, max === 0 ? 0 : d / max, max];
}

function hsvToRgb(h: number, s: number, v: number): [number, number, number] {
  const i = Math.floor(h * 6);
  const f = h * 6 - i;
  const p = v * (1 - s);
  const q = v * (1 - f * s);
  const t = v * (1 - (1 - f) * s);
  switch (i % 6) {
    case 0: return [v, t, p];
    case 1: return [q, v, p];
    case 2: return [p, v, t];
    case 3: return [p, q, v];
    case 4: return [t, p, v];
    default: return [v, p, q];
  }
}
