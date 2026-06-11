import { AssetDB } from './assets';

export interface DecodedTexture {
  name: string;
  width: number;
  height: number;
  /** RGBA8, rows bottom-to-top (matches GL/Unity UV origin) */
  rgba: Uint8Array;
}

// Unity TextureFormat values we care about
export const TexFormat = {
  Alpha8: 1,
  ARGB4444: 2,
  RGB24: 3,
  RGBA32: 4,
  ARGB32: 5,
  RGB565: 7,
  R8: 63,
  RGBA4444: 13,
  BGRA32: 14,
  DXT1: 10,
  DXT5: 12,
  BC7: 25,
} as const;

export function decodeTexture(db: AssetDB, texPathID: number): DecodedTexture | null {
  const tex = db.read(texPathID) as any;
  if (!tex) return null;
  const name = String(tex.m_Name ?? 'Texture');
  const width = Number(tex.m_Width ?? 0);
  const height = Number(tex.m_Height ?? 0);
  if (!width || !height) return null;
  const format = Number(tex.m_TextureFormat ?? 0);

  let data: Uint8Array =
    tex['image data'] instanceof Uint8Array ? tex['image data'] : new Uint8Array(0);
  if (data.length === 0 && tex.m_StreamData?.path) {
    const sd = tex.m_StreamData;
    const streamed = db.resourceData(String(sd.path), Number(sd.offset), Number(sd.size));
    if (streamed) data = streamed;
  }
  if (data.length === 0) return null;

  const rgba = new Uint8Array(width * height * 4);
  try {
    switch (format) {
      case TexFormat.RGBA32:
        rgba.set(data.subarray(0, width * height * 4));
        break;
      case TexFormat.ARGB32:
        for (let i = 0; i < width * height; i++) {
          rgba[i * 4] = data[i * 4 + 1];
          rgba[i * 4 + 1] = data[i * 4 + 2];
          rgba[i * 4 + 2] = data[i * 4 + 3];
          rgba[i * 4 + 3] = data[i * 4];
        }
        break;
      case TexFormat.BGRA32:
        for (let i = 0; i < width * height; i++) {
          rgba[i * 4] = data[i * 4 + 2];
          rgba[i * 4 + 1] = data[i * 4 + 1];
          rgba[i * 4 + 2] = data[i * 4];
          rgba[i * 4 + 3] = data[i * 4 + 3];
        }
        break;
      case TexFormat.RGB24:
        for (let i = 0; i < width * height; i++) {
          rgba[i * 4] = data[i * 3];
          rgba[i * 4 + 1] = data[i * 3 + 1];
          rgba[i * 4 + 2] = data[i * 3 + 2];
          rgba[i * 4 + 3] = 255;
        }
        break;
      case TexFormat.Alpha8:
        for (let i = 0; i < width * height; i++) {
          rgba[i * 4] = 255;
          rgba[i * 4 + 1] = 255;
          rgba[i * 4 + 2] = 255;
          rgba[i * 4 + 3] = data[i];
        }
        break;
      case TexFormat.R8:
        for (let i = 0; i < width * height; i++) {
          rgba[i * 4] = data[i];
          rgba[i * 4 + 1] = data[i];
          rgba[i * 4 + 2] = data[i];
          rgba[i * 4 + 3] = 255;
        }
        break;
      case TexFormat.RGB565: {
        const view = new DataView(data.buffer, data.byteOffset, data.byteLength);
        for (let i = 0; i < width * height; i++) {
          const v = view.getUint16(i * 2, true);
          rgba[i * 4] = ((v >> 11) & 0x1f) * 255 / 31;
          rgba[i * 4 + 1] = ((v >> 5) & 0x3f) * 255 / 63;
          rgba[i * 4 + 2] = (v & 0x1f) * 255 / 31;
          rgba[i * 4 + 3] = 255;
        }
        break;
      }
      case TexFormat.DXT1:
        decodeDXT(data, width, height, rgba, false);
        break;
      case TexFormat.DXT5:
        decodeDXT(data, width, height, rgba, true);
        break;
      case TexFormat.BC7:
        decodeBC7(data, width, height, rgba);
        break;
      default:
        console.warn(`Texture "${name}": unsupported format ${format}, using checker placeholder`);
        checker(rgba, width, height);
        break;
    }
  } catch (e) {
    console.warn(`Texture "${name}" decode failed`, e);
    checker(rgba, width, height);
  }

  return { name, width, height, rgba };
}

function checker(rgba: Uint8Array, w: number, h: number): void {
  for (let y = 0; y < h; y++) {
    for (let x = 0; x < w; x++) {
      const on = ((x >> 3) + (y >> 3)) & 1;
      const i = (y * w + x) * 4;
      rgba[i] = on ? 255 : 80;
      rgba[i + 1] = on ? 0 : 80;
      rgba[i + 2] = on ? 255 : 80;
      rgba[i + 3] = 255;
    }
  }
}

// ---------------------------------------------------------------------------
// DXT1 / DXT5 (BC1 / BC3)
// ---------------------------------------------------------------------------

function decodeDXT(data: Uint8Array, width: number, height: number, out: Uint8Array, dxt5: boolean): void {
  const bw = Math.max(1, (width + 3) >> 2);
  const bh = Math.max(1, (height + 3) >> 2);
  const blockSize = dxt5 ? 16 : 8;
  const view = new DataView(data.buffer, data.byteOffset, data.byteLength);
  const colors = new Uint8Array(16);
  const alphas = new Uint8Array(8);

  for (let by = 0; by < bh; by++) {
    for (let bx = 0; bx < bw; bx++) {
      let off = (by * bw + bx) * blockSize;
      let alphaBits0 = 0, alphaBits1 = 0;
      if (dxt5) {
        const a0 = data[off];
        const a1 = data[off + 1];
        alphas[0] = a0;
        alphas[1] = a1;
        if (a0 > a1) {
          for (let i = 1; i < 7; i++) alphas[i + 1] = ((7 - i) * a0 + i * a1) / 7;
        } else {
          for (let i = 1; i < 5; i++) alphas[i + 1] = ((5 - i) * a0 + i * a1) / 5;
          alphas[6] = 0;
          alphas[7] = 255;
        }
        alphaBits0 = data[off + 2] | (data[off + 3] << 8) | (data[off + 4] << 16);
        alphaBits1 = data[off + 5] | (data[off + 6] << 8) | (data[off + 7] << 16);
        off += 8;
      }
      const c0 = view.getUint16(off, true);
      const c1 = view.getUint16(off + 2, true);
      const lookup = view.getUint32(off + 4, true);
      rgb565(c0, colors, 0);
      rgb565(c1, colors, 4);
      colors[3] = 255;
      colors[7] = 255;
      if (c0 > c1 || dxt5) {
        for (let i = 0; i < 3; i++) {
          colors[8 + i] = (2 * colors[i] + colors[4 + i]) / 3;
          colors[12 + i] = (colors[i] + 2 * colors[4 + i]) / 3;
        }
        colors[11] = 255;
        colors[15] = 255;
      } else {
        for (let i = 0; i < 3; i++) {
          colors[8 + i] = (colors[i] + colors[4 + i]) / 2;
          colors[12 + i] = 0;
        }
        colors[11] = 255;
        colors[15] = 0;
      }

      for (let py = 0; py < 4; py++) {
        const y = by * 4 + py;
        if (y >= height) break;
        for (let px = 0; px < 4; px++) {
          const x = bx * 4 + px;
          if (x >= width) continue;
          const li = py * 4 + px;
          const ci = (lookup >> (li * 2)) & 3;
          const o = (y * width + x) * 4;
          out[o] = colors[ci * 4];
          out[o + 1] = colors[ci * 4 + 1];
          out[o + 2] = colors[ci * 4 + 2];
          if (dxt5) {
            const ai = li < 8
              ? (alphaBits0 >> (li * 3)) & 7
              : (alphaBits1 >> ((li - 8) * 3)) & 7;
            out[o + 3] = alphas[ai];
          } else {
            out[o + 3] = colors[ci * 4 + 3];
          }
        }
      }
    }
  }
}

function rgb565(v: number, out: Uint8Array, at: number): void {
  const r = (v >> 11) & 0x1f;
  const g = (v >> 5) & 0x3f;
  const b = v & 0x1f;
  out[at] = (r << 3) | (r >> 2);
  out[at + 1] = (g << 2) | (g >> 4);
  out[at + 2] = (b << 3) | (b >> 2);
}

// ---------------------------------------------------------------------------
// BC7 (sufficient subset: modes 4, 5, 6 cover almost all real content;
// modes 0-3, 7 are implemented via the generic path too)
// ---------------------------------------------------------------------------

const BC7_PARTITIONS_2 = [
  [0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1],[0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1],[0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1],[0,0,0,1,0,0,1,1,0,0,1,1,0,1,1,1],
  [0,0,0,0,0,0,0,1,0,0,0,1,0,0,1,1],[0,0,1,1,0,1,1,1,0,1,1,1,1,1,1,1],[0,0,0,1,0,0,1,1,0,1,1,1,1,1,1,1],[0,0,0,0,0,0,0,1,0,0,1,1,0,1,1,1],
  [0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,1],[0,0,1,1,0,1,1,1,1,1,1,1,1,1,1,1],[0,0,0,0,0,0,0,1,0,1,1,1,1,1,1,1],[0,0,0,0,0,0,0,0,0,0,0,1,0,1,1,1],
  [0,0,0,1,0,1,1,1,1,1,1,1,1,1,1,1],[0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1],[0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1],[0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1],
  [0,0,0,0,1,0,0,0,1,1,1,0,1,1,1,1],[0,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,1,0,0,0,1,1,1,0],[0,1,1,1,0,0,1,1,0,0,0,1,0,0,0,0],
  [0,0,1,1,0,0,0,1,0,0,0,0,0,0,0,0],[0,0,0,0,1,0,0,0,1,1,0,0,1,1,1,0],[0,0,0,0,0,0,0,0,1,0,0,0,1,1,0,0],[0,1,1,1,0,0,1,1,0,0,1,1,0,0,0,1],
  [0,0,1,1,0,0,0,1,0,0,0,1,0,0,0,0],[0,0,0,0,1,0,0,0,1,0,0,0,1,1,0,0],[0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0],[0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0],
  [0,0,0,1,0,1,1,1,1,1,1,0,1,0,0,0],[0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0],[0,1,1,1,0,0,0,1,1,0,0,0,1,1,1,0],[0,0,1,1,1,0,0,1,1,0,0,1,1,1,0,0],
  [0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1],[0,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1],[0,1,0,1,1,0,1,0,0,1,0,1,1,0,1,0],[0,0,1,1,0,0,1,1,1,1,0,0,1,1,0,0],
  [0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0],[0,1,0,1,0,1,0,1,1,0,1,0,1,0,1,0],[0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1],[0,1,0,1,1,0,1,0,1,0,1,0,0,1,0,1],
  [0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,0],[0,0,0,1,0,0,1,1,1,1,0,0,1,0,0,0],[0,0,1,1,0,0,1,0,0,1,0,0,1,1,0,0],[0,0,1,1,1,0,1,1,1,1,0,1,1,1,0,0],
  [0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0],[0,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1],[0,1,1,0,0,1,1,0,1,0,0,1,1,0,0,1],[0,0,0,0,0,1,1,0,0,1,1,0,0,0,0,0],
  [0,1,0,0,1,1,1,0,0,1,0,0,0,0,0,0],[0,0,1,0,0,1,1,1,0,0,1,0,0,0,0,0],[0,0,0,0,0,0,1,0,0,1,1,1,0,0,1,0],[0,0,0,0,0,1,0,0,1,1,1,0,0,1,0,0],
  [0,1,1,0,1,1,0,0,1,0,0,1,0,0,1,1],[0,0,1,1,0,1,1,0,1,1,0,0,1,0,0,1],[0,1,1,0,0,0,1,1,1,0,0,1,1,1,0,0],[0,0,1,1,1,0,0,1,1,1,0,0,0,1,1,0],
  [0,1,1,0,1,1,0,0,1,1,0,0,1,0,0,1],[0,1,1,0,0,0,1,1,0,0,1,1,1,0,0,1],[0,1,1,1,1,1,1,0,1,0,0,0,0,0,0,1],[0,0,0,1,1,0,0,0,1,1,1,0,0,1,1,1],
  [0,0,0,0,1,1,1,1,0,0,1,1,0,0,1,1],[0,0,1,1,0,0,1,1,1,1,1,1,0,0,0,0],[0,0,1,0,0,0,1,0,1,1,1,0,1,1,1,0],[0,1,0,0,0,1,0,0,0,1,1,1,0,1,1,1],
];

const BC7_PARTITIONS_3 = [
  [0,0,1,1,0,0,1,1,0,2,2,1,2,2,2,2],[0,0,0,1,0,0,1,1,2,2,1,1,2,2,2,1],[0,0,0,0,2,0,0,1,2,2,1,1,2,2,1,1],[0,2,2,2,0,0,2,2,0,0,1,1,0,1,1,1],
  [0,0,0,0,0,0,0,0,1,1,2,2,1,1,2,2],[0,0,1,1,0,0,1,1,0,0,2,2,0,0,2,2],[0,0,2,2,0,0,2,2,1,1,1,1,1,1,1,1],[0,0,1,1,0,0,1,1,2,2,1,1,2,2,1,1],
  [0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2],[0,0,0,0,1,1,1,1,1,1,1,1,2,2,2,2],[0,0,0,0,1,1,1,1,2,2,2,2,2,2,2,2],[0,0,1,2,0,0,1,2,0,0,1,2,0,0,1,2],
  [0,1,1,2,0,1,1,2,0,1,1,2,0,1,1,2],[0,1,2,2,0,1,2,2,0,1,2,2,0,1,2,2],[0,0,1,1,0,1,1,2,1,1,2,2,1,2,2,2],[0,0,1,1,2,0,0,1,2,2,0,0,2,2,2,0],
  [0,0,0,1,0,0,1,1,0,1,1,2,1,1,2,2],[0,1,1,1,0,0,1,1,2,0,0,1,2,2,0,0],[0,0,0,0,1,1,2,2,1,1,2,2,1,1,2,2],[0,0,2,2,0,0,2,2,0,0,2,2,1,1,1,1],
  [0,1,1,1,0,1,1,1,0,2,2,2,0,2,2,2],[0,0,0,1,0,0,0,1,2,2,2,1,2,2,2,1],[0,0,0,0,0,0,1,1,0,1,2,2,0,1,2,2],[0,0,0,0,1,1,0,0,2,2,1,0,2,2,1,0],
  [0,1,2,2,0,1,2,2,0,0,1,1,0,0,0,0],[0,0,1,2,0,0,1,2,1,1,2,2,2,2,2,2],[0,1,1,0,1,2,2,1,1,2,2,1,0,1,1,0],[0,0,0,0,0,1,1,0,1,2,2,1,1,2,2,1],
  [0,0,2,2,1,1,0,2,1,1,0,2,0,0,2,2],[0,1,1,0,0,1,1,0,2,0,0,2,2,2,2,2],[0,0,1,1,0,1,2,2,0,1,2,2,0,0,1,1],[0,0,0,0,2,0,0,0,2,2,1,1,2,2,2,1],
  [0,0,0,0,0,0,0,2,1,1,2,2,1,2,2,2],[0,2,2,2,0,0,2,2,0,0,1,2,0,0,1,1],[0,0,1,1,0,0,1,2,0,0,2,2,0,2,2,2],[0,1,2,0,0,1,2,0,0,1,2,0,0,1,2,0],
  [0,0,0,0,1,1,1,1,2,2,2,2,0,0,0,0],[0,1,2,0,1,2,0,1,2,0,1,2,0,1,2,0],[0,1,2,0,2,0,1,2,1,2,0,1,0,1,2,0],[0,0,1,1,2,2,0,0,1,1,2,2,0,0,1,1],
  [0,0,1,1,1,1,2,2,2,2,0,0,0,0,1,1],[0,1,0,1,0,1,0,1,2,2,2,2,2,2,2,2],[0,0,0,0,0,0,0,0,2,1,2,1,2,1,2,1],[0,0,2,2,1,1,2,2,0,0,2,2,1,1,2,2],
  [0,0,2,2,0,0,1,1,0,0,2,2,0,0,1,1],[0,2,2,0,1,2,2,1,0,2,2,0,1,2,2,1],[0,1,0,1,2,2,2,2,2,2,2,2,0,1,0,1],[0,0,0,0,2,1,2,1,2,1,2,1,2,1,2,1],
  [0,1,0,1,0,1,0,1,0,1,0,1,2,2,2,2],[0,2,2,2,0,1,1,1,0,2,2,2,0,1,1,1],[0,0,0,2,1,1,1,2,0,0,0,2,1,1,1,2],[0,0,0,0,2,1,1,2,2,1,1,2,2,1,1,2],
  [0,2,2,2,0,1,1,1,0,1,1,1,0,2,2,2],[0,0,0,2,1,1,1,2,1,1,1,2,0,0,0,2],[0,1,1,0,0,1,1,0,0,1,1,0,2,2,2,2],[0,0,0,0,0,0,0,0,2,1,1,2,2,1,1,2],
  [0,1,1,0,0,1,1,0,2,2,2,2,2,2,2,2],[0,0,2,2,0,0,1,1,0,0,1,1,0,0,2,2],[0,0,2,2,1,1,2,2,1,1,2,2,0,0,2,2],[0,0,0,0,0,0,0,0,0,0,0,0,2,1,1,2],
  [0,0,0,2,0,0,0,1,0,0,0,2,0,0,0,1],[0,2,2,2,1,2,2,2,0,2,2,2,1,2,2,2],[0,1,0,1,2,2,2,2,2,2,2,2,2,2,2,2],[0,1,1,1,2,0,1,1,2,2,0,1,2,2,2,0],
];

const BC7_ANCHOR_2 = [
  15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
  15, 2, 8, 2, 2, 8, 8,15, 2, 8, 2, 2, 8, 8, 2, 2,
  15,15, 6, 8, 2, 8,15,15, 2, 8, 2, 2, 2,15,15, 6,
   6, 2, 6, 8,15,15, 2, 2,15,15,15,15,15, 2, 2,15,
];
const BC7_ANCHOR_3A = [
   3, 3,15,15, 8, 3,15,15, 8, 8, 6, 6, 6, 5, 3, 3,
   3, 3, 8,15, 3, 3, 6,10, 5, 8, 8, 6, 8, 5,15,15,
   8,15, 3, 5, 6,10, 8,15,15, 3,15, 5,15,15,15,15,
   3,15, 5, 5, 5, 8, 5,10, 5,10, 8,13,15,12, 3, 3,
];
const BC7_ANCHOR_3B = [
  15, 8, 8, 3,15,15, 3, 8,15,15,15,15,15,15,15, 8,
  15, 8,15, 3,15, 8,15, 8, 3,15, 6,10,15,15,10, 8,
  15, 3,15,10,10, 8, 9,10, 6,15, 8,15, 3, 6, 6, 8,
  15, 3,15,15,15,15,15,15,15,15,15,15, 3,15,15, 8,
];

const WEIGHTS_2 = [0, 21, 43, 64];
const WEIGHTS_3 = [0, 9, 18, 27, 37, 46, 55, 64];
const WEIGHTS_4 = [0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64];

interface BC7Mode {
  ns: number; pb: number; rb: number; isb: number;
  cb: number; ab: number; epb: number; spb: number;
  ib: number; ib2: number;
}
const BC7_MODES: BC7Mode[] = [
  { ns: 3, pb: 4, rb: 0, isb: 0, cb: 4, ab: 0, epb: 1, spb: 0, ib: 3, ib2: 0 },
  { ns: 2, pb: 6, rb: 0, isb: 0, cb: 6, ab: 0, epb: 0, spb: 1, ib: 3, ib2: 0 },
  { ns: 3, pb: 6, rb: 0, isb: 0, cb: 5, ab: 0, epb: 0, spb: 0, ib: 2, ib2: 0 },
  { ns: 2, pb: 6, rb: 0, isb: 0, cb: 7, ab: 0, epb: 1, spb: 0, ib: 2, ib2: 0 },
  { ns: 1, pb: 0, rb: 2, isb: 1, cb: 5, ab: 6, epb: 0, spb: 0, ib: 2, ib2: 3 },
  { ns: 1, pb: 0, rb: 2, isb: 0, cb: 7, ab: 8, epb: 0, spb: 0, ib: 2, ib2: 2 },
  { ns: 1, pb: 0, rb: 0, isb: 0, cb: 7, ab: 7, epb: 1, spb: 0, ib: 4, ib2: 0 },
  { ns: 2, pb: 6, rb: 0, isb: 0, cb: 5, ab: 5, epb: 1, spb: 0, ib: 2, ib2: 0 },
];

class BitReader {
  private data: Uint8Array;
  private bit = 0;
  constructor(data: Uint8Array) { this.data = data; }
  read(n: number): number {
    let v = 0;
    for (let i = 0; i < n; i++) {
      v |= ((this.data[this.bit >> 3] >> (this.bit & 7)) & 1) << i;
      this.bit++;
    }
    return v;
  }
}

function decodeBC7(data: Uint8Array, width: number, height: number, out: Uint8Array): void {
  const bw = Math.max(1, (width + 3) >> 2);
  const bh = Math.max(1, (height + 3) >> 2);
  const px = new Uint8Array(64);
  for (let by = 0; by < bh; by++) {
    for (let bx = 0; bx < bw; bx++) {
      decodeBC7Block(data.subarray((by * bw + bx) * 16, (by * bw + bx) * 16 + 16), px);
      for (let y = 0; y < 4; y++) {
        const oy = by * 4 + y;
        if (oy >= height) break;
        for (let x = 0; x < 4; x++) {
          const ox = bx * 4 + x;
          if (ox >= width) continue;
          const o = (oy * width + ox) * 4;
          const i = (y * 4 + x) * 4;
          out[o] = px[i]; out[o + 1] = px[i + 1]; out[o + 2] = px[i + 2]; out[o + 3] = px[i + 3];
        }
      }
    }
  }
}

function decodeBC7Block(block: Uint8Array, out: Uint8Array): void {
  const br = new BitReader(block);
  let mode = 0;
  while (mode < 8 && br.read(1) === 0) mode++;
  if (mode === 8) { out.fill(0); return; }
  const m = BC7_MODES[mode];

  const partition = m.pb ? br.read(m.pb) : 0;
  const rotation = m.rb ? br.read(m.rb) : 0;
  const indexSel = m.isb ? br.read(m.isb) : 0;

  const numEP = m.ns * 2;
  // endpoints: colors[ep][channel]
  const ep: number[][] = Array.from({ length: numEP }, () => [0, 0, 0, 255]);
  for (let ch = 0; ch < 3; ch++) {
    for (let e = 0; e < numEP; e++) ep[e][ch] = br.read(m.cb);
  }
  if (m.ab) {
    for (let e = 0; e < numEP; e++) ep[e][3] = br.read(m.ab);
  }
  // p-bits
  let cb = m.cb, ab = m.ab;
  if (m.epb) {
    for (let e = 0; e < numEP; e++) {
      const p = br.read(1);
      for (let ch = 0; ch < 3; ch++) ep[e][ch] = (ep[e][ch] << 1) | p;
      if (m.ab) ep[e][3] = (ep[e][3] << 1) | p;
    }
    cb++; if (m.ab) ab++;
  } else if (m.spb) {
    for (let s = 0; s < m.ns; s++) {
      const p = br.read(1);
      for (let e = s * 2; e < s * 2 + 2; e++) {
        for (let ch = 0; ch < 3; ch++) ep[e][ch] = (ep[e][ch] << 1) | p;
        if (m.ab) ep[e][3] = (ep[e][3] << 1) | p;
      }
    }
    cb++; if (m.ab) ab++;
  }
  // expand to 8 bits
  for (const e of ep) {
    for (let ch = 0; ch < 3; ch++) e[ch] = expandTo8(e[ch], cb);
    if (m.ab) e[3] = expandTo8(e[3], ab);
    else e[3] = 255;
  }

  // subset assignment per pixel
  const subsetOf = (i: number): number => {
    if (m.ns === 1) return 0;
    if (m.ns === 2) return BC7_PARTITIONS_2[partition][i];
    return BC7_PARTITIONS_3[partition][i];
  };
  const isAnchor = (i: number, subset: number): boolean => {
    if (subset === 0) return i === 0;
    if (m.ns === 2) return i === BC7_ANCHOR_2[partition];
    return subset === 1 ? i === BC7_ANCHOR_3A[partition] : i === BC7_ANCHOR_3B[partition];
  };

  // index data
  const idx1 = new Array(16).fill(0);
  const idx2 = new Array(16).fill(0);
  for (let i = 0; i < 16; i++) {
    const s = subsetOf(i);
    idx1[i] = br.read(isAnchor(i, s) ? m.ib - 1 : m.ib);
  }
  if (m.ib2) {
    for (let i = 0; i < 16; i++) {
      idx2[i] = br.read(i === 0 ? m.ib2 - 1 : m.ib2);
    }
  }

  const w1 = m.ib === 2 ? WEIGHTS_2 : m.ib === 3 ? WEIGHTS_3 : WEIGHTS_4;
  const w2 = m.ib2 === 2 ? WEIGHTS_2 : WEIGHTS_3;

  for (let i = 0; i < 16; i++) {
    const s = subsetOf(i);
    const e0 = ep[s * 2], e1 = ep[s * 2 + 1];
    let cw = w1[idx1[i]];
    let aw = m.ib2 ? w2[idx2[i]] : cw;
    if (m.ib2 && indexSel) {
      const t = cw; cw = aw; aw = t;
    }
    let r = interp(e0[0], e1[0], cw);
    let g = interp(e0[1], e1[1], cw);
    let b = interp(e0[2], e1[2], cw);
    let a = interp(e0[3], e1[3], aw);
    // rotation swaps alpha with a color channel
    if (rotation === 1) { const t = r; r = a; a = t; }
    else if (rotation === 2) { const t = g; g = a; a = t; }
    else if (rotation === 3) { const t = b; b = a; a = t; }
    out[i * 4] = r; out[i * 4 + 1] = g; out[i * 4 + 2] = b; out[i * 4 + 3] = a;
  }
}

function expandTo8(v: number, bits: number): number {
  v = v << (8 - bits);
  return v | (v >> bits);
}
function interp(a: number, b: number, w: number): number {
  return (a * (64 - w) + b * w + 32) >> 6;
}
