<template>
  <Dialog
    :visible="visible"
    modal
    closable
    dismissableMask
    closeOnEscape
    :header="title"
    :style="{ width: 'min(760px, 96vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && emit('close')"
  >
    <div class="df-dialog-body">
      <p v-if="subtitle" class="df-muted">{{ subtitle }}</p>
      <div v-if="!rows.length" class="df-empty">
        {{ $t('delegateFollowup.dialog.historyEmpty') }}
      </div>
      <div v-else class="df-dialog-scroll df-table-wrap">
        <table class="df-table">
          <thead>
            <tr>
              <th v-for="c in columns" :key="c.key" scope="col">
                {{ c.label }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(r, i) in rows" :key="i">
              <td
                v-for="c in columns"
                :key="c.key"
                :class="{ wrap: c.wrap }"
              >
                <span v-if="c.num" class="df-num">{{ r[c.key] ?? '—' }}</span>
                <template v-else>{{ r[c.key] ?? '—' }}</template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <footer class="df-dialog-foot">
        <button type="button" class="df-btn ghost" @click="emit('close')">
          {{ $t('delegateFollowup.dialog.close') }}
        </button>
      </footer>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'

export interface HistoryColumn {
  key: string
  label: string
  num?: boolean
  wrap?: boolean
}

export type HistoryRow = Record<string, string | number | null | undefined>

defineProps<{
  visible: boolean
  title: string
  subtitle?: string
  columns: HistoryColumn[]
  rows: HistoryRow[]
}>()

const emit = defineEmits<{ close: [] }>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}
</script>
