import { chromium } from 'playwright'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const cred = JSON.parse(fs.readFileSync(process.env.MGMT_CRED_FILE, 'utf8'))

async function login(page, username, password) {
  await page.goto(`${cred.baseUrl}/auth`, { waitUntil: 'networkidle' })
  await page.evaluate(() => {
    localStorage.clear()
    sessionStorage.clear()
  })
  await page.goto(`${cred.baseUrl}/auth`, { waitUntil: 'networkidle' })
  await page.locator('input[name="username"]').fill(username)
  await page.locator('input[type="password"]').fill(password)
  await page.locator('button[type="submit"]').click()
  await page.waitForURL((u) => !u.pathname.includes('/auth'), { timeout: 45000 })
}

const browser = await chromium.launch({ headless: true })
const page = await browser.newPage({ viewport: { width: 1440, height: 900 } })
const hits = []
page.on('request', (r) => {
  if (r.url().includes('/api/')) hits.push({ t: 'req', u: r.url(), ts: Date.now() })
})

await login(page, cred.adminUser, cred.adminPass)
await page.goto(`${cred.baseUrl}/management/branch/customers`, {
  waitUntil: 'networkidle',
})
await page.waitForTimeout(1000)
hits.length = 0

const before = await page.evaluate(() => {
  const app = document.querySelector('#app')?.__vue_app__
  const pinia = app?.config?.globalProperties?.$pinia
  const stores = pinia?._s
  let auth = null
  if (stores) {
    for (const [k, v] of stores) {
      if (k === 'auth' || v?.changeSelectedBranch) auth = v
    }
  }
  return {
    hasPinia: !!pinia,
    storeKeys: stores ? [...stores.keys()] : [],
    branchId: auth?.selectedBranch?.id ?? null,
    branchGen: auth?.branchGeneration ?? null,
    branches: (auth?.branches || []).map((b) => ({ id: b.id, name: b.name })),
  }
})

const switched = await page.evaluate(() => {
  const app = document.querySelector('#app')?.__vue_app__
  const pinia = app?.config?.globalProperties?.$pinia
  const stores = pinia?._s
  let auth = null
  if (stores) {
    for (const [k, v] of stores) {
      if (k === 'auth' || v?.changeSelectedBranch) auth = v
    }
  }
  if (!auth) return { ok: false, reason: 'no-auth-store' }
  const other = (auth.branches || []).find((b) => b.id !== auth.selectedBranch?.id)
  if (!other) return { ok: false, reason: 'no-other-branch', branches: auth.branches }
  const genBefore = auth.branchGeneration
  auth.changeSelectedBranch(other)
  return {
    ok: true,
    from: genBefore,
    to: auth.branchGeneration,
    selected: auth.selectedBranch?.id,
  }
})

await page.waitForTimeout(1500)
await page.screenshot({
  path: path.join(__dirname, 'admin-after-branch-switch.png'),
  fullPage: true,
})

const after = await page.evaluate(() => {
  const app = document.querySelector('#app')?.__vue_app__
  const pinia = app?.config?.globalProperties?.$pinia
  const stores = pinia?._s
  let auth = null
  if (stores) {
    for (const [k, v] of stores) {
      if (k === 'auth' || v?.changeSelectedBranch) auth = v
    }
  }
  return {
    branchId: auth?.selectedBranch?.id ?? null,
    branchGen: auth?.branchGeneration ?? null,
  }
})

fs.writeFileSync(
  path.join(__dirname, 'branch_switch.json'),
  JSON.stringify(
    {
      before,
      switched,
      after,
      reqCount: hits.filter((h) => h.t === 'req').length,
      reqSample: hits.filter((h) => h.t === 'req').slice(0, 8),
    },
    null,
    2
  ),
  'utf8'
)

console.log(JSON.stringify({ before, switched, after, reqCount: hits.length }))
await browser.close()
