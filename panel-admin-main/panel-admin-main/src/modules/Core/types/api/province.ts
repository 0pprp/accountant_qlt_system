import type { Province } from '../model/province'

export type ProvinceListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<Province>
>
