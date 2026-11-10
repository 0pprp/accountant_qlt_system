function isLoopbackHost(hostname: string): boolean {
  const h = hostname.trim().toLowerCase()
  return h === 'localhost' || h === '127.0.0.1' || h === '::1' || h === '[::1]'
}

/** عنوان Hub المهام — نفس أصل الواجهة عبر proxy في DEV ضد loopback. */
export function resolveManagementTaskHubUrl(
  apiBaseUrl: string | undefined | null = import.meta.env.VITE_API_BASE_URL,
  options?: { isDev?: boolean; pageOrigin?: string }
): string {
  const raw = String(apiBaseUrl ?? '').trim()
  if (!raw) throw new Error('VITE_API_BASE_URL_missing')

  const isDev = options?.isDev ?? Boolean(import.meta.env.DEV)
  const pageOrigin =
    options?.pageOrigin ??
    (typeof window !== 'undefined' ? window.location.origin : '')

  try {
    const url = new URL(raw)
    if (isDev && isLoopbackHost(url.hostname) && pageOrigin) {
      return `${pageOrigin}/hubs/management-tasks`
    }
    return `${url.origin}/hubs/management-tasks`
  } catch {
    const base = raw.replace(/\/+$/, '')
    const origin = base.replace(/\/api(?:\/v\d+)?$/i, '')
    if (!origin) throw new Error('VITE_API_BASE_URL_invalid')
    if (isDev && pageOrigin && /localhost|127\.0\.0\.1|\[?::1\]?/i.test(origin)) {
      return `${pageOrigin}/hubs/management-tasks`
    }
    return `${origin}/hubs/management-tasks`
  }
}
