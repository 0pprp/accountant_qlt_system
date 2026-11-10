<template>
  <header
    class="list-header w-full bg-surface-3 rounded-2xl"
  >
    <!-- Top section -->
    <div
      class="list-toolbar bg-white w-full border border-line rounded-2xl"
    >
      <div class="list-toolbar-group">
        <form
          v-if="canSearch"
          @submit.prevent="handleSubmitForm"
          class="list-search relative"
        >
          <SvgIcon
            name="search"
            class="w-6 h-6 absolute top-1/2 -translate-y-1/2 right-2"
          />
          <InputText
            v-model="searchValue"
            :placeholder="$t(searchPlaceholder)"
            class="list-search-input h-11 w-full"
          />
        </form>
        <slot name="export"></slot>
      </div>

      <div class="list-toolbar-actions">
        <slot name="create"></slot>
        <slot name="calendar"></slot>
      </div>
    </div>

    <!-- bottom section -->
    <div class="py-5 px-6 flex items-center gap-2" v-if="chips.length">
      <Chip
        v-for="search in chips"
        :key="search?.toString()"
        :label="search?.toString()"
        removable
      >
        <template #removeicon>
          <SvgIcon
            @click="() => removeSearchQueryItem(search?.toString()!)"
            name="closeHeaderIcon"
            class="text-error w-4 h-4 cursor-pointer"
          />
        </template>
      </Chip>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Chip from 'primevue/chip'
// import HeaderDatePicker from '../../base/HeaderDatePicker/HeaderDatePicker.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const props = withDefaults(
  defineProps<{ canSearch?: boolean; searchPlaceholder?: string }>(),
  {
    canSearch: true,
    searchPlaceholder: 'header.input.placeholder',
  }
)

const canSearch = computed(() =>
  props.canSearch !== undefined ? props.canSearch : true
)

const searchValue = ref('')
const route = useRoute()
const router = useRouter()
const searchPlaceholder = computed(() => props.searchPlaceholder)

const chips = computed(() => {
  const s = route.query.s
  if (!s) return []
  return Array.isArray(s) ? s : [s]
})

function removeSearchQueryItem(key: string) {
  const updatedChips = chips.value.filter((c) => c !== key)
  const newQuery = {
    ...route.query,
    s: updatedChips.length > 0 ? updatedChips : undefined,
  }

  router.push({ query: newQuery })
}

function handleSubmitForm() {
  const currentS = route.query.s
  const existing = Array.isArray(currentS)
    ? currentS
    : currentS
      ? [currentS]
      : []

  const newS = searchValue.value
    .split(',')
    .map((k) => k.trim())
    .filter((k) => k.length > 0)

  const combinedS = [...existing, ...newS]

  const newQuery = {
    ...route.query,
    s: combinedS,
  }

  router.push({ query: newQuery })
  searchValue.value = ''
}
</script>

<style scoped>
.list-header { min-width:0; height:auto; }
.list-toolbar { display:flex; align-items:center; flex-wrap:wrap; gap:16px; padding:16px; }
.list-toolbar-group { display:flex; align-items:center; gap:8px; flex:1 1 300px; min-width:0; }
.list-search { width:100%; min-width:0; max-width:560px; }
.list-search-input { background:#f8f8f8; padding-inline-start:42px; text-align:start; font-size:16px; }
.list-toolbar-actions { display:flex; align-items:center; justify-content:flex-end; gap:8px; flex-wrap:wrap; }
@media(max-width:600px) {
  .list-toolbar { padding:12px; gap:12px; }
  .list-toolbar-group { flex-basis:100%; }
  .list-toolbar-actions { width:100%; justify-content:flex-start; }
  .list-toolbar-actions :deep(button) { min-height:44px; }
}
</style>
