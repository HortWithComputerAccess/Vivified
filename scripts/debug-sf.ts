import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { parseBundle } from '../src/unity/bundle';
import { parseSerializedFile, readObject } from '../src/unity/serializedFile';

const MAP_DIR = join(
  process.cwd(),
  'Reference-Do-Not-Include',
  'Beatsaber Map That Uses Vivify for reference'
);
const buf = readFileSync(join(MAP_DIR, 'bundleWindows2019.vivify'));
const bundle = parseBundle(buf.buffer.slice(buf.byteOffset, buf.byteOffset + buf.byteLength));
console.log('nodes:', bundle.nodes.map((n) => `${n.path} (${n.data.length})`));

const sfNode = bundle.nodes.find((n) => !n.path.endsWith('.resS'))!;
const sf = parseSerializedFile(sfNode.data);
console.log('version:', sf.version, 'unity:', sf.unityVersion, 'le:', sf.littleEndian);
console.log('dataOffset:', sf.dataOffset, 'types:', sf.types.length, 'objects:', sf.objects.size);

// class distribution
const counts = new Map<number, number>();
for (const [, info] of sf.objects) counts.set(info.classID, (counts.get(info.classID) ?? 0) + 1);
console.log('classes:', [...counts.entries()].sort((a, b) => b[1] - a[1]).slice(0, 20));

// dump typetree of the first few types
for (const t of sf.types.slice(0, 3)) {
  console.log(`--- type class ${t.classID} ---`);
  const dump = (n: any, depth: number) => {
    if (depth > 2) return;
    console.log('  '.repeat(depth) + `${n.type} ${n.name} (size ${n.byteSize}, flags ${n.typeFlags}, meta ${n.metaFlag.toString(16)})`);
    for (const c of n.children) dump(c, depth + 1);
  };
  if (t.tree) dump(t.tree, 0);
}

// try reading the smallest objects first
const sorted = [...sf.objects.values()].sort((a, b) => a.byteSize - b.byteSize);
for (const info of sorted.slice(0, 10)) {
  try {
    const v = readObject(sf, info) as any;
    const name = v && typeof v === 'object' && 'm_Name' in v ? v.m_Name : '';
    console.log(`obj ${info.pathID} class ${info.classID} size ${info.byteSize}: ok ${name}`);
  } catch (e) {
    console.log(`obj ${info.pathID} class ${info.classID} size ${info.byteSize}: FAIL ${(e as Error).message}`);
  }
}
