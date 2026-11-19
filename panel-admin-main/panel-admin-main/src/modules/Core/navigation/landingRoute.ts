import { evaluateAccess } from '@/modules/Core/access/evaluateAccess'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'
import { getReadyNavigationSections } from './navigation.registry'
import { requirementForNavSectionId } from './panelTabAccess'
import type { NavigationSectionDefinition } from './navigation.types'

const HOME_ROUTE = 'HomeMainRoute'

/**
 * أول قسم يفتحه الحساب فعلاً.
 *
 * حساب بتبويب واحد — مسؤول الأرشيف مثلاً — كان يهبط على الرئيسية المقفلة
 * فيرى «القسم غير متاح» بعد كل دخول. فنُنزله حيث يملك.
 */
export function resolveLandingRouteName(
  hasPermission: (module: PermissionModule, action: PermissionAction) => boolean,
): string {
  const allowed = (section: NavigationSectionDefinition) =>
    evaluateAccess({
      permissionsReady: true,
      isAuthenticated: true,
      hasPermission,
      requirement: requirementForNavSectionId(section.id, section.requirement),
    }) === 'allowed'

  const sections = getReadyNavigationSections()

  const home = sections.find((s) => s.routeName === HOME_ROUTE)
  if (home && allowed(home)) return HOME_ROUTE

  for (const section of sections) {
    if (section.children?.length) {
      const child = section.children.find((c) => c.routeName && allowed(c))
      if (child) return child.routeName
    }

    if (section.routeName && allowed(section)) return section.routeName
  }

  // لا شيء مفتوح — الرئيسية تعرض «القسم غير متاح» وهي الجواب الصحيح عندها.
  return HOME_ROUTE
}
