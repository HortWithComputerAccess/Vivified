import { AssetDB } from './assets';

export interface ParsedMesh {
  name: string;
  positions: Float32Array;
  normals: Float32Array | null;
  uvs: Float32Array | null;
  colors: Float32Array | null;
  indices: Uint32Array;
  /** [start, count] pairs into the index buffer, triangles topology only */
  subMeshes: { start: number; count: number }[];
}

// Vertex format enum for Unity 2019+
const FMT_SIZES = [4, 2, 1, 1, 2, 2, 1, 1, 2, 2, 4, 4]; // bytes per component

enum VFormat {
  Float = 0,
  Float16 = 1,
  UNorm8 = 2,
  SNorm8 = 3,
  UNorm16 = 4,
  SNorm16 = 5,
  UInt8 = 6,
  SInt8 = 7,
  UInt16 = 8,
  SInt16 = 9,
  UInt32 = 10,
  SInt32 = 11,
}

/** Parse a Unity Mesh object (uncompressed vertex data path). */
export function parseMesh(db: AssetDB, meshPathID: number): ParsedMesh | null {
  const mesh = db.read(meshPathID) as any;
  if (!mesh) return null;
  const name = String(mesh.m_Name ?? 'Mesh');

  if (mesh.m_MeshCompression && Number(mesh.m_MeshCompression) !== 0) {
    console.warn(`Mesh "${name}" uses compressed mesh format; skipping geometry`);
    return null;
  }

  const vd = mesh.m_VertexData;
  if (!vd) return null;
  const vertexCount: number = Number(vd.m_VertexCount ?? 0);
  let dataSize: Uint8Array = vd.m_DataSize instanceof Uint8Array ? vd.m_DataSize : new Uint8Array(0);

  // streamed vertex data
  if ((!dataSize || dataSize.length === 0) && mesh.m_StreamData?.path) {
    const sd = mesh.m_StreamData;
    const streamed = db.resourceData(String(sd.path), Number(sd.offset), Number(sd.size));
    if (streamed) dataSize = streamed;
  }
  if (!vertexCount || dataSize.length === 0) return null;

  const channels: any[] = vd.m_Channels ?? [];

  // compute stream strides/offsets (channels reference streams)
  const streamCount = channels.reduce((max, c) => Math.max(max, Number(c.stream ?? 0)), 0) + 1;
  const streamStride = new Array(streamCount).fill(0);
  for (const c of channels) {
    const dim = Number(c.dimension ?? 0) & 0xf;
    if (dim === 0) continue;
    const stream = Number(c.stream ?? 0);
    const fmt = Number(c.format ?? 0);
    const end = Number(c.offset ?? 0) + dim * FMT_SIZES[fmt];
    streamStride[stream] = Math.max(streamStride[stream], end);
  }
  const streamOffset = new Array(streamCount).fill(0);
  {
    let off = 0;
    for (let s = 0; s < streamCount; s++) {
      streamOffset[s] = off;
      off += streamStride[s] * vertexCount;
      off = (off + 15) & ~15; // streams are 16-byte aligned
    }
  }

  const view = new DataView(dataSize.buffer, dataSize.byteOffset, dataSize.byteLength);

  function readChannel(channelIndex: number, outDim: number): Float32Array | null {
    const c = channels[channelIndex];
    if (!c) return null;
    const dim = Number(c.dimension ?? 0) & 0xf;
    if (dim === 0) return null;
    const stream = Number(c.stream ?? 0);
    const fmt = Number(c.format ?? 0) as VFormat;
    const chanOffset = Number(c.offset ?? 0);
    const stride = streamStride[stream];
    const base = streamOffset[stream];
    const out = new Float32Array(vertexCount * outDim);
    const n = Math.min(dim, outDim);
    for (let v = 0; v < vertexCount; v++) {
      let p = base + v * stride + chanOffset;
      for (let d = 0; d < n; d++) {
        out[v * outDim + d] = readComponent(view, p + d * FMT_SIZES[fmt], fmt);
      }
    }
    return out;
  }

  // channel order (2019+): 0 pos, 1 normal, 2 tangent, 3 color, 4 uv0...
  const positions = readChannel(0, 3);
  if (!positions) return null;
  const normals = readChannel(1, 3);
  const colors = readChannel(3, 4);
  const uvs = readChannel(4, 2);

  // index buffer
  let indexBytes: Uint8Array =
    mesh.m_IndexBuffer instanceof Uint8Array ? mesh.m_IndexBuffer : new Uint8Array(0);
  if (indexBytes.length === 0 && Array.isArray(mesh.m_IndexBuffer)) {
    indexBytes = Uint8Array.from(mesh.m_IndexBuffer as number[]);
  }
  const use32 = Number(mesh.m_IndexFormat ?? 0) === 1;
  const iview = new DataView(indexBytes.buffer, indexBytes.byteOffset, indexBytes.byteLength);

  const subMeshesRaw: any[] = mesh.m_SubMeshes ?? [];
  const allIndices: number[] = [];
  const subMeshes: { start: number; count: number }[] = [];
  for (const sm of subMeshesRaw) {
    const topology = Number(sm.topology ?? sm.isTriStrip ?? 0);
    if (topology !== 0) continue; // triangles only
    const firstByte = Number(sm.firstByte ?? 0);
    const indexCount = Number(sm.indexCount ?? 0);
    const start = allIndices.length;
    for (let i = 0; i < indexCount; i++) {
      const idx = use32
        ? iview.getUint32(firstByte + i * 4, true)
        : iview.getUint16(firstByte + i * 2, true);
      allIndices.push(idx);
    }
    subMeshes.push({ start, count: indexCount });
  }

  return {
    name,
    positions,
    normals,
    uvs,
    colors,
    indices: Uint32Array.from(allIndices),
    subMeshes,
  };
}

function readComponent(view: DataView, offset: number, fmt: VFormat): number {
  switch (fmt) {
    case VFormat.Float:
      return view.getFloat32(offset, true);
    case VFormat.Float16:
      return halfToFloat(view.getUint16(offset, true));
    case VFormat.UNorm8:
      return view.getUint8(offset) / 255;
    case VFormat.SNorm8:
      return Math.max(view.getInt8(offset) / 127, -1);
    case VFormat.UNorm16:
      return view.getUint16(offset, true) / 65535;
    case VFormat.SNorm16:
      return Math.max(view.getInt16(offset, true) / 32767, -1);
    case VFormat.UInt8:
      return view.getUint8(offset);
    case VFormat.SInt8:
      return view.getInt8(offset);
    case VFormat.UInt16:
      return view.getUint16(offset, true);
    case VFormat.SInt16:
      return view.getInt16(offset, true);
    case VFormat.UInt32:
      return view.getUint32(offset, true);
    case VFormat.SInt32:
      return view.getInt32(offset, true);
    default:
      return 0;
  }
}

function halfToFloat(h: number): number {
  const sign = (h & 0x8000) ? -1 : 1;
  const exp = (h >> 10) & 0x1f;
  const frac = h & 0x3ff;
  if (exp === 0) return sign * Math.pow(2, -14) * (frac / 1024);
  if (exp === 31) return frac ? NaN : sign * Infinity;
  return sign * Math.pow(2, exp - 15) * (1 + frac / 1024);
}
