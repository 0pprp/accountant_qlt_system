<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerWorkContext } from '../composables/useFollowerWorkContext'
import { useFollowersQuery } from '../requests/queries/useFollowersQuery'
import { useRouter } from 'vue-router'
import { getImplementedSections } from '../domain/sections'
import type { FollowerWorkSort } from '../domain/types'
import FollowerBranchFilter from '../components/FollowerBranchFilter.vue'
import FollowerCard from '../components/FollowerCard.vue'
import FollowerSectionNavigation from '../components/FollowerSectionNavigation.vue'
import '../components/follower-work.css'

const auth = useAuthStore()
const router = useRouter()
const { ctx, patch, setBranch } = useFollowerWorkContext()

const canDirectory = computed(
  () =>
    auth.hasPermission('FollowerWork', 'Read') === true ||
    auth.hasPermission('FollowerSalesReview', 'Read') === true ||
    auth.hasPermission('FollowerDeclining', 'Read') === true ||
    auth.hasPermission('FollowerNotes', 'Read') === true ||
    auth.hasPermission('FollowerOldCustomers', 'Read') === true ||
    auth.hasPermission('FollowerDelegateNotes', 'Read') === true ||
    auth.hasPermission('FollowerCollection', 'Read') === true ||
    auth.hasPermission('FollowerLocation', 'Read') === true ||
    auth.hasPermission('FollowerRoute', 'Read') === true
)

const branches = computed(() => auth.branches ?? [])
const branchId = computed(() => ctx.value.branchId)
const searchInput = ref(ctx.value.search)
const pageSize = ref(24)
const sort = computed(() => ctx.value.sort)
const page = computed(() => ctx.value.page)
const selectedFollowerId = computed(() => ctx.value.followerId)
const hasImplementedSection = computed(() => getImplementedSections().length > 0)
const canOpenSales = computed(
  () => auth.hasPermission('FollowerSalesReview', 'Read') === true
)
const canOpenDeclining = computed(
  () => auth.hasPermission('FollowerDeclining', 'Read') === true
)
const canOpenNotes = computed(
  () => auth.hasPermission('FollowerNotes', 'Read') === true
)
const canOpenOldCustomers = computed(
  () => auth.hasPermission('FollowerOldCustomers', 'Read') === true
)
const canOpenDelegateNotes = computed(
  () => auth.hasPermission('FollowerDelegateNotes', 'Read') === true
)
const canOpenCollection = computed(
  () => auth.hasPermission('FollowerCollection', 'Read') === true
)

watch(
  () => ctx.value.search,
  (v) => {
    if (v !== searchInput.value) searchInput.value = v
  }
)

watch(branches, (list) => {
  if (branchId.value != null && list.some((b) => b.id === branchId.value) === false) {
    setBranch(null)
  }
  if (branchId.value == null && list.length === 1) {
    setBranch(list[0].id)
  }
})

const followersQuery = useFollowersQuery({
  branchId,
  search: computed(() => ctx.value.search),
  page,
  pageSize,
  sort,
  enabled: canDirectory,
})

watch(
  () => followersQuery.data.value?.items,
  (items) => {
    const fid = selectedFollowerId.value
    if (fid == null || !items) return
    if (items.some((c) => c.followerId === fid) === false) {
      patch({ followerId: null })
    }
  }
)

function applySearch() {
  patch({ search: searchInput.value.trim(), page: 1, followerId: null })
}

function onSort(v: string) {
  const next = (['name', 'rating', 'lists'].includes(v) ? v : 'name') as FollowerWorkSort
  patch({ sort: next, page: 1 })
}

function selectFollower(id: number) {
  patch({ followerId: id })
}

function openSalesReview(id: number, name: string) {
  if (canOpenSales.value !== true) return
  void router.push({
    name: 'ManagementFollowerSalesRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

function openDeclining(id: number, name: string) {
  if (canOpenDeclining.value !== true) return
  void router.push({
    name: 'ManagementFollowerDecliningRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

function openNotes(id: number, name: string) {
  if (canOpenNotes.value !== true) return
  void router.push({
    name: 'ManagementFollowerNotesRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

function openOldCustomers(id: number, name: string) {
  if (canOpenOldCustomers.value !== true) return
  void router.push({
    name: 'ManagementFollowerOldCustomersRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

function openDelegateNotes(id: number, name: string) {
  if (canOpenDelegateNotes.value !== true) return
  void router.push({
    name: 'ManagementFollowerDelegateNotesRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

function openCollection(id: number, name: string) {
  if (canOpenCollection.value !== true) return
  void router.push({
    name: 'ManagementFollowerCollectionRoute',
    query: {
      followerId: String(id),
      followerName: name,
      ...(branchId.value != null ? { branchId: String(branchId.value) } : {}),
    },
  })
}

const selectedCard = computed(() => {
  const id = selectedFollowerId.value
  if (id == null) return null
  return followersQuery.data.value?.items.find((c) => c.followerId === id) ?? null
})

const totalPages = computed(() => followersQuery.data.value?.totalPages ?? 0)
</script>

<template>
  <div class="fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">عمل المتابعين</h1>
      <p class="fw-page__sub">
        اختر المحافظة ثم المتابع للاطلاع على أعماله.
      </p>
    </header>

    <div v-if="!canDirectory" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية عرض دليل المتابعين.
    </div>

    <template v-else>
      <div class="fw-toolbar">
        <label class="fw-search">
          <span class="visually-hidden">بحث عن متابع</span>
          <input
            v-model="searchInput"
            class="fw-search__input"
            type="search"
            placeholder="بحث بالاسم أو المعرف"
            @keydown.enter.prevent="applySearch"
          />
          <button type="button" class="fw-search__btn" aria-label="بحث" @click="applySearch">
            بحث
          </button>
        </label>
        <FollowerBranchFilter
          :branches="branches"
          :model-value="branchId"
          @update:model-value="setBranch"
        />
        <label class="fw-sort">
          <span class="fw-sort__label">ترتيب</span>
          <select
            class="fw-sort__select"
            :value="sort"
            @change="onSort(($event.target as HTMLSelectElement).value)"
          >
            <option value="name">الاسم</option>
            <option value="rating">التقييم</option>
            <option value="lists">عدد القوائم</option>
          </select>
        </label>
      </div>

      <div v-if="branchId == null" class="fw-state">اختر محافظة لعرض بطاقات المتابعين.</div>

      <div
        v-else-if="followersQuery.isPending.value"
        class="fw-state fw-state--loading"
        aria-busy="true"
      >
        جاري تحميل البطاقات…
      </div>

      <div
        v-else-if="followersQuery.isError.value"
        class="fw-state fw-state--error"
        role="alert"
      >
        تعذر تحميل دليل المتابعين.
        <button type="button" class="fw-retry" @click="followersQuery.refetch()">
          إعادة المحاولة
        </button>
      </div>

      <div
        v-else-if="!(followersQuery.data.value?.items.length)"
        class="fw-state fw-state--empty"
      >
        لا متابعون بتكليف قوائم ضمن هذه المحافظة.
      </div>

      <template v-else>
        <div class="fw-grid" role="list">
          <FollowerCard
            v-for="card in followersQuery.data.value!.items"
            :key="card.followerId"
            role="listitem"
            :card="card"
            :selected="card.followerId === selectedFollowerId"
            :has-implemented-section="hasImplementedSection"
            :can-open-sales="canOpenSales"
            :can-open-declining="canOpenDeclining"
            :can-open-notes="canOpenNotes"
            :can-open-old-customers="canOpenOldCustomers"
            :can-open-delegate-notes="canOpenDelegateNotes"
            :can-open-collection="canOpenCollection"
            @select="selectFollower"
            @enter="openSalesReview(card.followerId, card.followerName)"
            @enter-declining="openDeclining(card.followerId, card.followerName)"
            @enter-notes="openNotes(card.followerId, card.followerName)"
            @enter-old-customers="openOldCustomers(card.followerId, card.followerName)"
            @enter-delegate-notes="openDelegateNotes(card.followerId, card.followerName)"
            @enter-collection="openCollection(card.followerId, card.followerName)"
          />
        </div>

        <div v-if="totalPages > 1" class="fw-pager">
          <button
            type="button"
            class="fw-pager__btn"
            :disabled="page <= 1"
            @click="patch({ page: page - 1 })"
          >
            السابق
          </button>
          <span class="fw-pager__info">{{ page }} / {{ totalPages }}</span>
          <button
            type="button"
            class="fw-pager__btn"
            :disabled="page >= totalPages"
            @click="patch({ page: page + 1 })"
          >
            التالي
          </button>
        </div>
      </template>

      <section v-if="selectedCard" class="fw-selected" aria-label="سياق المتابع المختار">
        <h2 class="fw-selected__title">المتابع المختار</h2>
        <p class="fw-selected__name">{{ selectedCard.followerName }}</p>
        <FollowerSectionNavigation />
      </section>
    </template>
  </div>
</template>
