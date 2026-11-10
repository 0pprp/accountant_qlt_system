function isLoopbackHost(hostname: string): boolean {
  const h = hostname.trim().toLowerCase()
  return h === 'localhost' || h === '127.0.0.1' || h === '::1' || h === '[::1]'
}

/**
 * يبني عنوان Hub من VITE_API_BASE_URL دون طباعة أسرار.
 * في DEV ضد API على loopback: نفس أصل الواجهة عبر Vite proxy (`/hubs`) —
 * يتجاوز فشل negotiate (Failed to fetch) في متصفحات تقطع cross-origin إلى المنفذ الآخر
 * دون توسيع CORS في الـAPI.
 * أمثلة:
 *  http://127.0.0.1:4200/api/v1/ → (dev) http://127.0.0.1:5174/hubs/sales-requests
 *  https://api.example.com/api/v1 → https://api.example.com/hubs/sales-requests
 */
export function resolveSalesRequestHubUrl(
  apiBaseUrl: string | undefined | null = import.meta.env.VITE_API_BASE_URL,
  options?: { isDev?: boolean; pageOrigin?: string }
): string {
  const raw = String(apiBaseUrl ?? '').trim()
  if (!raw) {
    throw new Error('VITE_API_BASE_URL_missing')
  }

  const isDev = options?.isDev ?? Boolean(import.meta.env.DEV)
  const pageOrigin =
    options?.pageOrigin ??
    (typeof window !== 'undefined' ? window.location.origin : '')

  try {
    const url = new URL(raw)
    if (isDev && isLoopbackHost(url.hostname) && pageOrigin) {
      return `${pageOrigin}/hubs/sales-requests`
    }
    // hub دائماً على جذر مضيف الـAPI (لا تحت /api)
    return `${url.origin}/hubs/sales-requests`
  } catch {
    const base = raw.replace(/\/+$/, '')
    const origin = base.replace(/\/api(?:\/v\d+)?$/i, '')
    if (!origin) {
      throw new Error('VITE_API_BASE_URL_invalid')
    }
    if (isDev && pageOrigin && /localhost|127\.0\.0\.1|\[?::1\]?/i.test(origin)) {
      return `${pageOrigin}/hubs/sales-requests`
    }
    return `${origin}/hubs/sales-requests`
  }
}

/** أصل المضيف فقط — للتشخيص دون توكن */
export function hubOriginLabel(hubUrl: string): string {
  try {
    return new URL(hubUrl).origin
  } catch {
    return '(invalid-hub-url)'
  }
}
