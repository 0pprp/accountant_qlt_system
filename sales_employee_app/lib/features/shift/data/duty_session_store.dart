import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Local duty (دوام) gate — expires after Baghdad midnight (new calendar day).
class DutySessionStore {
  static const _kActive = 'duty_active';
  static const _kStartedAt = 'duty_started_at';
  static const _kDutyDate = 'duty_baghdad_date';

  String _todayKey() {
    final b = baghdadNow();
    final y = b.year.toString().padLeft(4, '0');
    final m = b.month.toString().padLeft(2, '0');
    final d = b.day.toString().padLeft(2, '0');
    return '$y-$m-$d';
  }

  /// Active only if flagged AND started on today's Baghdad date (after 12:00 ص = new day).
  Future<bool> isActive() async {
    final p = await SharedPreferences.getInstance();
    if (!(p.getBool(_kActive) ?? false)) return false;
    final date = p.getString(_kDutyDate);
    if (date != _todayKey()) {
      await markEnded();
      return false;
    }
    return true;
  }

  Future<String?> startedAt() async {
    if (!await isActive()) return null;
    final p = await SharedPreferences.getInstance();
    return p.getString(_kStartedAt);
  }

  Future<String?> dutyDate() async {
    final p = await SharedPreferences.getInstance();
    return p.getString(_kDutyDate);
  }

  Future<void> markStarted() async {
    final p = await SharedPreferences.getInstance();
    await p.setBool(_kActive, true);
    await p.setString(_kStartedAt, DateTime.now().toUtc().toIso8601String());
    await p.setString(_kDutyDate, _todayKey());
  }

  Future<void> markEnded() async {
    final p = await SharedPreferences.getInstance();
    await p.setBool(_kActive, false);
    await p.remove(_kStartedAt);
    await p.remove(_kDutyDate);
  }
}
