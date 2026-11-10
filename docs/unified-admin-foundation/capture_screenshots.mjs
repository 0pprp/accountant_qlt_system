/**
 * التشغيل من مجلد الواجهة (حيث تتوفر playwright):
 *   node ../../docs/unified-admin-foundation/capture_screenshots.mjs
 * بعد: yarn build-only && yarn preview --host 127.0.0.1 --port 4173
 */
import { createRequire } from 'node:module'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const require = createRequire(
  path.join(
    path.dirname(fileURLToPath(import.meta.url)),
    '../../panel-admin-main/panel-admin-main/package.json'
  )
)
const { chromium } = require('playwright')

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const outDir = path.join(__dirname, 'screenshots')
const base = process.env.PANEL_URL || 'http://127.0.0.1:4173'

const auth = {
  accessToken: 'demo-token',
  refreshToken: 'demo-refresh',
  permissions: { OrderList: [{ name: 'Read', id: 1 }] },
  convertedPermissions: { OrderList: { Read: true } },
  branches: [{ id: 1, name: 'فرع تجريبي' }],
  selectedBranch: { id: 1, name: 'فرع تجريبي' },
  isAuthenticated: true,
  routesRegistered: false,
  isSuperAdmin: false,
}

async function seed(page) {
  await page.addInitScript((state) => {
    localStorage.setItem('auth', JSON.stringify(state))
  }, auth)
}

async function main() {
  const browser = await chromium.launch()
  const desktop = await browser.newPage({ viewport: { width: 1440, height: 900 } })
  await seed(desktop)
  await desktop.goto(base + '/', { waitUntil: 'networkidle' })
  await desktop.waitForTimeout(800)
  await desktop.screenshot({
    path: path.join(outDir, 'desktop-1440-sidebar-locks.png'),
    fullPage: false,
  })

  await desktop.getByRole('button', { name: /العملاء/ }).click()
  await desktop.waitForURL('**/access-denied**')
  await desktop.waitForTimeout(400)
  await desktop.screenshot({
    path: path.join(outDir, 'desktop-1440-access-denied.png'),
    fullPage: false,
  })

  const mobile = await browser.newPage({ viewport: { width: 360, height: 740 } })
  await seed(mobile)
  await mobile.goto(base + '/', { waitUntil: 'networkidle' })
  await mobile.waitForTimeout(600)
  await mobile.getByRole('button', { name: 'فتح القائمة' }).click()
  await mobile.waitForTimeout(400)
  await mobile.screenshot({
    path: path.join(outDir, 'mobile-360-drawer-locks.png'),
    fullPage: false,
  })

  await browser.close()
  console.log('screenshots written to', outDir)
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
