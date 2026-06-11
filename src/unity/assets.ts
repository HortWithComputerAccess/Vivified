import { parseBundle } from './bundle';
import {
  parseSerializedFile,
  readObject,
  SerializedFile,
  ObjectInfo,
  UnityValue,
} from './serializedFile';

export const ClassID = {
  GameObject: 1,
  Transform: 4,
  Material: 21,
  MeshRenderer: 23,
  Texture2D: 28,
  MeshFilter: 33,
  Mesh: 43,
  Shader: 48,
  TextAsset: 49,
  AudioClip: 83,
  Animator: 95,
  AnimatorController: 91,
  AnimationClip: 74,
  Light: 108,
  MonoBehaviour: 114,
  MonoScript: 115,
  SkinnedMeshRenderer: 137,
  AssetBundle: 142,
  ParticleSystem: 198,
  ParticleSystemRenderer: 199,
  SpriteRenderer: 212,
  RectTransform: 224,
  CanvasRenderer: 222,
} as const;

export interface PPtr {
  m_FileID: number;
  m_PathID: number;
}

export interface ContainerEntry {
  /** e.g. "assets/aether/prefabs/sections/intro.prefab" */
  path: string;
  pathID: number;
}

/**
 * A loaded AssetBundle: every serialized object indexed by pathID, with lazy
 * typetree deserialization, plus resource (.resS) blobs for streamed data.
 */
export class AssetDB {
  files: SerializedFile[] = [];
  /** pathID -> [file, objectInfo] (single-CAB bundles in practice) */
  objects = new Map<number, [SerializedFile, ObjectInfo]>();
  resources = new Map<string, Uint8Array>();
  containers: ContainerEntry[] = [];
  unityRevision = '';
  private cache = new Map<number, UnityValue>();

  static fromBundle(buffer: ArrayBuffer): AssetDB {
    const db = new AssetDB();
    const bundle = parseBundle(buffer);
    db.unityRevision = bundle.unityRevision;
    for (const node of bundle.nodes) {
      if (node.path.endsWith('.resS') || node.path.endsWith('.resource')) {
        db.resources.set(node.path, node.data);
      } else if (looksLikeSerializedFile(node.data)) {
        const sf = parseSerializedFile(node.data);
        db.files.push(sf);
        for (const [pathID, info] of sf.objects) {
          db.objects.set(pathID, [sf, info]);
        }
      }
    }
    db.buildContainers();
    return db;
  }

  private buildContainers(): void {
    for (const [pathID, [, info]] of this.objects) {
      if (info.classID !== ClassID.AssetBundle) continue;
      const ab = this.read(pathID) as any;
      if (!ab || !ab.m_Container) continue;
      const container = ab.m_Container;
      // m_Container is map<string, AssetInfo>; may arrive as object or pair array
      if (Array.isArray(container)) {
        for (const pair of container) {
          this.containers.push({
            path: String(pair.first),
            pathID: Number(pair.second?.asset?.m_PathID ?? 0),
          });
        }
      } else {
        for (const [path, entry] of Object.entries(container as Record<string, any>)) {
          this.containers.push({ path, pathID: Number(entry?.asset?.m_PathID ?? 0) });
        }
      }
    }
    this.containers.sort((a, b) => a.path.localeCompare(b.path));
  }

  classOf(pathID: number): number | undefined {
    return this.objects.get(pathID)?.[1].classID;
  }

  /** Deserialize an object by pathID (cached). */
  read(pathID: number): UnityValue | null {
    if (this.cache.has(pathID)) return this.cache.get(pathID)!;
    const entry = this.objects.get(pathID);
    if (!entry) return null;
    const [file, info] = entry;
    const value = readObject(file, info);
    this.cache.set(pathID, value);
    return value;
  }

  /** Resolve a PPtr within this bundle. Cross-bundle refs return null. */
  deref(pptr: PPtr | null | undefined): UnityValue | null {
    if (!pptr || pptr.m_PathID === 0) return null;
    if (pptr.m_FileID !== 0) {
      // external reference (e.g. unity default resources) - unavailable
      const entry = this.objects.get(pptr.m_PathID);
      if (!entry) return null;
    }
    return this.read(pptr.m_PathID);
  }

  derefClass(pptr: PPtr | null | undefined): number | undefined {
    if (!pptr || pptr.m_PathID === 0) return undefined;
    return this.classOf(pptr.m_PathID);
  }

  /** Bytes for streamed texture/mesh data, looking up the .resS node. */
  resourceData(path: string, offset: number, size: number): Uint8Array | null {
    // path looks like "archive:/CAB-xxxx/CAB-xxxx.resS"
    const base = path.split('/').pop()!;
    for (const [name, data] of this.resources) {
      if (name === base || name.endsWith(base)) {
        return data.subarray(offset, offset + size);
      }
    }
    return null;
  }

  /** All prefab-ish entries from the container map. */
  listAssets(): ContainerEntry[] {
    return this.containers;
  }
}

function looksLikeSerializedFile(data: Uint8Array): boolean {
  if (data.length < 20) return false;
  // SerializedFile header: metadataSize, fileSize, version, dataOffset (BE).
  const view = new DataView(data.buffer, data.byteOffset, data.byteLength);
  const version = view.getUint32(8, false);
  return version > 0 && version <= 60;
}

// ---------------------------------------------------------------------------
// GameObject hierarchy extraction
// ---------------------------------------------------------------------------

export interface UTransform {
  position: { x: number; y: number; z: number };
  rotation: { x: number; y: number; z: number; w: number };
  scale: { x: number; y: number; z: number };
}

export interface UGameObject {
  pathID: number;
  name: string;
  active: boolean;
  transform: UTransform;
  meshPathID: number | null;
  /** built-in primitive name when the mesh lives in unity default resources */
  meshBuiltin: 'Cube' | 'Cylinder' | 'Sphere' | 'Capsule' | 'Plane' | 'Quad' | null;
  materialPathIDs: number[];
  /** Animator's AnimatorController pathID, if this object has one */
  animatorControllerPathID: number | null;
  /** ParticleSystem component pathID, if present */
  particleSystemPathID: number | null;
  /** materials on the ParticleSystemRenderer */
  particleMaterialPathIDs: number[];
  /** class IDs of non-renderer components, for badges/diagnostics */
  componentClasses: number[];
  children: UGameObject[];
}

/** Build the GameObject tree rooted at a prefab's root GameObject. */
export function buildGameObjectTree(db: AssetDB, goPathID: number): UGameObject | null {
  const go = db.read(goPathID) as any;
  if (!go) return null;

  const result: UGameObject = {
    pathID: goPathID,
    name: String(go.m_Name ?? 'GameObject'),
    active: go.m_IsActive !== false && go.m_IsActive !== 0,
    transform: {
      position: { x: 0, y: 0, z: 0 },
      rotation: { x: 0, y: 0, z: 0, w: 1 },
      scale: { x: 1, y: 1, z: 1 },
    },
    meshPathID: null,
    meshBuiltin: null,
    materialPathIDs: [],
    animatorControllerPathID: null,
    particleSystemPathID: null,
    particleMaterialPathIDs: [],
    componentClasses: [],
    children: [],
  };

  const components: PPtr[] = (go.m_Component ?? [])
    .map((c: any) => (c.component ?? c.second ?? c) as PPtr)
    .filter((p: any) => p && typeof p.m_PathID === 'number');

  let transformPtr: PPtr | null = null;
  for (const comp of components) {
    const cls = db.derefClass(comp);
    if (cls === undefined) continue;
    if (cls === ClassID.Transform || cls === ClassID.RectTransform) {
      transformPtr = comp;
    } else if (cls === ClassID.MeshFilter) {
      const mf = db.deref(comp) as any;
      const meshPtr = mf?.m_Mesh;
      if (meshPtr?.m_PathID) {
        if (meshPtr.m_FileID === 0) {
          result.meshPathID = meshPtr.m_PathID;
        } else {
          result.meshBuiltin = builtinMeshName(db, comp, meshPtr);
        }
      }
    } else if (cls === ClassID.MeshRenderer || cls === ClassID.SkinnedMeshRenderer) {
      const mr = db.deref(comp) as any;
      if (cls === ClassID.SkinnedMeshRenderer && mr?.m_Mesh?.m_PathID) {
        result.meshPathID = mr.m_Mesh.m_PathID;
      }
      for (const mat of mr?.m_Materials ?? []) {
        if (mat?.m_PathID) result.materialPathIDs.push(mat.m_PathID);
      }
    } else if (cls === ClassID.Animator) {
      const animator = db.deref(comp) as any;
      if (animator?.m_Controller?.m_PathID) {
        result.animatorControllerPathID = animator.m_Controller.m_PathID;
      }
      result.componentClasses.push(cls);
    } else if (cls === ClassID.ParticleSystem) {
      result.particleSystemPathID = comp.m_PathID;
      result.componentClasses.push(cls);
    } else if (cls === ClassID.ParticleSystemRenderer) {
      const pr = db.deref(comp) as any;
      for (const mat of pr?.m_Materials ?? []) {
        if (mat?.m_PathID) result.particleMaterialPathIDs.push(mat.m_PathID);
      }
    } else {
      result.componentClasses.push(cls);
    }
  }

  if (transformPtr) {
    const tr = db.deref(transformPtr) as any;
    if (tr) {
      result.transform = {
        position: vec3(tr.m_LocalPosition),
        rotation: quat(tr.m_LocalRotation),
        scale: vec3(tr.m_LocalScale, 1),
      };
      for (const childPtr of tr.m_Children ?? []) {
        const childTr = db.deref(childPtr) as any;
        const childGoPtr = childTr?.m_GameObject;
        if (childGoPtr?.m_PathID) {
          const child = buildGameObjectTree(db, childGoPtr.m_PathID);
          if (child) result.children.push(child);
        }
      }
    }
  }

  return result;
}

/** Identify built-in primitives referenced from "unity default resources". */
function builtinMeshName(db: AssetDB, componentPtr: PPtr, meshPtr: PPtr): UGameObject['meshBuiltin'] {
  const entry = db.objects.get(componentPtr.m_PathID);
  const externals = entry ? entry[0].externals : db.files[0]?.externals ?? [];
  const extName = externals[meshPtr.m_FileID - 1] ?? '';
  if (!/unity default resources/i.test(extName)) return null;
  switch (meshPtr.m_PathID) {
    case 10202: return 'Cube';
    case 10206: return 'Cylinder';
    case 10207: return 'Sphere';
    case 10208: return 'Capsule';
    case 10209: return 'Plane';
    case 10210: return 'Quad';
    default: return null;
  }
}

function vec3(v: any, def = 0): { x: number; y: number; z: number } {
  return { x: num(v?.x, def), y: num(v?.y, def), z: num(v?.z, def) };
}
function quat(v: any): { x: number; y: number; z: number; w: number } {
  return { x: num(v?.x, 0), y: num(v?.y, 0), z: num(v?.z, 0), w: num(v?.w, 1) };
}
function num(v: any, def: number): number {
  return typeof v === 'number' && isFinite(v) ? v : def;
}
