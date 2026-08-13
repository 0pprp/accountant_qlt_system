import type { ComponentInternalInstance } from 'vue'
import { getCurrentInstance as _getCurrentInstance } from 'vue'

export function useCurrentInstance(name: string, message?: string) {
  const vm = _getCurrentInstance()

  if (!vm) {
    throw new Error(
      `[QalaatAlDahman] ${name} ${
        message || 'must be called from inside a setup function'
      }`
    )
  }

  return vm
}

let _uid = 0
let _map = new WeakMap<ComponentInternalInstance, number>()

export default function useUid() {
  const vm = useCurrentInstance('useUid')

  if (_map.has(vm)) {
    return _map.get(vm)!
  }

  const uid = _uid++
  _map.set(vm, uid)

  return uid
}

useUid.reset = () => {
  _uid = 0
  _map = new WeakMap()
}
