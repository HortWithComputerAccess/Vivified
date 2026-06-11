/**
 * Animation engine + map parsing smoke test against the reference map.
 * Run: npm run test:map
 */
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { TrackEngine } from '../src/anim/tracks';
import { BpmMap } from '../src/map/bpm';
import { V3Beatmap } from '../src/map/types';

const MAP_DIR = join(
  process.cwd(),
  'Reference-Do-Not-Include',
  'Beatsaber Map That Uses Vivify for reference'
);

let failures = 0;
function check(cond: boolean, msg: string): void {
  if (cond) console.log(`  ok: ${msg}`);
  else {
    failures++;
    console.error(`  FAIL: ${msg}`);
  }
}

const beatmap = JSON.parse(
  readFileSync(join(MAP_DIR, 'ExpertStandard.dat'), 'utf-8')
) as V3Beatmap;
const events = beatmap.customData?.customEvents ?? [];
console.log(`events: ${events.length}`);
check(events.length > 2000, 'custom events parsed');

const engine = TrackEngine.fromBeatmap(beatmap);
check(engine.instances.length === 6, `6 InstantiatePrefab instances (got ${engine.instances.length})`);
console.log('  instances:', engine.instances.map((i) => `${i.id}@${i.spawnBeat}${i.destroyBeat !== null ? `→${i.destroyBeat}` : ''}`).join(', '));

// destroy linkage: prefab_intro_2 destroyed at beat 77
const intro = engine.instances.find((i) => i.id === 'prefab_intro_2');
check(!!intro, 'prefab_intro_2 exists');
check(intro?.destroyBeat === 77, `prefab_intro_2 destroyBeat=77 (got ${intro?.destroyBeat})`);
check(engine.activeInstances(10).some((i) => i.id === 'prefab_intro_2'), 'active at beat 10');
check(!engine.activeInstances(80).some((i) => i.id === 'prefab_intro_2'), 'gone at beat 80');

// track evaluation: pauseTrack scale set to [0,0,0] at beat 0
const st = engine.evaluate('pauseTrack', 1);
check(
  !!st.scale && st.scale[0] === 0 && st.scale[1] === 0 && st.scale[2] === 0,
  `pauseTrack scale [0,0,0] at beat 1 (got ${JSON.stringify(st.scale)})`
);

// evaluate every track at a few beats; should not throw and produce finite numbers
const names = engine.trackNames();
console.log(`tracks: ${names.length}`);
let evals = 0;
let badValues = 0;
for (const t of names) {
  for (const b of [0, 8, 32, 64, 128, 256]) {
    const s = engine.evaluate(t, b);
    evals++;
    for (const v of [s.position, s.localPosition, s.rotation, s.localRotation, s.scale]) {
      if (v && v.some((x) => !isFinite(x))) badValues++;
    }
    if (s.dissolve !== null && !isFinite(s.dissolve)) badValues++;
  }
}
console.log(`  evaluated ${evals} track states`);
check(badValues === 0, `all values finite (${badValues} bad)`);

// bpm map
const bpm = new BpmMap(145, beatmap.bpmEvents ?? []);
check(Math.abs(bpm.secondsAt(145) - 60) < 1e-6 || (beatmap.bpmEvents ?? []).length > 0, 'bpm mapping sane');
const roundTrip = bpm.beatAt(bpm.secondsAt(123.5));
check(Math.abs(roundTrip - 123.5) < 1e-6, `beat<->seconds round trip (got ${roundTrip})`);

// parent chains resolve without cycles
for (const t of names) {
  engine.parentChain(t);
}
console.log('  parent chains ok');

console.log(failures ? `\n${failures} FAILURES` : '\nALL OK');
process.exit(failures ? 1 : 0);
