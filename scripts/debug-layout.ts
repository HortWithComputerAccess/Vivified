import { chromium } from 'playwright';
import { createServer } from 'vite';

async function main(): Promise<void> {
  const server = await createServer({ server: { port: 5199, strictPort: true } });
  await server.listen();
  const browser = await chromium.launch({ args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const page = await browser.newPage({ viewport: { width: 1600, height: 900 } });
  await page.addInitScript(() => { (window as any).__name = (fn: unknown) => fn; });
  await page.goto('http://localhost:5199/');
  await page.waitForFunction(() => (window as any).__vivified !== undefined);
  const info = await page.evaluate(() => {
    const out: Record<string, unknown> = {};
    for (const sel of ['#app', '#toolbar', '#main', '#timeline-wrap', '#timeline', '#viewport-wrap', '#viewport']) {
      const el = document.querySelector(sel) as HTMLElement | null;
      if (!el) { out[sel] = 'missing'; continue; }
      const cs = getComputedStyle(el);
      out[sel] = {
        display: cs.display, flex: cs.flex, height: cs.height,
        offsetH: el.offsetHeight, offsetW: el.offsetWidth,
      };
    }
    return out;
  });
  console.log(JSON.stringify(info, null, 2));
  await browser.close();
  await server.close();
  process.exit(0);
}
main().catch((e) => { console.error(e); process.exit(1); });
