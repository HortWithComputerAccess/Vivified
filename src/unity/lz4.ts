/** LZ4 block decompression (raw block format, as used by UnityFS). */
export function lz4Decompress(src: Uint8Array, uncompressedSize: number): Uint8Array {
  const dst = new Uint8Array(uncompressedSize);
  let sIdx = 0;
  let dIdx = 0;
  const sLen = src.length;

  while (sIdx < sLen) {
    const token = src[sIdx++];

    // literals
    let litLen = token >> 4;
    if (litLen === 15) {
      let b;
      do {
        b = src[sIdx++];
        litLen += b;
      } while (b === 255);
    }
    dst.set(src.subarray(sIdx, sIdx + litLen), dIdx);
    sIdx += litLen;
    dIdx += litLen;
    if (sIdx >= sLen) break; // last block ends with literals

    // match
    const offset = src[sIdx] | (src[sIdx + 1] << 8);
    sIdx += 2;
    if (offset === 0) throw new Error('LZ4: invalid zero offset');
    let matchLen = (token & 0x0f) + 4;
    if ((token & 0x0f) === 15) {
      let b;
      do {
        b = src[sIdx++];
        matchLen += b;
      } while (b === 255);
    }
    let mIdx = dIdx - offset;
    if (mIdx < 0) throw new Error('LZ4: offset out of range');
    // byte-by-byte copy: ranges may overlap
    for (let i = 0; i < matchLen; i++) dst[dIdx++] = dst[mIdx++];
  }
  if (dIdx !== uncompressedSize) {
    throw new Error(`LZ4: expected ${uncompressedSize} bytes, got ${dIdx}`);
  }
  return dst;
}
