<template>
  <CustomTab :tabs="steps" v-model:modelValue="activeTab">
    <template #content>
      <form
        novalidate
        @submit.prevent="onSubmit"
        class="mt-10 flex flex-col gap-6"
      >
        <AccountDetailsForm
          v-show="activeTab === UserSteps.accountDetails"
          ref="accountDetailsFormRef"
          :userName="userInfo.username"
          :userFormMode="userFormMode"
        />

        <EmployeeDataForm
          v-show="activeTab === UserSteps.employeeData"
          ref="employeeDataFormRef"
          :address="userInfo.address"
          :userFormMode="userFormMode"
          :fullName="userInfo.fullName"
          :motherName="userInfo.motherName"
          :nationalCode="userInfo.nationalCode"
          :birthDate="userInfo.birthDate"
          :roleId="userInfo.roles[0].id"
          :branchIds="userInfo.branches.map((branch) => branch.id)"
          :phoneNumber="userInfo.phoneNumber"
        />

        <EmployeeDocumentsForm
          v-if="canReadAttachment"
          v-show="activeTab === UserSteps.employeeDocuments"
          ref="employeeDocumentsFormRef"
          :userFormMode="getFormModeForAttachment"
          :userId="userId"
          :attachments="userInfo.attachments"
        />

        <SalaryDetailsForm
          v-show="activeTab === UserSteps.salaryDetails"
          ref="salaryDetailsFormRef"
          :userFormMode="userFormMode"
          :type="userInfo.salaryDetail.type"
          :amount="userInfo.salaryDetail.amount"
          :installmentSharePercent="
            userInfo.salaryDetail.installmentSharePercent
          "
          :saleSharePercent="userInfo.salaryDetail.saleSharePercent"
        />

        <PowersForm
          v-if="canReadPermission"
          v-show="activeTab === UserSteps.power"
          :roleId="userInfo.roles[0].id"
          ref="powersFormRef"
          :userFormMode="getFormModeForPermission"
          :selectedPermissions="userInfo.allPermissions"
        />

        <div
          v-if="userFormMode !== FormMode.IsView"
          class="py-4 w-full flex flex-row gap-4 justify-center"
        >
          <Button
            :disabled="isSubmitDisabled"
            :loading="isSubmitLoading"
            class="py-4 w-1/4"
            :class="'btn-primary'"
            type="submit"
          >
            {{ $t('words.save') }}
            <i v-if="isSubmitLoading" class="pi pi-spin pi-spinner"></i>
          </Button>
        </div>
      </form>
    </template>
  </CustomTab>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import {
  SalaryType,
  UserSteps,
  type SalaryDetailsFormTypes,
  type UserForm,
} from '../../types/model'
import {
  useSalaryDetailsMutation,
  useUpdateUserDocumentsStepMutation,
  useUpdateUserMutation,
  useUserPermissionMutation,
} from '../../requests/mutations'
import { FormMode, type Props } from './UserTabs.types'
import AccountDetailsForm from '../UserForms/AccountDetailsForm/AccountDetailsForm.vue'
import EmployeeDataForm from '../UserForms/EmployeeDataForm/EmployeeDataForm.vue'
import EmployeeDocumentsForm from '../UserForms/EmployeeDocumentsForm/EmployeeDocumentsForm.vue'
import SalaryDetailsForm from '../UserForms/SalaryDetailsForm/SalaryDetailsForm.vue'
import PowersForm from '../UserForms/PowersForm/PowersForm.vue'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import CustomTab from '@/modules/Core/components/shared/CustomTab/CustomTab.vue'
import type { TabItem } from '@/modules/Core/components/shared/CustomTab/CustomTab.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

defineProps<Props>()
const { t } = useI18n()
const router = useRouter()

const activeTab = ref<UserSteps>(UserSteps.accountDetails)

const { can } = usePermission()
const canUpdatePermission = can('Permission', 'Update')
const canUpdateAttachment = can('Attachment', 'Update')
const canUpdateUser = can('User', 'Update')
const canReadPermission = can('Permission', 'Read')
const canReadAttachment = can('Attachment', 'Read')

const getFormModeForAttachment = computed(() => {
  if (userFormMode.value === FormMode.IsView) return FormMode.IsView
  return canUpdateAttachment ? FormMode.IsUpdate : FormMode.IsView
})

const getFormModeForPermission = computed(() => {
  if (userFormMode.value === FormMode.IsView) return FormMode.IsView
  return canUpdatePermission ? FormMode.IsUpdate : FormMode.IsView
})

const accountDetailsFormRef = ref<InstanceType<
  typeof AccountDetailsForm
> | null>(null)

const employeeDataFormRef = ref<InstanceType<typeof EmployeeDataForm> | null>(
  null
)
const employeeDocumentsFormRef = ref<{ hasError: boolean }>()

const salaryDetailsFormRef = ref<InstanceType<typeof SalaryDetailsForm> | null>(
  null
)

const powersFormRef = ref<InstanceType<typeof PowersForm> | null>()

const allSteps = ref<TabItem[]>([
  {
    title: t('user.userCreateSteps.accountDetails.stepTitle'),
    icon: 'securityShieldUser',
    value: UserSteps.accountDetails,
    enTitle: 'accountDetails',
    isLastTab: false,
  },
  {
    title: t('user.userCreateSteps.employeeData.stepTitle'),
    icon: 'userPlus',
    value: UserSteps.employeeData,
    enTitle: 'employeeData',
    isLastTab: false,
  },
  {
    title: t('user.userCreateSteps.employeeDocuments.stepTitle'),
    icon: 'userCard',
    value: UserSteps.employeeDocuments,
    enTitle: 'employeeDocuments',
    isLastTab: false,
    permission: 'Attachment',
  },
  {
    title: t('user.userCreateSteps.salaryDetails.stepTitle'),
    icon: 'tagPrice',
    value: UserSteps.salaryDetails,
    enTitle: 'salaryDetails',
    isLastTab: false,
  },
  {
    title: t('user.userCreateSteps.powers.stepTitle'),
    icon: 'dashboard',
    value: UserSteps.power,
    enTitle: 'powers',
    isLastTab: true,
    permission: 'Permission',
  },
])

const steps = computed(() => {
  return allSteps.value.filter((step) => {
    if (!step.permission) return true

    if (step.permission === 'Attachment') {
      return canReadAttachment
    }
    if (step.permission === 'Permission') {
      return canReadPermission
    }

    return true
  })
})

function nextStep() {
  if (activeTab.value < steps.value.length) {
    activeTab.value++
  }
}

const canSubmitCurrentStep = computed(() => {
  if (userFormMode.value === FormMode.IsView) return false

  if (userFormMode.value === FormMode.IsUpdate) {
    if (
      activeTab.value === UserSteps.accountDetails ||
      activeTab.value === UserSteps.employeeData ||
      activeTab.value === UserSteps.salaryDetails
    ) {
      return canUpdateUser
    }

    if (activeTab.value === UserSteps.employeeDocuments) {
      return canUpdateAttachment
    }

    if (activeTab.value === UserSteps.power) {
      return canUpdatePermission
    }
  }

  return false
})

const isSubmitDisabled = computed(() => {
  return (
    hasError.value ||
    !canSubmitCurrentStep.value ||
    isUpdatingUser.value ||
    isCreatingSalaryDetails.value ||
    isUserPermission.value ||
    isUpdatePending.value
  )
})

const isSubmitLoading = computed(() => {
  return (
    isUpdatingUser.value ||
    isCreatingSalaryDetails.value ||
    isUserPermission.value ||
    isUpdatePending.value
  )
})

const hasError = computed(() => {
  if (activeTab.value === UserSteps.accountDetails) {
    return accountDetailsFormRef.value?.hasError || false
  } else if (activeTab.value === UserSteps.employeeData) {
    return employeeDataFormRef.value?.hasError || false
  } else if (activeTab.value === UserSteps.employeeDocuments) {
    return employeeDocumentsFormRef.value?.hasError || false
  } else if (activeTab.value === UserSteps.salaryDetails) {
    return salaryDetailsFormRef.value?.hasError || false
  }
  return false
})

const { isPending: isUpdatingUser, mutateAsync: updateUser } =
  useUpdateUserMutation()

const userForm = ref<UserForm>({
  hasError: false,
  userName: '',
  fullName: '',
  motherName: '',
  nationalCode: '',
  birthDate: '',
  roleId: 0,
  branchIds: [],
  address: '',
  phoneNumber: '',
})

const { isPending: isCreatingSalaryDetails, mutateAsync: salaryDetails } =
  useSalaryDetailsMutation()

const salaryForm = ref<SalaryDetailsFormTypes>({
  hasError: false,
  type: SalaryType.CommissionBased,
  amount: 0,
  saleSharePercent: null,
  installmentSharePercent: null,
})

const { mutateAsync: updateUserDocumentsStep, isPending: isUpdatePending } =
  useUpdateUserDocumentsStepMutation()

const { isPending: isUserPermission, mutateAsync: userPermission } =
  useUserPermissionMutation()

const toastStore = useToastStore()

watch([isUserPermission], () => {
  if (isUserPermission.value) {
    toastStore.setMassage({
      title: 'user.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function onSubmit() {
  if (!canSubmitCurrentStep.value) {
    toastStore.setMassage({
      title: t('errors.noPermission'),
      description: t('errors.noPermissionDescription'),
      dialogState: DialogState.Error,
      isOpen: true,
    })
    return
  }

  if (activeTab.value === UserSteps.accountDetails) {
    userForm.value.userName = accountDetailsFormRef.value!.formValues.userName
  } else if (activeTab.value === UserSteps.employeeData) {
    userForm.value.fullName = employeeDataFormRef.value!.formValues.fullName
    userForm.value.motherName = employeeDataFormRef.value!.formValues.motherName
    userForm.value.nationalCode =
      employeeDataFormRef.value!.formValues.nationalCode
    userForm.value.birthDate = employeeDataFormRef.value!.formValues.birthDate
    userForm.value.roleId = employeeDataFormRef.value!.formValues.roleId
    userForm.value.branchIds = employeeDataFormRef.value!.formValues.branchIds
    userForm.value.address = employeeDataFormRef.value!.formValues.address
    userForm.value.phoneNumber =
      employeeDataFormRef.value!.formValues.phoneNumber

    await updateUser({ id: userId.value!, payload: userForm.value })
  } else if (activeTab.value === UserSteps.employeeDocuments) {
    if (!canUpdateAttachment) {
      toastStore.setMassage({
        title: t('errors.noPermission'),
        description: t('errors.noPermissionForAttachment'),
        dialogState: DialogState.Error,
        isOpen: true,
      })
      return
    }
    await updateUserDocumentsStep({ id: userId.value! })
  } else if (activeTab.value === UserSteps.salaryDetails) {
    salaryForm.value.type = salaryDetailsFormRef.value!.formValues.type
    salaryForm.value.amount = salaryDetailsFormRef.value!.formValues.amount
    salaryForm.value.saleSharePercent =
      salaryDetailsFormRef.value!.formValues.saleSharePercent
    salaryForm.value.installmentSharePercent =
      salaryDetailsFormRef.value!.formValues.installmentSharePercent

    await salaryDetails({ id: userId.value!, payload: salaryForm.value })
  } else if (activeTab.value === UserSteps.power) {
    if (!canUpdatePermission) {
      toastStore.setMassage({
        title: t('errors.noPermission'),
        description: t('errors.noPermissionForPermission'),
        dialogState: DialogState.Error,
        isOpen: true,
      })
      return
    }
    const permissionIds = powersFormRef.value?.permissionIds || []

    await userPermission({
      id: userId.value!,
      payload: { permissionIds },
    })

    await router.push({ name: 'UserListRoute' })
  }
  nextStep()
}

const route = useRoute()

const userId = computed(() => Number(route.params.userId))

const userFormMode = computed(() => {
  return route.name === 'UserCreateRoute'
    ? FormMode.IsCreate
    : route.name === 'UserUpdateRoute'
      ? FormMode.IsUpdate
      : FormMode.IsView
})
</script>
