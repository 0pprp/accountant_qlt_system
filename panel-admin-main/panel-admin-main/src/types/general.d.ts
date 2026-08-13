import type { AxiosError } from 'axios'
import type { Router } from 'vue-router'

declare global {
  type ModuleRouteAdder = (router: Router) => void

  interface ModuleLocalization {
    [key: string]: unknown
  }

  interface SubModule extends Module {
    [key: string]: Module
  }

  interface Module {
    router?: ModuleRouteAdder
    localization?: ModuleLocalization
    subModules?: SubModule
  }

  interface Modules {
    [key: string]: Module
  }

  type AppLayouts = 'App' | 'Default' | 'Auth' | 'Error'

  interface Navigator extends Navigator {
    userLanguage: string
    standalone: boolean
  }

  type Maybe<T> = T | null | undefined

  type Nullable<T> = T | null

  interface ServerResponse<TError = object> {
    data: unknown
    error: Nullable<TError[]>
  }

  interface ServerSuccessResponse<TData> extends ServerResponse<null> {
    data: TData
  }

  type ErrorLevel = 'Error' | 'Warning'

  interface Error {
    [key: string]: string
  }

  interface ServerFailedResponse<TError = object> extends ServerResponse {
    data: null
    errors: TError[]
    isSuccess: false
  }

  type AxiosCustomError = AxiosError<ServerFailedResponse<Error>>

  interface Pagination {
    pageIndex: number
    pageSize: number
    totalCount: number
  }

  interface Paginated<T> extends Pagination {
    items: T
  }

  type ServerSuccessPaginatedResponse<T> = Paginated<T>
}

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $t: (
      key: string,
      named?: Record<string, unknown>,
      plural?: number
    ) => string
  }
}
