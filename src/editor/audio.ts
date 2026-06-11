/** Song playback with seconds-accurate position for timeline sync. */
export class SongPlayer {
  private ctx: AudioContext | null = null;
  private buffer: AudioBuffer | null = null;
  private source: AudioBufferSourceNode | null = null;
  private startCtxTime = 0;
  private startSongTime = 0;
  playing = false;

  get duration(): number {
    return this.buffer?.duration ?? 0;
  }

  get loaded(): boolean {
    return this.buffer !== null;
  }

  async load(file: File): Promise<void> {
    this.stop();
    this.ctx = this.ctx ?? new AudioContext();
    const data = await file.arrayBuffer();
    this.buffer = await this.ctx.decodeAudioData(data);
  }

  play(fromSeconds: number): void {
    if (!this.ctx || !this.buffer) return;
    this.stop();
    if (this.ctx.state === 'suspended') void this.ctx.resume();
    const src = this.ctx.createBufferSource();
    src.buffer = this.buffer;
    src.connect(this.ctx.destination);
    const offset = Math.min(Math.max(fromSeconds, 0), this.buffer.duration - 0.01);
    src.start(0, offset);
    src.onended = () => {
      if (this.source === src) this.playing = false;
    };
    this.source = src;
    this.startCtxTime = this.ctx.currentTime;
    this.startSongTime = offset;
    this.playing = true;
  }

  stop(): void {
    if (this.source) {
      try {
        this.source.stop();
      } catch {
        // already stopped
      }
      this.source.disconnect();
      this.source = null;
    }
    this.playing = false;
  }

  /** Current song position in seconds while playing. */
  get currentTime(): number {
    if (!this.ctx || !this.playing) return this.startSongTime;
    return this.startSongTime + (this.ctx.currentTime - this.startCtxTime);
  }
}
