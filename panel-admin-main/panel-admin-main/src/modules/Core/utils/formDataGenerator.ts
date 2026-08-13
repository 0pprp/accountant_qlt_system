export function createFormData<T extends Record<string, unknown>>(
  payload: T
): FormData {
  const formData = new FormData()

  for (const property in payload) {
    const value = payload[property]

    if (value === undefined || value === null) {
      continue
    }

    // Handle arrays (including attachments and other arrays)
    if (Array.isArray(value)) {
      handleArrayProperty(formData, property, value)
    }
    // Handle primitive types and Blob
    else if (
      typeof value === 'string' ||
      typeof value === 'number' ||
      typeof value === 'boolean' ||
      value instanceof Blob
    ) {
      const serverPropertyName = toPascalCase(property)
      formData.append(serverPropertyName, value.toString())
    }
    // Handle nested objects
    else if (typeof value === 'object') {
      handleNestedObject(formData, property, value as Record<string, unknown>)
    }
  }

  return formData
}

function handleArrayProperty(
  formData: FormData,
  propertyName: string,
  array: unknown[]
): void {
  const serverPropertyName = toPascalCase(propertyName)

  array.forEach((item, index) => {
    if (typeof item === 'object' && item !== null) {
      // Handle array of objects
      const itemObj = item as Record<string, unknown>
      for (const subProperty in itemObj) {
        const subValue = itemObj[subProperty]
        if (subValue !== undefined && subValue !== null) {
          const serverSubPropertyName = toPascalCase(subProperty)

          // Handle File objects specially
          if (subValue instanceof File) {
            formData.append(
              `${serverPropertyName}[${index}].${serverSubPropertyName}`,
              subValue
            )
          } else if (
            typeof subValue === 'string' ||
            typeof subValue === 'number' ||
            typeof subValue === 'boolean'
          ) {
            formData.append(
              `${serverPropertyName}[${index}].${serverSubPropertyName}`,
              subValue.toString()
            )
          }
        }
      }
    } else if (
      typeof item === 'string' ||
      typeof item === 'number' ||
      typeof item === 'boolean'
    ) {
      // Handle array of primitives
      formData.append(`${serverPropertyName}[${index}]`, item.toString())
    }
  })
}

function handleNestedObject(
  formData: FormData,
  propertyName: string,
  obj: Record<string, unknown>
): void {
  const serverPropertyName = toPascalCase(propertyName)

  for (const subProperty in obj) {
    const subValue = obj[subProperty]
    if (subValue !== undefined && subValue !== null) {
      const serverSubPropertyName = toPascalCase(subProperty)

      if (
        typeof subValue === 'string' ||
        typeof subValue === 'number' ||
        typeof subValue === 'boolean'
      ) {
        formData.append(
          `${serverPropertyName}.${serverSubPropertyName}`,
          subValue.toString()
        )
      }
    }
  }
}

function toPascalCase(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1)
}
