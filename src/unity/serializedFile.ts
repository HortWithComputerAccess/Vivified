import { BinaryReader, utf8Decode } from './reader';
import { commonString } from './commonStrings';

export interface TypeTreeNode {
  version: number;
  level: number;
  typeFlags: number;
  type: string;
  name: string;
  byteSize: number;
  index: number;
  metaFlag: number;
  children: TypeTreeNode[];
}

export interface SerializedType {
  classID: number;
  scriptTypeIndex: number;
  tree: TypeTreeNode | null;
}

export interface ObjectInfo {
  pathID: number;
  byteStart: number;
  byteSize: number;
  classID: number;
  serializedType: SerializedType | null;
}

export interface SerializedFile {
  version: number;
  unityVersion: string;
  littleEndian: boolean;
  dataOffset: number;
  types: SerializedType[];
  objects: Map<number, ObjectInfo>;
  externals: string[];
  data: Uint8Array;
}

/** Parse a SerializedFile (CAB-* node inside a bundle). */
export function parseSerializedFile(data: Uint8Array): SerializedFile {
  const r = new BinaryReader(data, false); // header is big-endian
  let metadataSize = r.u32();
  let fileSize: number = r.u32();
  const version = r.u32();
  let dataOffset: number = r.u32();
  if (version < 17 || version > 22) {
    throw new Error(
      `SerializedFile format ${version} not supported (need Unity 2017.x–2021.x era bundles)`
    );
  }
  const bigEndianData = r.u8() !== 0;
  r.readBytes(3); // reserved
  if (version >= 22) {
    metadataSize = r.u32();
    fileSize = r.i64();
    dataOffset = r.i64();
    r.i64(); // unknown
  }

  const littleEndian = !bigEndianData;
  const m = new BinaryReader(data, littleEndian);
  m.seek(r.pos);

  const unityVersion = m.cstring();
  m.i32(); // targetPlatform
  const enableTypeTree = version >= 13 ? m.bool() : true;

  // types
  const typeCount = m.i32();
  const types: SerializedType[] = [];
  for (let i = 0; i < typeCount; i++) {
    types.push(readSerializedType(m, version, enableTypeTree, false));
  }

  // objects
  const objectCount = m.i32();
  const objects = new Map<number, ObjectInfo>();
  for (let i = 0; i < objectCount; i++) {
    m.align(4);
    const pathID = m.i64();
    const byteStart = version >= 22 ? m.i64() : m.u32();
    const byteSize = m.u32();
    const typeID = m.i32();
    const serializedType = types[typeID] ?? null;
    objects.set(pathID, {
      pathID,
      byteStart,
      byteSize,
      classID: serializedType ? serializedType.classID : typeID,
      serializedType,
    });
  }

  // script types
  const scriptCount = m.i32();
  for (let i = 0; i < scriptCount; i++) {
    m.i32(); // localSerializedFileIndex
    m.align(4);
    m.i64(); // localIdentifierInFile
  }

  // externals
  const externalCount = m.i32();
  const externals: string[] = [];
  for (let i = 0; i < externalCount; i++) {
    m.cstring(); // tempEmpty
    m.readBytes(16); // guid
    m.i32(); // type
    externals.push(m.cstring()); // pathName
  }

  // ref types (v >= 20)
  if (version >= 20) {
    const refTypeCount = m.i32();
    for (let i = 0; i < refTypeCount; i++) {
      readSerializedType(m, version, enableTypeTree, true);
    }
  }

  return { version, unityVersion, littleEndian, dataOffset, types, objects, externals, data };
}

function readSerializedType(
  m: BinaryReader,
  version: number,
  enableTypeTree: boolean,
  isRefType: boolean
): SerializedType {
  const classID = m.i32();
  if (version >= 16) m.bool(); // isStripped
  const scriptTypeIndex = version >= 17 ? m.i16() : -1;
  if ((isRefType && scriptTypeIndex >= 0) || classID === 114) {
    m.readBytes(16); // scriptID hash
  }
  m.readBytes(16); // oldTypeHash

  let tree: TypeTreeNode | null = null;
  if (enableTypeTree) {
    tree = readTypeTreeBlob(m, version);
    if (isRefType) {
      m.cstring(); // klassName
      m.cstring(); // nameSpace
      m.cstring(); // asmName
    } else if (version >= 21) {
      const depCount = m.i32();
      m.skip(depCount * 4); // typeDependencies
    }
  }
  return { classID, scriptTypeIndex, tree };
}

function readTypeTreeBlob(m: BinaryReader, version: number): TypeTreeNode {
  const nodeCount = m.i32();
  const stringBufferSize = m.i32();
  const nodeSize = version >= 19 ? 32 : 24;
  const nodesStart = m.pos;
  const stringBuffer = new BinaryReader(
    m.view.buffer.slice(
      m.view.byteOffset + nodesStart + nodeCount * nodeSize,
      m.view.byteOffset + nodesStart + nodeCount * nodeSize + stringBufferSize
    ) as ArrayBuffer,
    true
  );

  const flat: TypeTreeNode[] = [];
  for (let i = 0; i < nodeCount; i++) {
    const ver = m.u16();
    const level = m.u8();
    const typeFlags = m.u8();
    const typeStrOffset = m.u32();
    const nameStrOffset = m.u32();
    const byteSize = m.i32();
    const index = m.i32();
    const metaFlag = m.i32();
    if (version >= 19) m.skip(8); // refTypeHash
    flat.push({
      version: ver,
      level,
      typeFlags,
      type: resolveString(stringBuffer, typeStrOffset),
      name: resolveString(stringBuffer, nameStrOffset),
      byteSize,
      index,
      metaFlag,
      children: [],
    });
  }
  m.skip(stringBufferSize);

  // rebuild hierarchy from levels
  const root = flat[0];
  const stack: TypeTreeNode[] = [root];
  for (let i = 1; i < flat.length; i++) {
    const node = flat[i];
    while (stack.length > node.level) stack.pop();
    stack[stack.length - 1].children.push(node);
    stack.push(node);
  }
  return root;
}

function resolveString(stringBuffer: BinaryReader, offset: number): string {
  if (offset & 0x80000000) {
    return commonString(offset & 0x7fffffff);
  }
  const save = stringBuffer.pos;
  stringBuffer.seek(offset);
  const s = stringBuffer.cstring();
  stringBuffer.seek(save);
  return s;
}

// ---------------------------------------------------------------------------
// Typetree-driven object deserialization
// ---------------------------------------------------------------------------

export type UnityValue =
  | number
  | bigint
  | boolean
  | string
  | Uint8Array
  | UnityValue[]
  | { [key: string]: UnityValue };

const ALIGN_FLAG = 0x4000;

/** Read one object's data by walking its typetree. */
export function readObject(file: SerializedFile, info: ObjectInfo): UnityValue {
  if (!info.serializedType || !info.serializedType.tree) {
    throw new Error(
      `Object ${info.pathID} (class ${info.classID}) has no typetree; ` +
        'bundle was built with typetrees disabled'
    );
  }
  const r = new BinaryReader(file.data, file.littleEndian);
  r.seek(file.dataOffset + info.byteStart);
  return readNode(r, info.serializedType.tree);
}

function readNode(r: BinaryReader, node: TypeTreeNode): UnityValue {
  let value: UnityValue;
  let align = (node.metaFlag & ALIGN_FLAG) !== 0;

  switch (node.type) {
    case 'SInt8':
      value = r.i8();
      break;
    case 'UInt8':
    case 'char':
      value = r.u8();
      break;
    case 'SInt16':
    case 'short':
      value = r.i16();
      break;
    case 'UInt16':
    case 'unsigned short':
      value = r.u16();
      break;
    case 'SInt32':
    case 'int':
      value = r.i32();
      break;
    case 'UInt32':
    case 'unsigned int':
    case 'Type*':
      value = r.u32();
      break;
    case 'SInt64':
    case 'long long':
      value = r.i64();
      break;
    case 'UInt64':
    case 'unsigned long long':
    case 'FileSize':
      value = r.u64();
      break;
    case 'float':
      value = r.f32();
      break;
    case 'double':
      value = r.f64();
      break;
    case 'bool':
      value = r.bool();
      break;
    case 'string': {
      const len = r.i32();
      value = utf8Decode(r.readBytes(len));
      // string data child carries its own align flag
      if (stringDataAligns(node)) align = true;
      break;
    }
    case 'TypelessData': {
      const len = r.i32();
      value = r.readBytes(len).slice();
      break;
    }
    default: {
      if (node.typeFlags & 1 || node.type === 'Array') {
        // Array node: children are [size, data]
        const dataNode = node.children[1];
        const len = r.i32();
        if (isByteArray(dataNode)) {
          value = r.readBytes(len).slice();
        } else {
          const arr: UnityValue[] = [];
          for (let i = 0; i < len; i++) arr.push(readNode(r, dataNode));
          value = arr;
        }
        if (node.metaFlag & ALIGN_FLAG || dataNode.metaFlag & ALIGN_FLAG) align = true;
      } else if (node.children.length === 1 && (node.children[0].typeFlags & 1)) {
        // vector/map/staticvector wrapper containing an Array child
        const arrayNode = node.children[0];
        value = readNode(r, arrayNode);
        if (arrayNode.metaFlag & ALIGN_FLAG) align = true;
        // maps: convert array of pairs to object when keys are strings
        if (node.type === 'map' && Array.isArray(value)) {
          value = pairsToObject(value as UnityValue[]);
        }
      } else {
        // plain struct
        const obj: { [key: string]: UnityValue } = {};
        for (const child of node.children) {
          obj[child.name] = readNode(r, child);
        }
        value = obj;
      }
      break;
    }
  }

  if (align) r.align(4);
  return value;
}

function isByteArray(dataNode: TypeTreeNode): boolean {
  return (
    dataNode.children.length === 0 &&
    (dataNode.type === 'UInt8' || dataNode.type === 'char')
  );
}

function stringDataAligns(node: TypeTreeNode): boolean {
  const arr = node.children[0];
  return !!arr && (arr.metaFlag & ALIGN_FLAG) !== 0;
}

function pairsToObject(pairs: UnityValue[]): UnityValue {
  const allStringKeys = pairs.every(
    (p) => typeof p === 'object' && p !== null && !Array.isArray(p) &&
      typeof (p as any).first === 'string'
  );
  if (!allStringKeys) return pairs;
  const obj: { [key: string]: UnityValue } = {};
  for (const p of pairs as { first: string; second: UnityValue }[]) {
    obj[p.first] = p.second;
  }
  return obj;
}
