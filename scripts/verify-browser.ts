/**
 * End-to-end browser verification: starts a vite dev server, loads the editor,
 * feeds it the reference bundle + map via test hooks, screenshots the result.
 * Run: npx tsx scripts/verify-browser.ts
 */
import { chromium } from 'playwright';
import { createServer } from 'vite';
import { readFileSync, mkdirSync } from 'node:fs';
import { join } from 'node:path';

const MAP_DIR = join(
  process.cwd(),
  'Reference-Do-Not-Include',
  'Beatsaber Map That Uses Vivify for reference'
);
const OUT_DIR = join(process.cwd(), '.verify');

async function main(): Promise<void> {
  mkdirSync(OUT_DIR, { recursive: true });
  const server = await createServer({ server: { port: 5199, strictPort: true } });
  await server.listen();
  console.log('vite dev server on :5199');

  const browser = await chromium.launch({
    args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
  });
  const page = await browser.newPage({ viewport: { width: 1600, height: 900 } });

  const errors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });
  page.on('pageerror', (err) => errors.push(String(err)));

  // tsx/esbuild keepNames injects __name calls into serialized functions
  await page.addInitScript(() => {
    (window as any).__name = (fn: unknown) => fn;
  });

  await page.goto('http://localhost:5199/');
  await page.waitForFunction(() => (window as any).__vivified !== undefined, { timeout: 20000 });
  console.log('editor loaded');
  await page.screenshot({ path: join(OUT_DIR, '1-empty.png') });

  // feed the map (info + difficulty + bundle; skip audio to keep it fast)
  const info = readFileSync(join(MAP_DIR, 'Info.dat'));
  const dat = readFileSync(join(MAP_DIR, 'ExpertStandard.dat'));
  const bundle = readFileSync(join(MAP_DIR, 'bundleWindows2019.vivify'));

  await page.evaluate(
    async ({ infoB64, datB64 }) => {
      const dec = (b64: string) => Uint8Array.from(atob(b64), (c) => c.charCodeAt(0)).buffer;
      await (window as any).__vivified.loadMapFiles({
        'Info.dat': dec(infoB64),
        'ExpertStandard.dat': dec(datB64),
      });
    },
    { infoB64: info.toString('base64'), datB64: dat.toString('base64') }
  );
  console.log('map loaded:', await page.evaluate(() => (window as any).__vivified.getState()));

  // bundle is 2.8MB: transfer in chunks via fetch from the dev server instead
  await page.evaluate(async () => {
    const res = await fetch(
      '/Reference-Do-Not-Include/Beatsaber%20Map%20That%20Uses%20Vivify%20for%20reference/bundleWindows2019.vivify'
    );
    if (!res.ok) throw new Error('bundle fetch failed: ' + res.status);
    const buf = await res.arrayBuffer();
    await (window as any).__vivified.loadBundleBytes(buf, 'bundleWindows2019.vivify');
  });
  const stateAfterBundle = await page.evaluate(() => (window as any).__vivified.getState());
  console.log('bundle loaded:', stateAfterBundle);
  if (stateAfterBundle.assets < 30) throw new Error('bundle did not load assets');

  // scrub to beat 10 (intro prefab should be active)
  await page.evaluate(() => (window as any).__vivified.setBeat(10));
  await page.waitForTimeout(600);
  const s10 = await page.evaluate(() => (window as any).__vivified.getState());
  console.log('beat 10:', s10);
  await page.screenshot({ path: join(OUT_DIR, '2-beat10.png') });

  // scrub to beat 100 (drop section)
  await page.evaluate(() => (window as any).__vivified.setBeat(100));
  await page.waitForTimeout(600);
  const s100 = await page.evaluate(() => (window as any).__vivified.getState());
  console.log('beat 100:', s100);
  await page.screenshot({ path: join(OUT_DIR, '3-beat100.png') });

  // select the intro instance and open inspector
  await page.evaluate(() => {
    (window as any).__vivified.setBeat(10);
    (window as any).__vivified.selectInstance('prefab_intro_2');
  });
  await page.waitForTimeout(400);
  await page.screenshot({ path: join(OUT_DIR, '4-selected.png') });

  // events tab
  await page.click('button[data-tab="events"]');
  await page.waitForTimeout(300);
  await page.screenshot({ path: join(OUT_DIR, '5-events.png') });

  const checks = {
    'instances == 6': s10.instances === 6,
    'intro active at 10': s10.active.includes('prefab_intro_2'),
    'drop active at 100': s100.active.includes('prefab_drop_2'),
    'intro gone at 100': !s100.active.includes('prefab_intro_2'),
    'assets loaded': stateAfterBundle.assets >= 30,
    'events loaded': s10.events === 2214,
  };
  let failed = 0;
  for (const [name, ok] of Object.entries(checks)) {
    console.log(`  ${ok ? 'ok' : 'FAIL'}: ${name}`);
    if (!ok) failed++;
  }
  if (errors.length) {
    console.log('\nconsole errors:');
    for (const e of errors.slice(0, 20)) console.log('  ', e);
  }

  await browser.close();
  await server.close();
  console.log(failed || errors.length ? `\n${failed} check failures, ${errors.length} console errors` : '\nALL OK');
  process.exit(failed ? 1 : 0);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
