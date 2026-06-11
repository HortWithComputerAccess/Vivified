/**
 * Verify muscle-clip decoding + CRC32 path binding against the reference
 * bundle. Run: npx tsx scripts/test-clips.ts
 */
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { AssetDB, ClassID, buildGameObjectTree, UGameObject } from '../src/unity/assets';
import { parseAnimationClip, clipForController, crc32 } from '../src/unity/animationClip';

const MAP_DIR = join(process.cwd(), 'Reference-Do-Not-Include', 'Beatsaber Map That Uses Vivify for reference');
const buf = readFileSync(join(MAP_DIR, 'bundleWindows2019.vivify'));
const db = AssetDB.fromBundle(buf.buffer.slice(buf.byteOffset, buf.byteOffset + buf.byteLength));

let failures = 0;
function check(cond: boolean, msg: string): void {
  if (cond) console.log(`  ok: ${msg}`);
  else {
    failures++;
    console.error(`  FAIL: ${msg}`);
  }
}

// crc32 sanity (standard IEEE test vector)
check(crc32('123456789') === 0xcbf43926, `crc32 test vector (got ${crc32('123456789').toString(16)})`);

// decode all clips
let decoded = 0;
for (const [pathID, [, info]] of db.objects) {
  if (info.classID !== ClassID.AnimationClip) continue;
  const clip = parseAnimationClip(db, pathID);
  if (!clip) {
    console.error(`  clip ${pathID} failed to decode`);
    continue;
  }
  decoded++;
  const withKeys = clip.bindings.filter(
    (b) => (b.position?.times.length ?? 0) + (b.euler?.times.length ?? 0) + (b.rotation?.times.length ?? 0) + (b.scale?.times.length ?? 0) > 1
  ).length;
  console.log(`  clip "${clip.name}": ${clip.bindings.length} bindings (${withKeys} animated), length ${clip.length.toFixed(1)}s, loop=${clip.loop}`);
}
check(decoded === 7, `all 7 clips decoded (got ${decoded})`);

// hash resolution: for each prefab with an animator, check binding hashes
// match CRC32 of its transform paths
function collectPaths(node: UGameObject, prefix: string, out: Map<number, string>): void {
  for (const child of node.children) {
    const p = prefix ? `${prefix}/${child.name}` : child.name;
    out.set(crc32(p), p);
    collectPaths(child, p, out);
  }
}

let checkedPrefabs = 0;
for (const c of db.containers) {
  if (!c.path.endsWith('.prefab')) continue;
  const tree = buildGameObjectTree(db, c.pathID);
  if (!tree) continue;
  // find animator nodes
  const stack: UGameObject[] = [tree];
  while (stack.length) {
    const n = stack.pop()!;
    if (n.animatorControllerPathID) {
      const clip = clipForController(db, n.animatorControllerPathID);
      if (clip) {
        const paths = new Map<number, string>();
        paths.set(0, '');
        collectPaths(n, '', paths);
        const resolved = clip.bindings.filter((b) => b.pathHash !== null && paths.has(b.pathHash)).length;
        const pct = Math.round((resolved / clip.bindings.length) * 100);
        console.log(`  ${c.path.split('/').pop()} -> clip "${clip.name}": ${resolved}/${clip.bindings.length} bindings resolved (${pct}%)`);
        // clips may bind transforms that no longer exist in the prefab; Unity
        // ignores those bindings as well
        check(pct >= 50, `${clip.name} binding resolution >= 50% (got ${pct}%)`);
        checkedPrefabs++;
      }
    }
    stack.push(...n.children);
  }
}
check(checkedPrefabs >= 5, `animated prefabs found (got ${checkedPrefabs})`);

// builtin mesh substitution counts
let builtins = 0;
for (const c of db.containers) {
  if (!c.path.endsWith('.prefab')) continue;
  const tree = buildGameObjectTree(db, c.pathID);
  if (!tree) continue;
  const stack: UGameObject[] = [tree];
  while (stack.length) {
    const n = stack.pop()!;
    if (n.meshBuiltin) builtins++;
    stack.push(...n.children);
  }
}
console.log(`  builtin primitive meshes substituted: ${builtins}`);
check(builtins >= 400, `builtin meshes recognized (got ${builtins})`);

console.log(failures ? `\n${failures} FAILURES` : '\nALL OK');
process.exit(failures ? 1 : 0);
