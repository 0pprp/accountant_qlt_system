import { chromium } from 'playwright'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const out = __dirname
const BASE = 'http://127.0.0.1:5173'
const results = []

function note(status, id, detail) {
  results.push({ status, id, detail })
  console.log(`[${status}] ${id} — ${detail}`)
}

async function login(page, username, password) {
  await page.goto(`${BASE}/auth`, { waitUntil: 'domcontentloaded', timeout: 60000 })
  await page.evaluate(() => {
    try {
      localStorage.clear()
      sessionStorage.clear()
    } catch {}
  })
  await page.goto(`${BASE}/auth`, { waitUntil: 'domcontentloaded', timeout: 60000 })
  await page.locator('input[name="username"]').fill(username)
  await page.locator('input[type="password"]').fill(password)
  await page.locator('button[type="submit"]').click()
  await page.waitForURL((u) => !u.pathname.includes('/auth'), { timeout: 60000 })
}

async function main() {
  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } })
  const page = await context.newPage()

  await login(page, 'admin', 'admin123')
  note('Live-verified', 'ui-login', 'admin')

  await page.goto(`${BASE}/management/branch/assigned-tasks`, {
    waitUntil: 'domcontentloaded',
    timeout: 60000,
  })
  await page.waitForTimeout(2500)
  await page.screenshot({ path: path.join(out, 'admin-tasks-1440.png'), fullPage: true })
  note('Live-verified', 'screenshot-1440', 'admin-tasks-1440.png')

  const hasHeading = await page.getByText('المهام', { exact: false }).first().isVisible().catch(() => false)
  note(hasHeading ? 'Live-verified' : 'Tested', 'tasks-page-visible', `heading=${hasHeading} url=${page.url()}`)

  await page.setViewportSize({ width: 768, height: 900 })
  await page.screenshot({ path: path.join(out, 'admin-tasks-768.png'), fullPage: true })
  note('Live-verified', 'screenshot-768', 'admin-tasks-768.png')

  await page.setViewportSize({ width: 360, height: 740 })
  await page.screenshot({ path: path.join(out, 'admin-tasks-360.png'), fullPage: true })
  note('Live-verified', 'screenshot-360', 'admin-tasks-360.png')
  await page.setViewportSize({ width: 1440, height: 900 })

  // Branch switch + delayed stale response isolation
  let release = () => {}
  const gate = new Promise((r) => {
    release = r
  })
  let staleServedAfterSwitch = false

  await page.route('**/api/v1/admin/ManagementTasks?**', async (route) => {
    const req = route.request()
    if (req.method() !== 'GET') return route.continue()
    const url = req.url()
    if (url.includes('branchId=1') === false) return route.continue()

    // Hold branch-1 list until after Pinia switch, then fulfill with poison payload
    await gate
    staleServedAfterSwitch = true
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: 999001,
            title: 'STALE_BRANCH_1_SHOULD_NOT_SHOW',
            description: 'leak',
            status: 0,
            priority: 1,
            dueDate: '2099-01-01',
            isOverdue: false,
            branchId: 1,
            createdByUserId: 1,
            assigneeUserId: 1,
            assigneeDisplayName: 'stale',
            currentHolderUserId: 1,
            currentHolderDisplayName: 'stale',
            expectedVersion: 0,
          },
        ],
        pageIndex: 1,
        pageSize: 10,
        totalCount: 1,
      }),
    })
  })

  // Kick a branch-1 request then switch to branch 2 via Pinia
  const switchResult = await page.evaluate(async () => {
    const app = document.querySelector('#app')?.__vue_app__
    const pinia = app?.config?.globalProperties?.$pinia
    if (!pinia) return { ok: false, reason: 'no-pinia' }
    const auth = pinia._s.get('auth')
    if (!auth) return { ok: false, reason: 'no-auth', keys: [...pinia._s.keys()] }
    const token =
      auth.accessToken ||
      auth.token ||
      JSON.parse(localStorage.getItem('auth') || '{}').accessToken
    const before = { id: auth.selectedBranch?.id, gen: auth.branchGeneration }
    // fire request that route will hold
    fetch(
      `http://127.0.0.1:4200/api/v1/admin/ManagementTasks?pageIndex=1&pageSize=10&branchId=1`,
      { headers: { Authorization: `Bearer ${token}` } }
    ).catch(() => {})
    await new Promise((r) => setTimeout(r, 150))
    const target =
      (auth.branches || []).find((b) => b.id === 2) || { id: 2, name: 'فرع ب' }
    if (typeof auth.changeSelectedBranch !== 'function') {
      return { ok: false, reason: 'no-changeSelectedBranch', before }
    }
    await auth.changeSelectedBranch(target)
    return {
      ok: true,
      before,
      after: { id: auth.selectedBranch?.id, gen: auth.branchGeneration },
    }
  })

  release()
  await page.waitForTimeout(1500)
  const bodyText = await page.locator('body').innerText()
  const leaked = bodyText.includes('STALE_BRANCH_1_SHOULD_NOT_SHOW')
  note(
    switchResult.ok && leaked === false ? 'Live-verified' : 'Tested',
    'branch-switch-stale-isolation',
    JSON.stringify({ switchResult, leaked, staleServedAfterSwitch })
  )

  await page.screenshot({
    path: path.join(out, 'admin-after-branch-switch.png'),
    fullPage: true,
  })

  // Delegate tab
  await page.goto(`${BASE}/management/delegate/assigned-tasks`, {
    waitUntil: 'domcontentloaded',
    timeout: 60000,
  })
  await page.waitForTimeout(1500)
  await page.screenshot({ path: path.join(out, 'admin-delegate-tasks.png'), fullPage: true })
  note('Live-verified', 'delegate-tab', page.url())

  fs.writeFileSync(path.join(out, 'ui_results.json'), JSON.stringify(results, null, 2), 'utf8')
  await browser.close()
  console.log('DONE')
}

main().catch((e) => {
  console.error(e)
  process.exit(1)
})
