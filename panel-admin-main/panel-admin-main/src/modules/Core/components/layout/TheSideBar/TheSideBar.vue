<template>
  <nav
    class="p-2 flex items-center justify-center h-full sidebar-menu sticky top-1 transition-all duration-200"
    :class="[
      showLabels ? 'min-w-[260px]' : 'min-w-[80px]',
      mobile ? 'sidebar-mobile' : '',
    ]"
    :aria-label="$t('sidebar.navigationLabel')"
  >
    <div
      class="flex flex-col justify-center bg-primary rounded-2xl py-6 px-3 h-full w-full overflow-y-auto"
    >
      <Menu
        :model="menuItems"
        class="h-full w-full flex flex-col justify-between"
      >
        <template #start>
          <span
            class="flex items-center justify-center gap-x-3 border-b border-white/40 pb-2"
          >
            <Avatar image="/logo.svg" shape="circle" />

            <span v-if="showLabels" class="text-xl text-white font-semibold">{{
              $t('brandName')
            }}</span>
          </span>

          <button
            v-if="!mobile"
            type="button"
            class="pi pi-angle-left bg-black p-1 rounded-full text-white sidebarIcon absolute left-0 top-[4.8rem] transition duration-200"
            :class="[rail ? 'rotate-180' : '']"
            :aria-label="$t('sidebar.toggleRail')"
            @click="toggleRail"
          />
        </template>

        <template #item="{ item, props }">
          <div :key="item.id || item.title">
            <template v-if="item.children?.length">
              <button
                type="button"
                class="flex items-center font-medium w-full text-white gap-2 my-1.5 py-2 px-2 min-h-11 rounded-lg"
                :aria-expanded="isSectionExpanded(item.id)"
                :aria-controls="sectionChildrenDomId(item.id)"
                :title="item.title"
                @click="toggleSection(item.id)"
              >
                <SvgIcon :name="item.icon!" class="w-7.5 h-7.5 text-white" />
                <span v-if="showLabels" class="flex-1 text-start">{{ item.title }}</span>
                <i
                  :class="isSectionExpanded(item.id) ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
                  aria-hidden="true"
                />
              </button>
              <div
                v-if="isSectionExpanded(item.id)"
                :id="sectionChildrenDomId(item.id)"
                class="ms-4 ps-3 border-s border-white/30"
              >
                <template v-for="child in item.children" :key="child.id">
                  <template v-if="child.children?.length">
                    <button
                      type="button"
                      class="flex items-center text-white gap-2 px-3 py-3 my-1 min-h-11 w-full text-start text-sm"
                      :aria-expanded="isSectionExpanded(child.id)"
                      @click="toggleSection(child.id)"
                    >
                      <span class="flex-1">{{ child.title }}</span>
                      <i
                        :class="isSectionExpanded(child.id) ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
                        aria-hidden="true"
                      />
                    </button>
                    <div
                      v-if="isSectionExpanded(child.id)"
                      class="ms-3 ps-2 border-s border-white/20"
                    >
                      <template v-for="grand in child.children" :key="grand.id">
                        <router-link
                          v-if="grand.state === 'allowed'"
                          :to="{ name: grand.link, query: grand.query }"
                          class="flex items-center text-white gap-2 px-3 py-2 my-1 rounded-lg text-sm"
                          :class="isNavChildActive(grand) ? 'activeSidebarItem' : ''"
                          @click="onAllowedNavigate(grand.link)"
                        >{{ grand.title }}</router-link>
                        <button
                          v-else
                          type="button"
                          class="flex items-center text-white/80 gap-2 px-3 py-2 min-h-11 w-full text-start text-sm"
                          @click="openLocked(grand.link)"
                        >
                          <i class="pi pi-lock" aria-hidden="true" />{{ grand.title }}
                        </button>
                      </template>
                    </div>
                  </template>
                  <router-link
                    v-else-if="child.state === 'allowed'"
                    :to="{ name: child.link, query: child.query }"
                    class="flex items-center text-white gap-2 px-3 py-3 my-1 rounded-lg text-sm"
                    :class="isNavChildActive(child) ? 'activeSidebarItem' : ''"
                    :title="child.title"
                    @click="onAllowedNavigate(child.link)"
                  >{{ child.title }}</router-link>
                  <button
                    v-else
                    type="button"
                    class="flex items-center text-white/80 gap-2 px-3 py-3 min-h-11 w-full text-start"
                    @click="openLocked(child.link)"
                  >
                    <i class="pi pi-lock" aria-hidden="true" />{{ child.title }}
                  </button>
                </template>
              </div>
            </template>
            <!-- عنصر مسموح: رابط فعلي -->
            <router-link
              v-else-if="item.link && item.state !== 'locked'"
              :to="{ name: item.link, query: item.query }"
              class="flex items-center font-medium w-full text-white gap-2 my-1.5 py-2 px-2 min-h-11 rounded-lg"
              :class="[
                showLabels ? '' : 'justify-center',
                route.name === item.link ? 'activeSidebarItem' : '',
              ]"
              v-bind="props.action"
              :title="showLabels ? undefined : item.title"
              @click="onAllowedNavigate(String(item.link))"
            >
              <div class="relative shrink-0">
                <SvgIcon :name="item.icon!" class="w-7.5 h-7.5 text-white" />
                <span
                  v-if="!showLabels && item.badgeCount && item.badgeCount > 0"
                  class="absolute -top-1 -left-1 inline-flex min-w-4 h-4 items-center justify-center rounded bg-warning px-1 text-[10px] font-semibold text-black leading-none"
                >
                  {{ item.badgeCount > 99 ? '99+' : item.badgeCount }}
                </span>
              </div>
              <span v-if="showLabels" class="leading-[32px] flex-1">{{
                item.title
              }}</span>
              <span
                v-if="showLabels && item.badgeCount && item.badgeCount > 0"
                class="inline-flex min-w-5 h-5 items-center justify-center rounded bg-warning px-1.5 text-xs font-semibold text-black shrink-0"
              >
                {{ item.badgeCount > 99 ? '99+' : item.badgeCount }}
              </span>
            </router-link>

            <!-- عنصر مقفل: قابل للوحة المفاتيح، لا يحمّل الصفحة -->
            <button
              v-else-if="item.link && item.state === 'locked'"
              type="button"
              class="flex items-center font-medium w-full text-white/80 gap-2 my-1.5 py-2 px-2 min-h-11 rounded-lg opacity-80 hover:opacity-100 focus:outline-none focus-visible:ring-2 focus-visible:ring-white/70"
              :class="[showLabels ? '' : 'justify-center']"
              :title="lockedTitle(item.title)"
              :aria-label="lockedTitle(item.title)"
              @click="openLocked(item.link)"
              @keydown.enter.prevent="openLocked(item.link)"
              @keydown.space.prevent="openLocked(item.link)"
            >
              <div class="relative shrink-0">
                <SvgIcon :name="item.icon!" class="w-7.5 h-7.5 text-white" />
                <i
                  class="pi pi-lock absolute -bottom-1 -left-1 text-[10px] bg-black/50 rounded-full p-0.5"
                  aria-hidden="true"
                />
              </div>
              <span v-if="showLabels" class="leading-[32px] flex-1 text-start">{{
                item.title
              }}</span>
              <i
                v-if="showLabels"
                class="pi pi-lock text-xs opacity-90"
                aria-hidden="true"
              />
            </button>

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
        </template>

        <template #end>
          <div>
            <Button
              class="flx items-center font-medium text-white gap-2 min-h-11"
              @click="showBranches"
            >
              <SvgIcon name="boldLayer" class="w-7.5 h-7.5 text-white" />
              <span v-if="showLabels" class="font-semibold">{{ branchName }}</span>
            </Button>

            <TheSideBarFooter :rail="showLabels" />
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
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import TheSideBarFooter from './TheSideBarFooter/TheSideBarFooter.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { useSideBar } from '@/modules/Core/composable'
import useAuthStore from '@/modules/Auth/store'
import type { Warehouse } from '@/modules/Warehouse/types/model'
import type { NavigationSidebarItem } from '@/modules/Core/navigation/navigation.types'

const props = withDefaults(
  defineProps<{
    mobile?: boolean
  }>(),
  { mobile: false }
)

const emit = defineEmits<{
  railChanged: [isExpanded: boolean]
  navigate: []
}>()

const route = useRoute()
const router = useRouter()
const { sideBarList, isSidebarLoading } = useSideBar()
const authStore = useAuthStore()
const isShowingBranches = ref(false)
const rail = ref(true)
/** توسعة عامة بمفتاح section.id — لا حالة مشتركة بين الأقسام */
const expandedBySectionId = ref<Record<string, boolean>>({})
const { t } = useI18n()

function sectionChildrenDomId(sectionId: string) {
  return `nav-section-${sectionId}-children`
}

function isSectionExpanded(sectionId: string | undefined) {
  if (!sectionId) return false
  return !!expandedBySectionId.value[sectionId]
}

function toggleSection(sectionId: string | undefined) {
  if (!sectionId) return
  expandedBySectionId.value = {
    ...expandedBySectionId.value,
    [sectionId]: !expandedBySectionId.value[sectionId],
  }
}

function autoExpandForRoute() {
  const items = sideBarList.value
  const nextExpanded: Record<string, boolean> = {}
  for (const item of items) {
    if (!item.children?.length || !item.id) continue
    const childActive = item.children.some(
      (c) =>
        c.link === route.name ||
        (c.link === route.name &&
          (!c.query?.tab || String(route.query.tab ?? '') === c.query.tab))
    )
    const pathHint =
      (item.id === 'sale' && route.path.startsWith('/sale')) ||
      (item.id === 'managementBranch' && route.path.startsWith('/management/branch')) ||
      (item.id === 'managementDelegate' && route.path.startsWith('/management/delegate')) ||
      (item.id === 'management' && route.path.startsWith('/management') && !/^\/management\/(branch|delegate)(\/|$)/.test(route.path))
    if (childActive || pathHint) {
      nextExpanded[item.id] = true
    }
  }
  expandedBySectionId.value = nextExpanded
}

watch(
  () => [route.fullPath, sideBarList.value] as const,
  () => autoExpandForRoute(),
  { immediate: true, deep: true }
)

const mobile = computed(() => props.mobile)
const showLabels = computed(() => props.mobile || rail.value)

const branchName = computed(() => {
  return isShowingBranches.value
    ? t('role.table.branchName')
    : authStore.selectedBranch?.name || ''
})

const menuItems = computed(() => {
  if (isShowingBranches.value) {
    return authStore.branches ?? []
  }
  if (isSidebarLoading.value) return []
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

function lockedTitle(title: string) {
  return `${title} — ${t('access.lockedHint')}`
}

function isNavChildActive(child: {
  link?: string
  query?: Record<string, string>
  id?: string
}) {
  if (child.id === 'managementBranch') {
    return (
      typeof route.name === 'string' &&
      route.name.startsWith('ManagementBranch')
    )
  }
  if (child.id === 'managementDelegate') {
    return (
      typeof route.name === 'string' &&
      route.name.startsWith('ManagementDelegate')
    )
  }
  if (route.name !== child.link) return false
  const childTab = child.query?.tab
  if (childTab) return String(route.query.tab ?? '') === childTab
  return true
}

function onAllowedNavigate(routeName: string) {
  emit('navigate')
  if (route.name === routeName) {
    // الضغط على القسم الحالي يغلق الـ Drawer دون انتظار تغير المسار
    return
  }
}

function openLocked(routeName: string) {
  emit('navigate')
  void router.push({
    name: 'AccessDeniedRoute',
    query: { from: routeName },
  })
}
</script>

<style scoped>
@reference '../../../../../assets/styles/main.css';

.activeSidebarItem {
  @apply bg-white text-primary rounded-lg;
}

:deep(.activeSidebarItem .text-white),
:deep(.activeSidebarItem svg) {
  color: var(--color-primary) !important;
}

.sidebarIcon {
  transform: translate(-50%, -50%);
}

@media (prefers-reduced-motion: reduce) {
  .transition,
  .transition-all,
  .duration-200 {
    transition: none !important;
  }
}
</style>
