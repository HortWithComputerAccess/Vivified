/**
 * LZMA decoder (LZMA1, raw stream with 5-byte properties header), as used by
 * UnityFS LZMA-compressed blocks. Self-contained port of the reference
 * decoder; output size is known in advance so no end-marker is required.
 */

const NUM_STATES = 12;

export function lzmaDecompress(src: Uint8Array, uncompressedSize: number): Uint8Array {
  if (src.length < 5) throw new Error('LZMA: stream too short');
  let d = src[0];
  if (d >= 9 * 5 * 5) throw new Error('LZMA: invalid properties byte');
  const lc = d % 9;
  d = (d / 9) | 0;
  const lp = d % 5;
  const pb = (d / 5) | 0;
  // bytes 1-4: dictionary size (not needed when decoding to a known-size buffer)

  const out = new Uint8Array(uncompressedSize);
  let outPos = 0;

  // --- range decoder ---
  let inPos = 5;
  let code = 0;
  let range = 0xffffffff >>> 0;
  inPos++; // first byte of range-coded data is always 0
  for (let i = 0; i < 4; i++) code = ((code << 8) | src[inPos++]) >>> 0;

  function normalize(): void {
    if (range < 0x1000000) {
      range = (range << 8) >>> 0;
      code = ((code << 8) | (src[inPos++] ?? 0)) >>> 0;
    }
  }

  function decodeBit(probs: Uint16Array, index: number): number {
    const prob = probs[index];
    const bound = ((range >>> 11) * prob) >>> 0;
    let bit: number;
    if ((code >>> 0) < bound) {
      range = bound;
      probs[index] = prob + ((2048 - prob) >> 5);
      bit = 0;
    } else {
      code = (code - bound) >>> 0;
      range = (range - bound) >>> 0;
      probs[index] = prob - (prob >> 5);
      bit = 1;
    }
    normalize();
    return bit;
  }

  function decodeDirectBits(count: number): number {
    let result = 0;
    for (let i = 0; i < count; i++) {
      range = range >>> 1;
      code = (code - range) >>> 0;
      const t = 0 - (code >>> 31);
      code = (code + (range & t)) >>> 0;
      normalize();
      result = ((result << 1) + (t + 1)) >>> 0;
    }
    return result;
  }

  function bitTreeDecode(probs: Uint16Array, offset: number, numBits: number): number {
    let m = 1;
    for (let i = 0; i < numBits; i++) m = (m << 1) + decodeBit(probs, offset + m);
    return m - (1 << numBits);
  }

  function bitTreeReverseDecode(probs: Uint16Array, offset: number, numBits: number): number {
    let m = 1;
    let sym = 0;
    for (let i = 0; i < numBits; i++) {
      const bit = decodeBit(probs, offset + m);
      m = (m << 1) + bit;
      sym |= bit << i;
    }
    return sym;
  }

  // --- probability models ---
  const newProbs = (n: number) => {
    const a = new Uint16Array(n);
    a.fill(1024);
    return a;
  };

  const posMask = (1 << pb) - 1;
  const litPosMask = (1 << lp) - 1;

  const isMatch = newProbs(NUM_STATES << 4);
  const isRep = newProbs(NUM_STATES);
  const isRepG0 = newProbs(NUM_STATES);
  const isRepG1 = newProbs(NUM_STATES);
  const isRepG2 = newProbs(NUM_STATES);
  const isRep0Long = newProbs(NUM_STATES << 4);
  const posSlotDecoder = newProbs(4 * 64); // 4 length classes x 64-entry bit tree
  const specPos = newProbs(115);
  const alignProbs = newProbs(16);
  const literal = newProbs(0x300 << (lc + lp));

  // length coders: [choice, choice2, low[16*8], mid[16*8], high[256]]
  interface LenCoder {
    choice: Uint16Array;
    low: Uint16Array;
    mid: Uint16Array;
    high: Uint16Array;
  }
  const makeLen = (): LenCoder => ({
    choice: newProbs(2),
    low: newProbs(16 * 8),
    mid: newProbs(16 * 8),
    high: newProbs(256),
  });
  const lenCoder = makeLen();
  const repLenCoder = makeLen();

  function decodeLen(c: LenCoder, posState: number): number {
    if (decodeBit(c.choice, 0) === 0) {
      return 2 + bitTreeDecode(c.low, posState * 8, 3);
    }
    if (decodeBit(c.choice, 1) === 0) {
      return 10 + bitTreeDecode(c.mid, posState * 8, 3);
    }
    return 18 + bitTreeDecode(c.high, 0, 8);
  }

  // --- main loop ---
  let state = 0;
  let rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

  while (outPos < uncompressedSize) {
    const posState = outPos & posMask;
    if (decodeBit(isMatch, (state << 4) + posState) === 0) {
      // literal
      const prevByte = outPos > 0 ? out[outPos - 1] : 0;
      const litState = (((outPos & litPosMask) << lc) + (prevByte >> (8 - lc))) * 0x300;
      let symbol = 1;
      if (state >= 7) {
        // matched literal
        let matchByte = out[outPos - rep0 - 1];
        do {
          const matchBit = (matchByte >> 7) & 1;
          matchByte = (matchByte << 1) & 0xff;
          const bit = decodeBit(literal, litState + ((1 + matchBit) << 8) + symbol);
          symbol = (symbol << 1) | bit;
          if (matchBit !== bit) break;
        } while (symbol < 0x100);
      }
      while (symbol < 0x100) {
        symbol = (symbol << 1) | decodeBit(literal, litState + symbol);
      }
      out[outPos++] = symbol & 0xff;
      state = state < 4 ? 0 : state < 10 ? state - 3 : state - 6;
      continue;
    }

    // match
    let len: number;
    if (decodeBit(isRep, state) !== 0) {
      // rep match
      if (decodeBit(isRepG0, state) === 0) {
        if (decodeBit(isRep0Long, (state << 4) + posState) === 0) {
          // short rep: copy 1 byte
          state = state < 7 ? 9 : 11;
          out[outPos] = out[outPos - rep0 - 1];
          outPos++;
          continue;
        }
      } else {
        let dist: number;
        if (decodeBit(isRepG1, state) === 0) {
          dist = rep1;
        } else {
          if (decodeBit(isRepG2, state) === 0) {
            dist = rep2;
          } else {
            dist = rep3;
            rep3 = rep2;
          }
          rep2 = rep1;
        }
        rep1 = rep0;
        rep0 = dist;
      }
      len = decodeLen(repLenCoder, posState);
      state = state < 7 ? 8 : 11;
    } else {
      // new match
      rep3 = rep2;
      rep2 = rep1;
      rep1 = rep0;
      len = decodeLen(lenCoder, posState);
      state = state < 7 ? 7 : 10;

      const lenClass = len - 2 < 4 ? len - 2 : 3;
      const posSlot = bitTreeDecode(posSlotDecoder, lenClass * 64, 6);
      if (posSlot < 4) {
        rep0 = posSlot;
      } else {
        const numDirect = (posSlot >> 1) - 1;
        rep0 = (2 | (posSlot & 1)) << numDirect;
        if (posSlot < 14) {
          rep0 += bitTreeReverseDecode(specPos, rep0 - posSlot - 1, numDirect);
        } else {
          rep0 = (rep0 + decodeDirectBits(numDirect - 4) * 16) >>> 0;
          rep0 = (rep0 + bitTreeReverseDecode(alignProbs, 0, 4)) >>> 0;
        }
        if (rep0 === 0xffffffff) break; // end marker
      }
    }

    // copy match
    if (rep0 >= outPos) throw new Error('LZMA: corrupt stream (distance too far)');
    let from = outPos - rep0 - 1;
    const end = Math.min(outPos + len, uncompressedSize);
    while (outPos < end) out[outPos++] = out[from++];
  }

  return out;
}
