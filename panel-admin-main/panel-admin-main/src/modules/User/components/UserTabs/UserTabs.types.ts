import type { UserInfo } from '../../types/model'

export enum FormMode {
  IsView,
  IsCreate,
  IsUpdate,
}

export interface Props {
  userInfo: UserInfo
}
