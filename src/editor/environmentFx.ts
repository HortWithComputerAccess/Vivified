import * as THREE from 'three';
import { CustomEvent, V3Beatmap } from '../map/types';
import { ease } from '../anim/easings';

/**
 * Recreates scene-level state in the browser view:
 * - SetRenderingSettings -> fog distances / ambient intensity
 * - basic lighting events (with Chroma colors / env color scheme) -> tints
 *   the editor's scene lights so drops glow roughly the right hue
 */

interface FogEvent {
  beat: number;
  duration: number;
  easing?: string;
  fog?: boolean;
  fogStart?: number;
  fogEnd?: number;
  ambient?: number;
}

interface LightEvent {
  beat: number;
  type: number;
  value: number;
  color: [number, number, number] | null;
}

export interface SceneLights {
  hemi: THREE.HemisphereLight;
  dir: THREE.DirectionalLight;
  dir2: THREE.DirectionalLight;
}

const DEFAULT_FOG_NEAR = 1500;
const DEFAULT_FOG_FAR = 6000;

export class EnvironmentFx {
  private fogEvents: FogEvent[] = [];
  private lightEvents = new Map<number, LightEvent[]>();
  private envLeft = new THREE.Color(0.85, 0.08, 0.14);
  private envRight = new THREE.Color(0.19, 0.62, 1.0);
  private baseHemi = new THREE.Color(0xbfc4ff);
  private baseDir = new THREE.Color(0xffffff);
  private baseDir2 = new THREE.Color(0x8888ff);

  setEnvColors(left: any, right: any): void {
    if (left) this.envLeft.setRGB(left.r ?? 1, left.g ?? 0, left.b ?? 0, THREE.SRGBColorSpace);
    if (right) this.envRight.setRGB(right.r ?? 0, right.g ?? 0, right.b ?? 1, THREE.SRGBColorSpace);
  }

  rebuild(map: V3Beatmap, customEvents: CustomEvent[]): void {
    this.fogEvents = [];
    for (const ev of customEvents) {
      if (ev.t !== 'SetRenderingSettings') continue;
      const rs = ev.d?.renderSettings ?? {};
      const fe: FogEvent = {
        beat: ev.b ?? 0,
        duration: typeof ev.d?.duration === 'number' ? ev.d.duration : 0,
        easing: typeof ev.d?.easing === 'string' ? ev.d.easing : undefined,
      };
      let any = false;
      if (rs.fog !== undefined) {
        fe.fog = rs.fog === true || rs.fog === 1;
        any = true;
      }
      if (typeof firstNum(rs.fogStartDistance) === 'number') {
        fe.fogStart = firstNum(rs.fogStartDistance)!;
        any = true;
      }
      if (typeof firstNum(rs.fogEndDistance) === 'number') {
        fe.fogEnd = firstNum(rs.fogEndDistance)!;
        any = true;
      }
      if (typeof firstNum(rs.ambientIntensity) === 'number') {
        fe.ambient = firstNum(rs.ambientIntensity)!;
        any = true;
      }
      if (any) this.fogEvents.push(fe);
    }
    this.fogEvents.sort((a, b) => a.beat - b.beat);

    this.lightEvents.clear();
    for (const ev of map.basicBeatmapEvents ?? []) {
      const type = Number(ev.et ?? -1);
      if (type < 0 || type > 5) continue;
      const cd = ev.customData ?? {};
      let color: [number, number, number] | null = null;
      if (Array.isArray(cd.color)) {
        color = [Number(cd.color[0]), Number(cd.color[1]), Number(cd.color[2])];
      }
      let list = this.lightEvents.get(type);
      if (!list) {
        list = [];
        this.lightEvents.set(type, list);
      }
      list.push({ beat: Number(ev.b ?? 0), type, value: Number(ev.i ?? 0), color });
    }
    for (const list of this.lightEvents.values()) list.sort((a, b) => a.beat - b.beat);
  }

  apply(scene: THREE.Scene, lights: SceneLights, beat: number): void {
    // --- fog / ambient ---
    let fogOn = true;
    let fogStart = DEFAULT_FOG_NEAR;
    let fogEnd = DEFAULT_FOG_FAR;
    let ambient = 1;
    let prev: { start: number; end: number; amb: number } = { start: fogStart, end: fogEnd, amb: ambient };
    for (const fe of this.fogEvents) {
      if (fe.beat > beat) break;
      let s = fe.duration > 0 ? (beat - fe.beat) / fe.duration : 1;
      s = ease(fe.easing, Math.min(Math.max(s, 0), 1));
      if (fe.fog !== undefined) fogOn = fe.fog;
      if (fe.fogStart !== undefined) fogStart = prev.start + (fe.fogStart - prev.start) * s;
      if (fe.fogEnd !== undefined) fogEnd = prev.end + (fe.fogEnd - prev.end) * s;
      if (fe.ambient !== undefined) ambient = prev.amb + (fe.ambient - prev.amb) * s;
      prev = {
        start: fe.fogStart ?? prev.start,
        end: fe.fogEnd ?? prev.end,
        amb: fe.ambient ?? prev.amb,
      };
    }
    const fog = scene.fog as THREE.Fog | null;
    if (fog) {
      if (fogOn && fogEnd > 1) {
        fog.near = Math.max(fogStart, 0.1);
        fog.far = Math.max(fogEnd, fog.near + 1);
      } else {
        fog.near = DEFAULT_FOG_NEAR;
        fog.far = DEFAULT_FOG_FAR;
      }
    }
    lights.hemi.intensity = 0.9 * Math.min(Math.max(ambient, 0.1), 2);

    // --- light colors ---
    this.applyLight(lights.dir2, 0, beat, this.baseDir2, 0.5); // back lasers
    this.applyLight(lights.dir, 4, beat, this.baseDir, 1.4); // center
    // ring lights tint the hemisphere sky color
    const ring = this.latest(1, beat);
    if (ring) {
      const c = this.eventColor(ring);
      if (c) lights.hemi.color.copy(c);
      else lights.hemi.color.copy(this.baseHemi);
    }
  }

  private applyLight(
    light: THREE.DirectionalLight,
    type: number,
    beat: number,
    base: THREE.Color,
    baseIntensity: number
  ): void {
    const ev = this.latest(type, beat);
    if (!ev) return;
    const c = this.eventColor(ev);
    if (c) light.color.copy(c);
    else light.color.copy(base);
    const elapsed = beat - ev.beat;
    let k = 1;
    const v = ev.value % 4; // 1 on, 2 flash, 3 fade (0 off), red/blue share shape
    if (ev.value === 0) k = 0.12;
    else if (v === 2) k = 1 + 0.5 * Math.max(0, 1 - elapsed * 2);
    else if (v === 3) k = Math.max(0.1, 1 - elapsed * 1.2);
    light.intensity = baseIntensity * k;
  }

  private latest(type: number, beat: number): LightEvent | null {
    const list = this.lightEvents.get(type);
    if (!list) return null;
    let best: LightEvent | null = null;
    for (const e of list) {
      if (e.beat <= beat) best = e;
      else break;
    }
    return best;
  }

  private eventColor(ev: LightEvent): THREE.Color | null {
    if (ev.color) {
      const c = new THREE.Color();
      const max = Math.max(ev.color[0], ev.color[1], ev.color[2], 1);
      c.setRGB(ev.color[0] / max, ev.color[1] / max, ev.color[2] / max, THREE.SRGBColorSpace);
      return c;
    }
    if (ev.value === 0) return null;
    return ev.value >= 5 ? this.envLeft.clone() : this.envRight.clone();
  }
}

function firstNum(v: any): number | null {
  if (typeof v === 'number') return v;
  if (Array.isArray(v) && typeof v[0] === 'number') return v[0];
  return null;
}
