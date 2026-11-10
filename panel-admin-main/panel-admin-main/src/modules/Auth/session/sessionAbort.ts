/**
 * إلغاء طلبات الجلسة الحالية عند إنهاء الجلسة أو تبديل الحساب.
 * تبديل الفرع: انظر branchAbort.ts + ختم branchGeneration في axios.
 */
let sessionAbortController = new AbortController()

export function getSessionAbortSignal(): AbortSignal {
  return sessionAbortController.signal
}

export function abortSessionRequests() {
  sessionAbortController.abort()
  sessionAbortController = new AbortController()
}

export function isAbortError(error: unknown): boolean {
  if (!error || typeof error !== 'object') return false
  const err = error as {
    code?: string
    name?: string
    message?: string
  }
  return (
    err.code === 'ERR_CANCELED' ||
    err.name === 'CanceledError' ||
    err.name === 'AbortError' ||
    err.message === 'stale_session' ||
    /abort|cancel|stale_session/i.test(err.message ?? '')
  )
}

export { mergeAbortSignals } from './mergeAbortSignals'
