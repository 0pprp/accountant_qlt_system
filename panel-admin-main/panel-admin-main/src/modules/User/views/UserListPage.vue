<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canCreateUser"
          :to="{ name: 'UserCreateRoute' }"
          class="bg-primary rounded-2xl text-white inline-flex items-center gap-2 px-4 py-2"
        >
          {{ $t('user.createUser') }}
          <SvgIcon name="addCircle" />
        </RouterLink>
      </template>

      <template #calendar>
        <HeaderDatePicker />
      </template>
    </TheHeader>

    <UserList v-if="canReadUser" />
  </div>
</template>

<script setup lang="ts">
import UserList from '../components/UserList/UserList.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { can } = usePermission()
const canReadUser = can('User', 'Read')
const canCreateUser = can('User', 'Create')
</script>
