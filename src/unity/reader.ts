/** Endian-aware binary reader over an ArrayBuffer. */
export class BinaryReader {
  view: DataView;
  pos = 0;
  littleEndian: boolean;
  private bytes: Uint8Array;

  constructor(data: ArrayBuffer | Uint8Array, littleEndian = true) {
    if (data instanceof Uint8Array) {
      this.view = new DataView(data.buffer, data.byteOffset, data.byteLength);
      this.bytes = data;
    } else {
      this.view = new DataView(data);
      this.bytes = new Uint8Array(data);
    }
    this.littleEndian = littleEndian;
  }

  get length(): number {
    return this.view.byteLength;
  }

  seek(pos: number): void {
    this.pos = pos;
  }

  skip(n: number): void {
    this.pos += n;
  }

  align(n: number): void {
    const rem = this.pos % n;
    if (rem !== 0) this.pos += n - rem;
  }

  u8(): number {
    return this.view.getUint8(this.pos++);
  }
  i8(): number {
    return this.view.getInt8(this.pos++);
  }
  u16(): number {
    const v = this.view.getUint16(this.pos, this.littleEndian);
    this.pos += 2;
    return v;
  }
  i16(): number {
    const v = this.view.getInt16(this.pos, this.littleEndian);
    this.pos += 2;
    return v;
  }
  u32(): number {
    const v = this.view.getUint32(this.pos, this.littleEndian);
    this.pos += 4;
    return v;
  }
  i32(): number {
    const v = this.view.getInt32(this.pos, this.littleEndian);
    this.pos += 4;
    return v;
  }
  /** 64-bit ints returned as JS number (safe for file offsets/sizes). */
  i64(): number {
    const v = this.view.getBigInt64(this.pos, this.littleEndian);
    this.pos += 8;
    return Number(v);
  }
  u64(): number {
    const v = this.view.getBigUint64(this.pos, this.littleEndian);
    this.pos += 8;
    return Number(v);
  }
  f32(): number {
    const v = this.view.getFloat32(this.pos, this.littleEndian);
    this.pos += 4;
    return v;
  }
  f64(): number {
    const v = this.view.getFloat64(this.pos, this.littleEndian);
    this.pos += 8;
    return v;
  }
  bool(): boolean {
    return this.u8() !== 0;
  }

  readBytes(n: number): Uint8Array {
    const v = this.bytes.subarray(this.pos, this.pos + n);
    this.pos += n;
    return v;
  }

  /** Null-terminated UTF-8 string. */
  cstring(): string {
    const start = this.pos;
    while (this.pos < this.length && this.bytes[this.pos] !== 0) this.pos++;
    const s = utf8Decode(this.bytes.subarray(start, this.pos));
    this.pos++; // skip null
    return s;
  }

  /** u32 length-prefixed UTF-8 string, aligned to 4 afterwards. */
  alignedString(): string {
    const len = this.i32();
    const s = utf8Decode(this.readBytes(len));
    this.align(4);
    return s;
  }
}

const decoder = new TextDecoder('utf-8');
export function utf8Decode(bytes: Uint8Array): string {
  return decoder.decode(bytes);
}
