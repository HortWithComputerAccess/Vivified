import * as THREE from 'three';
import { AssetDB, UGameObject, buildGameObjectTree, ClassID } from './assets';
import { parseMesh } from './mesh';
import { parseMaterial, ParsedMaterial } from './material';
import { decodeTexture } from './texture';
import { parseShader, resolveBlend, ShaderInfo } from './shader';
import { clipForController } from './animationClip';
import { parseParticleSystem, sampleParticles } from './particles';

const PARTICLE_CAP = 120; // per system
const UNLIT_NAME = /unlit|vfx|sky|blit|ray|flare|glow|lattice|rainbow|grid|wisp|star|arrow|fog|text/i;

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
  private materialInfoCache = new Map<number, { parsed: ParsedMaterial; shader: ShaderInfo | null } | null>();
  private textureCache = new Map<number, THREE.Texture | null>();
  private prefabCache = new Map<number, THREE.Object3D | null>();

  /** Find the three material instance for an asset path (e.g. "...mat"). */
  materialByPath(assetPath: string): { material: THREE.Material; parsed: ParsedMaterial; shader: ShaderInfo | null } | null {
    const entry = this.db.containers.find((c) => c.path.toLowerCase() === assetPath.toLowerCase());
    if (!entry || this.db.classOf(entry.pathID) !== ClassID.Material) return null;
    const info = this.materialInfo(entry.pathID);
    if (!info) return null;
    return { material: this.material(entry.pathID), parsed: info.parsed, shader: info.shader };
  }

  texturePathIDByPath(assetPath: string): number | null {
    const entry = this.db.containers.find((c) => c.path.toLowerCase() === assetPath.toLowerCase());
    if (!entry || this.db.classOf(entry.pathID) !== ClassID.Texture2D) return null;
    return entry.pathID;
  }

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

  /** Parsed material + recreated shader info, cached (for panels/animation). */
  materialInfo(matPathID: number): { parsed: ParsedMaterial; shader: ShaderInfo | null } | null {
    const cached = this.materialInfoCache.get(matPathID);
    if (cached !== undefined) return cached;
    const parsed = parseMaterial(this.db, matPathID);
    const info = parsed
      ? { parsed, shader: parsed.shaderPathID ? parseShader(this.db, parsed.shaderPathID) : null }
      : null;
    this.materialInfoCache.set(matPathID, info);
    return info;
  }

  material(matPathID: number): THREE.Material {
    const cached = this.materialCache.get(matPathID);
    if (cached) return cached;
    const info = this.materialInfo(matPathID);
    const result = info ? this.buildMaterial(info.parsed, info.shader) : defaultMaterial();
    this.materialCache.set(matPathID, result);
    return result;
  }

  /**
   * Recreate the shader's visual character from its serialized render state:
   * blend mode, depth write, culling, render queue. Unknown/opaque shaders
   * fall back to a standard lit material.
   */
  private buildMaterial(parsed: ParsedMaterial, shader: ShaderInfo | null): THREE.Material {
    const blend = resolveBlend(shader, parsed.floats);
    const color = unityColor(parsed.color);
    const alphaF = parsed.floats['_Alpha'] ?? parsed.floats['_Opacity'];
    const brightness = parsed.floats['_Brightness'] ?? parsed.floats['_Intensity'];
    const side = blend.cull === 0 ? THREE.DoubleSide : blend.cull === 1 ? THREE.BackSide : THREE.FrontSide;

    // Unlit detection: fixed-function lighting off, VFX-ish shader names, or
    // HDR colors (glow). These render full-bright instead of going black.
    const hdr = parsed.color[0] > 1.2 || parsed.color[1] > 1.2 || parsed.color[2] > 1.2;
    const isUnlit =
      shader?.lighting === false ||
      UNLIT_NAME.test(parsed.shaderName) ||
      hdr ||
      blend.mode === 'additive' ||
      blend.mode === 'multiply';

    let mat: THREE.Material;
    if (blend.mode === 'additive' || blend.mode === 'multiply') {
      const basic = new THREE.MeshBasicMaterial({
        color,
        side,
        transparent: true,
        depthWrite: false,
        blending: blend.mode === 'additive' ? THREE.AdditiveBlending : THREE.MultiplyBlending,
        opacity: clamp01(parsed.color[3] * (alphaF ?? 1)),
      });
      if (brightness !== undefined && brightness > 0) {
        basic.color.multiplyScalar(Math.min(brightness, 4));
      }
      mat = basic;
    } else if (blend.mode === 'alpha') {
      if (isUnlit) {
        mat = new THREE.MeshBasicMaterial({
          color,
          side,
          transparent: true,
          depthWrite: blend.depthWrite,
          opacity: clamp01(Math.max(parsed.color[3] * (alphaF ?? 1), 0.05)),
        });
      } else {
        mat = new THREE.MeshStandardMaterial({
          color,
          side,
          transparent: true,
          depthWrite: blend.depthWrite,
          opacity: clamp01(Math.max(parsed.color[3] * (alphaF ?? 1), 0.05)),
          roughness: 1 - (parsed.floats['_Smoothness'] ?? parsed.floats['_Glossiness'] ?? 0.5),
          metalness: parsed.floats['_Metallic'] ?? 0,
        });
      }
    } else if (isUnlit) {
      // opaque custom shader that doesn't use scene lighting
      mat = new THREE.MeshBasicMaterial({ color, side });
    } else {
      // opaque: standard lit fallback ("normal shader")
      const std = new THREE.MeshStandardMaterial({
        color,
        side,
        roughness: 1 - (parsed.floats['_Smoothness'] ?? parsed.floats['_Glossiness'] ?? 0.5),
        metalness: parsed.floats['_Metallic'] ?? 0,
      });
      const emissive = parsed.colors['_EmissionColor'];
      if (emissive && (emissive[0] > 0.01 || emissive[1] > 0.01 || emissive[2] > 0.01)) {
        std.emissive = unityColor(emissive);
        std.emissiveIntensity = 0.6;
      } else if (parsed.shaderName && !/standard|universal|lit/i.test(parsed.shaderName)) {
        // unknown custom shader: keep it lit, but never pitch black
        std.emissive = color.clone().multiplyScalar(0.22);
      }
      mat = std;
    }

    mat.name = parsed.name;
    if (parsed.mainTexPathID) {
      const tex = this.texture(parsed.mainTexPathID);
      if (tex) {
        (mat as THREE.MeshBasicMaterial | THREE.MeshStandardMaterial).map = tex;
        if (blend.mode === 'alpha' && (mat as any).alphaTest === 0 && parsed.floats['_Cutoff']) {
          (mat as any).alphaTest = clamp01(parsed.floats['_Cutoff']);
        }
      }
    }
    mat.userData.shaderName = parsed.shaderName;
    mat.userData.blendMode = blend.mode;
    mat.userData.baseColor = parsed.color.slice();
    return mat;
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

  private buildObject(node: UGameObject, path = ''): THREE.Object3D {
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
    obj.userData.uPath = path;

    // Unity AnimationClip playback (Animator -> controller -> first clip)
    if (node.animatorControllerPathID) {
      const clip = clipForController(this.db, node.animatorControllerPathID);
      if (clip) obj.userData.animClip = clip;
    }

    // particle system approximation
    if (node.particleSystemPathID) {
      const points = this.buildParticles(node);
      if (points) obj.add(points);
    }

    for (const child of node.children) {
      const childPath = path ? `${path}/${child.name}` : child.name;
      obj.add(this.buildObject(child, childPath));
    }
    return obj;
  }

  private buildParticles(node: UGameObject): THREE.Points | null {
    try {
      const params = parseParticleSystem(this.db, node.particleSystemPathID!);
      if (!params || params.rate <= 0) return null;
      const cap = Math.min(Math.floor(Math.min(params.rate * params.lifetime, params.maxParticles)), PARTICLE_CAP);
      if (cap <= 0) return null;

      const positions = new Float32Array(cap * 3);
      const alphas = new Float32Array(cap);
      sampleParticles(params, 0, positions, alphas, cap);
      // unity -> three: flip z
      for (let i = 2; i < positions.length; i += 3) positions[i] = -positions[i];

      const geo = new THREE.BufferGeometry();
      geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
      geo.boundingSphere = new THREE.Sphere(new THREE.Vector3(), Math.max(params.shapeRadius + params.speed * params.lifetime, 1) * 2);

      // material from the ParticleSystemRenderer
      let map: THREE.Texture | null = null;
      let blending: THREE.Blending = THREE.AdditiveBlending;
      let color = unityColor(params.startColor);
      if (node.particleMaterialPathIDs.length) {
        const info = this.materialInfo(node.particleMaterialPathIDs[0]);
        if (info) {
          if (info.parsed.mainTexPathID) map = this.texture(info.parsed.mainTexPathID);
          const blend = resolveBlend(info.shader, info.parsed.floats);
          blending = blend.mode === 'additive' ? THREE.AdditiveBlending : THREE.NormalBlending;
          color = color.multiply(unityColor(info.parsed.color));
        }
      }
      const mat = new THREE.PointsMaterial({
        size: Math.min(params.size, 4),
        sizeAttenuation: true,
        map: map ?? undefined,
        color,
        transparent: true,
        opacity: clamp01(params.startColor[3]) * 0.9,
        depthWrite: false,
        blending,
      });
      const points = new THREE.Points(geo, mat);
      points.name = 'particles';
      points.userData.particle = params;
      points.userData.particleCap = cap;
      points.frustumCulled = false;
      return points;
    } catch (e) {
      console.warn('particle conversion failed', e);
      return null;
    }
  }
}

function defaultMaterial(): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({ color: 0x9b6ad6, roughness: 0.7, side: THREE.DoubleSide });
}

function clamp01(v: number): number {
  return Math.min(Math.max(v, 0), 1);
}

/**
 * Unity colors are authored in sRGB; convert into three's linear working
 * space. HDR colors (>1) keep their intensity scale.
 */
export function unityColor(c: [number, number, number, number] | number[]): THREE.Color {
  const r = c[0] ?? 1;
  const g = c[1] ?? 1;
  const b = c[2] ?? 1;
  const max = Math.max(r, g, b, 1);
  const col = new THREE.Color();
  col.setRGB(clamp01(r / max), clamp01(g / max), clamp01(b / max), THREE.SRGBColorSpace);
  if (max > 1) col.multiplyScalar(Math.min(max, 4));
  return col;
}

/** Clone that shares geometries/materials (instances stay cheap). */
function cloneObject(src: THREE.Object3D): THREE.Object3D {
  return src.clone(true);
}
