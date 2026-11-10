/// Pure rating engine — same bands as accountant / SalesHaider phantom rating.
abstract final class CustomerRatingCalculator {
  static const excellentMaxDays = 150;
  static const goodMaxDays = 160;
  static const weakMaxDaysExclusive = 180;
  static const laggingMaxDaysExclusive = 200;

  static const excellentRate = 1000.0 / 150.0;
  static const goodRate = 6.25;
  static const weakRate = 1000.0 / 180.0;
  static const laggingMinRate = 1000.0 / 200.0;

  static final DateTime _floor = DateTime(2026, 5, 1);
  static final DateTime _boostEnd = DateTime(2026, 8, 31);

  static DateTime adjustSaleDate(DateTime d) {
    final day = DateTime(d.year, d.month, d.day);
    if (day.isBefore(_floor)) return _floor;
    if (!day.isAfter(_boostEnd)) return day.add(const Duration(days: 30));
    return day;
  }

  static int diffDaysInclusive(DateTime from, DateTime to) =>
      to.difference(from).inDays + 1;

  static int classifySettledDays(int days) {
    if (days <= excellentMaxDays) return 10;
    if (days <= goodMaxDays) return 5;
    if (days < weakMaxDaysExclusive) return 0;
    if (days < laggingMaxDaysExclusive) return -5;
    return -10;
  }

  static int classifyActiveRate(double rate) {
    if (rate >= excellentRate) return 10;
    if (rate >= goodRate) return 5;
    if (rate >= weakRate) return 0;
    if (rate >= laggingMinRate) return -5;
    return -10;
  }

  static DateTime? parseDate(String? raw) {
    if (raw == null || raw.trim().isEmpty) return null;
    final t = raw.trim();
    final dt = DateTime.tryParse(t);
    if (dt != null) return DateTime(dt.year, dt.month, dt.day);
    final parts = t.split(RegExp(r'[T\s]')).first.split(RegExp(r'[-/.]'));
    if (parts.length != 3) return null;
    final a = int.tryParse(parts[0]);
    final b = int.tryParse(parts[1]);
    final c = int.tryParse(parts[2]);
    if (a == null || b == null || c == null) return null;
    if (parts[0].length == 4) return DateTime(a, b, c);
    if (parts[2].length == 4) return DateTime(c, b, a);
    return null;
  }

  /// Null when sale data is insufficient.
  static int? scoreFor({
    required double amountTotalSales,
    required double receiptsTotal,
    required double amountRemaining,
    required DateTime? dateSaleDevice,
    DateTime? lastPaymentDate,
    required DateTime asOf,
  }) {
    if (dateSaleDevice == null || amountTotalSales <= 0) return null;
    if (amountRemaining < 0) return 0;

    final saleDate = adjustSaleDate(dateSaleDevice);
    final asOfDay = DateTime(asOf.year, asOf.month, asOf.day);

    if (amountRemaining == 0) {
      if (lastPaymentDate == null) return 0;
      final days = diffDaysInclusive(saleDate, lastPaymentDate);
      if (days <= 0) return 0;
      return classifySettledDays(days);
    }

    final daysSinceSale = diffDaysInclusive(saleDate, asOfDay);
    if (daysSinceSale <= 0) return 0;
    final rate =
        receiptsTotal * 1000.0 / (amountTotalSales * daysSinceSale);
    return classifyActiveRate(rate);
  }

  /// Cohort: remaining > 0 and last payment within 30 days.
  static bool inOverallCohort({
    required double remaining,
    required DateTime? lastPayment,
    required DateTime asOf,
  }) {
    if (remaining <= 0) return false;
    if (lastPayment == null) return false;
    final cutoff = asOf.subtract(const Duration(days: 30));
    return !lastPayment.isBefore(cutoff);
  }

  static double averageScore(Iterable<int> scores) {
    final list = scores.toList();
    if (list.isEmpty) return 0;
    return list.reduce((a, b) => a + b) / list.length;
  }

  static String trendArrow(double current, double previous, {double eps = 0.05}) {
    final d = current - previous;
    if (d > eps) return 'up';
    if (d < -eps) return 'down';
    return 'flat';
  }

  static String formatAverage(double average) {
    final rounded = double.parse(average.toStringAsFixed(2));
    if (rounded == rounded.roundToDouble()) return '${rounded.round()}';
    return rounded.toStringAsFixed(2);
  }

  static String levelName(int score) {
    switch (score) {
      case 10:
        return 'ممتاز';
      case 5:
        return 'جيد';
      case 0:
        return 'ضعيف';
      case -5:
        return 'متلكئ';
      default:
        return 'سيء';
    }
  }
}
