import 'package:intl/intl.dart';

abstract final class IraqTime {
  static const offset = Duration(hours: 3);
  static final _hm = DateFormat('HH:mm', 'en');
  static final _date = DateFormat('yyyy/MM/dd', 'en');
  static final _key = DateFormat('yyyy-MM-dd', 'en');

  static DateTime toIraq(DateTime t) {
    final u = t.toUtc().add(offset);
    return DateTime(u.year, u.month, u.day, u.hour, u.minute, u.second);
  }

  static DateTime now() => toIraq(DateTime.now());

  static DateTime today() {
    final n = now();
    return DateTime(n.year, n.month, n.day);
  }

  static DateTime? parse(Object? raw) {
    if (raw == null) return null;
    final s = '$raw'.trim();
    if (s.isEmpty) return null;
    final parsed = DateTime.tryParse(s);
    if (parsed == null) return null;
    if (parsed.isUtc || RegExp(r'([zZ]|[+-]\d{2}:?\d{2})$').hasMatch(s)) {
      return parsed.toUtc();
    }
    if (RegExp(r'^\d{4}-\d{2}-\d{2}$').hasMatch(s)) return parsed;
    return DateTime.utc(
      parsed.year,
      parsed.month,
      parsed.day,
      parsed.hour,
      parsed.minute,
      parsed.second,
    );
  }

  static String hm(DateTime? utc) => utc == null ? '—' : _hm.format(toIraq(utc));

  static String date(DateTime? utc) =>
      utc == null ? '—' : _date.format(toIraq(utc));

  static String dateOnly(DateTime? d) => d == null ? '—' : _date.format(d);

  static String dateTime(DateTime? utc) =>
      utc == null ? '—' : '${date(utc)} · ${hm(utc)}';

  static String smart(DateTime? utc) {
    if (utc == null) return '—';
    final local = toIraq(utc);
    final t = today();
    final day = DateTime(local.year, local.month, local.day);
    if (day == t) return _hm.format(local);
    if (day == t.subtract(const Duration(days: 1))) {
      return 'أمس ${_hm.format(local)}';
    }
    return _date.format(local);
  }

  static String key(DateTime day) => _key.format(day);

  static String minutesAsHm(int minutes) {
    final m = minutes < 0 ? 0 : minutes;
    final h = m ~/ 60;
    final r = m % 60;
    return '$h:${r.toString().padLeft(2, '0')}';
  }
}
