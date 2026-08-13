<template>
  <div
    class="p-2 flex items-center border border-line justify-around w-[220px] sidebar-menu sticky top-1 flex-col bg-white rounded-2xl overflow-y-auto"
  >
    <div class="text-center">
      <div class="pb-2">
        <!-- TODO set profile image if exist -->
        <Avatar
          v-if="profileImage"
          image="/images/avatar/amyelsner.png"
          class="!w-32 !h-32"
          size="xlarge"
          shape="circle"
        />
        <Avatar
          v-else
          :unstyled="false"
          icon="pi pi-user"
          class="!w-32 !h-32"
          size="xlarge"
          shape="circle"
        />
      </div>

      <strong>
        {{ userInfo.fullName }}
      </strong>
    </div>

    <div class="w-full">
      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.birthDate }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>
    </div>

    <div class="w-full">
      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>
    </div>

    <div class="w-full">
      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>

      <div class="flex flex-row border border-line rounded-2xl py-1 px-2 my-1">
        <span> x: </span>
        <span class="px-2 font-normal">
          {{ userInfo.creationStep }}
        </span>
      </div>
    </div>

    <div class="w-full">
      <Button
        v-if="!isUpdateMode"
        class="bg-primary text-white w-full"
        v-slot="slotProps"
      >
        <RouterLink
          :to="{ name: 'UserUpdateRoute', params: { userId: userId } }"
          :class="slotProps.a11yAttrs"
        >
          {{ $t('words.edit') }}</RouterLink
        >
        <SvgIcon name="boldPencil" />
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Avatar, Button } from 'primevue'
import { computed } from 'vue'
import type { Props } from './UserInfoBox.types'
import type { Attachment } from '../../types/model'
import { AttachmentType } from '@/modules/Core/types/model/attachment'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const props = defineProps<Props>()

const profileImage = computed(() => {
  return props.userInfo?.attachments.map(
    (attachment: Attachment) =>
      attachment.type === AttachmentType.ProfilePicture
  )[0]
})
</script>
