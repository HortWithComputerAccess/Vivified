import { InfoDat, V3Beatmap, DifficultyRef } from './types';

/**
 * A loaded map folder. Uses the File System Access API (Chrome/Edge) so the
 * difficulty file can be saved back in place.
 */
export interface LoadedMap {
  dirName: string;
  info: InfoDat;
  difficulties: DifficultyRef[];
  dirHandle: FileSystemDirectoryHandle | null;
  /** lowercase filename -> File for fallback (drag/drop or input) loading */
  files: Map<string, File>;
}

export interface LoadedDifficulty {
  ref: DifficultyRef;
  data: V3Beatmap;
}

export function supportsFsAccess(): boolean {
  return typeof (window as any).showDirectoryPicker === 'function';
}

export async function openMapFolder(): Promise<LoadedMap> {
  if (supportsFsAccess()) {
    const dirHandle: FileSystemDirectoryHandle = await (window as any).showDirectoryPicker({
      id: 'vivified-map',
      mode: 'readwrite',
    });
    const files = new Map<string, File>();
    for await (const entry of (dirHandle as any).values()) {
      if (entry.kind === 'file') {
        files.set(entry.name.toLowerCase(), await entry.getFile());
      }
    }
    return buildLoadedMap(dirHandle.name, dirHandle, files);
  }
  // fallback: <input type=file webkitdirectory>
  const files = await pickFolderFallback();
  return buildLoadedMap('map', null, files);
}

/** Build a LoadedMap from in-memory files (drag-drop / tests). */
export async function loadMapFromFiles(files: Map<string, File>): Promise<LoadedMap> {
  return buildLoadedMap('map', null, files);
}

async function pickFolderFallback(): Promise<Map<string, File>> {
  return new Promise((resolve, reject) => {
    const input = document.createElement('input');
    input.type = 'file';
    (input as any).webkitdirectory = true;
    input.onchange = () => {
      const map = new Map<string, File>();
      for (const f of Array.from(input.files ?? [])) {
        map.set(f.name.toLowerCase(), f);
      }
      if (map.size === 0) reject(new Error('No files selected'));
      else resolve(map);
    };
    input.click();
  });
}

async function buildLoadedMap(
  dirName: string,
  dirHandle: FileSystemDirectoryHandle | null,
  files: Map<string, File>
): Promise<LoadedMap> {
  const infoFile = files.get('info.dat');
  if (!infoFile) throw new Error('Info.dat not found in the selected folder');
  const info = JSON.parse(await infoFile.text()) as InfoDat;

  const difficulties: DifficultyRef[] = [];
  for (const set of info._difficultyBeatmapSets ?? []) {
    for (const d of set._difficultyBeatmaps ?? []) {
      difficulties.push({
        characteristic: set._beatmapCharacteristicName ?? 'Standard',
        difficulty: d._difficulty ?? '?',
        filename: d._beatmapFilename,
        requirements: d._customData?._requirements ?? [],
        njs: Number(d._noteJumpMovementSpeed ?? 10),
        startBeatOffset: Number(d._noteJumpStartBeatOffset ?? 0),
        colorLeft: d._customData?._colorLeft ?? null,
        colorRight: d._customData?._colorRight ?? null,
        envColorLeft: d._customData?._envColorLeft ?? null,
        envColorRight: d._customData?._envColorRight ?? null,
      });
    }
  }
  return { dirName, info, difficulties, dirHandle, files };
}

export async function loadDifficulty(map: LoadedMap, ref: DifficultyRef): Promise<LoadedDifficulty> {
  const file = map.files.get(ref.filename.toLowerCase());
  if (!file) throw new Error(`${ref.filename} not found`);
  const data = JSON.parse(await file.text()) as V3Beatmap;
  const version = String(data.version ?? (data as any)._version ?? '');
  if (!version.startsWith('3')) {
    throw new Error(
      `${ref.filename} is beatmap v${version || '2?'} — only v3 maps are supported for editing`
    );
  }
  data.customData = data.customData ?? {};
  data.customData.customEvents = data.customData.customEvents ?? [];
  return { ref, data };
}

/** Save the difficulty back into the map folder (writes in place). */
export async function saveDifficulty(map: LoadedMap, diff: LoadedDifficulty): Promise<string> {
  const json = JSON.stringify(diff.data);
  if (map.dirHandle) {
    const fh = await map.dirHandle.getFileHandle(diff.ref.filename, { create: true });
    const writable = await (fh as any).createWritable();
    await writable.write(json);
    await writable.close();
    // refresh cached File so a reload sees current data
    map.files.set(diff.ref.filename.toLowerCase(), await fh.getFile());
    return `saved ${diff.ref.filename}`;
  }
  // fallback: download
  const blob = new Blob([json], { type: 'application/json' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = diff.ref.filename;
  a.click();
  URL.revokeObjectURL(a.href);
  return `downloaded ${diff.ref.filename} (browser cannot write in place)`;
}

/** Find the song audio file for playback. */
export function songFile(map: LoadedMap): File | null {
  const name = String(map.info._songFilename ?? '').toLowerCase();
  return map.files.get(name) ?? null;
}

/** Find .vivify bundle files in the folder (prefer 2019 = most compatible). */
export function bundleFiles(map: LoadedMap): File[] {
  const out: File[] = [];
  for (const [name, file] of map.files) {
    if (name.endsWith('.vivify') || name.startsWith('bundle')) out.push(file);
  }
  out.sort((a, b) => a.name.localeCompare(b.name));
  return out;
}
