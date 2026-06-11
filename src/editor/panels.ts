import { CustomEvent, ALL_EVENT_TYPES, EVENT_TEMPLATES } from '../map/types';
import { AssetDB, ClassID } from '../unity/assets';

// ---------------------------------------------------------------------------
// Event list
// ---------------------------------------------------------------------------

export class EventListPanel {
  private container: HTMLElement;
  private filterInput: HTMLInputElement;
  private events: CustomEvent[] = [];
  private selected: CustomEvent | null = null;

  onSelect: (ev: CustomEvent) => void = () => {};

  constructor(container: HTMLElement, filterInput: HTMLInputElement) {
    this.container = container;
    this.filterInput = filterInput;
    filterInput.addEventListener('input', () => this.render());
  }

  setEvents(events: CustomEvent[]): void {
    this.events = events;
    this.render();
  }

  setSelected(ev: CustomEvent | null): void {
    this.selected = ev;
    this.render();
  }

  private render(): void {
    const filter = this.filterInput.value.toLowerCase();
    const sorted = [...this.events].sort((a, b) => (a.b ?? 0) - (b.b ?? 0));
    const frag = document.createDocumentFragment();
    let shown = 0;
    for (const ev of sorted) {
      const summary = summarize(ev);
      if (filter && !(`${ev.t} ${summary}`.toLowerCase().includes(filter))) continue;
      if (shown++ > 3000) break;
      const row = document.createElement('div');
      row.className = 'event-row' + (ev === this.selected ? ' selected' : '');
      row.innerHTML = `<span class="beat">${(ev.b ?? 0).toFixed(2)}</span><span class="type">${esc(ev.t)}</span><span class="summary">${esc(summary)}</span>`;
      row.addEventListener('click', () => this.onSelect(ev));
      frag.appendChild(row);
    }
    this.container.replaceChildren(frag);
    if (shown === 0) {
      this.container.innerHTML = '<p class="hint">No events match.</p>';
    }
  }

  scrollToSelected(): void {
    const el = this.container.querySelector('.event-row.selected');
    el?.scrollIntoView({ block: 'nearest' });
  }
}

function summarize(ev: CustomEvent): string {
  const d = ev.d ?? {};
  const parts: string[] = [];
  if (d.track) parts.push(`track:${Array.isArray(d.track) ? d.track.join(',') : d.track}`);
  if (d.id) parts.push(`id:${Array.isArray(d.id) ? d.id.join(',') : d.id}`);
  if (d.asset) parts.push(String(d.asset).split('/').pop() ?? '');
  if (d.parentTrack) parts.push(`parent:${d.parentTrack}`);
  if (typeof d.duration === 'number') parts.push(`dur:${d.duration}`);
  return parts.join(' ');
}

function esc(s: unknown): string {
  return String(s ?? '').replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]!);
}

// ---------------------------------------------------------------------------
// Inspector
// ---------------------------------------------------------------------------

export class Inspector {
  private container: HTMLElement;
  private current: CustomEvent | null = null;
  private textarea: HTMLTextAreaElement | null = null;

  onChange: (ev: CustomEvent) => void = () => {};
  onDelete: (ev: CustomEvent) => void = () => {};
  onDuplicate: (ev: CustomEvent) => void = () => {};
  onJumpToBeat: (beat: number) => void = () => {};

  constructor(container: HTMLElement) {
    this.container = container;
  }

  show(ev: CustomEvent | null): void {
    this.current = ev;
    if (!ev) {
      this.container.innerHTML = '<p class="hint">Select an event or a spawned object.</p>';
      this.textarea = null;
      return;
    }
    this.container.innerHTML = '';

    const title = document.createElement('h3');
    title.textContent = ev.t;
    this.container.appendChild(title);

    // beat + type row
    const row = document.createElement('div');
    row.className = 'row';
    const beatLabel = document.createElement('label');
    beatLabel.textContent = 'beat';
    const beatInput = document.createElement('input');
    beatInput.type = 'number';
    beatInput.step = '0.25';
    beatInput.value = String(ev.b ?? 0);
    beatInput.addEventListener('change', () => {
      ev.b = Number(beatInput.value) || 0;
      this.onChange(ev);
    });
    const typeSelect = document.createElement('select');
    for (const t of ALL_EVENT_TYPES) {
      const opt = document.createElement('option');
      opt.value = t;
      opt.textContent = t;
      if (t === ev.t) opt.selected = true;
      typeSelect.appendChild(opt);
    }
    if (!ALL_EVENT_TYPES.includes(ev.t)) {
      const opt = document.createElement('option');
      opt.value = ev.t;
      opt.textContent = ev.t;
      opt.selected = true;
      typeSelect.appendChild(opt);
    }
    typeSelect.addEventListener('change', () => {
      ev.t = typeSelect.value;
      this.onChange(ev);
      this.show(ev);
    });
    const jumpBtn = document.createElement('button');
    jumpBtn.textContent = '→ beat';
    jumpBtn.title = 'Move playhead to this event';
    jumpBtn.addEventListener('click', () => this.onJumpToBeat(ev.b ?? 0));
    row.append(beatLabel, beatInput, typeSelect, jumpBtn);
    this.container.appendChild(row);

    // quick transform editors for InstantiatePrefab
    if (ev.t === 'InstantiatePrefab') {
      this.container.appendChild(this.vecEditor(ev, 'position', 0));
      this.container.appendChild(this.vecEditor(ev, 'rotation', 0));
      this.container.appendChild(this.vecEditor(ev, 'scale', 1));
    }

    // JSON editor for d
    const label = document.createElement('label');
    label.textContent = 'data (d)';
    this.container.appendChild(label);
    const ta = document.createElement('textarea');
    ta.value = JSON.stringify(ev.d ?? {}, null, 2);
    ta.spellcheck = false;
    ta.addEventListener('input', () => {
      try {
        const parsed = JSON.parse(ta.value);
        ta.classList.remove('invalid');
        ev.d = parsed;
        this.onChange(ev);
      } catch {
        ta.classList.add('invalid');
      }
    });
    this.textarea = ta;
    this.container.appendChild(ta);

    // actions
    const btnRow = document.createElement('div');
    btnRow.className = 'btn-row';
    const dup = document.createElement('button');
    dup.textContent = 'Duplicate';
    dup.addEventListener('click', () => this.onDuplicate(ev));
    const del = document.createElement('button');
    del.textContent = 'Delete';
    del.className = 'danger';
    del.addEventListener('click', () => this.onDelete(ev));
    btnRow.append(dup, del);
    this.container.appendChild(btnRow);
  }

  /** Update the JSON textarea from the event (after gizmo edits). */
  refreshData(): void {
    if (this.current && this.textarea && document.activeElement !== this.textarea) {
      this.textarea.value = JSON.stringify(this.current.d ?? {}, null, 2);
      this.textarea.classList.remove('invalid');
      // refresh vec inputs
      this.container.querySelectorAll<HTMLInputElement>('input[data-vec]').forEach((inp) => {
        if (document.activeElement === inp) return;
        const [key, idxStr] = inp.dataset.vec!.split(':');
        const def = key === 'scale' ? 1 : 0;
        const arr = this.current!.d?.[key];
        inp.value = String(Array.isArray(arr) ? arr[Number(idxStr)] ?? def : def);
      });
    }
  }

  get event(): CustomEvent | null {
    return this.current;
  }

  private vecEditor(ev: CustomEvent, key: string, def: number): HTMLElement {
    const row = document.createElement('div');
    row.className = 'vec-row';
    const label = document.createElement('span');
    label.textContent = key;
    row.appendChild(label);
    for (let i = 0; i < 3; i++) {
      const inp = document.createElement('input');
      inp.type = 'number';
      inp.step = key === 'rotation' ? '5' : '0.1';
      inp.dataset.vec = `${key}:${i}`;
      const arr = ev.d?.[key];
      inp.value = String(Array.isArray(arr) ? arr[i] ?? def : def);
      inp.addEventListener('change', () => {
        const d = (ev.d = ev.d ?? {});
        if (!Array.isArray(d[key])) d[key] = [def, def, def];
        d[key][i] = Number(inp.value) || 0;
        this.onChange(ev);
        this.refreshData();
      });
      row.appendChild(inp);
    }
    return row;
  }
}

// ---------------------------------------------------------------------------
// Asset browser
// ---------------------------------------------------------------------------

export class AssetBrowser {
  private container: HTMLElement;
  onSpawn: (assetPath: string) => void = () => {};
  onPreview: (assetPath: string | null) => void = () => {};

  constructor(container: HTMLElement) {
    this.container = container;
  }

  setDb(db: AssetDB | null): void {
    if (!db) {
      this.container.innerHTML = '<p class="hint">Open a map folder or .vivify bundle to list assets.</p>';
      return;
    }
    this.container.innerHTML = '';
    const groups = new Map<string, { path: string; isPrefab: boolean }[]>();
    for (const entry of db.listAssets()) {
      const ext = entry.path.split('.').pop() ?? '';
      const cls = db.classOf(entry.pathID);
      const group = ext === 'prefab' ? 'Prefabs' : ext === 'mat' ? 'Materials' : clsGroup(cls) ?? `.${ext}`;
      if (!groups.has(group)) groups.set(group, []);
      groups.get(group)!.push({ path: entry.path, isPrefab: cls === ClassID.GameObject });
    }
    const order = ['Prefabs', 'Materials'];
    const keys = [...groups.keys()].sort((a, b) => {
      const ia = order.indexOf(a) === -1 ? 99 : order.indexOf(a);
      const ib = order.indexOf(b) === -1 ? 99 : order.indexOf(b);
      return ia - ib || a.localeCompare(b);
    });
    for (const key of keys) {
      const title = document.createElement('div');
      title.className = 'asset-group-title';
      title.textContent = `${key} (${groups.get(key)!.length})`;
      this.container.appendChild(title);
      for (const item of groups.get(key)!) {
        const row = document.createElement('div');
        row.className = 'asset-row';
        const name = document.createElement('span');
        name.className = 'name';
        name.textContent = item.path.replace(/^assets\//, '');
        name.title = item.path;
        row.appendChild(name);
        if (item.isPrefab) {
          const prevBtn = document.createElement('button');
          prevBtn.textContent = '👁';
          prevBtn.title = 'Preview at origin (click again to hide)';
          let previewing = false;
          prevBtn.addEventListener('click', () => {
            previewing = !previewing;
            this.onPreview(previewing ? item.path : null);
            prevBtn.classList.toggle('active', previewing);
          });
          const spawnBtn = document.createElement('button');
          spawnBtn.textContent = '+';
          spawnBtn.title = 'Create InstantiatePrefab event at current beat';
          spawnBtn.addEventListener('click', () => this.onSpawn(item.path));
          row.append(prevBtn, spawnBtn);
        }
        this.container.appendChild(row);
      }
    }
  }
}

function clsGroup(cls: number | undefined): string | null {
  switch (cls) {
    case ClassID.Texture2D: return 'Textures';
    case ClassID.AudioClip: return 'Audio';
    case ClassID.Shader: return 'Shaders';
    case ClassID.TextAsset: return 'Text';
    default: return null;
  }
}

export function makeEventFromTemplate(type: string, beat: number): CustomEvent {
  const tpl = EVENT_TEMPLATES[type] ?? {};
  return { b: beat, t: type, d: JSON.parse(JSON.stringify(tpl)) };
}
