export function formattedDate(date: string): string {
  const datePart = date.split('T')[0]

  return datePart.replace(/-/g, '/')
}

export function formattedDateTime(date: string): string {
  const [datePart, timePart] = date.split('T')
  if (!datePart) return ''

  const [year, month, day] = datePart.split('-')
  const formattedDatePart = `${Number(day)}/${Number(month)}/${year}`

  if (!timePart) return formattedDatePart

  const [hours, minutes] = timePart.split(':')
  return `${formattedDatePart} - ${hours}:${minutes}`
}
