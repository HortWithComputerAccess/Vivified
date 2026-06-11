import * as THREE from 'three';
import { CustomEvent } from '../map/types';
import { ease } from './easings';
import { parsePointDef, samplePointDef, PointDef } from './points';
import { ThreeConverter } from '../unity/toThree';

interface MatPropEvent {
  beat: number;
  duration: number;
  easing?: string;
  id: string;
  type: string; // Color | Float | Vector | Texture | Keyword
  static: any;
  def: PointDef | null;
}

/**
 * Evaluates SetMaterialProperty events over time and applies the result to
 * the converted Three.js materials, so color/alpha animations preview live.
 */
export class MaterialAnimator {
  /** asset path -> property id -> sorted events */
  private byAsset = new Map<string, Map<string, MatPropEvent[]>>();
  private warned = new Set<string>();

  rebuild(events: CustomEvent[], pointDefinitions: Record<string, any[]>): void {
    this.byAsset.clear();
    const sorted = [...events]
      .filter((e) => e.t === 'SetMaterialProperty' && e.d?.asset && Array.isArray(e.d?.properties))
      .sort((a, b) => (a.b ?? 0) - (b.b ?? 0));
    for (const ev of sorted) {
      const asset = String(ev.d.asset).toLowerCase();
      let perProp = this.byAsset.get(asset);
      if (!perProp) {
        perProp = new Map();
        this.byAsset.set(asset, perProp);
      }
      for (const p of ev.d.properties) {
        if (!p?.id || p.value === undefined) continue;
        const type = String(p.type ?? 'Float');
        const dim = type === 'Color' || type === 'Vector' ? 4 : 1;
        let def: PointDef | null = null;
        let staticVal: any = p.value;
        if (Array.isArray(p.value) || typeof p.value === 'string') {
          def = parsePointDef(p.value, dim, pointDefinitions);
          if (def?.unsupported && def.frames.length === 0) def = null;
        }
        const list = perProp.get(String(p.id)) ?? [];
        list.push({
          beat: ev.b ?? 0,
          duration: typeof ev.d.duration === 'number' ? ev.d.duration : 0,
          easing: typeof ev.d.easing === 'string' ? ev.d.easing : undefined,
          id: String(p.id),
          type,
          static: staticVal,
          def,
        });
        perProp.set(String(p.id), list);
      }
    }
  }

  get assetCount(): number {
    return this.byAsset.size;
  }

  /** Apply material property state at `beat` to all affected materials. */
  apply(converter: ThreeConverter | null, beat: number): void {
    if (!converter) return;
    for (const [asset, perProp] of this.byAsset) {
      const found = converter.materialByPath(asset);
      if (!found) continue;
      const mat = found.material as THREE.MeshBasicMaterial | THREE.MeshStandardMaterial;
      const primaryColorProp = found.parsed.colorProp;
      for (const [id, list] of perProp) {
        // last event that has started
        let entry: MatPropEvent | null = null;
        for (const e of list) {
          if (e.beat <= beat) entry = e;
          else break;
        }
        if (!entry) {
          // before the first event: restore the material's authored base state
          const base = mat.userData.baseColor as number[] | undefined;
          if (base && (id === primaryColorProp || /Color$/.test(id))) {
            mat.color.setRGB(base[0], base[1], base[2]);
            if (mat.transparent) mat.opacity = Math.min(Math.max(base[3] ?? 1, 0), 1);
          }
          continue;
        }
        let s = entry.duration > 0 ? (beat - entry.beat) / entry.duration : 1;
        s = ease(entry.easing, Math.min(Math.max(s, 0), 1));

        let value: number[] | null = null;
        if (entry.def) {
          value = samplePointDef(entry.def, s);
        } else if (typeof entry.static === 'number') {
          value = [entry.static];
        }
        if (!value) continue;
        this.applyProp(mat, id, entry.type, value, primaryColorProp);
      }
    }
  }

  private applyProp(
    mat: THREE.MeshBasicMaterial | THREE.MeshStandardMaterial,
    id: string,
    type: string,
    value: number[],
    primaryColorProp: string | null
  ): void {
    if (type === 'Color') {
      const isPrimary =
        id === primaryColorProp || /^_(Base)?Color$|^_TintColor$|^_FaceColor$/.test(id);
      if (isPrimary) {
        mat.color.setRGB(value[0] ?? 1, value[1] ?? 1, value[2] ?? 1);
        if (value.length > 3) {
          const a = Math.min(Math.max(value[3], 0), 1);
          if (mat.transparent) mat.opacity = a;
        }
      } else if (id === '_EmissionColor' && 'emissive' in mat) {
        (mat as THREE.MeshStandardMaterial).emissive.setRGB(value[0] ?? 0, value[1] ?? 0, value[2] ?? 0);
      }
      // other color ids (e.g. _AltColor1) have no analogue on a standard material
    } else if (type === 'Float') {
      if (/^_(Alpha|Opacity)$/i.test(id)) {
        mat.transparent = true;
        mat.opacity = Math.min(Math.max(value[0], 0), 1);
      } else if (/^_Cutoff$/i.test(id)) {
        mat.alphaTest = Math.min(Math.max(value[0], 0), 1);
        mat.needsUpdate = true;
      } else if (/^_(Brightness|Intensity)$/i.test(id)) {
        const base = (mat.userData.baseColor as number[]) ?? [1, 1, 1, 1];
        const k = Math.min(Math.max(value[0], 0), 4);
        mat.color.setRGB(base[0] * k, base[1] * k, base[2] * k);
      }
    }
    // Vector/Texture/Keyword: not previewable on standard materials
  }
}
