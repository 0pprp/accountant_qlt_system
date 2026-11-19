import { createApp, defineComponent, h, nextTick, ref } from 'vue'
import { describe, it, expect, vi } from 'vitest'
import EditSalesRequestDialog from '../EditSalesRequestDialog.vue'

vi.mock('primevue', () => ({ Dialog: defineComponent({ setup: (_, { slots }) => () => h('div', slots.default?.()) }) }))
vi.mock('@/modules/Core/requests/queries', () => ({ useProvinceDataQuery: () => ({ data: ref(undefined), isLoading: ref(true) }) }))
vi.mock('@/modules/Branch/requests/queries', () => ({ useBranchDataQuery: () => ({ data: ref(undefined), isLoading: ref(true) }) }))
vi.mock('../../requests/queries/useAssignableEmployeesQuery', () => ({ useAssignableEmployeesQuery: () => ({ data: ref([]), isLoading: ref(false) }) }))
vi.mock('@/modules/Core/components/ui', () => {
  const input = defineComponent({ props: ['modelValue', 'label'], emits: ['update:modelValue'], setup: (props, { emit }) => () => h('input', { 'aria-label': props.label, value: props.modelValue, onInput: (e: Event) => emit('update:modelValue', (e.target as HTMLInputElement).value) }) })
  return {
    AppInput: input, AppSearchField: input, AppSelect: input,
    DataState: defineComponent({ setup: () => () => h('div') }),
    AppButton: defineComponent({ props: ['disabled'], emits: ['click'], setup: (props, { emit, slots }) => () => h('button', { type: 'button', disabled: props.disabled, onClick: (e: Event) => emit('click', e) }, slots.default?.()) }),
  }
})

describe('sales request edit', () => {
  it('keeps the request branch while lookups load and saves normalized phone without reassignment', async () => {
    const open = ref(false)
    const confirm = vi.fn()
    const host = document.createElement('div')
    const app = createApp({ render: () => h(EditSalesRequestDialog, { open: open.value, request: { id: 7, customerName: 'احمد حسن كاظم', customerPhone: '٠٧٨٠١١١٢٢٣٣', branchId: 2, provinceId: 4, targetEmployeeId: 55 } as any, onConfirm: confirm }) })
    app.mount(host)
    try {
      open.value = true
      await nextTick()
      await nextTick()
      const save = Array.from(host.querySelectorAll('button')).find(b => b.textContent?.includes('حفظ التعديل'))!
      expect(save.disabled).toBe(false)
      save.click()
      expect(confirm).toHaveBeenCalledWith(expect.objectContaining({ branchId: 2, provinceId: 4, customerPhone: '07801112233', employeeId: undefined }))
    } finally { app.unmount() }
  })
})
