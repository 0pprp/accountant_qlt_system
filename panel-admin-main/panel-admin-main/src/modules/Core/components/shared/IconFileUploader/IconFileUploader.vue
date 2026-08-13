<template>
  <Button
    v-if="file"
    class="bg-white shadow-sm shadow-primary w-fit block max-w-[100px]"
    as="a"
    :href="baseUrl + file[0].relativePath"
    target="_blank"
    rel="noopener noreferrer"
  >
    <SvgIcon name="boldLetterOpened" class="text-primary" />

    <!-- <span v-if="file" class="max-w-[100px] text-wrap">
      {{ file[0].originalFileName }}
    </span> -->
  </Button>

  <Button v-else class="bg-white shadow-sm">
    <span class="opacity-0 absolute w-[100px]">
      <FileUpload
        ref="fileUploadRef"
        mode="basic"
        :disabled="props.disabled"
        :maxFileSize="1000000"
        :auto="false"
        class="custom-file-uploader"
        @select="onSelect($event)"
      >
        <template #empty>
          <span>Your custom text here</span>
        </template>
      </FileUpload>
    </span>
    <SvgIcon name="boldLetterOpened" class="text-primary" />
  </Button>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Button, FileUpload, type FileUploadSelectEvent } from 'primevue'
import type { Emits, Props } from './IconFileUploader.types'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const fileUploadRef = ref(null)

const baseUrl = import.meta.env.VITE_FILE_BASE_URL
async function onSelect(event: FileUploadSelectEvent) {
  emit('uploadFile', event.files)
}
</script>

<style scoped>
:deep(.custom-file-uploader + span) {
  display: none !important;
}

:deep([data-pc-section='basiccontent'] > span:last-child) {
  display: none !important;
}
</style>
