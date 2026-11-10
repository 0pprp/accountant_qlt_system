import * as signalR from '@microsoft/signalr'
import { writeFileSync } from 'node:fs'

const API = 'http://127.0.0.1:4200'
const HUB = `${API}/hubs/sales-requests`
const ORIGIN = process.env.ORIGIN || 'http://127.0.0.1:5173'

async function login(path, username, password) {
  const r = await fetch(`${API}/api/v1/${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Origin: ORIGIN,
    },
    body: JSON.stringify({ username, password }),
  })
  const j = await r.json()
  if (!r.ok) throw new Error(`login ${username}: ${JSON.stringify(j)}`)
  return j.accessToken
}

async function api(method, path, token, body) {
  const r = await fetch(`${API}/api/v1/${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      Origin: ORIGIN,
    },
    body: body ? JSON.stringify(body) : undefined,
  })
  const text = await r.text()
  return { ok: r.ok, status: r.status, text }
}

const evidence = { origin: ORIGIN, hub: HUB, steps: [] }
function log(step, data) {
  evidence.steps.push({ step, at: new Date().toISOString(), ...data })
  console.log(JSON.stringify({ step, ...data }))
}

const adminToken = await login('admin/Auth/login', 'admin', 'admin123')
log('admin_login', { ok: true })

let gotEvent = null
let saveDoneAt = null
const connection = new signalR.HubConnectionBuilder()
  .withUrl(HUB, {
    accessTokenFactory: () => adminToken,
    headers: { Origin: ORIGIN },
  })
  .withAutomaticReconnect([0, 500, 1000])
  .configureLogging(signalR.LogLevel.Information)
  .build()

connection.on('SalesRequestChanged', (payload) => {
  const receivedAt = Date.now()
  const latencyMs = saveDoneAt ? receivedAt - saveDoneAt : null
  gotEvent = { payload, receivedAt, latencyMs, transport: connection.connection?.transport?.name }
  log('event_received', {
    latencyMs,
    changeType: payload?.changeType || payload?.ChangeType,
    requestId: payload?.requestId || payload?.RequestId,
    eventId: payload?.eventId || payload?.EventId,
  })
})

try {
  await connection.start()
  log('hub_connected', {
    state: connection.state,
    connectionId: connection.connectionId,
  })
} catch (e) {
  log('hub_connect_failed', { error: String(e) })
  writeFileSync(new URL('./signalr_latency.json', import.meta.url), JSON.stringify(evidence, null, 2))
  process.exit(1)
}

// Create a fresh request via SQL is hard from node; use soft-delete on restored id 5 then restore to generate Invalidation
const t0 = Date.now()
const savePromise = api('POST', 'admin/SalesRequests/5/soft-delete', adminToken, {
  reason: '04F realtime latency probe',
})
// mark save completion as soon as HTTP returns
const saveRes = await savePromise
saveDoneAt = Date.now()
log('save_completed', { ok: saveRes.ok, status: saveRes.status, httpMs: saveDoneAt - t0, body: saveRes.text.slice(0, 200) })

// wait for event up to 8s
const deadline = Date.now() + 8000
while (!gotEvent && Date.now() < deadline) {
  await new Promise((r) => setTimeout(r, 50))
}

log('latency_summary', {
  measured: Boolean(gotEvent?.latencyMs != null),
  latencyMs: gotEvent?.latencyMs ?? null,
  claim_le_2s: gotEvent?.latencyMs != null ? gotEvent.latencyMs <= 2000 : false,
})

// reconnect compensation
await connection.stop()
log('disconnected', {})
await connection.start()
log('reconnected', { state: connection.state })
try {
  const n = await connection.invoke('RefreshSubscriptions')
  log('refresh_subscriptions', { groups: n })
} catch (e) {
  log('refresh_failed', { error: String(e) })
}

// duplicate / out-of-order handled client-side; emit restore for second event
gotEvent = null
saveDoneAt = null
const t1 = Date.now()
const restoreRes = await api('POST', 'admin/SalesRequests/5/restore', adminToken, {
  reason: '04F restore after latency',
})
saveDoneAt = Date.now()
log('restore_completed', { ok: restoreRes.ok, status: restoreRes.status, httpMs: saveDoneAt - t1 })
const deadline2 = Date.now() + 8000
while (!gotEvent && Date.now() < deadline2) {
  await new Promise((r) => setTimeout(r, 50))
}
log('second_event_latency', {
  measured: Boolean(gotEvent?.latencyMs != null),
  latencyMs: gotEvent?.latencyMs ?? null,
})

await connection.stop()
writeFileSync(new URL('./signalr_latency.json', import.meta.url), JSON.stringify(evidence, null, 2))
console.log('SIGNALR_LATENCY_DONE')
