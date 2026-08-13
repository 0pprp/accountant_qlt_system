export const PASS_THROUGH_COMPONENT_STYLES = {
  button: {
    root: 'btn',
    label: 'text-white',
    icon: 'text-white',
    size: {
      large: 'p-60',
      small: 'p-2',
      medium: 'p-4',
    },
  },
  inputText: {
    root: 'border-1 focus:ring-0 focus:border-transparent',
    label: 'text-md',
    input: 'text-md',
    icon: 'text-md',
  },
  chip: {
    root: 'chip',
    removeIcon: 'w-4 h-4 aspect-square rounded-full cursor-pointer',
  },

  dataTable: {
    root: 'relative',
    wrapper: 'overflow-auto',
    table: 'w-full table-auto',
    thead: 'sticky top-0 z-10 bg-white',
    tbody: 'divide-y divide-gray-200',
    tfoot: 'bg-gray-50',
    headerRow: 'border-b border-gray-200',
    bodyRow: 'hover:bg-gray-50 transition-colors duration-150',
    columnHeaderContent: 'flex items-center justify-between',
    columnTitle: 'font-medium text-gray-900',
    sort: 'ml-2 flex-none rounded text-gray-400 group-hover:visible',
    sortIcon: 'h-5 w-5',
    checkboxWrapper: 'relative flex items-start',
    checkbox: 'h-4 w-4 rounded border-gray-300',
    headerCheckbox: 'h-4 w-4 rounded border-gray-300',
  },

  skeleton: {
    root: 'animate-pulse',
  },

  dropdown: {
    root: 'relative inline-block text-left w-full',
    input:
      'block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500',
    trigger: 'absolute inset-y-0 right-0 flex items-center pr-2',
    panel:
      'absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5',
    item: 'relative cursor-default select-none py-2 pl-3 pr-9 hover:bg-gray-100 transition-colors duration-150',
  },
}

export const PERFORMANCE_OPTIMIZATIONS = {
  // Virtual scrolling threshold
  virtualScrollThreshold: 100,

  // Debounce delays
  searchDebounce: 300,
  resizeDebounce: 100,

  // Animation preferences
  reducedMotion: false,

  // Memory management
  maxCacheSize: 1000,
  cleanupInterval: 30000,
}
