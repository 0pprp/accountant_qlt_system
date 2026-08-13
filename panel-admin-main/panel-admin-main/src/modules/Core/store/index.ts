import { defineStore } from 'pinia'
import { type StatementDialog } from '../types/model/toast'

export const useToastStore = defineStore('messages', {
  state: () => ({
    messages: null as StatementDialog | null,
  }),

  actions: {
    setMassage(data: StatementDialog) {
      this.messages = { ...data, isOpen: true }
    },

    clearToast() {
      this.messages = null
    },
  },
})
