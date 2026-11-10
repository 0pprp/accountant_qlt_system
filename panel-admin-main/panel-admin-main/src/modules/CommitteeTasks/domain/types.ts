export interface CommitteeTaskRow {
  id: number
  body: string
  isEnabled: boolean
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
  updatedByName: string | null
  expectedVersion: number
}

export interface CommitteeTaskUpsertBody {
  body: string
  isEnabled: boolean
  expectedVersion?: number
}
