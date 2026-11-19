<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { formatNumber } from '@/modules/Core/utils/formatDisplay'
import { useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'
import {
  VISIT_HISTORY_PAGE_SIZE,
  type VisitHistoryAttachment,
  type VisitHistoryItem,
  type VisitHistoryResponse,
  visitBranchLabel,
  visitDateTime,
  visitMembersLine,
  visitStatus,
} from '../domain/visitHistory'

const auth = useAuthStore()
const route = useRoute()
const canRead = computed(() => auth.hasPermission('BranchVisit', 'Read') === true)
const branchId = useDelegateBranchId()
const branchNames = computed(() => new Map((auth.branches ?? []).map((b) => [b.id, b.name])))

const listQuery = useQuery({
  queryKey: computed(() => ['branch-visits', 'history', branchId.value, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<VisitHistoryResponse>('/admin/BranchVisits', {
      signal,
      params: { page: 1, pageSize: VISIT_HISTORY_PAGE_SIZE, branchId: branchId.value ?? undefined },
    })
    return data
  },
})

const items = computed(() => listQuery.data.value?.items ?? [])
const downloadError = ref<string | null>(null)
const downloadingId = ref<number | null>(null)

function branchLabel(v: VisitHistoryItem) {
  return visitBranchLabel(v, branchNames.value.get(v.branchId))
}

async function download(visit: VisitHistoryItem, att: VisitHistoryAttachment) {
  downloadError.value = null
  downloadingId.value = att.id
  try {
    const response = await axiosInstance.get<Blob>(
      `/admin/BranchVisits/${visit.id}/attachments/${att.id}`,
      { responseType: 'blob' }
    )
    const url = URL.createObjectURL(response.data)
    const a = document.createElement('a')
    a.href = url
    a.download = att.originalFileName || `attachment-${att.id}`
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)
  } catch {
    downloadError.value = 'تعذر تنزيل المرفق.'
  } finally {
    downloadingId.value = null
  }
}
</script>

<template>
  <section
    class="bvh"
    dir="rtl"
  >
    <header>
      <h2 class="dfb-title">أعمال إدارة الفرع</h2>
      <p class="dfb-subtitle">آخر زيارات إدارة الفروع ضمن نطاقك، الأحدث أولاً.</p>
    </header>

    <p
      v-if="!canRead"
      class="dfb-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الزيارات.
    </p>
    <template v-else>
      <div
        v-if="listQuery.isError.value"
        class="dfb-error"
        role="alert"
      >
        تعذر تحميل الزيارات.
        <button
          type="button"
          class="dfb-button is-small"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p
        v-else-if="listQuery.isPending.value"
        class="dfb-state"
      >
        جاري التحميل…
      </p>
      <p
        v-else-if="items.length === 0"
        class="dfb-state"
      >
        لا توجد زيارات مسجّلة
      </p>
      <template v-else>
        <p class="bvh-meta">
          المعروض <span class="dfb-num">{{ formatNumber(items.length) }}</span>
          من <span class="dfb-num">{{ formatNumber(listQuery.data.value?.totalCount ?? items.length) }}</span>
        </p>
        <p
          v-if="downloadError"
          class="dfb-error"
          role="alert"
        >
          {{ downloadError }}
        </p>
        <ul class="bvh-list">
          <li
            v-for="v in items"
            :key="v.id"
            class="bvh-card"
          >
            <div class="bvh-card-head">
              <h3 class="bvh-branch">{{ branchLabel(v) }}</h3>
              <span
                class="dfb-status"
                :class="visitStatus(v.status).open ? 'is-open' : 'is-sold'"
              >
                {{ visitStatus(v.status).label }}
              </span>
            </div>
            <p class="bvh-muted">{{ v.openedByName || '—' }}</p>
            <dl class="bvh-facts">
              <div><dt>الوصول:</dt><dd class="dfb-num">{{ visitDateTime(v.arrivedAtUtc) }}</dd></div>
              <div><dt>المغادرة:</dt><dd class="dfb-num">{{ visitDateTime(v.departedAtUtc) }}</dd></div>
              <div class="is-wide"><dt>الأعضاء:</dt><dd>{{ visitMembersLine(v.members ?? []) }}</dd></div>
            </dl>
            <ul
              v-if="v.ratings?.length"
              class="bvh-ratings"
            >
              <li
                v-for="r in v.ratings"
                :key="r.orderListId"
              >
                {{ r.listName }}: <span class="dfb-num">{{ formatNumber(r.score) }}/10</span>
              </li>
            </ul>
            <p
              v-if="v.departNote?.trim()"
              class="bvh-note"
            >
              {{ v.departNote }}
            </p>
            <div
              v-if="v.attachments?.length"
              class="bvh-attachments"
            >
              <button
                v-for="a in v.attachments"
                :key="a.id"
                type="button"
                class="dfb-link"
                :disabled="downloadingId === a.id"
                @click="download(v, a)"
              >
                {{ a.originalFileName || 'مرفق' }}
              </button>
            </div>
          </li>
        </ul>
      </template>
    </template>
  </section>
</template>

<style scoped>
.bvh {
  display: flex;
  flex-direction: column;
  gap: 18px;
  min-width: 0;
}

.bvh-meta,
.bvh-muted {
  color: var(--dfb-muted);
  font-size: 13px;
  margin: 0;
}

.bvh-list {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 340px), 1fr));
  list-style: none;
  margin: 0;
  padding: 0;
}

.bvh-card {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
  padding: 14px 16px;
}

.bvh-card-head {
  align-items: center;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

.bvh-branch {
  font-size: 16px;
  font-weight: 700;
  margin: 0;
  overflow-wrap: anywhere;
}

.bvh-facts {
  display: grid;
  gap: 4px;
  margin: 0;
}

.bvh-facts div {
  display: flex;
  flex-wrap: wrap;
  font-size: 13px;
  gap: 6px;
}

.bvh-facts dt {
  color: var(--dfb-muted);
  font-weight: 600;
}

.bvh-facts dd {
  margin: 0;
  overflow-wrap: anywhere;
}

.bvh-ratings {
  background: var(--dfb-well);
  border-radius: 12px;
  box-shadow: var(--dfb-well-shadow);
  display: grid;
  font-size: 13px;
  gap: 4px;
  list-style: none;
  margin: 0;
  padding: 8px 12px;
}

.bvh-note {
  font-size: 13px;
  line-height: 1.7;
  margin: 0;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}

.bvh-attachments {
  display: flex;
  flex-wrap: wrap;
  font-size: 13px;
  gap: 8px 14px;
}
</style>
