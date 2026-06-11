import { chromium } from 'playwright';
import { createServer } from 'vite';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

const MAP_DIR = join(process.cwd(), 'Reference-Do-Not-Include', 'Beatsaber Map That Uses Vivify for reference');

async function main(): Promise<void> {
  const server = await createServer({ server: { port: 5199, strictPort: true } });
  await server.listen();
  const browser = await chromium.launch({ args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const page = await browser.newPage({ viewport: { width: 1600, height: 900 } });
  await page.addInitScript(() => { (window as any).__name = (fn: unknown) => fn; });
  page.on('console', (m) => { if (m.type() === 'error' || m.type() === 'warning') console.log('[page]', m.text()); });
  await page.goto('http://localhost:5199/');
  await page.waitForFunction(() => (window as any).__vivified !== undefined);

  const info = readFileSync(join(MAP_DIR, 'Info.dat'));
  const dat = readFileSync(join(MAP_DIR, 'ExpertStandard.dat'));
  await page.evaluate(
    async ({ infoB64, datB64 }) => {
      const dec = (b64: string) => Uint8Array.from(atob(b64), (c) => c.charCodeAt(0)).buffer;
      await (window as any).__vivified.loadMapFiles({ 'Info.dat': dec(infoB64), 'ExpertStandard.dat': dec(datB64) });
    },
    { infoB64: info.toString('base64'), datB64: dat.toString('base64') }
  );
  await page.evaluate(async () => {
    const res = await fetch('/Reference-Do-Not-Include/Beatsaber%20Map%20That%20Uses%20Vivify%20for%20reference/bundleWindows2019.vivify');
    await (window as any).__vivified.loadBundleBytes(await res.arrayBuffer(), 'b.vivify');
  });
  await page.evaluate(() => (window as any).__vivified.setBeat(10));
  await page.waitForTimeout(800);

  const stats = await page.evaluate(() => {
    const vp = (window as any).__vivifiedDebug;
    return vp ? vp() : 'no debug hook';
  });
  console.log(JSON.stringify(stats, null, 2));
  await browser.close();
  await server.close();
  process.exit(0);
}
main().catch((e) => { console.error(e); process.exit(1); });
