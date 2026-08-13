<template>
  <header
    :class="`w-full bg-surface-3 rounded-2xl transition-all ${chips.length ? 'h-36' : 'h-20'}`"
  >
    <!-- Top section -->
    <div
      class="flex flex-row justify-between bg-white w-full py-5 px-6 border border-line rounded-2xl"
    >
      <div class="flex gap-2">
        <form
          v-if="canSearch"
          @submit.prevent="handleSubmitForm"
          class="relative w-[560px]"
        >
          <SvgIcon
            name="search"
            class="w-6 h-6 absolute top-1/2 -translate-y-1/2 right-2"
          />
          <InputText
            v-model="searchValue"
            :placeholder="$t(searchPlaceholder)"
            class="text-center h-10 w-full"
          />
        </form>
        <slot name="export"></slot>
      </div>

      <div class="flex gap-2">
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
