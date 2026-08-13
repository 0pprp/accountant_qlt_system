import PrimeVue, { type PrimeVueConfiguration } from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import { type App } from 'vue'
import { PASS_THROUGH_COMPONENT_STYLES } from './primevue.constants'

export const PrimeVueConfig = {
  unstyled: true,
  ripple: false,
  pt: PASS_THROUGH_COMPONENT_STYLES,
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: 'none',
      cssLayer: false,
      prefix: 'p',
    },
  },
} satisfies PrimeVueConfiguration

export default function registerPrimeVue(app: App) {
  app.use(PrimeVue, PrimeVueConfig)
}
