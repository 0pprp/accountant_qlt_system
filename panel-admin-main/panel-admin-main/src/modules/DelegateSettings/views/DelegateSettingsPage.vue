<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'

const auth = useAuthStore()
const router = useRouter()
const canTutorials = computed(() => auth.hasPermission('Tutorial', 'Read') === true)

function logout() {
  auth.logout?.()
  router.push({ name: 'LoginRoute' }).catch(() => {
    router.push('/login')
  })
}
</script>

<template>
  <div
    class="ds-page"
    dir="rtl"
  >
    <header>
      <h1 class="ds-title">الإعدادات</h1>
      <p class="ds-sub">
        خيارات وظيفية للجلسة الحالية — ليست إعدادات مركزية للنظام.
      </p>
    </header>

    <section class="ds-card">
      <h2>الحساب</h2>
      <p>جلسة مفوض نشطة</p>
      <p class="ds-meta">
        الفروع:
        {{
          (auth.branches ?? [])
            .map((b: { name?: string; id?: number }) => b.name || b.id)
            .join('، ') || '—'
        }}
      </p>
    </section>

    <section class="ds-card">
      <h2>الفيديو التعليمي</h2>
      <p class="ds-meta">
        روابط خارجية مُدارة مركزيًا — تُفتح من تبويب الفيديوهات إن توفرت الصلاحية.
      </p>
      <button
        type="button"
        class="ds-btn"
        :disabled="!canTutorials"
        @click="canTutorials && router.push('/management/oversight/tutorials')"
      >
        فتح الفيديوهات التعليمية
      </button>
      <p
        v-if="!canTutorials"
        class="ds-meta"
      >
        لا صلاحية للوجهة — الزر غير قابل للتنفيذ.
      </p>
    </section>

    <section class="ds-card">
      <h2>الجلسة</h2>
      <button
        type="button"
        class="ds-btn danger"
        @click="logout"
      >
        تسجيل الخروج
      </button>
    </section>
  </div>
</template>

<style scoped>
.ds-page {
  max-width: 720px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.ds-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.ds-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.ds-card {
  background: #f8f8f8;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 0.75rem;
}
.ds-meta {
  font-size: 0.85rem;
  opacity: 0.85;
}
.ds-btn {
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  cursor: pointer;
  margin-top: 0.5rem;
}
.ds-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.ds-btn.danger {
  background: #8b1a1a;
}
</style>
