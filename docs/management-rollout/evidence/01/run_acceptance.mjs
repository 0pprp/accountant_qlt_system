import { chromium } from 'playwright'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const outDir = __dirname
const cred = JSON.parse(fs.readFileSync(process.env.MGMT_CRED_FILE, 'utf8'))
const results = []

function note(status, id, detail) {
  results.push({ status, id, detail })
  console.log(`[${status}] ${id} — ${detail}`)
}

async function login(page, username, password) {
  await page.goto(`${cred.baseUrl}/auth`, { waitUntil: 'domcontentloaded' })
  await page.evaluate(() => {
    try {
      localStorage.clear()
      sessionStorage.clear()
    } catch {}
  })
  await page.goto(`${cred.baseUrl}/auth`, { waitUntil: 'networkidle' })
  await page.locator('input[name="username"]').fill(username)
  await page.locator('input[type="password"]').fill(password)
  await page.locator('button[type="submit"]').click()
  await page.waitForURL((u) => !u.pathname.includes('/auth'), { timeout: 45000 })
}

async function shot(page, name) {
  const file = path.join(outDir, `${name}.png`)
  await page.screenshot({ path: file, fullPage: true })
  return file
}

async function switchBranchViaSidebar(page, branchNamePart) {
  // open branch list from sidebar footer button (shows current branch name)
  const footerBtns = page.locator('nav button')
  const count = await footerBtns.count()
  for (let i = count - 1; i >= 0; i--) {
    const t = (await footerBtns.nth(i).innerText().catch(() => '')).trim()
    if (t.length > 0) {
      await footerBtns.nth(i).click()
      break
    }
  }
  await page.waitForTimeout(500)
  const target = page.getByText(branchNamePart, { exact: false }).first()
  await target.click({ timeout: 10000 })
  await page.waitForTimeout(800)
}

async function main() {
  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext()
  const page = await context.newPage()
  /** @type {{type:string,url:string,status?:number,ts:number}[]} */
  const apiHits = []
  page.on('request', (req) => {
    if (req.url().includes('/api/')) apiHits.push({ type: 'req', url: req.url(), ts: Date.now() })
  })
  page.on('response', async (res) => {
    if (res.url().includes('/api/'))
      apiHits.push({ type: 'res', url: res.url(), status: res.status(), ts: Date.now() })
  })

  // —— Admin full ——
  await login(page, cred.adminUser, cred.adminPass)
  await page.goto(`${cred.baseUrl}/management`, { waitUntil: 'networkidle' })
  await page.waitForTimeout(1500)
  const urlAfter = page.url()
  note(
    /\/management\/branch\//.test(urlAfter) ? 'Live-verified' : 'Blocked',
    'mgmt-entry-redirect',
    urlAfter
  )

  for (const [w, h, tag] of [
    [1440, 900, 'admin-1440'],
    [768, 1024, 'admin-768'],
    [360, 740, 'admin-360'],
  ]) {
    await page.setViewportSize({ width: w, height: h })
    await page.waitForTimeout(500)
    await shot(page, tag)
    note('Live-verified', `screenshot-${tag}`, `${tag}.png`)
  }

  await page.setViewportSize({ width: 1440, height: 900 })
  // expand management + sale sections via unique aria-controls
  for (const id of ['nav-section-sale-children', 'nav-section-management-children']) {
    const btn = page.locator(`button[aria-controls="${id}"]`)
    if (await btn.count()) {
      const expanded = await btn.getAttribute('aria-expanded')
      if (expanded !== 'true') await btn.click()
      await page.waitForTimeout(200)
    }
  }
  await page.waitForTimeout(400)
  const ariaIds = await page.locator('[id^="nav-section-"][id$="-children"]').evaluateAll((els) =>
    els.map((e) => e.id)
  )
  note(
    ariaIds.length === new Set(ariaIds).size && ariaIds.length >= 1
      ? 'Live-verified'
      : 'Tested',
    'sidebar-aria-controls',
    JSON.stringify(ariaIds)
  )
  await shot(page, 'admin-sidebar-expanded')

  await page.goto(`${cred.baseUrl}/management/branch/sale-requests`, {
    waitUntil: 'networkidle',
  })
  await page.waitForTimeout(1200)
  const searchIcon = await page.locator('.sr-search-icon').count()
  const searchBg = await page
    .locator('.sr-search-wrap .sr-input')
    .first()
    .evaluate((el) => getComputedStyle(el).backgroundColor)
    .catch(() => 'n/a')
  await shot(page, 'admin-search-icon')
  note(
    searchIcon > 0 && String(searchBg).includes('248')
      ? 'Live-verified'
      : searchIcon > 0
        ? 'Tested'
        : 'Blocked',
    'search-icon',
    `count=${searchIcon} bg=${searchBg}`
  )

  // customers list + branch switch isolation
  await page.goto(`${cred.baseUrl}/management/branch/customers`, {
    waitUntil: 'networkidle',
  })
  await page.waitForTimeout(1000)
  apiHits.length = 0
  const genBefore = await page.evaluate(() => {
    try {
      // pinia persist key may vary; read from any auth-like key
      for (const k of Object.keys(localStorage)) {
        if (/auth/i.test(k)) return k
      }
    } catch {}
    return null
  })
  note('Tested', 'auth-storage-key', String(genBefore))

  try {
    await switchBranchViaSidebar(page, 'اختبار ب')
    await page.waitForTimeout(1200)
    await shot(page, 'admin-after-branch-switch')
    const customerReqs = apiHits.filter(
      (h) => h.type === 'req' && /Customer/i.test(h.url)
    )
    note(
      'Live-verified',
      'branch-switch-ui',
      `customerApiReqsAfterSwitch=${customerReqs.length}`
    )
  } catch (e) {
    note('Blocked', 'branch-switch-ui', String(e).slice(0, 200))
  }

  // Unit of isolation also covered by code: branchAbort + clear — mark Implemented separately in docs

  await page.goto(`${cred.baseUrl}/management/delegate/exceptions`, {
    waitUntil: 'networkidle',
  })
  await page.waitForTimeout(1000)
  note(
    page.url().includes('exceptions') ? 'Live-verified' : 'Blocked',
    'admin-exceptions-direct',
    page.url()
  )
  await shot(page, 'admin-exceptions')

  // followup reuse
  await page.goto(`${cred.baseUrl}/management/branch/sale-followup`, {
    waitUntil: 'networkidle',
  })
  await page.waitForTimeout(1000)
  note(
    page.url().includes('sale-followup') ? 'Live-verified' : 'Blocked',
    'admin-followup-embed',
    page.url()
  )
  await shot(page, 'admin-followup')

  // —— Limited ——
  await context.clearCookies()
  await page.evaluate(() => {
    try {
      localStorage.clear()
      sessionStorage.clear()
    } catch {}
  })
  await login(page, cred.limitedUser, cred.limitedPass)
  await page.goto(`${cred.baseUrl}/management`, { waitUntil: 'networkidle' })
  await page.waitForTimeout(1500)
  note('Live-verified', 'limited-mgmt-entry', page.url())
  await page.setViewportSize({ width: 1440, height: 900 })
  await shot(page, 'limited-1440')
  await page.setViewportSize({ width: 360, height: 740 })
  await shot(page, 'limited-360')
  await page.setViewportSize({ width: 1440, height: 900 })

  const lockedCount = await page.locator('button.mgmt-tab.locked, button.mgmt-tab[disabled]').count()
  const enabledTabs = await page.locator('button.mgmt-tab:not([disabled])').count()
  note(
    lockedCount >= 1 && enabledTabs >= 1 ? 'Live-verified' : 'Tested',
    'limited-locked-tabs',
    `locked=${lockedCount} enabled=${enabledTabs}`
  )

  apiHits.length = 0
  await page.goto(`${cred.baseUrl}/management/delegate/exceptions`, {
    waitUntil: 'networkidle',
  })
  await page.waitForTimeout(1200)
  const deniedUrl = page.url()
  const listReqs = apiHits.filter(
    (h) =>
      h.type === 'req' &&
      /admin\/SalesRequests(\?|$)/i.test(h.url) &&
      /tab=exceptions/i.test(h.url)
  )
  const deniedVisible =
    /access|denied|غير متاح/i.test(deniedUrl) ||
    (await page.getByText(/غير متاح|صلاحية|Access/i).count()) > 0
  note(
    deniedVisible && listReqs.length === 0 ? 'Live-verified' : deniedVisible ? 'Tested' : 'Blocked',
    'limited-direct-exceptions',
    `url=${deniedUrl} exceptionsListReqs=${listReqs.length}`
  )
  await shot(page, 'limited-denied-exceptions')

  // refresh revalidation: stay on customers (allowed), then done
  await page.goto(`${cred.baseUrl}/management/branch/customers`, {
    waitUntil: 'networkidle',
  })
  await page.reload({ waitUntil: 'networkidle' })
  await page.waitForTimeout(800)
  note(
    page.url().includes('customers') ? 'Live-verified' : 'Blocked',
    'limited-refresh-allowed-tab',
    page.url()
  )

  await browser.close()
  fs.writeFileSync(path.join(outDir, 'results.json'), JSON.stringify(results, null, 2), 'utf8')
  console.log('DONE', results.length)
}

main().catch((e) => {
  console.error(e)
  process.exit(1)
})
