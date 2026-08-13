// utils/permission.ts

interface BackendPermission {
  [module: string]: Array<{
    id: number
    name: string
  }>
}

interface FrontendPermission {
  [module: string]: {
    [action: string]: boolean
  }
}

// تبدیل PascalCase به camelCase
function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1)
}

// تبدیل action name به lowercase
function normalizeAction(action: string): string {
  return action.charAt(0).toLowerCase() + action.slice(1)
}

// تبدیل دیتای بکند به فرمت فرانت
export function transformPermissions(
  backendPermissions: BackendPermission,
  userPermissionIds: number[] // آرایه‌ای از id هایی که یوزر داره
): FrontendPermission {
  const result: FrontendPermission = {}

  Object.keys(backendPermissions).forEach((moduleName) => {
    const moduleCamelCase = toCamelCase(moduleName)
    result[moduleCamelCase] = {}

    backendPermissions[moduleName].forEach((permission) => {
      const actionName = normalizeAction(permission.name)
      // چک می‌کنیم که آیا این permission در لیست یوزر هست یا نه
      result[moduleCamelCase][actionName] = userPermissionIds.includes(
        permission.id
      )
    })
  })

  return result
}
