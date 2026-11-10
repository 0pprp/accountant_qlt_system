import { describe, expect, it } from 'vitest'
import {
  FOLLOWER_WORK_SECTIONS,
  getImplementedSections,
} from '../sections'

describe('FollowerWork sections', () => {
  it('كل الأقسام الثمانية ready', () => {
    expect(FOLLOWER_WORK_SECTIONS).toHaveLength(8)
    expect(getImplementedSections()).toHaveLength(8)
    expect(
      FOLLOWER_WORK_SECTIONS.filter((s) => s.status === 'not_implemented')
    ).toHaveLength(0)
    for (const id of [
      'sales',
      'declining',
      'notes',
      'old-customers',
      'delegate-notes',
      'collection',
      'live-map',
      'routes',
    ]) {
      expect(FOLLOWER_WORK_SECTIONS.find((s) => s.id === id)!.status).toBe(
        'ready'
      )
    }
  })

  it('صلاحيات الأقسام مستقلة وليست FollowerWork', () => {
    for (const s of FOLLOWER_WORK_SECTIONS) {
      expect(s.permissionModule).not.toBe('FollowerWork')
      expect(s.permissionAction).toBe('Read')
    }
  })
})
