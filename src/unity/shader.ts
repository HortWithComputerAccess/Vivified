import { AssetDB } from './assets';

/**
 * Recreated render state from Unity's SerializedShader (m_ParsedForm).
 * We can't run the compiled HLSL, but blend mode, culling, depth state and
 * the render queue describe most of a shader's visual character. Anything we
 * can't interpret falls back to a standard lit material.
 */
export interface ShaderInfo {
  name: string;
  queue: string | null; // QUEUE tag, e.g. "Transparent", "Geometry+1"
  renderType: string | null;
  /** unity BlendMode enum values, or null when driven by a material float */
  srcBlend: number | null;
  dstBlend: number | null;
  srcBlendProp: string | null;
  dstBlendProp: string | null;
  zWrite: boolean | null;
  zWriteProp: string | null;
  cull: number | null; // 0 off, 1 front, 2 back
  cullProp: string | null;
  /** declared properties: name -> type (0 color, 1 vector, 2 float, 3 range, 4 texture) */
  props: { name: string; type: number }[];
}

// Unity BlendMode enum
export const Blend = {
  Zero: 0,
  One: 1,
  DstColor: 2,
  SrcColor: 3,
  OneMinusDstColor: 4,
  SrcAlpha: 5,
  OneMinusSrcColor: 6,
  DstAlpha: 7,
  OneMinusDstAlpha: 8,
  SrcAlphaSaturate: 9,
  OneMinusSrcAlpha: 10,
} as const;

const cache = new WeakMap<AssetDB, Map<number, ShaderInfo | null>>();

export function parseShader(db: AssetDB, shaderPathID: number): ShaderInfo | null {
  let perDb = cache.get(db);
  if (!perDb) {
    perDb = new Map();
    cache.set(db, perDb);
  }
  if (perDb.has(shaderPathID)) return perDb.get(shaderPathID)!;
  const info = parseShaderUncached(db, shaderPathID);
  perDb.set(shaderPathID, info);
  return info;
}

function parseShaderUncached(db: AssetDB, shaderPathID: number): ShaderInfo | null {
  const sh = db.read(shaderPathID) as any;
  const pf = sh?.m_ParsedForm;
  if (!pf) return null;

  const props: { name: string; type: number }[] = [];
  for (const p of pf.m_PropInfo?.m_Props ?? []) {
    if (p?.m_Name) props.push({ name: String(p.m_Name), type: Number(p.m_Type ?? 2) });
  }

  const sub = (pf.m_SubShaders ?? [])[0];
  const tags = sub?.m_Tags?.tags ?? {};
  // pick the first pass with a state (skip shadow caster passes when tagged)
  let state: any = null;
  for (const pass of sub?.m_Passes ?? []) {
    const passTags = pass?.m_State?.m_Tags?.tags ?? pass?.m_Tags?.tags ?? {};
    if (String(passTags.LIGHTMODE ?? '').toLowerCase() === 'shadowcaster') continue;
    if (pass?.m_State) {
      state = pass.m_State;
      break;
    }
  }

  const val = (sv: any): { num: number | null; prop: string | null } => {
    if (!sv) return { num: null, prop: null };
    const name = String(sv.name ?? '');
    if (name && name !== '<noninit>') return { num: null, prop: name };
    return { num: Number(sv.val ?? 0), prop: null };
  };

  const blend = state?.rtBlend0;
  const src = val(blend?.srcBlend);
  const dst = val(blend?.destBlend);
  const zw = val(state?.zWrite);
  const cull = val(state?.culling);

  return {
    name: String(pf.m_Name ?? ''),
    queue: tags.QUEUE != null ? String(tags.QUEUE) : null,
    renderType: tags.RenderType != null ? String(tags.RenderType) : null,
    srcBlend: src.num,
    dstBlend: dst.num,
    srcBlendProp: src.prop,
    dstBlendProp: dst.prop,
    zWrite: zw.num === null ? null : zw.num !== 0,
    zWriteProp: zw.prop,
    cull: cull.num,
    cullProp: cull.prop,
    props,
  };
}

export interface ResolvedBlend {
  mode: 'opaque' | 'alpha' | 'additive' | 'multiply';
  transparent: boolean;
  depthWrite: boolean;
  cull: number; // 0 off, 1 front, 2 back
}

/** Resolve final blend state using material floats for property-driven state. */
export function resolveBlend(info: ShaderInfo | null, floats: Record<string, number>): ResolvedBlend {
  if (!info) return { mode: 'opaque', transparent: false, depthWrite: true, cull: 2 };

  const src = info.srcBlend ?? (info.srcBlendProp ? floats[info.srcBlendProp] : undefined) ?? Blend.One;
  const dst = info.dstBlend ?? (info.dstBlendProp ? floats[info.dstBlendProp] : undefined) ?? Blend.Zero;
  const zWrite = info.zWrite ?? (info.zWriteProp ? floats[info.zWriteProp] !== 0 : true);
  const cull = info.cull ?? (info.cullProp ? Number(floats[info.cullProp] ?? 2) : 2);

  let mode: ResolvedBlend['mode'] = 'opaque';
  if (dst === Blend.One) {
    mode = 'additive';
  } else if (dst === Blend.OneMinusSrcAlpha) {
    mode = 'alpha';
  } else if (dst === Blend.SrcColor || (src === Blend.DstColor && dst === Blend.Zero)) {
    mode = 'multiply';
  } else if (src === Blend.SrcAlpha) {
    mode = 'alpha';
  } else if (/transparent/i.test(info.queue ?? '')) {
    // transparent queue but opaque-looking blend: custom shader doing its own
    // thing; treat as alpha so it at least sorts/blends sanely
    mode = src === Blend.Zero && dst === Blend.Zero ? 'additive' : 'alpha';
  }

  return {
    mode,
    transparent: mode !== 'opaque',
    depthWrite: zWrite && mode === 'opaque',
    cull,
  };
}
