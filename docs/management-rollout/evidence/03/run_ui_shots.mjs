import { chromium } from 'playwright'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const BASE = 'http://127.0.0.1:5173'
const results = []

function note(status, id, detail) {
  results.push({ status, id, detail })
  console.log(`[${status}] ${id} — ${detail}`)
}

async function login(page) {
  await page.goto(`${BASE}/auth`, { waitUntil: 'domcontentloaded', timeout: 60000 })
  await page.evaluate(() => {
    try {
      localStorage.clear()
      sessionStorage.clear()
    } catch {}
  })
  await page.goto(`${BASE}/auth`, { waitUntil: 'domcontentloaded' })
  await page.locator('input[name="username"]').fill('admin')
  await page.locator('input[type="password"]').fill('admin123')
  await page.locator('button[type="submit"]').click()
  await page.waitForURL((u) => !u.pathname.includes('/auth'), { timeout: 60000 })
}

async function main() {
  const browser = await chromium.launch({ headless: true })
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } })
  await login(page)
  await page.goto(`${BASE}/management/director/delegate-followup`, {
    waitUntil: 'domcontentloaded',
    timeout: 60000,
  })
  await page.waitForTimeout(3000)
  const title = await page.getByText('متابعة المندوبين', { exact: false }).first().isVisible().catch(() => false)
  note(title ? 'Live-verified' : 'Blocked', 'ui-page-title', `visible=${title} url=${page.url()}`)

  // pick branch card if shown
  const branchBtn = page.locator('.df-card-btn').first()
  if (await branchBtn.isVisible().catch(() => false)) {
    await branchBtn.click()
    await page.waitForTimeout(2000)
  }

  await page.screenshot({ path: path.join(__dirname, 'df-1440.png'), fullPage: true })
  note('Live-verified', 'shot-1440', 'df-1440.png')

  const listBtn = page.locator('.df-card-btn').first()
  if (await listBtn.isVisible().catch(() => false)) {
    await listBtn.click()
    await page.waitForTimeout(3000)
    await page.screenshot({ path: path.join(__dirname, 'df-detail-1440.png'), fullPage: true })
    const unresolved = await page.getByText('غير محسوم', { exact: false }).first().isVisible().catch(() => false)
    note('Live-verified', 'shot-detail', `df-detail-1440.png unresolved=${unresolved}`)
  } else {
    note('Tested', 'shot-detail', 'no list card button visible')
  }

  await page.setViewportSize({ width: 768, height: 900 })
  await page.screenshot({ path: path.join(__dirname, 'df-768.png'), fullPage: true })
  note('Live-verified', 'shot-768', 'df-768.png')

  await page.setViewportSize({ width: 360, height: 740 })
  await page.screenshot({ path: path.join(__dirname, 'df-360.png'), fullPage: true })
  note('Live-verified', 'shot-360', 'df-360.png')

  // SalesFollowup still reachable
  await page.setViewportSize({ width: 1440, height: 900 })
  await page.goto(`${BASE}/management/branch/sale-followup`, { waitUntil: 'domcontentloaded' })
  await page.waitForTimeout(1500)
  const sf = await page.getByText('متابعة المبيعات', { exact: false }).first().isVisible().catch(() => false)
  note(sf ? 'Live-verified' : 'Tested', 'salesfollowup-still-open', `visible=${sf}`)

  fs.writeFileSync(path.join(__dirname, 'ui_results.json'), JSON.stringify(results, null, 2), 'utf8')
  await browser.close()
  console.log('DONE')
}

main().catch((e) => {
  console.error(e)
  process.exit(1)
})
