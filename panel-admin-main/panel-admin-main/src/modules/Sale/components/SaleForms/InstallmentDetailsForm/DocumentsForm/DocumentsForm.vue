<template>
  <Accordion :unstyled="false" :value="activeTab">
    <AccordionPanel
      :unstyled="false"
      v-for="tab in tabs"
      :key="tab.title"
      :value="tab.value"
      class="mb-4"
    >
      <AccordionHeader
        :unstyled="false"
        @click="activeTab = tab.value"
        class="border"
      >
        <div class="flex items-center">
          <span class="rounded-2xl bg-white p-3 ml-2">
            <SvgIcon :name="tab.iconName" class="text-secondary" />
          </span>

          <div>
            <span class="text-gray-700">
              {{ tab.title }}
            </span>

            <br />

            <span class="text-gray-300">
              {{ tab.description }}
            </span>
          </div>
        </div>

        <div class="mr-auto ml-2">
          <SvgIcon
            name="boldCheckCircle"
            :class="[tab.isCompleted ? 'text-success' : 'text-surface-2']"
          />
        </div>
      </AccordionHeader>

      <AccordionContent :unstyled="false">
        <div class="flex flex-wrap gap-4 justify-center">
          <BaseFileUploader
            v-for="n in Math.max(0, tab.requiredUploads - tab.completedUploads)"
            :key="`empty-${tab.value}-${n}`"
            :formMode="saleFormMode"
            :loading="isUpdateLoading"
            @upload-file="(file) => uploadChosenFile(file, tab.value)"
          />

          <BaseFileUploader
            v-for="(upload, index) in tab.uploads"
            :key="`upload-${tab.value}-${upload.id}-${index}`"
            :formMode="saleFormMode"
            :loading="isUpdateLoading"
            :existingFile="upload"
            @upload-file="
              (file) => uploadChosenFile(file, tab.value, upload.id)
            "
          />
        </div>
      </AccordionContent>
    </AccordionPanel>
  </Accordion>
</template>

<script setup lang="ts">
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
} from 'primevue'
import { computed, nextTick, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import type { Props } from './DocumentsForm.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import BaseFileUploader from '@/modules/Core/components/base/BaseFileUploader/BaseFileUploader.vue'
import { useUpdateAttachmentMutation } from '@/modules/Core/requests/mutation'
import { AttachmentType } from '@/modules/Core/types/model/attachment'
import type { Attachment } from '@/modules/User/types/model'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'

export interface AttachmentForm {
  file: File
  type: AttachmentType
  userId?: number
  orderId?: number
  customerId?: number
}

const props = defineProps<Props>()

const activeTab = ref(AttachmentType.NationalCard)
const { t } = useI18n()

const documentValue = ref<Array<AttachmentForm>>([])

const tabs = ref<
  Array<{
    title: string
    description: string
    iconName: string
    value: AttachmentType
    isCompleted: boolean
    completedUploads: number
    requiredUploads: number
    uploads: Attachment[]
  }>
>([
  {
    title: t('sale.saleForm.documents.purchaseReceipt.title'),
    description: t('sale.saleForm.documents.purchaseReceipt.description'),
    iconName: 'boldSecurityObject',
    value: AttachmentType.PurchaseReceipt,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 1,
    uploads: [],
  },
  {
    title: t('sale.saleForm.documents.trustReceipt.title'),
    description: t('sale.saleForm.documents.trustReceipt.description'),
    iconName: 'boldSecurityScanner',
    value: AttachmentType.TrustReceipt,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 1,
    uploads: [],
  },
  {
    title: t('sale.saleForm.documents.saleContract.title'),
    description: t('sale.saleForm.documents.saleContract.description'),
    iconName: 'boldSecurityFaceScanner',
    value: AttachmentType.SaleContract,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 1,
    uploads: [],
  },
])

function loadExistingAttachments() {
  if (props.attachments && props.attachments.length > 0) {
    tabs.value.forEach((tab) => {
      tab.uploads = []
      tab.completedUploads = 0
      tab.isCompleted = false
    })

    props.attachments.forEach((attachment) => {
      const tab = tabs.value.find((t) => t.value === attachment.type)
      if (tab) {
        const existsInUploads = tab.uploads.some(
          (upload) => upload.id === attachment.id
        )

        if (!existsInUploads) {
          tab.uploads.push({
            id: attachment.id,
            originalFileName: attachment.originalFileName,
            relativePath: attachment.relativePath,
            fileSizeInByte: attachment.fileSizeInByte,
            type: attachment.type,
          })
        }
      }
    })

    tabs.value.forEach((tab) => {
      tab.uploads = tab.uploads.filter(
        (upload, index, self) =>
          index === self.findIndex((u) => u.id === upload.id)
      )

      tab.completedUploads = tab.uploads.length

      if (tab.completedUploads >= tab.requiredUploads) {
        tab.isCompleted = true
      }
    })
  }
}

watch(() => props.attachments, loadExistingAttachments, { immediate: true })

const { isPending: isUpdateLoading, mutateAsync: updateAttachment } =
  useUpdateAttachmentMutation()

async function uploadChosenFile(
  file: File,
  attachmentType: AttachmentType,
  existingId?: number
) {
  const tab = tabs.value.find((tab) => tab.value === attachmentType)
  if (!tab) return

  if (existingId) {
    await updateAttachment({
      id: existingId,
      payload: { file, type: attachmentType },
    })
  } else {
    if (props.saleFormMode === FormMode.IsCreate) {
      documentValue.value = documentValue.value.filter(
        (doc) => doc.type !== attachmentType
      )
    }
    const newUpload: AttachmentForm = {
      file: file,
      type: attachmentType,
    }

    documentValue.value.push(newUpload)

    tab.completedUploads = documentValue.value.filter(
      (doc) => doc.type === tab.value
    ).length

    if (tab.completedUploads >= tab.requiredUploads) {
      tab.isCompleted = true
    }
  }
  await nextTick()
}

const hasError = computed(() => {
  return tabs.value.some((tab) => !tab.isCompleted)
})

defineExpose({ documentValue, hasError })
</script>
