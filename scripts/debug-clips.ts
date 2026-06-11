import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { AssetDB, ClassID, buildGameObjectTree } from '../src/unity/assets';

const MAP_DIR = join(process.cwd(), 'Reference-Do-Not-Include', 'Beatsaber Map That Uses Vivify for reference');
const buf = readFileSync(join(MAP_DIR, 'bundleWindows2019.vivify'));
const db = AssetDB.fromBundle(buf.buffer.slice(buf.byteOffset, buf.byteOffset + buf.byteLength));

// --- clips: editor curves vs muscle clip ---
for (const [pathID, [, info]] of db.objects) {
  if (info.classID !== ClassID.AnimationClip) continue;
  const clip = db.read(pathID) as any;
  const mc = clip.m_MuscleClip;
  console.log(`clip "${clip.m_Name}":`);
  console.log(`  editor curves: pos=${clip.m_PositionCurves?.length ?? 0} euler=${clip.m_EulerCurves?.length ?? 0} rot=${clip.m_RotationCurves?.length ?? 0} scale=${clip.m_ScaleCurves?.length ?? 0} float=${clip.m_FloatCurves?.length ?? 0}`);
  if (mc) {
    const c = mc.m_Clip?.data ?? mc.m_Clip;
    console.log(`  muscle: keys ${Object.keys(mc).join(',')}`);
    if (c) {
      console.log(`  m_Clip keys: ${Object.keys(c).join(',')}`);
      const sc = c.m_StreamedClip;
      const dc = c.m_DenseClip;
      const cc = c.m_ConstantClip;
      console.log(`  streamed: curveCount=${sc?.curveCount} dataLen=${(sc?.data?.length ?? 0)}`);
      console.log(`  dense: curveCount=${dc?.m_CurveCount} frames=${dc?.m_FrameCount} rate=${dc?.m_SampleRate} samples=${dc?.m_SampleArray?.length ?? 0}`);
      console.log(`  constant: dataLen=${cc?.data?.length ?? 0}`);
    }
  }
  const bind = clip.m_ClipBindingConstant;
  if (bind) {
    const gb = bind.genericBindings ?? [];
    console.log(`  bindings: ${gb.length}`, gb.slice(0, 6).map((b: any) => `path=${b.path} attr=${b.attribute} cls=${b.typeID ?? b.classID}`).join(' | '));
  }
  console.log(`  stopTime=${clip.m_AnimationClipSettings?.m_StopTime} loop=${clip.m_AnimationClipSettings?.m_LoopTime} sampleRate=${clip.m_SampleRate}`);
}

// --- controllers: m_TOS hash->path ---
for (const [pathID, [, info]] of db.objects) {
  if (info.classID !== ClassID.AnimatorController) continue;
  const ctrl = db.read(pathID) as any;
  const tos = ctrl.m_TOS;
  const entries = Array.isArray(tos) ? tos : Object.entries(tos ?? {});
  console.log(`controller "${ctrl.m_Name}": clips=${ctrl.m_AnimationClips?.length ?? 0}, TOS=${entries.length}`);
  for (const e of (Array.isArray(tos) ? tos.slice(0, 5) : entries.slice(0, 5))) {
    console.log('   tos:', JSON.stringify(e));
  }
}

// --- external mesh references (builtin primitives) ---
const sf = db.files[0];
console.log('externals:', sf.externals);
let extMesh = 0;
const extIDs = new Map<string, number>();
for (const [, [, info]] of db.objects) {
  if (info.classID !== ClassID.MeshFilter) continue;
  const mf = db.read(info.pathID) as any;
  const ptr = mf?.m_Mesh;
  if (ptr && ptr.m_FileID !== 0) {
    extMesh++;
    const key = `${ptr.m_FileID}:${ptr.m_PathID}`;
    extIDs.set(key, (extIDs.get(key) ?? 0) + 1);
  }
}
console.log(`MeshFilters referencing external meshes: ${extMesh}`);
console.log('by id:', [...extIDs.entries()].sort((a, b) => b[1] - a[1]).slice(0, 12));

// also count null-geometry children across prefabs
let totalMeshNodes = 0;
let missingMesh = 0;
for (const c of db.containers) {
  if (!c.path.endsWith('.prefab')) continue;
  const tree = buildGameObjectTree(db, c.pathID);
  if (!tree) continue;
  const stack = [tree];
  while (stack.length) {
    const n = stack.pop()!;
    if (n.meshPathID) {
      totalMeshNodes++;
      if (!db.objects.has(n.meshPathID)) missingMesh++;
    }
    stack.push(...n.children);
  }
}
console.log(`mesh nodes: ${totalMeshNodes}, meshPathID missing from bundle: ${missingMesh}`);
