import { createRequire } from 'module'
const require = createRequire(import.meta.url)
const { chromium } = require('../../management-rollout/evidence/01/node_modules/playwright')
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const out20 = path.join(__dirname, '20-sales-summary')
const out21 = path.join(__dirname, '21-sales-legal')
fs.mkdirSync(out20, { recursive: true })
fs.mkdirSync(out21, { recursive: true })

const browser = await chromium.launch({ headless: true })
const context = await browser.newContext({ locale: 'ar' })
const page = await context.newPage()

await page.goto('http://127.0.0.1:5173/auth', { waitUntil: 'networkidle' })
await page.fill('input[placeholder*="المستخدم"]', 'admin')
await page.fill('input[type="password"], input[placeholder*="المرور"]', 'admin123')
await page.click('button:has-text("تسجيل الدخول")')
await page.waitForURL((u) => !u.pathname.includes('/auth'), { timeout: 15000 })

for (const [w, name] of [[1440, '1440'], [768, '768'], [360, '360']]) {
  await page.setViewportSize({ width: w, height: 900 })
  await page.goto('http://127.0.0.1:5173/sale/summary?year=2026&month=11', { waitUntil: 'networkidle' })
  await page.waitForTimeout(800)
  await page.screenshot({ path: path.join(out20, `summary-${name}.png`), fullPage: true })
  await page.goto('http://127.0.0.1:5173/sale/legal?status=InProgress', { waitUntil: 'networkidle' })
  await page.waitForTimeout(800)
  await page.screenshot({ path: path.join(out21, `legal-${name}.png`), fullPage: true })
}

await browser.close()
console.log('screenshots_ok')
