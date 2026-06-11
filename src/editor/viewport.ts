import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { TransformControls } from 'three/examples/jsm/controls/TransformControls.js';
import { ThreeConverter, unityEulerToQuaternion } from '../unity/toThree';
import { TrackEngine, PrefabInstance } from '../anim/tracks';
import { ParsedClip, sampleVec, sampleQuat, crc32 } from '../unity/animationClip';
import { sampleParticles, ParticleParams } from '../unity/particles';

interface BoundClip {
  clip: ParsedClip;
  /** song time when this clip starts (prefab spawn time; 0 for previews) */
  startSeconds: number;
  targets: { obj: THREE.Object3D; binding: ParsedClip['bindings'][number] }[];
}

export type GizmoMode = 'translate' | 'rotate' | 'scale';

interface InstanceNode {
  instance: PrefabInstance;
  object: THREE.Object3D;
  /** wrapper that receives track/parent transforms */
  root: THREE.Group;
  missing: boolean;
}

/**
 * The 3D scene. Renders the Beat Saber player space (grid at origin, track
 * lane) plus all live prefab instances at the current beat.
 */
export class Viewport {
  scene = new THREE.Scene();
  camera: THREE.PerspectiveCamera;
  renderer: THREE.WebGLRenderer;
  controls: OrbitControls;
  gizmo: TransformControls;
  private instanceNodes: InstanceNode[] = [];
  private instancesGroup = new THREE.Group();
  private previewGroup = new THREE.Group();
  private raycaster = new THREE.Raycaster();
  private converter: ThreeConverter | null = null;
  private selectedNode: InstanceNode | null = null;

  onSelectInstance: (inst: PrefabInstance | null) => void = () => {};
  onGizmoChange: (inst: PrefabInstance, pos: [number, number, number], rotEulerDeg: [number, number, number], scale: [number, number, number]) => void = () => {};

  constructor(canvas: HTMLCanvasElement) {
    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
    this.renderer.setPixelRatio(window.devicePixelRatio);
    this.scene.background = new THREE.Color(0x101016);
    // Vivify environments are often hundreds of meters across
    this.scene.fog = new THREE.Fog(0x101016, 1500, 6000);

    this.camera = new THREE.PerspectiveCamera(60, 1, 0.05, 8000);
    this.camera.position.set(5, 4, 7);

    this.controls = new OrbitControls(this.camera, canvas);
    this.controls.target.set(0, 1.2, -8);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.12;

    this.gizmo = new TransformControls(this.camera, canvas);
    this.gizmo.addEventListener('dragging-changed', (e: any) => {
      this.controls.enabled = !e.value;
      if (!e.value) this.emitGizmo(); // commit on release
    });
    this.scene.add(this.gizmo as unknown as THREE.Object3D);

    this.buildEnvironment();
    this.scene.add(this.instancesGroup);
    this.scene.add(this.previewGroup);

    canvas.addEventListener('pointerdown', (e) => this.onPointerDown(e));
    new ResizeObserver(() => this.resize()).observe(canvas.parentElement!);
    this.resize();
  }

  lights!: { hemi: THREE.HemisphereLight; dir: THREE.DirectionalLight; dir2: THREE.DirectionalLight };

  private buildEnvironment(): void {
    const hemi = new THREE.HemisphereLight(0xbfc4ff, 0x202028, 0.9);
    this.scene.add(hemi);
    const dir = new THREE.DirectionalLight(0xffffff, 1.4);
    dir.position.set(6, 12, 6);
    this.scene.add(dir);
    const dir2 = new THREE.DirectionalLight(0x8888ff, 0.5);
    dir2.position.set(-8, 6, -10);
    this.scene.add(dir2);
    this.lights = { hemi, dir, dir2 };

    // floor grid (1m cells)
    const grid = new THREE.GridHelper(60, 60, 0x3a3a48, 0x26262f);
    grid.position.y = 0;
    this.scene.add(grid);

    // Beat Saber note lane: 4 columns x 0.6m, notes travel along -Z (three)
    const laneGeo = new THREE.PlaneGeometry(0.6 * 4, 40);
    const laneMat = new THREE.MeshBasicMaterial({ color: 0x9b6ad6, transparent: true, opacity: 0.07, side: THREE.DoubleSide });
    const lane = new THREE.Mesh(laneGeo, laneMat);
    lane.rotation.x = -Math.PI / 2;
    lane.position.set(0, 0.005, -20);
    this.scene.add(lane);

    const laneEdge = new THREE.LineSegments(
      new THREE.EdgesGeometry(laneGeo),
      new THREE.LineBasicMaterial({ color: 0x9b6ad6, transparent: true, opacity: 0.35 })
    );
    laneEdge.rotation.x = -Math.PI / 2;
    laneEdge.position.copy(lane.position);
    this.scene.add(laneEdge);

    // player marker at origin
    const player = new THREE.Mesh(
      new THREE.ConeGeometry(0.18, 0.5, 4),
      new THREE.MeshBasicMaterial({ color: 0x5ad6c0, wireframe: true })
    );
    player.position.set(0, 1.7, 0);
    player.rotation.x = Math.PI;
    this.scene.add(player);

    const axes = new THREE.AxesHelper(1.2);
    axes.position.set(0, 0.01, 0);
    this.scene.add(axes);
  }

  setConverter(converter: ThreeConverter | null): void {
    this.converter = converter;
    // existing instances must be rebuilt with the new bundle
    this.rebuildPending = true;
  }

  setGizmoMode(mode: GizmoMode): void {
    this.gizmo.setMode(mode);
  }

  /** Preview a prefab at origin without creating an event. */
  showPreview(assetPath: string | null): void {
    this.previewGroup.clear();
    if (assetPath && this.converter) {
      const obj = this.converter.prefabByPath(assetPath);
      if (obj) this.previewGroup.add(obj);
    }
    this.collectAnimated();
  }

  // --- unity clip + particle playback ---------------------------------------
  private boundClips: BoundClip[] = [];
  private particleTargets: THREE.Points[] = [];
  /** set by the app so section clips start at their prefab's spawn time */
  beatToSeconds: (beat: number) => number = () => 0;

  /** Gather AnimationClip roots and particle systems after scene changes. */
  private collectAnimated(): void {
    this.boundClips = [];
    this.particleTargets = [];
    const scan = (container: THREE.Object3D, startSeconds = 0) => {
      container.traverse((o) => {
        const clip = o.userData.animClip as ParsedClip | undefined;
        if (clip) {
          // release clips bind by CRC32 of the transform path relative to the
          // Animator object; hash every descendant path to match
          const basePath = (o.userData.uPath as string) ?? '';
          const byHash = new Map<number, THREE.Object3D>();
          byHash.set(0, o);
          o.traverse((d) => {
            const p = d.userData.uPath as string | undefined;
            if (typeof p !== 'string' || d === o) return;
            const rel = basePath && p.startsWith(basePath + '/') ? p.slice(basePath.length + 1) : p;
            byHash.set(crc32(rel), d);
          });
          const targets: BoundClip['targets'] = [];
          for (const binding of clip.bindings) {
            let target: THREE.Object3D | null = null;
            if (binding.pathHash !== null) {
              target = byHash.get(binding.pathHash) ?? null;
            } else {
              target = binding.path === '' ? o : findByPath(o, binding.path);
            }
            if (target) targets.push({ obj: target, binding });
          }
          if (targets.length) this.boundClips.push({ clip, startSeconds, targets });
        }
        if ((o as THREE.Points).isPoints && o.userData.particle) {
          this.particleTargets.push(o as THREE.Points);
        }
      });
    };
    for (const node of this.instanceNodes) {
      scan(node.root, this.beatToSeconds(node.instance.spawnBeat));
    }
    scan(this.previewGroup, 0);
  }

  private applyClips(timeSeconds: number): void {
    for (const { clip, startSeconds, targets } of this.boundClips) {
      const local = Math.max(timeSeconds - startSeconds, 0);
      const t = clip.loop || clip.length <= 0 ? ((local % clip.length) + clip.length) % clip.length : Math.min(local, clip.length);
      for (const { obj, binding } of targets) {
        if (binding.position) {
          const v = sampleVec(binding.position, t);
          obj.position.set(v[0], v[1], -v[2]);
        }
        if (binding.rotation) {
          const q = sampleQuat(binding.rotation, t);
          obj.quaternion.set(-q[0], -q[1], q[2], q[3]);
        } else if (binding.euler) {
          const e = sampleVec(binding.euler, t);
          obj.quaternion.copy(unityEulerToQuaternion(e[0], e[1], e[2]));
        }
        if (binding.scale) {
          const s = sampleVec(binding.scale, t);
          obj.scale.set(s[0], s[1], s[2]);
        }
      }
    }
  }

  private applyParticles(timeSeconds: number): void {
    for (const points of this.particleTargets) {
      const params = points.userData.particle as ParticleParams;
      const cap = points.userData.particleCap as number;
      const attr = points.geometry.getAttribute('position') as THREE.BufferAttribute;
      const alphas = (points.userData.alphaScratch as Float32Array) ?? new Float32Array(cap);
      points.userData.alphaScratch = alphas;
      sampleParticles(params, timeSeconds, attr.array as Float32Array, alphas, cap);
      const arr = attr.array as Float32Array;
      for (let i = 2; i < arr.length; i += 3) arr[i] = -arr[i];
      attr.needsUpdate = true;
    }
  }

  private rebuildPending = false;
  private lastInstanceKey = '';

  /** Sync scene objects with engine state at the given beat. */
  update(engine: TrackEngine | null, beat: number, timeSeconds = 0): void {
    this.applyClips(timeSeconds);
    this.applyParticles(timeSeconds);
    if (!engine) {
      if (this.instanceNodes.length) {
        this.instancesGroup.clear();
        this.instanceNodes = [];
      }
      return;
    }
    const active = engine.activeInstances(beat);
    const key = active.map((i) => i.id + '|' + i.asset).join(';');
    if (key !== this.lastInstanceKey || this.rebuildPending) {
      this.rebuildInstances(active);
      this.lastInstanceKey = key;
      this.rebuildPending = false;
    } else {
      // engine rebuilds create fresh PrefabInstance objects; re-sync refs so
      // spawn-transform edits take effect without a scene rebuild
      for (let i = 0; i < active.length; i++) {
        this.instanceNodes[i].instance = active[i];
      }
    }

    // apply transforms
    for (const node of this.instanceNodes) {
      if (this.gizmo.dragging && node === this.selectedNode) continue;
      this.applyTransform(node, engine, beat);
    }
  }

  private rebuildInstances(active: PrefabInstance[]): void {
    const selectedId = this.selectedNode?.instance.id ?? null;
    this.gizmo.detach();
    this.instancesGroup.clear();
    this.instanceNodes = [];
    this.selectedNode = null;

    for (const inst of active) {
      const root = new THREE.Group();
      root.name = `inst:${inst.id}`;
      let object: THREE.Object3D | null = null;
      let missing = false;
      if (this.converter) {
        object = this.converter.prefabByPath(inst.asset);
      }
      if (!object) {
        missing = true;
        object = makeMissingPlaceholder(inst.asset);
      }
      root.add(object);
      root.userData.instanceId = inst.id;
      this.instancesGroup.add(root);
      const node: InstanceNode = { instance: inst, object, root, missing };
      this.instanceNodes.push(node);
      if (inst.id === selectedId) {
        this.selectedNode = node;
        this.gizmo.attach(root);
      }
    }
    this.collectAnimated();
  }

  private applyTransform(node: InstanceNode, engine: TrackEngine, beat: number): void {
    const inst = node.instance;
    // base: spawn transform
    let pos = inst.spawnPosition;
    let rot = inst.spawnRotation;
    let scl = inst.spawnScale;
    let dissolve: number | null = null;

    // track override (first track wins, channel-wise)
    for (const track of inst.tracks) {
      const st = engine.evaluate(track, beat);
      const p = st.localPosition ?? st.position;
      const r = st.localRotation ?? st.rotation;
      if (p) pos = p;
      if (r) rot = r;
      if (st.scale) scl = st.scale;
      if (st.dissolve !== null) dissolve = st.dissolve;
      if (p || r || st.scale || st.dissolve !== null) break;
    }

    node.root.position.set(pos[0], pos[1], -pos[2]);
    node.root.quaternion.copy(unityEulerToQuaternion(rot[0], rot[1], rot[2]));
    node.root.scale.set(scl[0], scl[1], scl[2]);

    // parent chain from AssignTrackParent (applied as world-space pre-transform)
    const firstTrack = inst.tracks[0];
    if (firstTrack) {
      const chain = engine.parentChain(firstTrack);
      if (chain.length) {
        const m = new THREE.Matrix4();
        for (let i = chain.length - 1; i >= 0; i--) {
          const ps = engine.evaluate(chain[i], beat);
          const pp = ps.localPosition ?? ps.position ?? [0, 0, 0];
          const pr = ps.localRotation ?? ps.rotation ?? [0, 0, 0];
          const psc = ps.scale ?? [1, 1, 1];
          const pm = new THREE.Matrix4().compose(
            new THREE.Vector3(pp[0], pp[1], -pp[2]),
            unityEulerToQuaternion(pr[0], pr[1], pr[2]),
            new THREE.Vector3(psc[0], psc[1], psc[2])
          );
          m.multiply(pm);
        }
        const own = new THREE.Matrix4().compose(node.root.position, node.root.quaternion, node.root.scale);
        m.multiply(own);
        m.decompose(node.root.position, node.root.quaternion, node.root.scale);
      }
    }

    if (dissolve !== null) {
      setOpacity(node.object, Math.min(Math.max(dissolve, 0), 1));
    }
  }

  /** Frame the selected instance (or the whole scene content) in view. */
  focusSelected(): void {
    const target = this.selectedNode?.root ?? (this.instancesGroup.children.length ? this.instancesGroup : null);
    if (!target) return;
    const box = new THREE.Box3().setFromObject(target);
    if (!isFinite(box.min.x) || box.isEmpty()) return;
    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3()).length();
    const dist = Math.min(Math.max(size * 0.4, 2), 600);
    const dir = this.camera.position.clone().sub(this.controls.target).normalize();
    this.controls.target.copy(center);
    this.camera.position.copy(center).add(dir.multiplyScalar(dist));
  }

  selectInstanceById(id: string | null): void {
    const node = id ? this.instanceNodes.find((n) => n.instance.id === id) ?? null : null;
    this.selectedNode = node;
    if (node) this.gizmo.attach(node.root);
    else this.gizmo.detach();
  }

  private onPointerDown(e: PointerEvent): void {
    if (e.button !== 0 || this.gizmo.dragging) return;
    const rect = (e.target as HTMLCanvasElement).getBoundingClientRect();
    const ndc = new THREE.Vector2(
      ((e.clientX - rect.left) / rect.width) * 2 - 1,
      -((e.clientY - rect.top) / rect.height) * 2 + 1
    );
    this.raycaster.setFromCamera(ndc, this.camera);
    const hits = this.raycaster.intersectObjects(this.instancesGroup.children, true);
    if (hits.length === 0) return;
    // find owning instance root
    let obj: THREE.Object3D | null = hits[0].object;
    while (obj && !obj.userData.instanceId) obj = obj.parent;
    if (!obj) return;
    const node = this.instanceNodes.find((n) => n.instance.id === obj!.userData.instanceId);
    if (!node) return;
    this.selectedNode = node;
    this.gizmo.attach(node.root);
    this.onSelectInstance(node.instance);
  }

  /** Selected object's pose in unity space (position, euler deg, scale). */
  getSelectedPoseUnity(): {
    position: [number, number, number];
    rotation: [number, number, number];
    scale: [number, number, number];
  } | null {
    const node = this.selectedNode;
    if (!node) return null;
    const p = node.root.position;
    const q = node.root.quaternion;
    const s = node.root.scale;
    // three -> unity: pos (x,y,-z); quaternion (-x,-y,z,w); euler from unity quat
    const uq = new THREE.Quaternion(-q.x, -q.y, q.z, q.w);
    const eul = unityQuatToEuler(uq);
    return {
      position: [round3(p.x), round3(p.y), round3(-p.z)],
      rotation: [round3(eul[0]), round3(eul[1]), round3(eul[2])],
      scale: [round3(s.x), round3(s.y), round3(s.z)],
    };
  }

  private emitGizmo(): void {
    const node = this.selectedNode;
    const pose = this.getSelectedPoseUnity();
    if (!node || !pose) return;
    this.onGizmoChange(node.instance, pose.position, pose.rotation, pose.scale);
  }

  // --- player POV mode ---
  povMode = false;
  private savedCamPos: THREE.Vector3 | null = null;
  private savedCamTarget: THREE.Vector3 | null = null;

  setPov(on: boolean): void {
    if (on === this.povMode) return;
    this.povMode = on;
    if (on) {
      this.savedCamPos = this.camera.position.clone();
      this.savedCamTarget = this.controls.target.clone();
      this.controls.enabled = false;
      this.gizmo.detach();
    } else {
      this.controls.enabled = true;
      if (this.savedCamPos && this.savedCamTarget) {
        this.camera.position.copy(this.savedCamPos);
        this.controls.target.copy(this.savedCamTarget);
      }
    }
  }

  /** Per-frame POV pose from player track animation (unity space). */
  applyPovPose(pos: [number, number, number] | null, rotEulerDeg: [number, number, number] | null): void {
    if (!this.povMode) return;
    const base = new THREE.Vector3(0, 1.7, 0);
    if (pos) base.add(new THREE.Vector3(pos[0], pos[1], -pos[2]));
    this.camera.position.copy(base);
    const q = rotEulerDeg
      ? unityEulerToQuaternion(rotEulerDeg[0], rotEulerDeg[1], rotEulerDeg[2])
      : new THREE.Quaternion();
    // look along unity forward (+Z -> three -Z)
    const look = new THREE.Quaternion().setFromEuler(new THREE.Euler(0, Math.PI, 0));
    this.camera.quaternion.copy(q).multiply(look);
  }

  resize(): void {
    const parent = this.renderer.domElement.parentElement;
    if (!parent) return;
    const w = parent.clientWidth || 1;
    const h = parent.clientHeight || 1;
    this.renderer.setSize(w, h, false);
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
  }

  render(): void {
    if (!this.povMode) this.controls.update();
    this.renderer.render(this.scene, this.camera);
  }
}

/** Unity quaternion -> unity euler (degrees), Unity order (Y-X-Z application). */
function unityQuatToEuler(q: THREE.Quaternion): [number, number, number] {
  // Unity: q = qy * qx * qz (left-handed). Solve for x,y,z.
  const { x, y, z, w } = q;
  const r = 180 / Math.PI;
  // from quaternion to euler ZXY (unity convention)
  const sinX = 2 * (w * x - y * z);
  const X = Math.abs(sinX) >= 1 ? (Math.PI / 2) * Math.sign(sinX) : Math.asin(sinX);
  let Y: number;
  let Z: number;
  if (Math.abs(sinX) < 0.9999) {
    Y = Math.atan2(2 * (w * y + x * z), 1 - 2 * (x * x + y * y));
    Z = Math.atan2(2 * (w * z + x * y), 1 - 2 * (x * x + z * z));
  } else {
    Y = Math.atan2(2 * (w * y - x * z), 1 - 2 * (y * y + x * x));
    Z = 0;
  }
  return [X * r, Y * r, Z * r];
}

function setOpacity(obj: THREE.Object3D, opacity: number): void {
  obj.traverse((o) => {
    const mesh = o as THREE.Mesh;
    if (!mesh.isMesh) return;
    const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
    for (const m of mats) {
      m.transparent = opacity < 0.999 || m.transparent;
      m.opacity = opacity;
    }
  });
}

function makeMissingPlaceholder(asset: string): THREE.Object3D {
  const group = new THREE.Group();
  const box = new THREE.Mesh(
    new THREE.BoxGeometry(0.5, 0.5, 0.5),
    new THREE.MeshStandardMaterial({ color: 0xd65a6a, wireframe: true })
  );
  box.name = `missing:${asset}`;
  group.add(box);
  return group;
}

function round3(v: number): number {
  return Math.round(v * 1000) / 1000;
}

/** Resolve a unity transform path ("A/B/C") below a root object. */
function findByPath(root: THREE.Object3D, path: string): THREE.Object3D | null {
  let cur: THREE.Object3D | null = root;
  for (const part of path.split('/')) {
    if (!cur) return null;
    cur = cur.children.find((c) => c.name === part) ?? null;
  }
  return cur;
}
