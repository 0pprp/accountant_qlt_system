import { defineStore } from 'pinia'
import type { SafeCurrent } from '../types/model'

interface State {
  safeData: SafeCurrent | null
  isLoading: boolean
  error: string | null
}

function initState(): State {
  return {
    safeData: null,
    isLoading: false,
    error: null,
  }
}

export const useSafeStore = defineStore('safe', {
  state: () => ({ ...initState() }),

  getters: {
    safeId: (state) => state.safeData?.id || null,
    hasSafeData: (state) => state.safeData !== null,
    safeName: (state) => state.safeData?.name || '',
    remainingCashAmount: (state) => state.safeData?.remainingCashAmount || 0,
    netBalance: (state) => state.safeData?.netBalance || 0,
    totalAmount: (state) => state.safeData?.totalAmount || 0,
    totalUndeliveredCashAmount: (state) =>
      state.safeData?.totalUndeliveredCashAmount || null,
    totalBranchesRemainingCashAmount: (state) =>
      state.safeData?.totalBranchesRemainingCashAmount || null,
  },

  actions: {
    setSafeData(data: SafeCurrent) {
      this.safeData = data
      this.error = null
    },

    setLoading(loading: boolean) {
      this.isLoading = loading
    },

    setError(error: string) {
      this.error = error
    },

    clearSafeData() {
      this.safeData = null
      this.error = null
    },

    updateRemainingCashAmount(amount: number) {
      if (this.safeData) {
        this.safeData.remainingCashAmount = amount
      }
    },

    reset() {
      this.$reset()
    },
  },
})

export default useSafeStore
