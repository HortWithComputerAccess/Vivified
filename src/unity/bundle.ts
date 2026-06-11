import { BinaryReader } from './reader';
import { lz4Decompress } from './lz4';
import { lzmaDecompress } from './lzma';

export interface BundleNode {
  path: string;
  data: Uint8Array;
}

export interface ParsedBundle {
  signature: string;
  unityRevision: string;
  nodes: BundleNode[];
}

const COMP_NONE = 0;
const COMP_LZMA = 1;
const COMP_LZ4 = 2;
const COMP_LZ4HC = 3;

/** Parse a UnityFS AssetBundle (.vivify is a plain AssetBundle). */
export function parseBundle(buffer: ArrayBuffer): ParsedBundle {
  const r = new BinaryReader(buffer, false); // bundle header is big-endian
  const signature = r.cstring();
  if (signature !== 'UnityFS') {
    throw new Error(`Unsupported bundle signature "${signature}" (only UnityFS is supported)`);
  }
  const version = r.u32();
  r.cstring(); // unityVersion ("5.x.x")
  const unityRevision = r.cstring();
  r.i64(); // total size
  const compressedBlocksInfoSize = r.u32();
  const uncompressedBlocksInfoSize = r.u32();
  const flags = r.u32();
  if (version >= 7) r.align(16);

  // blocks info
  let blocksInfoBytes: Uint8Array;
  if (flags & 0x80) {
    // at end of file
    const start = buffer.byteLength - compressedBlocksInfoSize;
    blocksInfoBytes = new Uint8Array(buffer, start, compressedBlocksInfoSize);
  } else {
    blocksInfoBytes = r.readBytes(compressedBlocksInfoSize);
  }
  blocksInfoBytes = decompress(blocksInfoBytes, flags & 0x3f, uncompressedBlocksInfoSize);

  const bi = new BinaryReader(blocksInfoBytes, false);
  bi.readBytes(16); // uncompressedDataHash
  const blockCount = bi.i32();
  const blocks: { uSize: number; cSize: number; flags: number }[] = [];
  let totalUncompressed = 0;
  for (let i = 0; i < blockCount; i++) {
    const uSize = bi.u32();
    const cSize = bi.u32();
    const bFlags = bi.u16();
    blocks.push({ uSize, cSize, flags: bFlags });
    totalUncompressed += uSize;
  }
  const nodeCount = bi.i32();
  const nodeInfos: { offset: number; size: number; path: string }[] = [];
  for (let i = 0; i < nodeCount; i++) {
    const offset = bi.i64();
    const size = bi.i64();
    bi.u32(); // node flags
    const path = bi.cstring();
    nodeInfos.push({ offset, size, path });
  }

  // decompress data blocks into one contiguous buffer
  const data = new Uint8Array(totalUncompressed);
  let writePos = 0;
  for (const block of blocks) {
    const compressed = r.readBytes(block.cSize);
    const out = decompress(compressed, block.flags & 0x3f, block.uSize);
    data.set(out, writePos);
    writePos += block.uSize;
  }

  const nodes: BundleNode[] = nodeInfos.map((n) => ({
    path: n.path,
    data: data.subarray(n.offset, n.offset + n.size),
  }));

  return { signature, unityRevision, nodes };
}

function decompress(src: Uint8Array, compression: number, uncompressedSize: number): Uint8Array {
  switch (compression) {
    case COMP_NONE:
      return src;
    case COMP_LZ4:
    case COMP_LZ4HC:
      return lz4Decompress(src, uncompressedSize);
    case COMP_LZMA:
      return lzmaDecompress(src, uncompressedSize);
    default:
      throw new Error(`Unknown bundle compression type ${compression}`);
  }
}
