<template>
  <div v-if="userInfo" class="flex flex-row h-full gap-6">
    <div class="w-full border border-line rounded-2xl overflow-auto">
      <UserTabs :userInfo="userInfo" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed } from 'vue'
import UserTabs from '../components/UserTabs/UserTabs.vue'
import { useUserByIdDataQuery } from '../requests/queries'

const route = useRoute()

const userId = computed(() => Number(route.params.userId))

const { data: userInfo } = useUserByIdDataQuery(userId)
</script>
