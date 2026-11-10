type SessionEndedListener = (endedGeneration: number) => void

const listeners = new Set<SessionEndedListener>()

export function onSessionEnded(listener: SessionEndedListener): () => void {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}

export function notifySessionEnded(endedGeneration: number) {
  for (const listener of [...listeners]) {
    listener(endedGeneration)
  }
}
