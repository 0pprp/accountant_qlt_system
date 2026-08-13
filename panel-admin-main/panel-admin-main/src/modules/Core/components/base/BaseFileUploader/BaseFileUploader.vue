<template>
  <div class="card flex flex-col items-center gap-6">
    <FileUpload
      ref="fileUploadRef"
      :unstyled="false"
      class="!border-dashed"
      accept="image/*"
      name="demo[]"
      :multiple="false"
      url="/api/upload"
      @select="onFileSelect"
      @click="chooseCallback()"
    >
      <template #header>
        <div class="text-info"></div>
        <div class="text-warning"></div>
        <div class="bg-warning"></div>
        <div class="text-purple"></div>
      </template>

      <template
        #content="{
          files,
          uploadedFiles,
          removeUploadedFileCallback,
          removeFileCallback,
          messages,
        }"
      >
        <div class="relative">
          <div
            v-if="formMode === FormMode.IsUpdate && src"
            class="absolute inset-0 rounded-xl bg-black opacity-25"
          ></div>

          <img
            v-if="finalSrc"
            :src="finalSrc"
            alt="Image"
            class="shadow-md rounded-xl h-[175px] w-full afterFilter"
          />

          <Button
            v-if="formMode === FormMode.IsUpdate && src"
            class="absolute bg-white text-primary set-center"
            @click="triggerFileSelect"
          >
            {{ $t('words.edit') }}
            <SvgIcon name="boldPencil" />
          </Button>
        </div>
      </template>

      <template #empty>
        <div
          v-if="src === null"
          class="flex items-center justify-center flex-col"
        >
          <i class="pi pi-images !p-8 !text-4xl !text-gray-300" />
          <p class="text-center !p-6 !pt-0">
            <strong class="text-primary"> أضغط لتحميل </strong>
            <strong class="text-gray-700"> او اسحب و افلت </strong>
            <br />
            <span class="text-gray-300"> JPG, JPEG, PNG </span>
          </p>
        </div>
      </template>
    </FileUpload>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, computed } from 'vue'
import { Button, FileUpload } from 'primevue'
import type { Emits, Props } from './BaseFileUploader.types'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const baseFileUrl = import.meta.env.VITE_FILE_BASE_URL

const src = computed(() => {
  if (props.existingFile?.relativePath) {
    return baseFileUrl + props.existingFile.relativePath
  }
  return null
})

const tempSrc = ref<string | null>(null)
const finalSrc = computed(() => {
  if (tempSrc.value) {
    return tempSrc.value
  }
  return src.value
})

interface FileUploadInstance extends InstanceType<typeof FileUpload> {
  choose: () => void
  clear: () => void
  $el: HTMLElement
  files: File[]
}

const fileUploadRef = ref<FileUploadInstance | null>(null)

function resetFileInput() {
  if (fileUploadRef.value?.$el) {
    const fileInput = fileUploadRef.value.$el.querySelector(
      'input[type="file"]'
    ) as HTMLInputElement
    if (fileInput) {
      fileInput.value = ''
    }
  }
}

function clearFiles() {
  if (fileUploadRef.value) {
    fileUploadRef.value.clear?.()

    if (fileUploadRef.value.files) {
      fileUploadRef.value.files.length = 0
    }
  }
}

function chooseCallback() {
  if (
    fileUploadRef.value &&
    (props.formMode === FormMode.IsUpdate || src.value === null)
  ) {
    clearFiles()
    resetFileInput()
    fileUploadRef.value.choose()
  }
  // if (fileUploadRef.value && props.formMode !== FormMode.IsView) {
  //   clearFiles()
  //   resetFileInput()
  //   fileUploadRef.value.choose()
  // }
}

function triggerFileSelect() {
  if (fileUploadRef.value) {
    clearFiles()
    resetFileInput()
  }
}

async function onFileSelect(event: { files: File[] }) {
  const file = event.files[event.files.length - 1]

  await nextTick()

  if (fileUploadRef.value) {
    const latestFile = event.files[event.files.length - 1]
    event.files.length = 0
    event.files.push(latestFile)
  }

  const reader = new FileReader()

  reader.onload = async (e: ProgressEvent<FileReader>) => {
    tempSrc.value = e.target?.result as string
  }

  reader.readAsDataURL(file)
  emit('uploadFile', file)
}
</script>
