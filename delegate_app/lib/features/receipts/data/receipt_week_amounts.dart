/// نسخ مبسّط من ReceiptWeekAmounts في SalesHaider.
class ReceiptWeekAmounts {
  static const arabicDayLabels = <String>[
    'اليوم',
    'أمس',
    'قبل يومين',
    'قبل 3 أيام',
    'قبل 4 أيام',
    'قبل 5 أيام',
    'قبل 6 أيام',
  ];

  static List<String> ensureDayLabels(List<String?>? raw) {
    final out = <String>[];
    for (var i = 0; i < 7; i++) {
      final v = (raw != null && i < raw.length) ? (raw[i] ?? '').trim() : '';
      out.add(v.isNotEmpty ? v : arabicDayLabels[i]);
    }
    return out;
  }

  static List<double> ensureAmounts(List<double?>? raw) {
    final out = <double>[];
    for (var i = 0; i < 7; i++) {
      final v = (raw != null && i < raw.length) ? (raw[i] ?? 0) : 0;
      out.add(v.isFinite ? v.toDouble() : 0.0);
    }
    return out;
  }

  static List<double> guaranteeTodayShowsPayment({
    required List<double> weekAmounts,
    required double amountPush,
  }) {
    final amounts = ensureAmounts(weekAmounts);
    if (amountPush > 0 && amounts[0] + 0.0001 < amountPush) {
      amounts[0] = amountPush;
    }
    return amounts;
  }

  static String printedReceiptNumber(String? countReceiptDevice) {
    final raw = (countReceiptDevice ?? '').trim();
    if (raw.isEmpty) return '1';
    final n = int.tryParse(raw);
    if (n == null) return raw;
    return (n + 1).toString();
  }
}
