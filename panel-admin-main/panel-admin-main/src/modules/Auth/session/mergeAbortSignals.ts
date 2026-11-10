/**
 * دمج إشارات الإلغاء دون تجاهل أي منها.
 * يفضّل AbortSignal.any عند التوفر، مع تنظيف المستمعين في المسار الاحتياطي.
 */
export function mergeAbortSignals(
  ...signals: Array<AbortSignal | undefined | null>
): AbortSignal | undefined {
  const active = signals.filter((s): s is AbortSignal => Boolean(s))
  if (active.length === 0) return undefined
  if (active.length === 1) return active[0]

  for (const signal of active) {
    if (signal.aborted) {
      const ctrl = new AbortController()
      ctrl.abort(signal.reason)
      return ctrl.signal
    }
  }

  const anyFactory = (
    AbortSignal as unknown as {
      any?: (signals: AbortSignal[]) => AbortSignal
    }
  ).any

  if (typeof anyFactory === 'function') {
    return anyFactory.call(AbortSignal, active)
  }

  const controller = new AbortController()
  const listeners: Array<{ signal: AbortSignal; handler: () => void }> = []

  const cleanup = () => {
    for (const { signal, handler } of listeners) {
      signal.removeEventListener('abort', handler)
    }
    listeners.length = 0
  }

  const onAbort = (source: AbortSignal) => {
    cleanup()
    if (!controller.signal.aborted) {
      controller.abort(source.reason)
    }
  }

  for (const signal of active) {
    const handler = () => onAbort(signal)
    signal.addEventListener('abort', handler, { once: true })
    listeners.push({ signal, handler })
  }

  // تنظيف عند اكتمال الإلغاء الناتج
  controller.signal.addEventListener('abort', cleanup, { once: true })

  return controller.signal
}
