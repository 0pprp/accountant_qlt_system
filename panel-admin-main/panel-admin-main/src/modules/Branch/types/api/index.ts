import type { Branch, BranchForm } from '../model'
import type { Warehouse } from '@/modules/Warehouse/types/model'

export type BranchCreatePayload = BranchForm

export type BranchListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<Branch>
>

export type BranchWarehousesServerSuccessResponse = Array<Warehouse>
