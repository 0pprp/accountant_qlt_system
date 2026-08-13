import { REGEX } from '../constants'

export function blockNegativeNumbers(event: KeyboardEvent) {
  const disAllowedControlKeys = [
    'Backspace',
    'Tab',
    'Enter',
    'Escape',
    'ArrowLeft',
    'ArrowRight',
    'ArrowUp',
    'ArrowDown',
    'Delete',
    'Home',
    'End',
  ]

  if (event.ctrlKey || event.metaKey || event.altKey) {
    return
  }

  if (disAllowedControlKeys.includes(event.key)) {
    return
  }

  if (REGEX.DIGIT_REGEX.test(event.key)) {
    return
  }

  event.preventDefault()
}
