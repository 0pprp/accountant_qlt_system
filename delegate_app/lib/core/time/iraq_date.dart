/// Baghdad business day helpers (UTC+3; day rolls at 03:00 Baghdad).
abstract final class IraqDate {
  static DateTime baghdadNow([DateTime? utcNow]) {
    final u = (utcNow ?? DateTime.now().toUtc());
    return u.add(const Duration(hours: 3));
  }

  static DateTime businessDay([DateTime? utcNow]) {
    final b = baghdadNow(utcNow);
    final adjusted = b.hour < 3 ? b.subtract(const Duration(days: 1)) : b;
    return DateTime(adjusted.year, adjusted.month, adjusted.day);
  }

  static bool isSameBusinessDay(DateTime aUtc, DateTime bUtc) {
    final a = businessDay(aUtc.toUtc());
    final b = businessDay(bUtc.toUtc());
    return a.year == b.year && a.month == b.month && a.day == b.day;
  }

  static String isoDate([DateTime? utcNow]) {
    final d = businessDay(utcNow);
    final m = d.month.toString().padLeft(2, '0');
    final day = d.day.toString().padLeft(2, '0');
    return '${d.year}-$m-$day';
  }

  /// Calendar day in Baghdad (date-only) for rating math.
  static DateTime calendarDate([DateTime? utcNow]) {
    final b = baghdadNow(utcNow);
    return DateTime(b.year, b.month, b.day);
  }
}
