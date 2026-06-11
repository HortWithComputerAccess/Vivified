import { AssetDB } from './assets';

/**
 * Approximated Unity ParticleSystem: a deterministic, stateless particle
 * cloud derived from the system's main/emission/shape modules. Not a real
 * simulation — but it puts the right *kind* of glow in the right place,
 * with the right material, color, size and motion.
 */
export interface ParticleParams {
  maxParticles: number;
  rate: number; // particles per second
  lifetime: number; // seconds
  speed: number;
  size: number;
  size3D: boolean;
  startColor: [number, number, number, number];
  gravity: number;
  looping: boolean;
  shape: 'sphere' | 'hemisphere' | 'cone' | 'box' | 'circle' | 'edge' | 'point';
  shapeRadius: number;
  shapeAngle: number; // cone angle degrees
  shapeScale: [number, number, number];
  worldSpace: boolean;
  duration: number;
}

export function parseParticleSystem(db: AssetDB, psPathID: number): ParticleParams | null {
  const ps = db.read(psPathID) as any;
  if (!ps) return null;

  const scalar = (mmc: any, def: number): number => {
    if (!mmc) return def;
    const v = Number(mmc.scalar ?? def);
    return isFinite(v) ? v : def;
  };
  const init = ps.InitialModule ?? {};
  const emission = ps.EmissionModule ?? {};
  const shapeM = ps.ShapeModule ?? {};

  const lifetime = Math.max(scalar(init.startLifetime, 5), 0.05);
  const rate = emission.enabled === false ? 0 : Math.max(scalar(emission.rateOverTime, 10), 0);
  const maxParticles = Math.max(Number(init.maxNumParticles ?? 100), 1);

  const grad = init.startColor ?? {};
  const colObj = grad.maxColor ?? grad.minColor ?? { r: 1, g: 1, b: 1, a: 1 };
  const startColor: [number, number, number, number] = [
    num(colObj.r, 1),
    num(colObj.g, 1),
    num(colObj.b, 1),
    num(colObj.a, 1),
  ];

  // Unity ShapeModule type enum (subset)
  const shapeType = Number(shapeM.type ?? 0);
  const shape: ParticleParams['shape'] =
    shapeM.enabled === false
      ? 'point'
      : shapeType === 0
        ? 'sphere'
        : shapeType === 1
          ? 'hemisphere'
          : shapeType === 2
            ? 'cone'
            : shapeType === 5
              ? 'box'
              : shapeType === 10
                ? 'circle'
                : shapeType === 12
                  ? 'edge'
                  : 'sphere';

  const radiusParam = shapeM.radius;
  const shapeRadius = Math.max(
    typeof radiusParam === 'object' ? num(radiusParam?.value, 1) : num(radiusParam, 1),
    0.01
  );
  const sc = shapeM.m_Scale ?? { x: 1, y: 1, z: 1 };

  return {
    maxParticles,
    rate,
    lifetime,
    speed: scalar(init.startSpeed, 1),
    size: Math.max(scalar(init.startSize, 0.2), 0.01),
    size3D: init.size3D === true,
    startColor,
    gravity: scalar(init.gravityModifier, 0) * -9.81,
    looping: ps.looping !== false,
    shape,
    shapeRadius,
    shapeAngle: num(shapeM.angle, 25),
    shapeScale: [num(sc.x, 1), num(sc.y, 1), num(sc.z, 1)],
    worldSpace: false,
    duration: Math.max(num(ps.lengthInSec, 5), 0.1),
  };
}

/**
 * Deterministic particle state at time t: fills `positions` (xyz triplets,
 * unity space — caller flips z) and `alphas`. Returns particle count.
 */
export function sampleParticles(
  p: ParticleParams,
  t: number,
  positions: Float32Array,
  alphas: Float32Array,
  cap: number
): number {
  const count = Math.min(Math.floor(Math.min(p.rate * p.lifetime, p.maxParticles)), cap);
  if (count <= 0) return 0;
  for (let i = 0; i < count; i++) {
    // stable per-particle randoms
    const r1 = hash(i * 4 + 1);
    const r2 = hash(i * 4 + 2);
    const r3 = hash(i * 4 + 3);
    const r4 = hash(i * 4 + 4);

    // staggered births so the cloud is steady-state when looping
    const phase = (t / p.lifetime + i / count) % 1;
    const age = phase * p.lifetime;

    // spawn position + direction from the emitter shape
    let px = 0, py = 0, pz = 0;
    let dx = 0, dy = 0, dz = 0;
    const theta = r1 * Math.PI * 2;
    const u = r2 * 2 - 1;
    switch (p.shape) {
      case 'sphere': {
        const s = Math.sqrt(1 - u * u);
        dx = s * Math.cos(theta);
        dy = s * Math.sin(theta);
        dz = u;
        const rr = p.shapeRadius * Math.cbrt(r3);
        px = dx * rr; py = dy * rr; pz = dz * rr;
        break;
      }
      case 'hemisphere': {
        const s = Math.sqrt(1 - u * u);
        dx = s * Math.cos(theta);
        dy = s * Math.sin(theta);
        dz = Math.abs(u);
        const rr = p.shapeRadius * Math.cbrt(r3);
        px = dx * rr; py = dy * rr; pz = dz * rr;
        break;
      }
      case 'cone': {
        const ang = (p.shapeAngle * Math.PI) / 180;
        const rr = p.shapeRadius * Math.sqrt(r3);
        px = Math.cos(theta) * rr;
        py = Math.sin(theta) * rr;
        pz = 0;
        const spread = Math.tan(ang) * (rr / Math.max(p.shapeRadius, 1e-4));
        dx = Math.cos(theta) * spread;
        dy = Math.sin(theta) * spread;
        dz = 1;
        const dl = Math.hypot(dx, dy, dz) || 1;
        dx /= dl; dy /= dl; dz /= dl;
        break;
      }
      case 'box': {
        px = (r1 - 0.5) * p.shapeScale[0];
        py = (r2 - 0.5) * p.shapeScale[1];
        pz = (r3 - 0.5) * p.shapeScale[2];
        dz = 1;
        break;
      }
      case 'circle': {
        const rr = p.shapeRadius * Math.sqrt(r3);
        px = Math.cos(theta) * rr;
        py = Math.sin(theta) * rr;
        dx = Math.cos(theta);
        dy = Math.sin(theta);
        break;
      }
      case 'edge': {
        px = (r1 - 0.5) * 2 * p.shapeRadius;
        dy = 1;
        break;
      }
      default: {
        const s = Math.sqrt(1 - u * u);
        dx = s * Math.cos(theta);
        dy = s * Math.sin(theta);
        dz = u;
        break;
      }
    }

    const speed = p.speed * (0.7 + 0.6 * r4);
    positions[i * 3] = px + dx * speed * age;
    positions[i * 3 + 1] = py + dy * speed * age + 0.5 * p.gravity * age * age;
    positions[i * 3 + 2] = pz + dz * speed * age;
    // fade in fast, fade out toward end of life
    alphas[i] = Math.min(phase * 8, 1) * (1 - phase * phase);
  }
  return count;
}

function hash(n: number): number {
  let x = Math.sin(n * 127.1 + 311.7) * 43758.5453;
  return x - Math.floor(x);
}

function num(v: any, def: number): number {
  return typeof v === 'number' && isFinite(v) ? v : def;
}
