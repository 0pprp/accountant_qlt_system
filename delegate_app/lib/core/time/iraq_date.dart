/// Baghdad business day helpers (UTC+3; day rolls at 03:00 Baghdad).
abstract final class IraqDate {
  static const iraqOffset = Duration(hours: 3);
  static const businessDayStartHour = 3;

  static DateTime baghdadNow([DateTime? utcNow]) {
    final u = (utcNow ?? DateTime.now().toUtc());
    return u.add(iraqOffset);
  }

  static DateTime businessDay([DateTime? utcNow]) {
    final b = baghdadNow(utcNow);
    final adjusted = b.hour < businessDayStartHour
        ? b.subtract(const Duration(days: 1))
        : b;
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

  /// Duration until the next Baghdad 03:00 (UTC 00:00 same civil date).
  static Duration delayUntilNextBusinessDayStart([DateTime? utcNow]) {
    final nowUtc = (utcNow ?? DateTime.now()).toUtc();
    final iraq = baghdadNow(nowUtc);
    final DateTime nextStartUtc;
    if (iraq.hour < businessDayStartHour) {
      nextStartUtc = DateTime.utc(iraq.year, iraq.month, iraq.day);
    } else {
      final nextCal = DateTime.utc(iraq.year, iraq.month, iraq.day)
          .add(const Duration(days: 1));
      nextStartUtc = DateTime.utc(nextCal.year, nextCal.month, nextCal.day);
    }
    final d = nextStartUtc.difference(nowUtc);
    return d.isNegative ? Duration.zero : d;
  }
}
