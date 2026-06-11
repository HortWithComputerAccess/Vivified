import { readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { AssetDB, ClassID } from '../src/unity/assets';

const MAP_DIR = join(process.cwd(), 'Reference-Do-Not-Include', 'Beatsaber Map That Uses Vivify for reference');
const buf = readFileSync(join(MAP_DIR, 'bundleWindows2019.vivify'));
const db = AssetDB.fromBundle(buf.buffer.slice(buf.byteOffset, buf.byteOffset + buf.byteLength));

let count = 0;
const out: string[] = [];
for (const [pathID, [, info]] of db.objects) {
  if (info.classID !== ClassID.Shader) continue;
  const sh = db.read(pathID) as any;
  const pf = sh?.m_ParsedForm;
  if (!pf) { out.push(`shader ${pathID}: no parsed form; keys: ${Object.keys(sh ?? {}).join(',')}`); continue; }
  count++;
  if (count <= 4) {
    out.push(`=== shader ${pathID}: ${pf.m_Name} ===`);
    out.push(`pf keys: ${Object.keys(pf).join(', ')}`);
    const props = pf.m_PropInfo?.m_Props ?? [];
    out.push(`props: ${props.slice(0, 12).map((p: any) => `${p.m_Name}:${p.m_Type}=${JSON.stringify(p.m_DefValue ?? p.m_DefValue0)}`).join(' | ')}`);
    const sub = (pf.m_SubShaders ?? [])[0];
    if (sub) {
      out.push(`subshader keys: ${Object.keys(sub).join(', ')}; tags: ${JSON.stringify(sub.m_Tags)}`);
      const pass = (sub.m_Passes ?? [])[0];
      if (pass) {
        out.push(`pass keys: ${Object.keys(pass).join(', ')}`);
        out.push(`pass tags: ${JSON.stringify(pass.m_Tags)} nameIndices?`);
        const st = pass.m_State;
        if (st) {
          out.push(`state keys: ${Object.keys(st).join(', ')}`);
          out.push(`state: ${JSON.stringify({
            rtBlend0: st.rtBlend0, culling: st.culling, zWrite: st.zWrite, zTest: st.zTest,
            tags: st.m_Tags,
          })}`);
        }
      }
    }
    out.push('');
  }
}
out.push(`total shaders with parsed form: ${count}`);
// also dump all shader names + queue tags
for (const [pathID, [, info]] of db.objects) {
  if (info.classID !== ClassID.Shader) continue;
  const sh = db.read(pathID) as any;
  const pf = sh?.m_ParsedForm;
  if (!pf) continue;
  const sub = (pf.m_SubShaders ?? [])[0];
  const tags = sub?.m_Tags ?? {};
  const pass = (sub?.m_Passes ?? [])[0];
  const st = pass?.m_State;
  const blend = st?.rtBlend0 ? `${st.rtBlend0.srcBlend?.val}->${st.rtBlend0.destBlend?.val}` : '?';
  out.push(`${pf.m_Name}  queue=${JSON.stringify(tags)} blend=${blend} zw=${st?.zWrite?.val} cull=${st?.culling?.val}`);
}
writeFileSync(join(process.cwd(), '.verify', 'shaders.txt'), out.join('\n'));
console.log(out.slice(0, 60).join('\n'));
