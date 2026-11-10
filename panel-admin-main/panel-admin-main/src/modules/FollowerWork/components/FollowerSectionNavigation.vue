<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/modules/Auth/store'
import { FOLLOWER_WORK_SECTIONS } from '../domain/sections'

const { t } = useI18n({ useScope: 'global' })
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const stickyQuery = computed(() => {
  const q: Record<string, string> = {}
  const branchId = route.query.branchId
  const followerId = route.query.followerId
  if (typeof branchId === 'string' && branchId) q.branchId = branchId
  if (typeof followerId === 'string' && followerId) q.followerId = followerId
  return q
})

const sections = computed(() =>
  FOLLOWER_WORK_SECTIONS.map((s) => {
    const allowed =
      auth.hasPermission(s.permissionModule, s.permissionAction) === true
    return { ...s, allowed }
  })
)

function go(segment: string) {
  void router.push({
    path: `/management/follower-work/${segment}`,
    query: stickyQuery.value,
  })
}

function isActive(segment: string) {
  return String(route.path).includes(`/follower-work/${segment}`)
}
</script>

<template>
  <nav class="fw-sections" aria-label="أقسام عمل المتابع">
    <p class="fw-sections__hint">
      اختيار المتابع والفرع يبقى داخل مجموعة العمل فقط عند التنقل بين الأقسام.
    </p>
    <ul class="fw-sections__list">
      <li v-for="s in sections" :key="s.id">
        <button
          type="button"
          class="fw-sections__item"
          :class="{
            'fw-sections__item--active': isActive(s.pathSegment),
            'fw-sections__item--locked': !s.allowed,
          }"
          :disabled="!s.allowed"
          :aria-current="isActive(s.pathSegment) ? 'page' : undefined"
          @click="go(s.pathSegment)"
        >
          <i v-if="!s.allowed" class="pi pi-lock" aria-hidden="true" />
          {{ t(`followerWork.${s.labelKey}`) }}
        </button>
      </li>
    </ul>
  </nav>
</template>

<style scoped>
.fw-sections {
  margin-top: 1rem;
  position: sticky;
  top: 0;
  z-index: 2;
  padding: 12px 0;
  background: #f4f7f7;
}
.fw-sections__hint {
  margin: 0 0 0.5rem;
  font-size: 14px;
  color: #3d524f;
}
.fw-sections__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.fw-sections__item {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  min-height: 44px;
  padding: 0.35rem 0.75rem;
  border: 1px solid #dce7e4;
  border-radius: 8px;
  background: #fff;
  font: inherit;
  font-size: 14px;
  color: #00201e;
  cursor: pointer;
}
.fw-sections__item--active {
  color: #fff;
  background: #006a64;
  border-color: #006a64;
}
.fw-sections__item--locked {
  opacity: 0.72;
  cursor: not-allowed;
  color: #526b67;
}
.fw-sections__item:focus-visible {
  outline: 3px solid #71b4a9;
  outline-offset: 2px;
}
</style>
