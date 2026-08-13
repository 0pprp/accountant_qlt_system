/**
 * Serializes axios params with support for sortCriteria array of objects
 * Formats sortCriteria as sortCriteria[0].property instead of sortCriteria[0][property]
 */
export function serializeParamsWithSortCriteria(
  params: Record<string, unknown>
): string {
  const searchParams = new URLSearchParams()

  Object.entries(params).forEach(([key, value]) => {
    if (key === 'sortCriteria' && Array.isArray(value)) {
      // Serialize sortCriteria array of objects as sortCriteria[0].property format
      value.forEach((item, index) => {
        if (item && typeof item === 'object') {
          Object.entries(item).forEach(([propKey, propValue]) => {
            if (propValue !== null && propValue !== undefined) {
              searchParams.append(
                `${key}[${index}].${propKey}`,
                String(propValue)
              )
            }
          })
        }
      })
    } else if (
      key === 'filter' &&
      value &&
      typeof value === 'object' &&
      !Array.isArray(value)
    ) {
      Object.entries(value as Record<string, unknown>).forEach(
        ([propKey, propValue]) => {
          if (propValue !== null && propValue !== undefined) {
            searchParams.append(`filter.${propKey}`, String(propValue))
          }
        }
      )
    } else if (Array.isArray(value)) {
      value.forEach((item) => {
        if (item !== null && item !== undefined) {
          searchParams.append(key, String(item))
        }
      })
    } else if (value !== null && value !== undefined) {
      searchParams.append(key, String(value))
    }
  })

  return searchParams.toString()
}
