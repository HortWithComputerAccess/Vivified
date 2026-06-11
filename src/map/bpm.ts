/** Piecewise beat <-> seconds mapping from base BPM + v3 bpmEvents. */
export class BpmMap {
  private segments: { beat: number; seconds: number; bpm: number }[] = [];

  constructor(baseBpm: number, bpmEvents: { b: number; m: number }[] = []) {
    const events = [...bpmEvents].sort((a, b) => a.b - b.b);
    let bpm = baseBpm;
    let beat = 0;
    let seconds = 0;
    if (events.length && events[0].b === 0) {
      bpm = events[0].m;
    }
    this.segments.push({ beat: 0, seconds: 0, bpm });
    for (const ev of events) {
      if (ev.b <= 0) continue;
      seconds += ((ev.b - beat) / bpm) * 60;
      beat = ev.b;
      bpm = ev.m;
      this.segments.push({ beat, seconds, bpm });
    }
  }

  secondsAt(beat: number): number {
    const seg = this.segmentForBeat(beat);
    return seg.seconds + ((beat - seg.beat) / seg.bpm) * 60;
  }

  beatAt(seconds: number): number {
    let seg = this.segments[0];
    for (const s of this.segments) {
      if (s.seconds <= seconds) seg = s;
      else break;
    }
    return seg.beat + ((seconds - seg.seconds) / 60) * seg.bpm;
  }

  private segmentForBeat(beat: number) {
    let seg = this.segments[0];
    for (const s of this.segments) {
      if (s.beat <= beat) seg = s;
      else break;
    }
    return seg;
  }
}
