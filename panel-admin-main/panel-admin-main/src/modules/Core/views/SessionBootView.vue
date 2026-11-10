<template>
  <section
    class="min-h-screen flex flex-col items-center justify-center gap-4 px-6 bg-background text-[#00201E]"
    role="status"
    aria-live="polite"
  >
    <template v-if="status === 'loading'">
      <div
        class="boot-pulse rounded-xl bg-white/80 min-h-16 w-full max-w-sm"
        aria-busy="true"
      />
      <p class="text-sm m-0">{{ $t('access.sessionBootLoading') }}</p>
    </template>

    <template v-else>
      <h1 class="text-lg font-bold m-0">{{ $t('access.sessionBootErrorTitle') }}</h1>
      <p class="text-sm max-w-md text-center m-0 leading-6">
        {{ $t('access.sessionBootErrorMessage') }}
      </p>
      <div class="flex flex-wrap gap-3 justify-center">
        <button
          type="button"
          class="min-h-11 px-4 rounded-lg bg-primary text-white font-medium"
          :disabled="retrying"
          @click="onRetry"
        >
          {{ $t('access.sessionBootRetry') }}
        </button>
        <button
          type="button"
          class="min-h-11 px-4 rounded-lg border border-primary text-primary font-medium bg-white"
          @click="onLogin"
        >
          {{ $t('words.login') }}
        </button>
      </div>
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import {
  bootstrapSession,
  retrySessionBootstrap,
} from '@/modules/Auth/session/bootstrapSession'
import { consumeIntendedNavigation } from '@/modules/Auth/session/intendedNavigation'

const router = useRouter()
const authStore = useAuthStore()
const retrying = ref(false)

const status = computed(() => authStore.permissionsStatus)

async function resumeAfterReady() {
  const intended = consumeIntendedNavigation()
  if (intended?.fullPath) {
    // الحارس يعيد تقييم الوجهة بعد الجاهزية
    await router.replace(intended.fullPath)
    return
  }
  await router.replace({ name: 'HomeMainRoute' })
}

watch(
  () => [authStore.permissionsStatus, authStore.isAuthenticated] as const,
  async ([value, authed]) => {
    if (value === 'ready' && authed) {
      await resumeAfterReady()
    }
  },
  { immediate: true }
)

async function onRetry() {
  retrying.value = true
  try {
    await retrySessionBootstrap()
  } finally {
    retrying.value = false
  }
}

async function onLogin() {
  authStore.endSession('bootstrap_failed')
  await router.replace({ name: 'AuthLoginRoute' })
}

if (authStore.permissionsStatus === 'loading') {
  void bootstrapSession()
}
</script>

<style scoped>
.boot-pulse {
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
  .boot-pulse {
    animation: none;
  }
}
</style>
