import { defineStore } from 'pinia'
import type {
  ConvertedPermissionTypes,
  ModulePermissions,
  PermissionAction,
  PermissionModule,
  PermissionTypes,
} from '../types/model'
import { type AuthLoginSuccessResponse } from '../types/api'
import type { Warehouse } from '@/modules/Warehouse/types/model'
import {
  clearSessionQueryCache,
  cancelBranchQueries,
  invalidateSessionQueries,
} from '@/modules/Core/plugins/vue-query'
import { abortSessionRequests } from '@/modules/Auth/session/sessionAbort'
import { abortBranchRequests } from '@/modules/Auth/session/branchAbort'
import { nextGeneration } from '@/modules/Auth/session/sessionGeneration'
import type {
  PermissionsLifecycleStatus,
  SessionEndReason,
} from '@/modules/Auth/session/session.types'
import { clearIntendedNavigation } from '@/modules/Auth/session/intendedNavigation'
import { notifySessionEnded } from '@/modules/Auth/session/sessionHttpBridge'

interface State {
  accessToken: string
  refreshToken: string
  permissions: PermissionTypes | null
  convertedPermissions: ConvertedPermissionTypes | null
  branches: Array<Warehouse> | null
  selectedBranch: Warehouse | null
  isAuthenticated: boolean
  routesRegistered: boolean
  isSuperAdmin: boolean
  permissionsStatus: PermissionsLifecycleStatus
  sessionGeneration: number
  branchGeneration: number
}

function initState(): State {
  return {
    accessToken: '',
    refreshToken: '',
    permissions: null,
    branches: null,
    selectedBranch: null,
    convertedPermissions: null,
    isAuthenticated: false,
    routesRegistered: false,
    isSuperAdmin: false,
    permissionsStatus: 'ready',
    sessionGeneration: 0,
    branchGeneration: 0,
  }
}

function permissionFingerprint(
  permissions: ConvertedPermissionTypes | null
): string {
  if (!permissions) return ''
  return JSON.stringify(permissions)
}

function branchFingerprint(branches: Array<Warehouse> | null): string {
  if (!branches) return ''
  return branches
    .map((b) => b.id)
    .sort((a, b) => a - b)
    .join(',')
}

export const useAuthStore = defineStore('auth', {
  state: () => ({ ...initState() }),
  getters: {
    permissionsReady: (state) =>
      state.isAuthenticated && state.permissionsStatus === 'ready',

    hasPermission: (state) => {
      return (module: PermissionModule, action: PermissionAction): boolean => {
        if (state.permissionsStatus !== 'ready' || !state.convertedPermissions) {
          return false
        }

        const moduleKey = module.charAt(0).toUpperCase() + module.slice(1)
        const modulePermissions =
          state.convertedPermissions[
            moduleKey as keyof ConvertedPermissionTypes
          ]

        if (!modulePermissions) return false

        return modulePermissions[action] === true
      }
    },

    hasAnyPermission: (state) => {
      return (
        checks: Array<{ module: PermissionModule; action: PermissionAction }>
      ): boolean => {
        if (state.permissionsStatus !== 'ready') return false
        return checks.some((check) => {
          const moduleKey =
            check.module.charAt(0).toUpperCase() + check.module.slice(1)
          const modulePermissions =
            state.convertedPermissions?.[
              moduleKey as keyof ConvertedPermissionTypes
            ]
          return modulePermissions?.[check.action] === true
        })
      }
    },

    hasAllPermissions: (state) => {
      return (
        checks: Array<{ module: PermissionModule; action: PermissionAction }>
      ): boolean => {
        if (state.permissionsStatus !== 'ready') return false
        return checks.every((check) => {
          const moduleKey =
            check.module.charAt(0).toUpperCase() + check.module.slice(1)
          const modulePermissions =
            state.convertedPermissions?.[
              moduleKey as keyof ConvertedPermissionTypes
            ]
          return modulePermissions?.[check.action] === true
        })
      }
    },

    getModulePermissions: (state) => {
      return (module: PermissionModule): ModulePermissions | null => {
        if (state.permissionsStatus !== 'ready' || !state.convertedPermissions) {
          return null
        }

        const moduleKey = module.charAt(0).toUpperCase() + module.slice(1)
        return (
          state.convertedPermissions[
            moduleKey as keyof ConvertedPermissionTypes
          ] || null
        )
      }
    },
  },
  actions: {
    convertPermissions(Permissions: PermissionTypes): ConvertedPermissionTypes {
      const converted: ConvertedPermissionTypes = {}

      Object.keys(Permissions).forEach((moduleKey) => {
        const modulePermissions =
          Permissions[moduleKey as keyof PermissionTypes]

        if (modulePermissions && Array.isArray(modulePermissions)) {
          const modulePerms: ModulePermissions = {}

          modulePermissions.forEach((perm) => {
            modulePerms[perm.name] = true
          })

          converted[moduleKey as keyof ConvertedPermissionTypes] = modulePerms
        }
      })

      return converted
    },

    endSession(_reason: SessionEndReason = 'logout') {
      const endedGeneration = this.sessionGeneration
      this.sessionGeneration = nextGeneration(this.sessionGeneration)
      abortSessionRequests()
      notifySessionEnded(endedGeneration)
      clearSessionQueryCache()
      clearIntendedNavigation()

      const generation = this.sessionGeneration
      const branchGeneration = this.branchGeneration
      Object.assign(this, initState())
      this.sessionGeneration = generation
      this.branchGeneration = branchGeneration
      this.permissionsStatus = 'ready'
      this.isAuthenticated = false
    },

    logout() {
      this.endSession('logout')
    },

    /**
     * تطبيق حمولة مصادقة ضمن جيل محدد.
     * refresh العادي لا يلغي الطلب المنتظر؛ تغيّر الصلاحيات/الفروع يبطل البيانات غير المسموحة.
     */
    applyAuthPayload(
      data: AuthLoginSuccessResponse,
      expectedGeneration: number,
      options?: { source?: 'login' | 'refresh' | 'bootstrap' }
    ): boolean {
      if (expectedGeneration !== this.sessionGeneration) {
        return false
      }

      const prevPerms = permissionFingerprint(this.convertedPermissions)
      const prevBranches = branchFingerprint(this.branches)

      this.accessToken = data.accessToken
      this.refreshToken = data.refreshToken
      this.permissions = data.permissions
      this.branches = data.branches as Warehouse[]
      this.convertedPermissions = this.convertPermissions(data.permissions)
      this.isAuthenticated = true
      this.isSuperAdmin = Boolean(data.isSuperAdmin)
      this.permissionsStatus = 'ready'

      this.syncSelectedBranchAfterAuth()

      const nextPerms = permissionFingerprint(this.convertedPermissions)
      const nextBranches = branchFingerprint(this.branches)
      const scopeChanged =
        prevPerms !== nextPerms || prevBranches !== nextBranches

      if (options?.source === 'refresh') {
        // لا cancel للطلب الذي ينتظر إعادة المحاولة بعد تدوير التوكن
        if (scopeChanged) {
          invalidateSessionQueries()
        }
      } else if (scopeChanged && options?.source === 'bootstrap') {
        invalidateSessionQueries()
      }

      return true
    },

    syncSelectedBranchAfterAuth() {
      const branches = this.branches ?? []
      if (branches.length === 0) {
        this.selectedBranch = null
        return
      }

      const stillValid =
        this.selectedBranch &&
        branches.some((b) => b.id === this.selectedBranch!.id)

      if (!stillValid) {
        this.selectedBranch = branches[0]
      }
    },

    replaceSession(data: AuthLoginSuccessResponse) {
      this.endSession('replace_account')
      const generation = this.sessionGeneration
      this.applyAuthPayload(data, generation, { source: 'login' })
    },

    markPermissionsLoading() {
      this.permissionsStatus = 'loading'
    },

    markPermissionsError() {
      this.permissionsStatus = 'error'
    },

    markPermissionsReady() {
      this.permissionsStatus = 'ready'
    },

    reclaimPersistedSession() {
      if (this.refreshToken) {
        this.isAuthenticated = true
        this.permissionsStatus = 'loading'
        this.convertedPermissions = null
        this.permissions = null
        return true
      }

      this.endSession('bootstrap_failed')
      return false
    },

    changeSelectedBranch(branch: Warehouse) {
      this.branchGeneration = nextGeneration(this.branchGeneration)
      abortBranchRequests()
      cancelBranchQueries()
      this.selectedBranch = branch
    },
  },
  persist: {
    storage:
      typeof window !== 'undefined'
        ? window.localStorage
        : {
            getItem: () => null,
            setItem: () => undefined,
          },
    pick: [
      'accessToken',
      'refreshToken',
      'branches',
      'selectedBranch',
      'isAuthenticated',
      'isSuperAdmin',
      'routesRegistered',
    ],
    afterHydrate: (ctx) => {
      const store = ctx.store as ReturnType<typeof useAuthStore>
      if (store.refreshToken) {
        store.reclaimPersistedSession()
      } else {
        store.permissionsStatus = 'ready'
        store.isAuthenticated = false
      }
    },
  },
})

export default useAuthStore
