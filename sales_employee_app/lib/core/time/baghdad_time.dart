DateTime baghdadNow() {
  return DateTime.now().toUtc().add(const Duration(hours: 3));
}

DateTime toBaghdad(DateTime dt) {
  return dt.isUtc
      ? dt.add(const Duration(hours: 3))
      : dt.toUtc().add(const Duration(hours: 3));
}

DateTime? parseFlexibleDateTime(String? raw) {
  if (raw == null) return null;
  final s = raw.trim();
  if (s.isEmpty) return null;
  final parsed = DateTime.tryParse(s);
  if (parsed != null) return parsed;
  return null;
}

/// تاريخ فقط: 2026/10/31
String formatBaghdadDateOnly(DateTime dt) {
  final local = toBaghdad(dt);
  final y = local.year.toString().padLeft(4, '0');
  final m = local.month.toString().padLeft(2, '0');
  final d = local.day.toString().padLeft(2, '0');
  return '$y/$m/$d';
}

/// وقت 12 ساعة مع دقائق + صباحاً/مساءً (منفصل عن التاريخ)
String formatBaghdadTime12(DateTime dt) {
  final local = toBaghdad(dt);
  final isAm = local.hour < 12;
  var h = local.hour % 12;
  if (h == 0) h = 12;
  final min = local.minute.toString().padLeft(2, '0');
  final period = isAm ? 'صباحاً' : 'مساءً';
  return '$h:$min $period';
}

String formatBaghdadDateTime(DateTime dt) {
  return '${formatBaghdadDateOnly(dt)} ${formatBaghdadTime12(dt)}';
}

({String date, String time}) splitBaghdadDateTime(String? raw) {
  final dt = parseFlexibleDateTime(raw);
  if (dt == null) {
    return (date: raw?.trim().isNotEmpty == true ? raw!.trim() : '—', time: '');
  }
  return (date: formatBaghdadDateOnly(dt), time: formatBaghdadTime12(dt));
}
