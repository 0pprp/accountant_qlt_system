<template>
  <div class="flex flex-col gap-4 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.employeeDocuments.personalDocumentsTitle') }}
    </span>

    <Accordion :unstyled="false" :value="activeTab">
      <AccordionPanel
        :unstyled="false"
        v-for="tab in tabs"
        :key="tab.title"
        :value="tab.value"
        class="mt-4"
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
              v-for="slotIndex in tab.requiredUploads"
              :key="`slot-${tab.value}-${slotIndex}`"
              :formMode="customerFormMode"
              :loading="isCreateLoading || isUpdateLoading"
              :existingFile="tab.uploads[slotIndex - 1] || null"
              @upload-file="
                (file) =>
                  uploadChosenFile(
                    file,
                    tab.value,
                    tab.uploads[slotIndex - 1]?.id,
                    slotIndex - 1
                  )
              "
            />
          </div>
        </AccordionContent>
      </AccordionPanel>
    </Accordion>
  </div>
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
import type { Props } from './CustomerDocumentsForm.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import BaseFileUploader from '@/modules/Core/components/base/BaseFileUploader/BaseFileUploader.vue'
import {
  useCreateAttachmentMutation,
  useUpdateAttachmentMutation,
} from '@/modules/Core/requests/mutation'
import { AttachmentType } from '@/modules/Core/types/model/attachment'
import type { Attachment } from '@/modules/User/types/model'

const props = defineProps<Props>()

const activeTab = ref(AttachmentType.NationalCard)

const { t } = useI18n()
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
    title: t('user.employeeDocumentsForm.nationalIdentity.title'),
    description: t('user.employeeDocumentsForm.nationalIdentity.description'),
    iconName: 'boldSecurityObject',
    value: AttachmentType.NationalCard,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 2,
    uploads: [],
  },
  {
    title: t('user.employeeDocumentsForm.housingCard.title'),
    description: t('user.employeeDocumentsForm.housingCard.description'),
    iconName: 'boldSecurityScanner',
    value: AttachmentType.ResidenceCard,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 2,
    uploads: [],
  },
  {
    title: t('user.employeeDocumentsForm.rationCard.title'),
    description: t('user.employeeDocumentsForm.rationCard.description'),
    iconName: 'boldSecurityCodeScanner',
    value: AttachmentType.RationCard,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 1,
    uploads: [],
  },
  {
    title: t('user.employeeDocumentsForm.personalPhoto.title'),
    description: t('user.employeeDocumentsForm.personalPhoto.description'),
    iconName: 'boldSecurityFaceScanner',
    value: AttachmentType.PersonalPicture,
    isCompleted: false,
    completedUploads: 0,
    requiredUploads: 1,
    uploads: [],
  },
])

function loadExistingAttachments() {
  if (props.attachments && props.attachments.length > 0) {
    tabs.value.forEach((tab) => {
      tab.uploads = new Array(tab.requiredUploads).fill(null)
      tab.completedUploads = 0
      tab.isCompleted = false
    })

    props.attachments.forEach((attachment) => {
      const tab = tabs.value.find((t) => t.value === attachment.type)
      if (tab) {
        const emptySlotIndex = tab.uploads.findIndex(
          (upload) => upload === null
        )
        if (emptySlotIndex !== -1) {
          tab.uploads[emptySlotIndex] = {
            id: attachment.id,
            originalFileName: attachment.originalFileName,
            relativePath: attachment.relativePath,
            fileSizeInByte: attachment.fileSizeInByte,
            type: attachment.type,
          }
        }
      }
    })

    tabs.value.forEach((tab) => {
      tab.completedUploads = tab.uploads.filter(
        (upload) => upload !== null
      ).length
      tab.isCompleted = tab.completedUploads >= tab.requiredUploads
    })
  } else {
    tabs.value.forEach((tab) => {
      tab.uploads = new Array(tab.requiredUploads).fill(null)
      tab.completedUploads = 0
      tab.isCompleted = false
    })
  }
}

watch(() => props.attachments, loadExistingAttachments, { immediate: true })

const { isPending: isCreateLoading, mutateAsync: createAttachment } =
  useCreateAttachmentMutation()

const { isPending: isUpdateLoading, mutateAsync: updateAttachment } =
  useUpdateAttachmentMutation()

async function uploadChosenFile(
  file: File,
  attachmentType: AttachmentType,
  existingId?: number,
  slotIndex?: number
) {
  const payload = {
    customerId: props.customerId,
    file: file,
    type: attachmentType,
  }

  const tab = tabs.value.find((tab) => tab.value === attachmentType)
  if (!tab) return

  if (existingId) {
    await updateAttachment({ id: existingId, payload: payload })

    if (slotIndex !== undefined && tab.uploads[slotIndex]) {
      tab.uploads[slotIndex] = {
        ...tab.uploads[slotIndex],
        originalFileName: file.name,
        fileSizeInByte: file.size,
      }
    }
  } else {
    const result = await createAttachment(payload)

    const newUpload = {
      id: result.data.id || 0,
      originalFileName: file.name,
      relativePath: result.data?.relativePath || '',
      fileSizeInByte: file.size,
      type: attachmentType,
    }

    if (slotIndex !== undefined) {
      tab.uploads[slotIndex] = newUpload
    } else {
      const emptySlotIndex = tab.uploads.findIndex((upload) => upload === null)
      if (emptySlotIndex !== -1) {
        tab.uploads[emptySlotIndex] = newUpload
      }
    }
  }

  tab.completedUploads = tab.uploads.filter((upload) => upload !== null).length
  tab.isCompleted = tab.completedUploads >= tab.requiredUploads

  await nextTick()
}

const hasError = computed(() => {
  return tabs.value.some((tab) => !tab.isCompleted)
})

defineExpose({ hasError })
</script>
