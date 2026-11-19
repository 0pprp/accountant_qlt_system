<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'

interface DelegateAccount {
  fullName: string
  username: string
  allBranches: boolean
  branchNames: string[]
}

const auth = useAuthStore()
const router = useRouter()
const confirmLogout = ref(false)
const canTutorials = computed(() => auth.hasPermission('Tutorial', 'Read') === true)
const canAccount = computed(() => auth.hasPermission('DelegateHome', 'Read') === true)

const accountQuery = useQuery({
  queryKey: computed(() => ['delegate-account', auth.sessionGeneration]),
  enabled: computed(() => canAccount.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<DelegateAccount>('/admin/DelegateHome/account', { signal })
    return data
  },
})

const account = computed(() => accountQuery.data.value ?? null)

const scopeText = computed(() => {
  if (account.value) {
    if (account.value.allBranches) return 'كل المحافظات'
    if (account.value.branchNames.length) return account.value.branchNames.join('، ')
  }
  const names = (auth.branches ?? []).map((b) => b.name || String(b.id))
  return names.length ? names.join('، ') : '—'
})

function openTutorials() {
  if (!canTutorials.value) return
  router.push({ name: 'ManagementTutorialsRoute' }).catch(() => {
    router.push('/management/oversight/tutorials')
  })
}

function logout() {
  confirmLogout.value = false
  auth.logout()
  router.push({ name: 'AuthLoginRoute' }).catch(() => {
    router.push('/auth')
  })
}
</script>

<template>
  <section class="dst" dir="rtl">
    <header>
      <h2 class="dfb-title">الإعدادات</h2>
      <p class="dfb-subtitle">بيانات حسابك في لوحة المدير المفوض.</p>
    </header>

    <div class="dst-card">
      <h3 class="dst-card-title">الحساب</h3>
      <p v-if="canAccount && accountQuery.isPending.value" class="dfb-state">جاري التحميل…</p>
      <div v-else-if="canAccount && accountQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل بيانات الحساب.
        <button type="button" class="dfb-button is-small" @click="accountQuery.refetch()">إعادة المحاولة</button>
      </div>
      <dl class="dst-facts">
        <div><dt>الاسم</dt><dd>{{ account?.fullName || '—' }}</dd></div>
        <div><dt>اسم الدخول</dt><dd class="dfb-num">{{ account?.username || '—' }}</dd></div>
        <div><dt>الصفة</dt><dd>مدير مفوض</dd></div>
        <div class="is-wide"><dt>النطاق</dt><dd>{{ scopeText }}</dd></div>
      </dl>
    </div>

    <div class="dst-card">
      <h3 class="dst-card-title">الفيديوهات التعليمية</h3>
      <p class="dst-hint">شروحات استخدام التطبيقات، تُدار مركزياً.</p>
      <div>
        <button type="button" class="dfb-button" :disabled="!canTutorials" @click="openTutorials">
          فتح الفيديوهات التعليمية
        </button>
      </div>
      <p v-if="!canTutorials" class="dst-hint">لا تملك صلاحية عرض الفيديوهات التعليمية.</p>
    </div>

    <div class="dst-card">
      <h3 class="dst-card-title">الجلسة</h3>
      <div>
        <button type="button" class="dfb-button dst-danger" @click="confirmLogout = true">تسجيل الخروج</button>
      </div>
    </div>

    <NeuDialog :visible="confirmLogout" title="تسجيل الخروج" size="sm" @close="confirmLogout = false">
      <p class="dst-confirm">هل تريد تسجيل الخروج من الجلسة الحالية؟</p>
      <template #footer>
        <button type="button" class="dfb-button" @click="confirmLogout = false">إلغاء</button>
        <button type="button" class="dfb-button is-primary" @click="logout">تسجيل الخروج</button>
      </template>
    </NeuDialog>
  </section>
</template>

<style scoped>
.dst {
  display: flex;
  flex-direction: column;
  gap: 18px;
  max-width: 760px;
  min-width: 0;
}

.dst-card {
  background: var(--dfb-raised);
  border-radius: 18px;
  box-shadow: var(--dfb-up);
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px 18px;
}

.dst-card-title {
  font-size: 16px;
  font-weight: 700;
  margin: 0;
}

.dst-facts {
  display: grid;
  gap: 12px 18px;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 180px), 1fr));
  margin: 0;
}

.dst-facts dt {
  color: var(--dfb-muted);
  font-size: 12px;
  font-weight: 600;
}

.dst-facts dd {
  font-weight: 600;
  margin: 2px 0 0;
  overflow-wrap: anywhere;
}

.dst-facts .is-wide { grid-column: 1 / -1; }

.dst-hint {
  color: var(--dfb-muted);
  font-size: 13px;
  margin: 0;
}

.dst-danger { color: var(--dfb-tier-bad); }

.dst-confirm {
  line-height: 1.8;
  margin: 0;
}
</style>
