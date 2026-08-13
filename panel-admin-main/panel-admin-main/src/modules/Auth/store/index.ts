import { defineStore } from 'pinia'
import type {
  AuthTokens,
  ConvertedPermissionTypes,
  ModulePermissions,
  PermissionAction,
  PermissionModule,
  PermissionTypes,
} from '../types/model'
import { type AuthLoginSuccessResponse } from '../types/api'
import type { Warehouse } from '@/modules/Warehouse/types/model'

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
}

function initState() {
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
  } as State
}

export const useAuthStore = defineStore('auth', {
  state: () => ({ ...initState() }),
  getters: {
    hasPermission: (state) => {
      return (module: PermissionModule, action: PermissionAction): boolean => {
        if (!state.convertedPermissions) return false

        const moduleKey = module.charAt(0).toUpperCase() + module.slice(1)
        const modulePermissions =
          state.convertedPermissions[
            moduleKey as keyof ConvertedPermissionTypes
          ]

        if (!modulePermissions) return false

        return modulePermissions[action] === true
      }
    },

    // Check if user has any of the specified permissions
    hasAnyPermission: (state) => {
      return (
        checks: Array<{ module: PermissionModule; action: PermissionAction }>
      ): boolean => {
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

    // Check if user has all specified permissions
    hasAllPermissions: (state) => {
      return (
        checks: Array<{ module: PermissionModule; action: PermissionAction }>
      ): boolean => {
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

    // Get all permissions for a specific module
    getModulePermissions: (state) => {
      return (module: PermissionModule): ModulePermissions | null => {
        if (!state.convertedPermissions) return null

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
    setLoginState(data: AuthLoginSuccessResponse) {
      this.accessToken = data.accessToken
      this.refreshToken = data.refreshToken
      this.permissions = data.permissions
      this.branches = data.branches
      this.convertedPermissions = this.convertPermissions(data.permissions)
      this.selectedBranch = data.branches[0]
      this.isAuthenticated = true
      this.isSuperAdmin = data.isSuperAdmin
    },
    setToken(data: AuthTokens) {
      this.accessToken = data.accessToken
      this.refreshToken = data.refreshToken
    },
    logout() {
      this.isAuthenticated = false
      this.$reset()
    },
    convertPermissions(Permissions: PermissionTypes): ConvertedPermissionTypes {
      const converted: ConvertedPermissionTypes = {}

      // Loop through each module in permissions
      Object.keys(Permissions).forEach((moduleKey) => {
        const modulePermissions =
          Permissions[moduleKey as keyof PermissionTypes]

        if (modulePermissions && Array.isArray(modulePermissions)) {
          // Convert array of permissions to ModulePermissions object
          const modulePerms: ModulePermissions = {}

          modulePermissions.forEach((perm) => {
            modulePerms[perm.name] = true
          })

          converted[moduleKey as keyof ConvertedPermissionTypes] = modulePerms
        }
      })

      return converted
    },
    changeSelectedBranch(branch: Warehouse) {
      this.selectedBranch = branch
    },
  },
  persist: {
    storage: window.localStorage,
  },
})

export default useAuthStore
