import { describe, expect, it } from 'vitest'
import { hubOriginLabel, resolveSalesRequestHubUrl } from '../hubUrl'

describe('resolveSalesRequestHubUrl', () => {
  it('dev + loopback uses page origin via vite proxy', () => {
    expect(
      resolveSalesRequestHubUrl('http://127.0.0.1:4200/api/v1/', {
        isDev: true,
        pageOrigin: 'http://127.0.0.1:5174',
      })
    ).toBe('http://127.0.0.1:5174/hubs/sales-requests')
  })

  it('prod build keeps api origin even for loopback', () => {
    expect(
      resolveSalesRequestHubUrl('http://127.0.0.1:4200/api/v1/', {
        isDev: false,
        pageOrigin: 'http://127.0.0.1:5174',
      })
    ).toBe('http://127.0.0.1:4200/hubs/sales-requests')
  })

  it('remote api keeps api origin in dev', () => {
    expect(
      resolveSalesRequestHubUrl('https://api.qalataldhaman.com/api/v1/', {
        isDev: true,
        pageOrigin: 'http://127.0.0.1:5174',
      })
    ).toBe('https://api.qalataldhaman.com/hubs/sales-requests')
  })

  it('rejects empty', () => {
    expect(() => resolveSalesRequestHubUrl('')).toThrow(/missing/i)
  })

  it('hubOriginLabel hides path', () => {
    expect(hubOriginLabel('http://127.0.0.1:5174/hubs/sales-requests')).toBe(
      'http://127.0.0.1:5174'
    )
  })
})
