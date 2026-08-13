<template>
  <div>
    <TheHeader>
      <template #create>
        <Button
          v-if="canCreateBranch"
          class="bg-primary text-white"
          @click="handleCreate()"
        >
          {{ $t('branch.createBranch') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>
    </TheHeader>

    <BranchList v-if="canReadBranch" @edit-item="handleEdit" />

    <CustomDialog
      v-model="isOpen"
      title="branch.createBranch"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <BranchForm
          :loading="isCreating || isUpdating"
          :selectedItem
          :formState
          @submit="onSubmit"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Button } from 'primevue'
import BranchForm from '../components/BranchForm/BranchForm.vue'
import BranchList from '../components/BranchList/BranchList.vue'
import {
  useCreateBranchMutation,
  useUpdateBranchMutation,
} from '../requests/mutations'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import type {
  Branch,
  BranchForm as BranchFormType,
} from '@/modules/Branch/types/model'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const isOpen = ref(false)

const formState = ref<FormsState>(FormsState.Create)
const { can } = usePermission()
const canCreateBranch = can('Branch', 'Create')
const canReadBranch = can('Branch', 'Read')

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const { isPending: isCreating, mutateAsync: createBranch } =
  useCreateBranchMutation()

const { isPending: isUpdating, mutateAsync: updateBranch } =
  useUpdateBranchMutation()

const toastStore = useToastStore()

watch([isCreating, isUpdating], () => {
  if (isCreating.value) {
    toastStore.setMassage({
      title: 'branch.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  } else if (isUpdating.value) {
    toastStore.setMassage({
      title: 'branch.updatePending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function onSubmit(values: BranchFormType) {
  if (formState.value === FormsState.Create) {
    await createBranch(values)
  } else {
    await updateBranch({ id: selectedItem.value!.id, payload: values })
  }

  toggleIsOpen()
}
const selectedItem = ref<Branch>()

function handleCreate() {
  formState.value = FormsState.Create
  selectedItem.value = undefined
  toggleIsOpen()
}

function handleEdit(item: Branch) {
  formState.value = FormsState.Update
  selectedItem.value = item
  toggleIsOpen()
}
</script>
