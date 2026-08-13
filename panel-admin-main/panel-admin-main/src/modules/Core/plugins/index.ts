import type { App } from 'vue'
import { registerAxios } from './axios'
import { vueQuery } from './vue-query'
import { registerLayouts } from './registerLayouts'
import i18n from './i18n'
import registerPrimeVue from './primevue/index.ts'
import router from '@/router'
import pinia from '@/store'

export function registerPlugins(app: App) {
  registerPrimeVue(app)
  registerAxios(app)
  registerLayouts(app)
  app.use(pinia).use(router).use(i18n).use(vueQuery)
}
