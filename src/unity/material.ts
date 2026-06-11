import { AssetDB } from './assets';

export interface ParsedMaterial {
  name: string;
  shaderName: string;
  color: [number, number, number, number];
  mainTexPathID: number | null;
  transparent: boolean;
  floats: Record<string, number>;
  colors: Record<string, [number, number, number, number]>;
}

/** Extract a renderable approximation of a Unity material. */
export function parseMaterial(db: AssetDB, matPathID: number): ParsedMaterial | null {
  const mat = db.read(matPathID) as any;
  if (!mat) return null;
  const name = String(mat.m_Name ?? 'Material');

  let shaderName = '';
  const shader = db.deref(mat.m_Shader) as any;
  if (shader) {
    shaderName = String(shader.m_ParsedForm?.m_Name ?? shader.m_Name ?? '');
  }

  const props = mat.m_SavedProperties ?? {};
  const floats: Record<string, number> = {};
  const colors: Record<string, [number, number, number, number]> = {};
  let mainTexPathID: number | null = null;

  for (const [key, val] of entries(props.m_Floats)) {
    floats[key] = Number(val);
  }
  for (const [key, val] of entries(props.m_Colors)) {
    const c = val as any;
    colors[key] = [num(c?.r, 1), num(c?.g, 1), num(c?.b, 1), num(c?.a, 1)];
  }
  for (const [key, val] of entries(props.m_TexEnvs)) {
    const ptr = (val as any)?.m_Texture;
    if (!mainTexPathID && ptr?.m_PathID && (key === '_MainTex' || key === '_BaseMap')) {
      mainTexPathID = ptr.m_PathID;
    }
  }
  // fall back to any texture if no _MainTex
  if (!mainTexPathID) {
    for (const [, val] of entries(props.m_TexEnvs)) {
      const ptr = (val as any)?.m_Texture;
      if (ptr?.m_PathID && db.classOf(ptr.m_PathID) === 28) {
        mainTexPathID = ptr.m_PathID;
        break;
      }
    }
  }

  const color = colors['_Color'] ?? colors['_BaseColor'] ?? colors['_TintColor'] ?? [1, 1, 1, 1];
  const queue = Number(mat.m_CustomRenderQueue ?? 0);
  const keywords = String(mat.m_ShaderKeywords ?? '');
  const transparent =
    queue >= 3000 || color[3] < 0.999 || /_ALPHABLEND|_ALPHAPREMULTIPLY|TRANSPARENT/i.test(keywords);

  return { name, shaderName, color, mainTexPathID, transparent, floats, colors };
}

function entries(container: any): [string, unknown][] {
  if (!container) return [];
  if (Array.isArray(container)) {
    return container.map((p: any) => [String(p.first), p.second]);
  }
  return Object.entries(container);
}

function num(v: any, def: number): number {
  return typeof v === 'number' && isFinite(v) ? v : def;
}
