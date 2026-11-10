/**
 * 04G: غيّر طلبًا عبر API بينما اللوحة متصلة — قياس لا يُستخدم كبديل عن إثبات Vue،
 * بل كمحفّز حدث؛ إثبات الظهور يتم من المتصفح.
 */
const API = 'http://127.0.0.1:4200'

async function login() {
  const r = await fetch(`${API}/api/v1/admin/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: 'admin', password: 'admin123' }),
  })
  const j = await r.json()
  if (!r.ok) throw new Error(JSON.stringify(j))
  return j.accessToken
}

async function api(method, path, token, body) {
  const r = await fetch(`${API}/api/v1/${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: body ? JSON.stringify(body) : undefined,
  })
  const text = await r.text()
  return { ok: r.ok, status: r.status, text, at: Date.now() }
}

const token = await login()
const t0 = Date.now()
// استخدم طلب #5 إن وُجد؛ soft-delete ثم restore لتوليد Invalidation
let res = await api('POST', 'admin/SalesRequests/5/soft-delete', token, {
  reason: '04G vue live probe long reason — soft delete from second session',
})
if (!res.ok) {
  res = await api('POST', 'admin/SalesRequests/5/restore', token, {
    reason: '04G restore before delete',
  })
  res = await api('POST', 'admin/SalesRequests/5/soft-delete', token, {
    reason: '04G vue live probe long reason — soft delete from second session',
  })
}
console.log(JSON.stringify({ step: 'soft_delete', ...res, elapsedMs: Date.now() - t0 }))
await new Promise((r) => setTimeout(r, 1500))
const restore = await api('POST', 'admin/SalesRequests/5/restore', token, {
  reason: '04G restore after vue live probe',
})
console.log(JSON.stringify({ step: 'restore', ...restore, at: restore.at }))
console.log('VUE_HUB_PROBE_DONE')
