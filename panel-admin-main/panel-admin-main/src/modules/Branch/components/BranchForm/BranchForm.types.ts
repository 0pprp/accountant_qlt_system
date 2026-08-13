import type { Branch, BranchForm } from '../../types/model'
import type { FormsState } from '@/modules/Core/types/model/forms'

export interface Props {
  selectedItem?: Branch
  formState: FormsState
  loading: boolean
}

export interface Emits {
  (event: 'submit', values: BranchForm): void
}
