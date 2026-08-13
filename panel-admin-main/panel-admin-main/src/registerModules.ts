import router from './router'
import i18n from '@/modules/Core/plugins/i18n'
import MainModule from '@/modules/Core'
import AuthModule from '@/modules/Auth'
import BranchModule from '@/modules/Branch'
import WarehouseModule from '@/modules/Warehouse'
import OrderModule from '@/modules/Order'
import UserModule from '@/modules/User'
import RoleModule from '@/modules/Role'
import SaleModule from '@/modules/Sale'
import PaymentModule from '@/modules/Payment'
import CustomerModule from '@/modules/Customer'
import PurchasesModule from '@/modules/Purchases'
import ExpensesModule from '@/modules/Expenses'
import SafeModule from '@/modules/Safe'
import ActivityLogModule from '@/modules/ActivityLog'
import NotificationModule from '@/modules/Notification'

function flattenModules(modules: Modules): Array<[string, Module]> {
  const entries: Array<[string, Module]> = []

  Object.keys(modules).forEach((moduleKey) => {
    const qualifiedKey = moduleKey
    const module = modules[moduleKey]
    entries.push([qualifiedKey, module])

    if (module.subModules) {
      entries.push(...flattenModules(module.subModules))
    }
  })

  return entries
}

export function registerModules(modules: Modules) {
  const flatModules = flattenModules(modules)

  flatModules.forEach(([moduleKey, module]) => {
    if (module.localization) {
      for (const locale in module.localization) {
        i18n.global.mergeLocaleMessage(
          locale,
          moduleKey === 'main'
            ? module.localization[locale]
            : {
                [moduleKey]: module.localization[locale],
              }
        )
      }
    }

    if (module.router) {
      module.router(router)
    }
  })
}

registerModules({
  main: MainModule,
  auth: AuthModule,
  branch: BranchModule,
  warehouse: WarehouseModule,
  order: OrderModule,
  role: RoleModule,
  user: UserModule,
  sale: SaleModule,
  payment: PaymentModule,
  customer: CustomerModule,
  purchases: PurchasesModule,
  expenses: ExpensesModule,
  safe: SafeModule,
  activityLog: ActivityLogModule,
  notification: NotificationModule,
})
