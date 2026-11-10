<script setup lang="ts">
import { computed, ref } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Rating {
  orderListId: number
  listName: string
  score: number
  ratedAtUtc: string
}

interface Member {
  userId: number
  displayName: string
}
interface AssignedTask {
  id: number
  title: string
  description: string | null
  status: string
  dueAtUtc: string | null
  priority: string | null
}
interface Attachment {
  id: number
  originalFileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

interface Visit {
  id: number
  branchId: number
  openedByUserId: number
  openedByName: string
  status: string
  arrivedAtUtc: string
  departedAtUtc: string | null
  departNote: string | null
  expectedVersion: number
  ratings: Rating[]
  members: Member[]
  assignedTasks: AssignedTask[]
  attachments: Attachment[]
}

interface ListResponse {
  items: Visit[]
  totalCount: number
}

const props = withDefaults(
  defineProps<{ mode?: 'workspace' | 'history' }>(),
  { mode: 'workspace' }
)

const auth = useAuthStore()
const qc = useQueryClient()
const canRead = computed(() => auth.hasPermission('BranchVisit', 'Read') === true)
const canOpen = computed(() => auth.hasPermission('BranchVisit', 'Open') === true)
const canRate = computed(() => auth.hasPermission('BranchVisit', 'Rate') === true)
const canDepart = computed(() => auth.hasPermission('BranchVisit', 'Depart') === true)
const branchChoices = computed(() => auth.branches ?? [])
const openBranchId = ref<number | null>(null)
const rateListId = ref<number | null>(null)
const rateScore = ref(5)
const departNote = ref('')
const departFiles = ref<File[]>([])
const formError = ref<string | null>(null)
const draftNote = ref('')

const activeQuery = useQuery({
  queryKey: computed(() => ['branch-visit-active', auth.sessionGeneration]),
  enabled: computed(
    () => props.mode === 'workspace' && canRead.value && auth.permissionsReady
  ),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<Visit | null>(
      '/admin/BranchVisits/active',
      { signal }
    )
    return data
  },
})

const listQuery = useQuery({
  queryKey: computed(() => ['branch-visits', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchVisits', {
      signal,
      params: { page: 1, pageSize: 50 },
    })
    return data
  },
})

const openMut = useMutation({
  mutationFn: async () => {
    if (!openBranchId.value) throw new Error('branch')
    const { data } = await axiosInstance.post<Visit>('/admin/BranchVisits/open', {
      branchId: openBranchId.value,
    })
    return data
  },
  onSuccess: async () => {
    formError.value = null
    await Promise.all([
      qc.invalidateQueries({ queryKey: ['branch-visit-active'] }),
      qc.invalidateQueries({ queryKey: ['branch-visits'] }),
    ])
  },
  onError: () => {
    formError.value = 'تعذر فتح الزيارة (صلاحية/نطاق/زيارة مفتوحة).'
  },
})

const rateMut = useMutation({
  mutationFn: async () => {
    const visit = activeQuery.data.value
    if (!visit || !rateListId.value) throw new Error('rate')
    const { data } = await axiosInstance.put<Visit>(
      `/admin/BranchVisits/${visit.id}/ratings`,
      {
        orderListId: rateListId.value,
        score: rateScore.value,
        expectedVersion: visit.expectedVersion,
      }
    )
    return data
  },
  onSuccess: async () => {
    formError.value = null
    await qc.invalidateQueries({ queryKey: ['branch-visit-active'] })
  },
  onError: () => {
    formError.value = 'تعذر حفظ التقييم (حدود/نسخة/ملكية).'
  },
})

const departMut = useMutation({
  mutationFn: async () => {
    const visit = activeQuery.data.value
    if (!visit) throw new Error('depart')
    draftNote.value = departNote.value
    const form = new FormData()
    form.append('note', departNote.value)
    form.append('expectedVersion', String(visit.expectedVersion))
    for (const f of departFiles.value) form.append('files', f)
    const { data } = await axiosInstance.post<Visit>(
      `/admin/BranchVisits/${visit.id}/depart`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    )
    return data
  },
  onSuccess: async () => {
    formError.value = null
    departNote.value = ''
    draftNote.value = ''
    departFiles.value = []
    await Promise.all([
      qc.invalidateQueries({ queryKey: ['branch-visit-active'] }),
      qc.invalidateQueries({ queryKey: ['branch-visits'] }),
    ])
  },
  onError: () => {
    if (draftNote.value) departNote.value = draftNote.value
    formError.value =
      'تعذر المغادرة. أبقينا الملاحظة والمرفقات المختارة — تحقق من التقييمات (≥50 حرفًا) وصيغ الملفات.'
  },
})

function onPickFiles(e: Event) {
  const input = e.target as HTMLInputElement
  departFiles.value = Array.from(input.files ?? []).slice(0, 10)
}
</script>

<template>
  <div
    class="bv-page"
    dir="rtl"
  >
    <header>
      <h1 class="bv-title">
        {{ mode === 'workspace' ? 'أعمال إدارة الفرع' : 'سجل زيارات الفرع' }}
      </h1>
      <p class="bv-sub">
        فتح → تقييم قوائم → مغادرة بملاحظة. تبديل الفرع أو إغلاق المتصفح ليس مغادرة.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="bv-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الزيارات.
    </div>
    <template v-else>
      <section
        v-if="mode === 'workspace'"
        class="bv-panel"
      >
        <h2>الزيارة النشطة</h2>
        <div
          v-if="activeQuery.isPending.value"
          class="bv-state"
        >
          جاري التحميل…
        </div>
        <div
          v-else-if="!activeQuery.data.value"
          class="bv-empty"
        >
          <p>لا زيارة مفتوحة.</p>
          <div
            v-if="canOpen"
            class="bv-form"
          >
            <label>
              الفرع
              <select v-model.number="openBranchId">
                <option :value="null">اختر</option>
                <option
                  v-for="b in branchChoices"
                  :key="b.id"
                  :value="b.id"
                >
                  {{ b.name }}
                </option>
              </select>
            </label>
            <button
              type="button"
              class="bv-btn"
              :disabled="openMut.isPending.value || !openBranchId"
              @click="openMut.mutate()"
            >
              فتح زيارة
            </button>
          </div>
        </div>
        <div
          v-else
          class="bv-active"
        >
          <p>
            فرع {{ activeQuery.data.value.branchId }} · وصلت
            {{ activeQuery.data.value.arrivedAtUtc }} · تقييمات:
            {{ activeQuery.data.value.ratings.length }}
          </p>
          <h3>أعضاء اللجنة (لقطة عند الفتح)</h3>
          <ul>
            <li
              v-for="m in activeQuery.data.value.members ?? []"
              :key="m.userId"
            >
              {{ m.displayName }}
            </li>
            <li v-if="!(activeQuery.data.value.members?.length)">لا أعضاء معيَّنون لهذا الفرع</li>
          </ul>
          <h3>المهام المكلَّفة (حية — ليست إنشاء)</h3>
          <ul>
            <li
              v-for="t in activeQuery.data.value.assignedTasks ?? []"
              :key="t.id"
            >
              {{ t.title }} · {{ t.status }}
            </li>
            <li v-if="!(activeQuery.data.value.assignedTasks?.length)">لا مهام مكلَّفة حالياً</li>
          </ul>
          <ul>
            <li
              v-for="r in activeQuery.data.value.ratings"
              :key="r.orderListId"
            >
              {{ r.listName }}: {{ r.score }}/10
            </li>
          </ul>
          <div
            v-if="canRate"
            class="bv-form"
          >
            <label>
              قائمة (معرّف)
              <input
                v-model.number="rateListId"
                type="number"
                min="1"
              >
            </label>
            <label>
              درجة 0–10
              <input
                v-model.number="rateScore"
                type="number"
                min="0"
                max="10"
              >
            </label>
            <button
              type="button"
              class="bv-btn"
              :disabled="rateMut.isPending.value"
              @click="rateMut.mutate()"
            >
              حفظ تقييم
            </button>
          </div>
          <div
            v-if="canDepart"
            class="bv-form"
          >
            <label>
              ملاحظة المغادرة (≥50)
              <textarea
                v-model="departNote"
                rows="4"
              />
            </label>
            <label>
              مرفقات اختيارية (صور/PDF ≤20MB)
              <input
                type="file"
                accept=".jpg,.jpeg,.png,.webp,.gif,.pdf,image/*,application/pdf"
                multiple
                @change="onPickFiles"
              >
            </label>
            <p
              v-if="departFiles.length"
              class="bv-meta"
            >
              ملفات مختارة: {{ departFiles.length }}
            </p>
            <button
              type="button"
              class="bv-btn"
              :disabled="departMut.isPending.value"
              @click="departMut.mutate()"
            >
              مغادرة
            </button>
          </div>
        </div>
        <p
          v-if="formError"
          class="bv-err"
          role="alert"
        >
          {{ formError }}
        </p>
      </section>

      <section class="bv-panel">
        <h2>السجل</h2>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="bv-btn"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
        <div
          v-if="listQuery.isPending.value"
          class="bv-state"
        >
          جاري التحميل…
        </div>
        <div
          v-else-if="(listQuery.data.value?.totalCount ?? 0) === 0"
          class="bv-state"
        >
          لا زيارات ضمن النطاق.
        </div>
        <ul
          v-else
          class="bv-list"
        >
          <li
            v-for="v in listQuery.data.value?.items ?? []"
            :key="v.id"
          >
            #{{ v.id }} · فرع {{ v.branchId }} · {{ v.status }} ·
            {{ v.openedByName }} · تقييمات {{ v.ratings.length }}
          </li>
        </ul>
      </section>
    </template>
  </div>
</template>

<style scoped>
.bv-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.bv-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.bv-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.bv-panel {
  background: #f8f8f8;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}
.bv-state,
.bv-err {
  padding: 0.75rem;
}
.bv-err {
  color: #8b1a1a;
}
.bv-btn {
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  cursor: pointer;
}
.bv-form {
  display: grid;
  gap: 0.5rem;
  margin-top: 0.75rem;
}
.bv-form label {
  display: grid;
  gap: 0.25rem;
}
.bv-form input,
.bv-form select,
.bv-form textarea {
  background: #fff;
  border: 1px solid #cfd8d7;
  border-radius: 6px;
  padding: 0.4rem 0.55rem;
  font: inherit;
}
.bv-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.35rem;
}
</style>
