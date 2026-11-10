<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import { useCommitteeTasksQuery } from '../requests/queries/useCommitteeTasksQuery'
import {
  useCreateCommitteeTaskMutation,
  useDeleteCommitteeTaskMutation,
  useUpdateCommitteeTaskMutation,
} from '../requests/mutations/useCommitteeTaskMutations'
import type { CommitteeTaskRow } from '../domain/types'
import '../components/committee-tasks.css'

const auth = useAuthStore()
const canRead = computed(() => auth.hasPermission('CommitteeTask', 'Read') === true)
const canWrite = computed(() => auth.hasPermission('CommitteeTask', 'Write') === true)

const listQuery = useCommitteeTasksQuery(canRead)
const createMut = useCreateCommitteeTaskMutation()
const updateMut = useUpdateCommitteeTaskMutation()
const deleteMut = useDeleteCommitteeTaskMutation()

const search = ref('')
const dialogOpen = ref(false)
const editing = ref<CommitteeTaskRow | null>(null)
const draftBody = ref('')
const draftEnabled = ref(true)
const formError = ref<string | null>(null)
const dialogTitleRef = ref<HTMLElement | null>(null)
const bodyRef = ref<HTMLTextAreaElement | null>(null)

const rows = computed(() => {
  const all = listQuery.data.value ?? []
  const q = search.value.trim()
  if (!q) return all
  return all.filter((r) => r.body.includes(q) || String(r.id).includes(q))
})

watch(dialogOpen, async (open) => {
  if (!open) return
  await nextTick()
  ;(bodyRef.value ?? dialogTitleRef.value)?.focus()
})

function openCreate() {
  editing.value = null
  draftBody.value = ''
  draftEnabled.value = true
  formError.value = null
  dialogOpen.value = true
}

function openEdit(row: CommitteeTaskRow) {
  editing.value = row
  draftBody.value = row.body
  draftEnabled.value = row.isEnabled
  formError.value = null
  dialogOpen.value = true
}

function closeDialog() {
  dialogOpen.value = false
  editing.value = null
  formError.value = null
}

async function save() {
  const body = draftBody.value.trim()
  if (!body) {
    formError.value = 'نص المهمة إلزامي.'
    return
  }
  if (body.length > 8000) {
    formError.value = 'الحد الأقصى 8000 حرف.'
    return
  }
  formError.value = null
  try {
    if (editing.value) {
      await updateMut.mutateAsync({
        id: editing.value.id,
        body: {
          body,
          isEnabled: draftEnabled.value,
          expectedVersion: editing.value.expectedVersion,
        },
      })
    } else {
      await createMut.mutateAsync({ body, isEnabled: draftEnabled.value })
    }
    closeDialog()
  } catch {
    formError.value = 'تعذر الحفظ. قد تكون النسخة قديمة — حدّث القائمة وحاول مجددًا.'
  }
}

async function removeRow(row: CommitteeTaskRow) {
  if (!window.confirm('حذف هذه المهمة من النشرة؟')) return
  try {
    await deleteMut.mutateAsync({ id: row.id, expectedVersion: row.expectedVersion })
  } catch {
    formError.value = 'تعذر الحذف بسبب تعارض أو صلاحية.'
  }
}

function fmt(v: string) {
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(v))
  } catch {
    return v
  }
}

function onDialogKey(e: KeyboardEvent) {
  if (e.key === 'Escape') closeDialog()
}
</script>

<template>
  <div
    class="ct-page"
    dir="rtl"
  >
    <header>
      <h1 class="ct-page__title">مهام اللجنة</h1>
      <p class="ct-page__sub">
        نشرة تعليمات تظهر في تطبيق مسؤول اللجنة عند التفعيل — ليست مهامًا مسندة بقبول وإنجاز.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="ct-state ct-state--forbidden"
      role="alert"
    >
      ليس لديك صلاحية قراءة مهام اللجنة.
    </div>

    <template v-else>
      <div class="ct-toolbar">
        <div class="ct-search">
          <input
            v-model="search"
            class="ct-search__input"
            type="search"
            placeholder="بحث في نص المهمة…"
            aria-label="بحث في مهام اللجنة"
            autocomplete="off"
          >
          <span
            class="ct-search__icon"
            aria-hidden="true"
          >🔍</span>
        </div>
        <button
          v-if="canWrite"
          type="button"
          class="ct-btn primary"
          @click="openCreate"
        >
          إضافة مهمة
        </button>
      </div>

      <p
        v-if="listQuery.isError.value"
        class="ct-error"
        role="alert"
      >
        تعذر تحميل القائمة.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="ct-state"
        aria-busy="true"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="rows.length === 0"
        class="ct-state"
      >
        لا مهام في النشرة حاليًا.
      </div>
      <div
        v-else
        class="ct-table-wrap"
      >
        <table class="ct-table">
          <thead>
            <tr>
              <th scope="col">النص</th>
              <th scope="col">الحالة</th>
              <th scope="col">الترتيب</th>
              <th scope="col">آخر تحديث</th>
              <th
                v-if="canWrite"
                scope="col"
              >
                إجراءات
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in rows"
              :key="row.id"
            >
              <td>{{ row.body }}</td>
              <td>
                <span
                  class="ct-status"
                  :class="row.isEnabled ? 'ct-status--on' : 'ct-status--off'"
                >
                  {{ row.isEnabled ? 'مفعّلة' : 'موقوفة' }}
                </span>
              </td>
              <td>{{ row.sortOrder }}</td>
              <td>
                {{ fmt(row.updatedAtUtc) }}
                <div v-if="row.updatedByName">{{ row.updatedByName }}</div>
              </td>
              <td v-if="canWrite">
                <div class="ct-actions">
                  <button
                    type="button"
                    class="ct-btn"
                    @click="openEdit(row)"
                  >
                    تعديل
                  </button>
                  <button
                    type="button"
                    class="ct-btn danger"
                    @click="removeRow(row)"
                  >
                    حذف
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div
      v-if="dialogOpen"
      class="ct-dialog-backdrop"
      role="presentation"
      @click.self="closeDialog"
      @keydown="onDialogKey"
    >
      <div
        class="ct-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="ct-dialog-title"
      >
        <h2
          id="ct-dialog-title"
          ref="dialogTitleRef"
          tabindex="-1"
        >
          {{ editing ? 'تعديل مهمة' : 'إضافة مهمة' }}
        </h2>
        <label class="ct-field">
          <span>نص المهمة</span>
          <textarea
            ref="bodyRef"
            v-model="draftBody"
            maxlength="8000"
          />
        </label>
        <label class="ct-field">
          <span>
            <input
              v-model="draftEnabled"
              type="checkbox"
            >
            مفعّلة للعرض في التطبيق
          </span>
        </label>
        <p
          v-if="formError"
          class="ct-error"
          role="alert"
        >
          {{ formError }}
        </p>
        <div class="ct-dialog__actions">
          <button
            type="button"
            class="ct-btn"
            @click="closeDialog"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="ct-btn primary"
            :disabled="createMut.isPending.value || updateMut.isPending.value"
            @click="save"
          >
            حفظ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
