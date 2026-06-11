/**
 * Parser smoke test against the reference Vivify bundles.
 * Run: npm run test:bundle
 */
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { AssetDB, buildGameObjectTree, ClassID } from '../src/unity/assets';
import { parseMesh } from '../src/unity/mesh';
import { parseMaterial } from '../src/unity/material';
import { decodeTexture } from '../src/unity/texture';

const MAP_DIR = join(
  process.cwd(),
  'Reference-Do-Not-Include',
  'Beatsaber Map That Uses Vivify for reference'
);

let failures = 0;
function check(cond: boolean, msg: string): void {
  if (cond) {
    console.log(`  ok: ${msg}`);
  } else {
    failures++;
    console.error(`  FAIL: ${msg}`);
  }
}

for (const bundleName of ['bundleWindows2019.vivify', 'bundleWindows2021.vivify']) {
  console.log(`\n=== ${bundleName} ===`);
  const buf = readFileSync(join(MAP_DIR, bundleName));
  const ab = buf.buffer.slice(buf.byteOffset, buf.byteOffset + buf.byteLength);

  const db = AssetDB.fromBundle(ab);
  console.log(`  unity ${db.unityRevision}, ${db.objects.size} objects, ${db.resources.size} resS`);
  check(db.objects.size > 0, 'objects parsed');
  check(db.containers.length > 0, `containers found (${db.containers.length})`);

  const classCounts = new Map<number, number>();
  for (const [, [, info]] of db.objects) {
    classCounts.set(info.classID, (classCounts.get(info.classID) ?? 0) + 1);
  }
  console.log(
    '  classes:',
    [...classCounts.entries()]
      .sort((a, b) => b[1] - a[1])
      .map(([c, n]) => `${c}:${n}`)
      .join(' ')
  );

  // container listing
  const prefabs = db.containers.filter((c) => c.path.endsWith('.prefab'));
  const mats = db.containers.filter((c) => c.path.endsWith('.mat'));
  console.log(`  prefabs: ${prefabs.length}, materials: ${mats.length}`);
  for (const p of prefabs.slice(0, 8)) console.log(`    ${p.path} -> ${p.pathID}`);
  check(prefabs.length > 0, 'prefab containers');

  // build a gameobject tree from the first prefab
  let meshCount = 0;
  let nodeCount = 0;
  for (const p of prefabs) {
    const cls = db.classOf(p.pathID);
    if (cls !== ClassID.GameObject) continue;
    const tree = buildGameObjectTree(db, p.pathID);
    if (!tree) continue;
    const stack = [tree];
    while (stack.length) {
      const n = stack.pop()!;
      nodeCount++;
      if (n.meshPathID) meshCount++;
      stack.push(...n.children);
    }
  }
  console.log(`  gameobject nodes across prefabs: ${nodeCount}, with meshes: ${meshCount}`);
  check(nodeCount > 0, 'gameobject trees built');

  // parse every mesh
  let meshOk = 0;
  let meshFail = 0;
  let vertTotal = 0;
  for (const [pathID, [, info]] of db.objects) {
    if (info.classID !== ClassID.Mesh) continue;
    try {
      const mesh = parseMesh(db, pathID);
      if (mesh && mesh.positions.length > 0 && mesh.indices.length > 0) {
        meshOk++;
        vertTotal += mesh.positions.length / 3;
        const bad = mesh.indices.some((i) => i >= mesh.positions.length / 3);
        if (bad) {
          meshFail++;
          console.error(`    mesh ${mesh.name}: index out of range`);
        }
      } else {
        meshFail++;
      }
    } catch (e) {
      meshFail++;
      console.error(`    mesh ${pathID} threw: ${(e as Error).message}`);
    }
  }
  console.log(`  meshes ok: ${meshOk}, failed/skipped: ${meshFail}, total verts: ${vertTotal}`);
  check(meshOk > 0, 'meshes parsed');

  // parse every material
  let matOk = 0;
  for (const [pathID, [, info]] of db.objects) {
    if (info.classID !== ClassID.Material) continue;
    const m = parseMaterial(db, pathID);
    if (m) matOk++;
  }
  console.log(`  materials parsed: ${matOk}`);
  check(matOk > 0, 'materials parsed');

  // textures: report formats, decode all
  const formats = new Map<number, number>();
  let texOk = 0;
  let texFail = 0;
  for (const [pathID, [, info]] of db.objects) {
    if (info.classID !== ClassID.Texture2D) continue;
    const raw = db.read(pathID) as any;
    const fmt = Number(raw?.m_TextureFormat ?? -1);
    formats.set(fmt, (formats.get(fmt) ?? 0) + 1);
    try {
      const t = decodeTexture(db, pathID);
      if (t) texOk++;
      else texFail++;
    } catch {
      texFail++;
    }
  }
  console.log(
    `  texture formats: ${[...formats.entries()].map(([f, n]) => `${f}x${n}`).join(' ')}`
  );
  console.log(`  textures decoded: ${texOk}, failed: ${texFail}`);
}

console.log(failures ? `\n${failures} FAILURES` : '\nALL OK');
process.exit(failures ? 1 : 0);
