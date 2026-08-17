<template>
  <CustomStepper :steps="steps" v-model:modelValue="activeStep">
    <template #content>
      <form
        novalidate
        @submit.prevent="onSubmit"
        class="mt-10 flex flex-col gap-6"
      >
        <AccountDetailsForm
          v-show="activeStep === UserSteps.accountDetails"
          :userFormMode="FormMode.IsCreate"
          ref="accountDetailsFormRef"
        />

        <EmployeeDataForm
          v-show="activeStep === UserSteps.employeeData"
          ref="employeeDataFormRef"
        />

        <EmployeeDocumentsForm
          v-if="canCreateAttachment"
          v-show="activeStep === UserSteps.employeeDocuments"
          ref="employeeDocumentsFormRef"
          :userId="userId"
        />

        <SalaryDetailsForm
          v-show="activeStep === UserSteps.salaryDetails"
          ref="salaryDetailsFormRef"
        />

        <div class="py-4 w-full flex flex-row gap-4 justify-center">
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
  </CustomStepper>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import {
  SalaryType,
  UserSteps,
  type SalaryDetailsFormTypes,
  type UserForm,
} from '../../types/model'
import {
  useCreateUserMutation,
  useSalaryDetailsMutation,
  useUpdateUserDocumentsStepMutation,
  useUpdateUserMutation,
} from '../../requests/mutations'
import AccountDetailsForm from '../UserForms/AccountDetailsForm/AccountDetailsForm.vue'
import EmployeeDataForm from '../UserForms/EmployeeDataForm/EmployeeDataForm.vue'
import { FormMode } from '../UserTabs/UserTabs.types'
import EmployeeDocumentsForm from '../UserForms/EmployeeDocumentsForm/EmployeeDocumentsForm.vue'
import SalaryDetailsForm from '../UserForms/SalaryDetailsForm/SalaryDetailsForm.vue'
import CustomStepper from '@/modules/Core/components/shared/CustomStepper/CustomStepper.vue'
import type { StepItem } from '@/modules/Core/components/shared/CustomStepper/CustomStepper.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { t } = useI18n()
const router = useRouter()

const activeStep = ref<UserSteps>(UserSteps.accountDetails)

const { can } = usePermission()
const canCreateUser = can('User', 'Create')
const canCreateAttachment = can('Attachment', 'Create')

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

const allSteps = ref<StepItem[]>([
  {
    title: t('user.userCreateSteps.accountDetails.stepTitle'),
    icon: 'securityShieldUser',
    value: UserSteps.accountDetails,
    enTitle: 'accountDetails',
    isLastStep: false,
  },
  {
    title: t('user.userCreateSteps.employeeData.stepTitle'),
    icon: 'userPlus',
    value: UserSteps.employeeData,
    enTitle: 'employeeData',
    isLastStep: false,
  },
  {
    title: t('user.userCreateSteps.employeeDocuments.stepTitle'),
    icon: 'userCard',
    value: UserSteps.employeeDocuments,
    enTitle: 'employeeDocuments',
    isLastStep: false,
    permission: 'Attachment',
  },
  {
    title: t('user.userCreateSteps.salaryDetails.stepTitle'),
    icon: 'tagPrice',
    value: UserSteps.salaryDetails,
    enTitle: 'salaryDetails',
    isLastStep: true,
  },
])

const steps = computed(() => {
  return allSteps.value
    .filter((step) => {
      if (!step.permission) return true

      if (step.permission === 'Attachment') {
        return canCreateAttachment
      }

      return true
    })
    .map((step, index, array) => ({
      ...step,
      isLastStep: index === array.length - 1,
    }))
})

onMounted(() => {
  const storedStep = localStorage.getItem('activeStep')
  if (storedStep) {
    const stepValue = Number(storedStep)
    if (stepValue >= UserSteps.accountDetails && stepValue <= UserSteps.salaryDetails) {
      activeStep.value = stepValue
    } else {
      activeStep.value = UserSteps.accountDetails
    }
  } else {
    activeStep.value = UserSteps.accountDetails
  }

  if (activeStep.value > UserSteps.employeeData) {
    localStorage.removeItem('activeStep')
    router.push({ name: 'UserListRoute' })
  }
})

function nextStep() {
  if (activeStep.value < steps.value.length) {
    activeStep.value++
    localStorage.setItem('activeStep', activeStep.value.toString())
  }
}

const hasError = computed(() => {
  if (activeStep.value === UserSteps.accountDetails) {
    return accountDetailsFormRef.value?.hasError || false
  } else if (activeStep.value === UserSteps.employeeData) {
    return employeeDataFormRef.value?.hasError || false
  } else if (activeStep.value === UserSteps.employeeDocuments) {
    return employeeDocumentsFormRef.value?.hasError || false
  } else if (activeStep.value === UserSteps.salaryDetails) {
    return salaryDetailsFormRef.value?.hasError || false
  }
  return false
})

const { isPending: isCreatingUser, mutateAsync: createUser } =
  useCreateUserMutation()

const { isPending: isUpdatingUser, mutateAsync: updateUser } =
  useUpdateUserMutation()

const userForm = ref<UserForm>({
  hasError: false,
  password: '',
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

const canSubmitCurrentStep = computed(() => {
  if (
    activeStep.value === UserSteps.accountDetails ||
    activeStep.value === UserSteps.employeeData ||
    activeStep.value === UserSteps.salaryDetails
  ) {
    return canCreateUser
  }

  if (activeStep.value === UserSteps.employeeDocuments) {
    return canCreateAttachment
  }

  return false
})

const isSubmitDisabled = computed(() => {
  return hasError.value || !canSubmitCurrentStep.value || isSubmitLoading.value
})

const isSubmitLoading = computed(() => {
  return (
    isCreatingUser.value ||
    isUpdatingUser.value ||
    isCreatingSalaryDetails.value ||
    isUpdatePending.value
  )
})

const userId = ref<number>(-1)

async function onSubmit() {
  if (activeStep.value === UserSteps.accountDetails) {
    userForm.value.password = accountDetailsFormRef.value!.formValues.password
    userForm.value.userName = accountDetailsFormRef.value!.formValues.userName
  } else if (activeStep.value === UserSteps.employeeData) {
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

    if (userId.value > 0) {
      await updateUser({ id: userId.value, payload: userForm.value })
    } else {
      userId.value = (await createUser(userForm.value)).data.id
    }
  } else if (activeStep.value === UserSteps.employeeDocuments) {
    await updateUserDocumentsStep({ id: userId.value! })
  } else if (activeStep.value === UserSteps.salaryDetails) {
    salaryForm.value.type = salaryDetailsFormRef.value!.formValues.type
    salaryForm.value.amount = salaryDetailsFormRef.value!.formValues.amount
    salaryForm.value.saleSharePercent =
      salaryDetailsFormRef.value!.formValues.saleSharePercent
    salaryForm.value.installmentSharePercent =
      salaryDetailsFormRef.value!.formValues.installmentSharePercent

    await salaryDetails({ id: userId.value!, payload: salaryForm.value })
    localStorage.removeItem('activeStep')
    await router.push({ name: 'UserListRoute' })
    return
  }
  nextStep()
}
</script>
