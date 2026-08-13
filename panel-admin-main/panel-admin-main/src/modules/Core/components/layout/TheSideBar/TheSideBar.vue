<template>
  <nav
    class="p-2 flex items-center justify-center h-full sidebar-menu sticky top-1 transition-all"
    :class="[rail ? 'min-w-[260px]' : 'min-w-[113px]']"
  >
    <div
      class="flex flex-col justify-center bg-primary rounded-2xl py-6 px-4 h-full w-full overflow-y-auto"
    >
      <Menu
        :model="menuItems"
        class="h-full w-full flex flex-col justify-between"
      >
        <template #start>
          <span
            class="flex items-center justify-center gap-x-3 border-b border-white pb-2"
          >
            <Avatar image="/logo.svg" shape="circle" />

            <span v-if="rail" class="text-xl text-white font-semibold">{{
              $t('brandName')
            }}</span>
          </span>

          <i
            @click="toggleRail"
            class="pi pi-angle-left bg-black p-1 rounded-full text-white sidebarIcon absolute left-0 top-[4.8rem] transition delay-100 duration-300"
            :class="[rail ? 'rotate-180' : '']"
          ></i>
        </template>

        <template #item="{ item, props }">
          <transition name="slide-fade" mode="out-in">
            <div :key="item.title">
              <router-link
                v-if="item.link"
                :to="{ name: item.link, query: item.query }"
                class="flex items-center font-medium w-full text-white gap-2 my-2 py-1.5 px-2"
                :class="[
                  rail ? '' : 'justify-center',
                  route.name === item.link ? 'activeSidebarItem' : '',
                ]"
                v-bind="props.action"
              >
                <div class="relative shrink-0">
                  <SvgIcon :name="item.icon!" class="w-7.5 h-7.5 text-white" />
                  <span
                    v-if="!rail && item.badgeCount && item.badgeCount > 0"
                    class="absolute -top-1 -left-1 inline-flex min-w-4 h-4 items-center justify-center rounded bg-warning px-1 text-[10px] font-semibold text-black leading-none"
                  >
                    {{ item.badgeCount > 99 ? '99+' : item.badgeCount }}
                  </span>
                </div>
                <span v-if="rail" class="leading-[32px] flex-1">{{
                  item.title
                }}</span>
                <span
                  v-if="rail && item.badgeCount && item.badgeCount > 0"
                  class="inline-flex min-w-5 h-5 items-center justify-center rounded bg-warning px-1.5 text-xs font-semibold text-black shrink-0"
                >
                  {{ item.badgeCount > 99 ? '99+' : item.badgeCount }}
                </span>
              </router-link>

              <Button
                v-else
                class="items-start text-start font-medium w-full text-white gap-2 my-2.5"
                @click="revertToSidebar(item as Warehouse)"
              >
                <span class="leading-[32px] w-full test-start px-4">{{
                  item.name
                }}</span>
              </Button>
            </div>
          </transition>
        </template>

        <template #end>
          <div>
            <Button
              class="flx items-center font-medium text-white gap-2"
              @click="showBranches"
            >
              <SvgIcon name="boldLayer" class="w-7.5 h-7.5 text-white" />
              <span v-if="rail" class="font-semibold">{{ branchName }}</span>
            </Button>

            <TheSideBarFooter :rail />
          </div>
        </template>
      </Menu>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import Menu from 'primevue/menu'
import Avatar from 'primevue/avatar'
import { useI18n } from 'vue-i18n'
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import TheSideBarFooter from './TheSideBarFooter/TheSideBarFooter.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { useSideBar } from '@/modules/Core/composable'
import useAuthStore from '@/modules/Auth/store'
import type { SideBarItem } from '@/modules/Core/types/model/sidebar'
import type { Warehouse } from '@/modules/Warehouse/types/model'

const emit = defineEmits<{
  railChanged: [isExpanded: boolean]
}>()

const route = useRoute()

const { sideBarList } = useSideBar()
const authStore = useAuthStore()
const isShowingBranches = ref(false)

const rail = ref(true)

const { t } = useI18n()

const branchName = computed(() => {
  return isShowingBranches.value
    ? t('role.table.branchName')
    : authStore.selectedBranch?.name || ''
})

const menuItems = computed<Array<SideBarItem> | Array<Warehouse>>(() => {
  if (isShowingBranches.value) {
    return authStore.branches ?? []
  }

  return sideBarList.value
})

function showBranches() {
  isShowingBranches.value = true
}

function toggleRail() {
  rail.value = !rail.value
  emit('railChanged', rail.value)
}

function revertToSidebar(branch: Warehouse) {
  isShowingBranches.value = false
  authStore.changeSelectedBranch(branch)
}
</script>

<style>
.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.25s ease;
}
.slide-fade-enter,
.slide-fade-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

.activeSidebarItem {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}
</style>
