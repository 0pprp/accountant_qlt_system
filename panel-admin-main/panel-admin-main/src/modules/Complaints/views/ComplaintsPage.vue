<script setup lang="ts">
import { computed, ref } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Row {
  id: number
  sourceApp: string
  senderDisplayName: string
  senderRole: string
  branchId: number | null
  branchName: string | null
  subject: string
  body: string
  status: string
  createdAtUtc: string
  expectedVersion: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
  unreadCount: number
}

const auth = useAuthStore()
const qc = useQueryClient()
const canRead = computed(() => auth.hasPermission('Complaint', 'Read') === true)
const canMark = computed(() => auth.hasPermission('Complaint', 'MarkRead') === true)
const unreadOnly = ref(true)
const selected = ref<Row | null>(null)
const actionError = ref<string | null>(null)

const listQuery = useQuery({
  queryKey: computed(() => ['complaints', unreadOnly.value, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/Complaints', {
      signal,
      params: { page: 1, pageSize: 100, unreadOnly: unreadOnly.value || undefined },
    })
    return data
  },
})

const markMut = useMutation({
  mutationFn: async (row: Row) => {
    const { data } = await axiosInstance.post<Row>(`/admin/Complaints/${row.id}/mark-read`, {
      expectedVersion: row.expectedVersion,
    })
    return data
  },
  onSuccess: async () => {
    selected.value = null
    actionError.value = null
    await qc.invalidateQueries({ queryKey: ['complaints'] })
  },
  onError: () => {
    actionError.value = 'تعذر تعليم المقروء. أبقينا التفاصيل مفتوحة.'
  },
})
</script>

<template>
  <div
    class="cp-page"
    dir="rtl"
  >
    <header>
      <h1 class="cp-title">الرسائل والشكاوى</h1>
      <p class="cp-sub">
        صندوق شكاوى التطبيقات — ليست ملاحظات AppNote.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="cp-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الشكاوى.
    </div>
    <template v-else>
      <div class="cp-toolbar">
        <label>
          <input
            v-model="unreadOnly"
            type="checkbox"
          >
          غير المقروء فقط
        </label>
        <span class="cp-meta">
          الإجمالي: {{ listQuery.data.value?.totalCount ?? '—' }} · غير مقروء:
          {{ listQuery.data.value?.unreadCount ?? '—' }}
        </span>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="cp-btn"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p
        v-if="listQuery.isError.value"
        class="cp-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="cp-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="(listQuery.data.value?.totalCount ?? 0) === 0"
        class="cp-state"
      >
        لا شكاوى ضمن النطاق.
      </div>
      <ul
        v-else
        class="cp-list"
      >
        <li
          v-for="row in listQuery.data.value?.items ?? []"
          :key="row.id"
          class="cp-row"
          :class="{ unread: row.status === 'Unread' }"
        >
          <button
            type="button"
            class="cp-open"
            @click="selected = row; actionError = null"
          >
            <strong>{{ row.subject }}</strong>
            <span>{{ row.senderDisplayName }} · {{ row.branchName || '—' }}</span>
          </button>
        </li>
      </ul>
    </template>

    <div
      v-if="selected"
      class="cp-modal"
      role="dialog"
      aria-modal="true"
      @keydown.esc="selected = null"
    >
      <div class="cp-dialog">
        <h2>{{ selected.subject }}</h2>
        <p class="cp-meta">
          {{ selected.senderDisplayName }} ({{ selected.senderRole }}) ·
          {{ selected.branchName || '—' }}
        </p>
        <p class="cp-body">{{ selected.body }}</p>
        <p
          v-if="actionError"
          class="cp-err"
          role="alert"
        >
          {{ actionError }}
        </p>
        <div class="cp-actions">
          <button
            v-if="canMark && selected.status === 'Unread'"
            type="button"
            class="cp-btn"
            :disabled="markMut.isPending.value"
            @click="markMut.mutate(selected)"
          >
            تعليم مقروء
          </button>
          <button
            type="button"
            class="cp-btn ghost"
            @click="selected = null"
          >
            إغلاق
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cp-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.cp-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.cp-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.cp-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  margin-bottom: 0.75rem;
}
.cp-meta {
  font-size: 0.85rem;
  opacity: 0.85;
}
.cp-state,
.cp-err {
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.cp-err {
  color: #8b1a1a;
}
.cp-btn {
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  cursor: pointer;
}
.cp-btn.ghost {
  background: #f8f8f8;
  color: #00201e;
}
.cp-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.5rem;
}
.cp-row {
  background: #f8f8f8;
  border-radius: 8px;
}
.cp-row.unread {
  outline: 2px solid #006a64;
}
.cp-open {
  width: 100%;
  text-align: right;
  background: transparent;
  border: 0;
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: grid;
  gap: 0.25rem;
  font: inherit;
  color: inherit;
}
.cp-modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 40;
}
.cp-dialog {
  background: #fff;
  border-radius: 10px;
  padding: 1.25rem;
  width: min(560px, 100%);
  max-height: 90vh;
  overflow: auto;
}
.cp-body {
  white-space: pre-wrap;
  background: #f8f8f8;
  padding: 0.75rem;
  border-radius: 6px;
}
.cp-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
}
</style>
