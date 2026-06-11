import * as THREE from 'three';
import { AssetDB, UGameObject, buildGameObjectTree, ClassID } from './assets';
import { parseMesh } from './mesh';
import { parseMaterial } from './material';
import { decodeTexture } from './texture';

/**
 * Unity is left-handed (+Z forward); Three.js is right-handed (-Z forward).
 * We mirror Z: position (x,y,-z), quaternion (-qx,-qy,qz,qw), vertex/normal
 * z negated. Unity's clockwise front faces become counter-clockwise after the
 * mirror, which matches Three's default winding, so indices stay as-is.
 */
export function convertPosition(p: { x: number; y: number; z: number }): THREE.Vector3 {
  return new THREE.Vector3(p.x, p.y, -p.z);
}

export function convertQuaternion(q: { x: number; y: number; z: number; w: number }): THREE.Quaternion {
  return new THREE.Quaternion(-q.x, -q.y, q.z, q.w);
}

/** Converts a Unity euler (degrees, left-handed) to a Three quaternion. */
export function unityEulerToQuaternion(xDeg: number, yDeg: number, zDeg: number): THREE.Quaternion {
  const d = Math.PI / 180;
  // Unity applies euler as Z then X then Y, in left-handed space.
  const qx = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(1, 0, 0), xDeg * d);
  const qy = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 1, 0), yDeg * d);
  const qz = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 0, 1), zDeg * d);
  const unityQ = qy.multiply(qx).multiply(qz); // left-handed composition
  // unityQ is expressed with left-handed components; mirror it
  return new THREE.Quaternion(-unityQ.x, -unityQ.y, unityQ.z, unityQ.w);
}

/** Caches converted resources per bundle so repeated instantiation is cheap. */
export class ThreeConverter {
  private db: AssetDB;
  private meshCache = new Map<number, THREE.BufferGeometry | null>();
  private materialCache = new Map<number, THREE.Material>();
  private textureCache = new Map<number, THREE.Texture | null>();
  private prefabCache = new Map<number, THREE.Object3D | null>();

  constructor(db: AssetDB) {
    this.db = db;
  }

  geometry(meshPathID: number): THREE.BufferGeometry | null {
    if (this.meshCache.has(meshPathID)) return this.meshCache.get(meshPathID)!;
    let geo: THREE.BufferGeometry | null = null;
    try {
      const mesh = parseMesh(this.db, meshPathID);
      if (mesh) {
        geo = new THREE.BufferGeometry();
        geo.name = mesh.name;
        // mirror z
        const pos = mesh.positions.slice();
        for (let i = 2; i < pos.length; i += 3) pos[i] = -pos[i];
        geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
        if (mesh.normals) {
          const nrm = mesh.normals.slice();
          for (let i = 2; i < nrm.length; i += 3) nrm[i] = -nrm[i];
          geo.setAttribute('normal', new THREE.BufferAttribute(nrm, 3));
        }
        if (mesh.uvs) geo.setAttribute('uv', new THREE.BufferAttribute(mesh.uvs.slice(), 2));
        if (mesh.colors) geo.setAttribute('color', new THREE.BufferAttribute(mesh.colors.slice(), 4));
        geo.setIndex(new THREE.BufferAttribute(mesh.indices, 1));
        if (mesh.subMeshes.length > 1) {
          for (let i = 0; i < mesh.subMeshes.length; i++) {
            geo.addGroup(mesh.subMeshes[i].start, mesh.subMeshes[i].count, i);
          }
        }
        if (!mesh.normals) geo.computeVertexNormals();
      }
    } catch (e) {
      console.warn(`mesh ${meshPathID} conversion failed`, e);
    }
    this.meshCache.set(meshPathID, geo);
    return geo;
  }

  texture(texPathID: number): THREE.Texture | null {
    if (this.textureCache.has(texPathID)) return this.textureCache.get(texPathID)!;
    let tex: THREE.Texture | null = null;
    try {
      const decoded = decodeTexture(this.db, texPathID);
      if (decoded) {
        const dt = new THREE.DataTexture(
          new Uint8Array(decoded.rgba),
          decoded.width,
          decoded.height,
          THREE.RGBAFormat
        );
        // Unity image data is bottom-up; GL v=0 is the first row, so no flip.
        dt.flipY = false;
        dt.wrapS = THREE.RepeatWrapping;
        dt.wrapT = THREE.RepeatWrapping;
        dt.magFilter = THREE.LinearFilter;
        dt.minFilter = THREE.LinearMipmapLinearFilter;
        dt.generateMipmaps = true;
        dt.colorSpace = THREE.SRGBColorSpace;
        dt.needsUpdate = true;
        dt.name = decoded.name;
        tex = dt;
      }
    } catch (e) {
      console.warn(`texture ${texPathID} conversion failed`, e);
    }
    this.textureCache.set(texPathID, tex);
    return tex;
  }

  material(matPathID: number): THREE.Material {
    const cached = this.materialCache.get(matPathID);
    if (cached) return cached;
    let result: THREE.Material;
    const parsed = parseMaterial(this.db, matPathID);
    if (!parsed) {
      result = defaultMaterial();
    } else {
      const mat = new THREE.MeshStandardMaterial({
        name: parsed.name,
        color: new THREE.Color(parsed.color[0], parsed.color[1], parsed.color[2]),
        roughness: 1 - (parsed.floats['_Smoothness'] ?? parsed.floats['_Glossiness'] ?? 0.5),
        metalness: parsed.floats['_Metallic'] ?? 0,
        side: THREE.DoubleSide,
      });
      if (parsed.mainTexPathID) {
        const tex = this.texture(parsed.mainTexPathID);
        if (tex) mat.map = tex;
      }
      if (parsed.transparent) {
        mat.transparent = true;
        mat.opacity = Math.max(parsed.color[3], 0.05);
        mat.depthWrite = false;
      }
      // unlit-ish shaders: give them some emissive so they don't render black
      if (/unlit|additive|particle|sprite/i.test(parsed.shaderName)) {
        mat.emissive = new THREE.Color(parsed.color[0], parsed.color[1], parsed.color[2]);
        mat.emissiveIntensity = 0.8;
        if (mat.map) mat.emissiveMap = mat.map;
      }
      result = mat;
    }
    this.materialCache.set(matPathID, result);
    return result;
  }

  /** Instantiate a prefab (by container pathID of its root GameObject). */
  prefab(goPathID: number): THREE.Object3D | null {
    if (this.prefabCache.has(goPathID)) {
      const cached = this.prefabCache.get(goPathID);
      return cached ? cloneObject(cached) : null;
    }
    const tree = buildGameObjectTree(this.db, goPathID);
    const obj = tree ? this.buildObject(tree) : null;
    this.prefabCache.set(goPathID, obj);
    return obj ? cloneObject(obj) : null;
  }

  /** Find a container asset path's pathID and instantiate if it's a GameObject. */
  prefabByPath(assetPath: string): THREE.Object3D | null {
    const norm = assetPath.toLowerCase();
    const entry = this.db.containers.find((c) => c.path.toLowerCase() === norm);
    if (!entry) return null;
    if (this.db.classOf(entry.pathID) !== ClassID.GameObject) return null;
    return this.prefab(entry.pathID);
  }

  private buildObject(node: UGameObject): THREE.Object3D {
    let obj: THREE.Object3D;
    if (node.meshPathID) {
      const geo = this.geometry(node.meshPathID);
      if (geo) {
        const mats = node.materialPathIDs.map((id) => this.material(id));
        const material = mats.length === 0 ? defaultMaterial() : mats.length === 1 ? mats[0] : mats;
        obj = new THREE.Mesh(geo, material);
        obj.castShadow = true;
        obj.receiveShadow = true;
      } else {
        obj = new THREE.Object3D();
      }
    } else {
      obj = new THREE.Object3D();
    }
    obj.name = node.name;
    obj.position.copy(convertPosition(node.transform.position));
    obj.quaternion.copy(convertQuaternion(node.transform.rotation));
    obj.scale.set(node.transform.scale.x, node.transform.scale.y, node.transform.scale.z);
    obj.visible = node.active;
    obj.userData.unityPathID = node.pathID;
    if (node.componentClasses.includes(ClassID.ParticleSystem)) {
      obj.userData.hasParticles = true;
    }
    for (const child of node.children) {
      obj.add(this.buildObject(child));
    }
    return obj;
  }
}

function defaultMaterial(): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({ color: 0x9b6ad6, roughness: 0.7, side: THREE.DoubleSide });
}

/** Clone that shares geometries/materials (instances stay cheap). */
function cloneObject(src: THREE.Object3D): THREE.Object3D {
  return src.clone(true);
}
