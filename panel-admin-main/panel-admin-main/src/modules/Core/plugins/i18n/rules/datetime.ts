import type { I18nOptions } from 'vue-i18n'

const datetimeFormats: I18nOptions['datetimeFormats'] = {
  ar: {
    shortFormat: {
      dateStyle: 'short',
    },
    longFormat: {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      weekday: 'short',
      hour: 'numeric',
      minute: 'numeric',
    },
  },
}

export default datetimeFormats
