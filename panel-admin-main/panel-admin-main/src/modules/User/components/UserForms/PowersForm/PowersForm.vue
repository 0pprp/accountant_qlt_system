<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.powers.stepTitle') }}
    </span>

    <div
      v-for="(permissionActions, permissionName) in data?.permissions"
      :key="permissionName"
      class="col-span-12 gap-6 flex flex-row justify-between bg-surface-1 border border-line p-2 rounded-2xl"
    >
      <div class="flex items-center">
        <div>
          <span class="text-gray-700">
            {{ permissionActions[0].scopeDisplayName }}
          </span>
          <br />
          <span class="text-gray-300">
            {{ $t('user.permission.description') }}
            {{ permissionActions[0].scopeDisplayName }}</span
          >
        </div>
      </div>

      <div class="flex flex-wrap justify-end gap-2 overflow-auto">
        <ToggleSwitchField
          v-for="action in permissionActions"
          :key="action.id"
          :readonly="isReadOnly(action.id) || userFormMode === FormMode.IsView"
          v-model="permissionStates[permissionName][action.id]"
          :iconName="getIconName(action.name)"
          :color="getColor(action.name)"
          :label="action.displayName"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Props } from './PowersForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import ToggleSwitchField from '@/modules/Core/components/base/Fields/ToggleSwitchField/ToggleSwitchField.vue'
import { useRolePermissionDataQuery } from '@/modules/Role/requests/queries'
import { usePermissionDataQuery } from '@/modules/User/requests/queries/usePermissionListQuery'
import type { Permission, RolePermission } from '@/modules/User/types/model'

const props = defineProps<Props>()

const roleId = computed(() => props.roleId)
const userFormMode = computed(() => props.userFormMode)
const selectedPermissions = computed(() => props.selectedPermissions || [])

const { data } = usePermissionDataQuery()

const { data: rolePermissions } = useRolePermissionDataQuery(roleId)

const permissionStates = ref<{ [key: string]: { [key: string]: boolean } }>({})

const rolePermissionIds = computed(() => {
  if (!rolePermissions.value?.permissions) return []

  const ids: number[] = []
  Object.values(rolePermissions.value.permissions).forEach(
    (permissionList: RolePermission[]) => {
      permissionList.forEach((permission) => {
        ids.push(permission.id)
      })
    }
  )
  return ids
})

const selectedPermissionIds = computed(() => {
  if (!selectedPermissions.value) return []
  return selectedPermissions.value.map(
    (permission: RolePermission) => permission.id
  )
})

function isReadOnly(actionId: number): boolean {
  return rolePermissionIds.value.includes(actionId)
}

watch(
  [data, rolePermissions, selectedPermissionIds],
  ([permissionsData, , selectedIds]) => {
    if (!permissionsData?.permissions) return

    const newPermissionStates: { [key: string]: { [key: string]: boolean } } =
      {}

    Object.entries(permissionsData.permissions).forEach(
      ([permissionName, actions]) => {
        newPermissionStates[permissionName] = {}

        actions.forEach((action: Permission) => {
          const isRolePermission = rolePermissionIds.value.includes(action.id)
          const isSelectedPermission = selectedIds.includes(action.id)

          newPermissionStates[permissionName][action.id] =
            isRolePermission || isSelectedPermission
        })
      }
    )

    permissionStates.value = newPermissionStates
  },
  { immediate: true }
)

const permissionIds = computed(() => {
  const selectedIds = new Set<number>()

  rolePermissionIds.value.forEach((id) => selectedIds.add(id))

  Object.values(permissionStates.value).forEach((actions) => {
    Object.entries(actions).forEach(([actionId, isSelected]) => {
      if (isSelected) {
        selectedIds.add(Number(actionId))
      }
    })
  })

  return Array.from(selectedIds)
})

function getIconName(action: string) {
  switch (action) {
    case 'Create':
      return 'boldAddCircle'
    case 'Read':
      return 'boldEye'
    case 'Update':
      return 'boldPencil'
    case 'Delete':
      return 'boldTrash'
    case 'Collect':
      return 'boldCheckCircle'
    default:
      return 'boldAddCircle'
  }
}

function getColor(action: string) {
  switch (action) {
    case 'Create':
      return 'info'
    case 'Read':
      return 'purple'
    case 'Update':
      return 'warning'
    case 'Delete':
      return 'error'
    case 'Collect':
      return 'success'
    default:
      return 'info'
  }
}

defineExpose({ permissionIds })
</script>
