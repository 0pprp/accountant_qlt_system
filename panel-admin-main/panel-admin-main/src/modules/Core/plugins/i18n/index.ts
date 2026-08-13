import { createI18n } from 'vue-i18n'
import numberFormats from './rules/numbers'
import datetimeFormats from './rules/datetime'
import pluralRules from './rules/pluralization'
import messages from '@/modules/Core/localization'

export * from './i18n.types'

export default createI18n({
  legacy: false,
  locale: 'ar',
  globalInjection: true,
  messages: {
    ...messages,
  },
  pluralRules,
  numberFormats,
  datetimeFormats,
  fallbackLocale: 'ar',
})
