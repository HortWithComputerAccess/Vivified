/** Beat Saber v3 beatmap + Vivify/Heck custom event types. */

export interface CustomEvent {
  b: number;
  t: string;
  d: Record<string, any>;
}

export interface V3Beatmap {
  version: string;
  bpmEvents?: { b: number; m: number }[];
  colorNotes?: any[];
  bombNotes?: any[];
  obstacles?: any[];
  customData?: {
    customEvents?: CustomEvent[];
    pointDefinitions?: Record<string, any[]>;
    environment?: any[];
    [key: string]: any;
  };
  [key: string]: any;
}

export interface DifficultyRef {
  characteristic: string;
  difficulty: string;
  filename: string;
  requirements: string[];
}

export interface InfoDat {
  _songName: string;
  _beatsPerMinute: number;
  _songFilename: string;
  _difficultyBeatmapSets?: any[];
  _customData?: any;
  [key: string]: any;
}

/** Vivify event types (from the mod's deserializer), plus Heck's. */
export const VIVIFY_EVENT_TYPES = [
  'SetMaterialProperty',
  'SetGlobalProperty',
  'Blit',
  'CreateCamera',
  'CreateScreenTexture',
  'InstantiatePrefab',
  'DestroyObject',
  'SetAnimatorProperty',
  'SetCameraProperty',
  'AssignObjectPrefab',
  'SetRenderingSettings',
] as const;

export const HECK_EVENT_TYPES = [
  'AnimateTrack',
  'AssignPathAnimation',
  'AssignTrackParent',
  'AssignPlayerToTrack',
] as const;

export const ALL_EVENT_TYPES: string[] = [...VIVIFY_EVENT_TYPES, ...HECK_EVENT_TYPES];

/** Default payloads when creating a new event in the editor. */
export const EVENT_TEMPLATES: Record<string, Record<string, any>> = {
  InstantiatePrefab: { asset: '', id: '', track: '', position: [0, 0, 0], rotation: [0, 0, 0], scale: [1, 1, 1] },
  DestroyObject: { id: '' },
  AnimateTrack: { track: '', duration: 1, position: [[0, 0, 0, 0], [0, 5, 0, 1]] },
  AssignPathAnimation: { track: '', offsetPosition: [[0, 0, 0, 0], [0, 0, 0, 0.5]] },
  AssignTrackParent: { childrenTracks: [], parentTrack: '' },
  SetMaterialProperty: { asset: '', properties: [{ id: '_Color', type: 'Color', value: [1, 1, 1, 1] }] },
  SetGlobalProperty: { properties: [{ id: '_MyProperty', type: 'Float', value: 1 }] },
  Blit: { asset: '', duration: 0, priority: 0 },
  CreateCamera: { id: 'camera1', texture: '_CameraTexture' },
  CreateScreenTexture: { id: '_ScreenTexture' },
  SetCameraProperty: { properties: { depthTextureMode: ['Depth'] } },
  SetAnimatorProperty: { id: '', properties: [{ id: 'Trigger', type: 'Trigger', value: true }] },
  AssignObjectPrefab: { loadMode: 'Single', colorNotes: { track: '' } },
  SetRenderingSettings: { renderSettings: { fog: true } },
  AssignPlayerToTrack: { track: 'player' },
};
