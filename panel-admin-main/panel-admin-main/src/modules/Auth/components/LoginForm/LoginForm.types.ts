import type { AuthLoginForm } from '../../types/model'

export interface Props {
  loading: boolean
}

export interface Emits {
  (event: 'submit', values: AuthLoginForm): void
}
