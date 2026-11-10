<template>
  <div
    class="p-2 md:p-2 flex justify-between h-screen overflow-hidden w-full bg-background relative"
  >
    <!-- سطح المكتب -->
    <div v-if="isDesktop" class="hidden md:flex h-full shrink-0">
      <TheSideBar @rail-changed="onRailChanged" @navigate="onSidebarNavigate" />
    </div>

    <!-- هاتف: زر فتح القائمة -->
    <button
      v-show="!mobileOpen"
      ref="menuButtonRef"
      type="button"
      class="md:hidden fixed z-40 top-3 inset-inline-start-3 inline-flex h-11 w-11 items-center justify-center rounded-full bg-primary text-white shadow-md"
      :aria-label="$t('sidebar.openMenu')"
      :aria-expanded="mobileOpen"
      @click="openMobile"
    >
      <i class="pi pi-bars" aria-hidden="true" />
    </button>

    <!-- هاتف: Drawer -->
    <div
      v-if="mobileOpen && !isDesktop"
      class="md:hidden fixed inset-0 z-50 flex"
      role="dialog"
      aria-modal="true"
      :aria-label="$t('sidebar.navigationLabel')"
    >
      <button
        type="button"
        class="absolute inset-0 bg-black/40"
        :aria-label="$t('sidebar.closeMenu')"
        @click="closeMobile"
      />
      <div
        ref="drawerRef"
        class="relative z-10 h-full w-[min(86vw,280px)] me-auto bg-background p-2 shadow-xl flex flex-col"
      >
        <div class="flex justify-end p-1 shrink-0">
          <button
            type="button"
            class="inline-flex h-11 w-11 items-center justify-center rounded-full text-[#00201E]"
            :aria-label="$t('sidebar.closeMenu')"
            @click="closeMobile"
          >
            <i class="pi pi-times" aria-hidden="true" />
          </button>
        </div>
        <div class="flex-1 min-h-0">
          <TheSideBar
            mobile
            @rail-changed="() => undefined"
            @navigate="onSidebarNavigate"
          />
        </div>
      </div>
    </div>

    <main
      class="p-4 md:p-6 flex-grow main overflow-auto w-full"
      :class="isDesktop ? desktopMainClass : 'max-w-full'"
      :aria-hidden="mobileOpen && !isDesktop ? 'true' : undefined"
      :inert="mobileOpen && !isDesktop ? true : undefined"
    >
      <div
        v-if="showBootPlaceholder"
        class="boot-skeleton rounded-xl bg-white/70 min-h-16 mb-4"
        aria-busy="true"
      />
      <slot v-else />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useMediaQuery } from '@vueuse/core'
import { useRoute } from 'vue-router'
import TheSideBar from '@/modules/Core/components/layout/TheSideBar/TheSideBar.vue'
import { useAuthStore } from '@/modules/Auth/store'
import { useRouteAccessRevalidation } from '@/modules/Core/composable/useRouteAccessRevalidation'

defineOptions({
  name: 'TheDefaultLayout',
})

const authStore = useAuthStore()
const route = useRoute()
const isRailExpanded = ref(true)
const mobileOpen = ref(false)
const menuButtonRef = ref<HTMLButtonElement | null>(null)
const drawerRef = ref<HTMLElement | null>(null)

const isDesktop = useMediaQuery('(min-width: 768px)')

useRouteAccessRevalidation()

const showBootPlaceholder = computed(
  () =>
    authStore.isAuthenticated && authStore.permissionsStatus === 'loading'
)

const desktopMainClass = computed(() =>
  isRailExpanded.value ? 'main-with-rail' : 'main-with-rail-collapsed'
)

function onRailChanged(railState: boolean) {
  isRailExpanded.value = railState
}

function openMobile() {
  mobileOpen.value = true
}

function closeMobile() {
  if (!mobileOpen.value) return
  mobileOpen.value = false
  nextTick(() => menuButtonRef.value?.focus())
}

function onSidebarNavigate() {
  closeMobile()
}

const FOCUSABLE =
  'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'

function getFocusable(): HTMLElement[] {
  if (!drawerRef.value) return []
  return Array.from(
    drawerRef.value.querySelectorAll<HTMLElement>(FOCUSABLE)
  ).filter(
    (el) =>
      !el.hasAttribute('disabled') && el.getAttribute('aria-hidden') !== 'true'
  )
}

function onKeydown(event: KeyboardEvent) {
  if (!mobileOpen.value || isDesktop.value) return

  if (event.key === 'Escape') {
    event.preventDefault()
    closeMobile()
    return
  }

  if (event.key !== 'Tab') return

  const nodes = getFocusable()
  if (nodes.length === 0) return

  const first = nodes[0]
  const last = nodes[nodes.length - 1]
  const active = document.activeElement as HTMLElement | null

  if (event.shiftKey && active === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && active === last) {
    event.preventDefault()
    first.focus()
  }
}

watch(mobileOpen, async (open) => {
  if (!open) return
  await nextTick()
  getFocusable()[0]?.focus()
})

watch(isDesktop, (desktop) => {
  if (desktop && mobileOpen.value) {
    mobileOpen.value = false
  }
})

watch(
  () => route.fullPath,
  () => {
    if (mobileOpen.value) closeMobile()
  }
)

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
})
</script>

<style scoped>
.main-with-rail {
  max-width: calc(100% - 260px);
}
.main-with-rail-collapsed {
  max-width: calc(100% - 80px);
}

.boot-skeleton {
  animation: boot-pulse 1.2s ease-in-out infinite;
}

@keyframes boot-pulse {
  0%,
  100% {
    opacity: 0.55;
  }
  50% {
    opacity: 1;
  }
}

@media (prefers-reduced-motion: reduce) {
  .boot-skeleton {
    animation: none;
  }
}
</style>
