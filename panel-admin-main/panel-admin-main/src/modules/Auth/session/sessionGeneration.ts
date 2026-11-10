/**
 * أجيال الجلسة والفرع — نقية وقابلة للاختبار.
 * جيل الجلسة يمنع تطبيق نتائج حساب سابق على حساب لاحق.
 * جيل الفرع يمنع عرض نتيجة فرع سابق ضمن الفرع الجديد دون خلطه بهوية الحساب.
 */
export function nextGeneration(current: number): number {
  return current + 1
}

export function isStaleGeneration(
  startedAt: number,
  current: number
): boolean {
  return startedAt !== current
}
