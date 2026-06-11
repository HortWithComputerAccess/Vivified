import { CustomEvent } from '../map/types';

const TYPE_COLORS: Record<string, string> = {
  AnimateTrack: '#5ad6c0',
  AssignPathAnimation: '#4aa3d6',
  AssignTrackParent: '#d6c05a',
  AssignPlayerToTrack: '#d6995a',
  InstantiatePrefab: '#9b6ad6',
  DestroyObject: '#d65a6a',
  SetMaterialProperty: '#d65ab8',
  SetGlobalProperty: '#a8d65a',
  Blit: '#5a7ad6',
  CreateCamera: '#5ad672',
  CreateScreenTexture: '#36b8a0',
  SetCameraProperty: '#7ad65a',
  SetAnimatorProperty: '#d6855a',
  AssignObjectPrefab: '#c4a4ee',
  SetRenderingSettings: '#8a8ad6',
};

const HEADER_H = 22;
const LANE_H = 16;
const LABEL_W = 130;

/**
 * Canvas timeline: beat ruler, one lane per event type, draggable playhead
 * and event markers, wheel zoom.
 */
export class Timeline {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private events: CustomEvent[] = [];
  private lanes: string[] = [];
  private beatsPerPx = 0.05;
  private scrollBeat = -2;
  private maxBeat = 64;
  beat = 0;
  selected: CustomEvent | null = null;
  snap = 0.25;

  onScrub: (beat: number) => void = () => {};
  onSelect: (ev: CustomEvent | null) => void = () => {};
  /** fired once when an event marker drag starts (for undo snapshots) */
  onBeginMoveEvent: (ev: CustomEvent) => void = () => {};
  onMoveEvent: (ev: CustomEvent, newBeat: number) => void = () => {};

  private dragMode: 'none' | 'scrub' | 'event' | 'pan' = 'none';
  private dragEvent: CustomEvent | null = null;
  private panStartX = 0;
  private panStartScroll = 0;

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    new ResizeObserver(() => this.draw()).observe(canvas.parentElement!);

    canvas.addEventListener('pointerdown', (e) => this.pointerDown(e));
    window.addEventListener('pointermove', (e) => this.pointerMove(e));
    window.addEventListener('pointerup', () => (this.dragMode = 'none'));
    canvas.addEventListener('wheel', (e) => this.wheel(e), { passive: false });
  }

  setEvents(events: CustomEvent[], songEndBeat: number): void {
    this.events = events;
    this.maxBeat = Math.max(
      16,
      songEndBeat,
      ...events.map((e) => (e.b ?? 0) + (typeof e.d?.duration === 'number' ? e.d.duration : 0))
    );
    const types = new Set<string>();
    for (const e of events) types.add(e.t);
    this.lanes = [...types].sort(
      (a, b) => (laneOrder(a) - laneOrder(b)) || a.localeCompare(b)
    );
    this.draw();
  }

  setBeat(beat: number): void {
    this.beat = beat;
    // auto-follow when playhead leaves view
    const w = this.canvas.clientWidth - LABEL_W;
    const right = this.scrollBeat + w * this.beatsPerPx;
    if (beat > right - 2 || beat < this.scrollBeat) {
      this.scrollBeat = beat - 2;
    }
    this.draw();
  }

  setSelected(ev: CustomEvent | null): void {
    this.selected = ev;
    this.draw();
  }

  private xToBeat(x: number): number {
    return this.scrollBeat + (x - LABEL_W) * this.beatsPerPx;
  }
  private beatToX(beat: number): number {
    return LABEL_W + (beat - this.scrollBeat) / this.beatsPerPx;
  }

  private pointerDown(e: PointerEvent): void {
    const rect = this.canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    if (e.button === 1 || e.button === 2 || (e.button === 0 && e.shiftKey)) {
      this.dragMode = 'pan';
      this.panStartX = x;
      this.panStartScroll = this.scrollBeat;
      e.preventDefault();
      return;
    }
    if (x < LABEL_W) return;

    if (y >= HEADER_H) {
      const hit = this.hitTest(x, y);
      if (hit) {
        this.selected = hit;
        this.dragMode = 'event';
        this.dragEvent = hit;
        this.onBeginMoveEvent(hit);
        this.onSelect(hit);
        this.draw();
        return;
      }
    }
    this.dragMode = 'scrub';
    this.scrubTo(x, e.altKey);
  }

  private pointerMove(e: PointerEvent): void {
    if (this.dragMode === 'none') return;
    const rect = this.canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    if (this.dragMode === 'scrub') {
      this.scrubTo(x, e.altKey);
    } else if (this.dragMode === 'pan') {
      this.scrollBeat = this.panStartScroll - (x - this.panStartX) * this.beatsPerPx;
      this.draw();
    } else if (this.dragMode === 'event' && this.dragEvent) {
      let b = this.xToBeat(x);
      if (!e.altKey && this.snap > 0) b = Math.round(b / this.snap) * this.snap;
      b = Math.max(0, Math.round(b * 1000) / 1000);
      if (b !== this.dragEvent.b) {
        this.onMoveEvent(this.dragEvent, b);
        this.draw();
      }
    }
  }

  private scrubTo(x: number, noSnap: boolean): void {
    let b = this.xToBeat(x);
    if (!noSnap && this.snap > 0) b = Math.round(b / this.snap) * this.snap;
    b = Math.max(0, b);
    this.beat = b;
    this.onScrub(b);
    this.draw();
  }

  private wheel(e: WheelEvent): void {
    e.preventDefault();
    const rect = this.canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    if (e.ctrlKey || !e.shiftKey) {
      // zoom around cursor
      const beforeBeat = this.xToBeat(x);
      const factor = e.deltaY > 0 ? 1.2 : 1 / 1.2;
      this.beatsPerPx = Math.min(2, Math.max(0.002, this.beatsPerPx * factor));
      this.scrollBeat += beforeBeat - this.xToBeat(x);
    } else {
      this.scrollBeat += e.deltaY * this.beatsPerPx * 0.5;
    }
    this.draw();
  }

  private hitTest(x: number, y: number): CustomEvent | null {
    const lane = Math.floor((y - HEADER_H) / LANE_H);
    if (lane < 0 || lane >= this.lanes.length) return null;
    const type = this.lanes[lane];
    const beat = this.xToBeat(x);
    const tol = 5 * this.beatsPerPx;
    let best: CustomEvent | null = null;
    let bestDist = Infinity;
    for (const ev of this.events) {
      if (ev.t !== type) continue;
      const dur = typeof ev.d?.duration === 'number' ? ev.d.duration : 0;
      const b = ev.b ?? 0;
      const inBody = beat >= b - tol && beat <= b + Math.max(dur, 0) + tol;
      if (!inBody) continue;
      const dist = Math.abs(beat - b);
      if (dist < bestDist) {
        best = ev;
        bestDist = dist;
      }
    }
    return best;
  }

  draw(): void {
    const dpr = window.devicePixelRatio || 1;
    const w = this.canvas.clientWidth;
    const h = this.canvas.clientHeight;
    if (this.canvas.width !== w * dpr || this.canvas.height !== h * dpr) {
      this.canvas.width = w * dpr;
      this.canvas.height = h * dpr;
    }
    const ctx = this.ctx;
    ctx.save();
    ctx.scale(dpr, dpr);
    ctx.clearRect(0, 0, w, h);
    ctx.fillStyle = '#1e1e26';
    ctx.fillRect(0, 0, w, h);

    // beat grid
    const beatStep = pickBeatStep(this.beatsPerPx);
    const firstBeat = Math.floor(this.scrollBeat / beatStep) * beatStep;
    const lastBeat = this.scrollBeat + (w - LABEL_W) * this.beatsPerPx;
    ctx.font = '10px Segoe UI';
    for (let b = firstBeat; b <= lastBeat; b += beatStep) {
      if (b < 0) continue;
      const x = this.beatToX(b);
      const major = Math.abs(b / (beatStep * 4) - Math.round(b / (beatStep * 4))) < 1e-6;
      ctx.strokeStyle = major ? '#3a3a48' : '#2a2a34';
      ctx.beginPath();
      ctx.moveTo(x, HEADER_H);
      ctx.lineTo(x, h);
      ctx.stroke();
      if (major) {
        ctx.fillStyle = '#8a8a98';
        ctx.fillText(formatBeat(b), x + 3, 14);
      }
    }

    // lanes
    for (let i = 0; i < this.lanes.length; i++) {
      const y = HEADER_H + i * LANE_H;
      ctx.fillStyle = i % 2 ? '#20202a' : '#1e1e26';
      ctx.fillRect(LABEL_W, y, w - LABEL_W, LANE_H);
    }

    // events
    for (const ev of this.events) {
      const lane = this.lanes.indexOf(ev.t);
      if (lane < 0) continue;
      const y = HEADER_H + lane * LANE_H;
      const x = this.beatToX(ev.b ?? 0);
      if (x < LABEL_W - 50 || x > w + 50) {
        const dur = typeof ev.d?.duration === 'number' ? ev.d.duration : 0;
        if (this.beatToX((ev.b ?? 0) + dur) < LABEL_W) continue;
        if (x > w + 50) continue;
      }
      const color = TYPE_COLORS[ev.t] ?? '#888';
      const dur = typeof ev.d?.duration === 'number' ? ev.d.duration : 0;
      if (dur > 0) {
        const x2 = this.beatToX((ev.b ?? 0) + dur);
        ctx.fillStyle = color + '44';
        ctx.fillRect(x, y + 3, Math.max(x2 - x, 2), LANE_H - 6);
      }
      ctx.fillStyle = color;
      const sel = ev === this.selected;
      ctx.fillRect(x - (sel ? 3 : 2), y + (sel ? 1 : 3), sel ? 6 : 4, LANE_H - (sel ? 2 : 6));
      if (sel) {
        ctx.strokeStyle = '#fff';
        ctx.strokeRect(x - 3.5, y + 0.5, 7, LANE_H - 1);
      }
    }

    // lane labels (drawn after events so they stay readable)
    ctx.fillStyle = '#1a1a22';
    ctx.fillRect(0, 0, LABEL_W, h);
    ctx.strokeStyle = '#32323e';
    ctx.beginPath();
    ctx.moveTo(LABEL_W + 0.5, 0);
    ctx.lineTo(LABEL_W + 0.5, h);
    ctx.stroke();
    ctx.font = '11px Segoe UI';
    for (let i = 0; i < this.lanes.length; i++) {
      const y = HEADER_H + i * LANE_H;
      ctx.fillStyle = TYPE_COLORS[this.lanes[i]] ?? '#888';
      ctx.fillRect(6, y + 4, 8, 8);
      ctx.fillStyle = '#d8d8e0';
      ctx.fillText(this.lanes[i], 20, y + 12, LABEL_W - 26);
    }
    ctx.fillStyle = '#8a8a98';
    ctx.fillText('beats', 6, 14);

    // playhead
    const px = this.beatToX(this.beat);
    if (px >= LABEL_W) {
      ctx.strokeStyle = '#5ad6c0';
      ctx.lineWidth = 1.5;
      ctx.beginPath();
      ctx.moveTo(px, 0);
      ctx.lineTo(px, h);
      ctx.stroke();
      ctx.fillStyle = '#5ad6c0';
      ctx.beginPath();
      ctx.moveTo(px - 5, 0);
      ctx.lineTo(px + 5, 0);
      ctx.lineTo(px, 8);
      ctx.closePath();
      ctx.fill();
      ctx.lineWidth = 1;
    }
    ctx.restore();
  }
}

function laneOrder(type: string): number {
  const order = [
    'InstantiatePrefab',
    'DestroyObject',
    'AnimateTrack',
    'AssignPathAnimation',
    'AssignTrackParent',
    'AssignObjectPrefab',
  ];
  const i = order.indexOf(type);
  return i === -1 ? order.length : i;
}

function pickBeatStep(beatsPerPx: number): number {
  const steps = [0.25, 0.5, 1, 2, 4, 8, 16, 32, 64];
  for (const s of steps) {
    if (s / beatsPerPx >= 12) return s;
  }
  return 128;
}

function formatBeat(b: number): string {
  return Number.isInteger(b) ? String(b) : b.toFixed(2);
}
