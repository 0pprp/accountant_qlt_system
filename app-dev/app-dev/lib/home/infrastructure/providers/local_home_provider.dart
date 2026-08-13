import 'dart:convert';

import 'package:hive_ce/hive.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class LocalHomeProvider {
  late final Box _box;

  final _homeBoxKey = 'homeBoxKey';
  final _isDarkMode = 'isDarkMode';
  final _dailyReportKey = 'dailyReportKey';
  final _lastFetchDateKey = 'lastFetchDateKey';

  Future<void> ensureInitialized() async {
    _box = await Hive.openBox(_homeBoxKey);
  }

  Future<void> saveDailyReports(List<OrderList> dailyReport) async {
    await _box.put(_dailyReportKey, jsonEncode(dailyReport.map((e) => e.toJson()).toList()));
    await _box.put(_lastFetchDateKey, DateTime.now().toIso8601String());
  }

  List<OrderList>? getDailyReports() {
    final data = _box.get(_dailyReportKey);

    if (data == null) return null;

    final json = jsonDecode(data) as List;
    return json.map((e) => OrderList.fromJson(e)).toList();
  }

  bool isDataFromToday() {
    final savedDateString = _box.get(_lastFetchDateKey);

    if (savedDateString == null) return false;

    final savedDate = DateTime.parse(savedDateString);
    final today = DateTime.now();

    // Check if the saved date is from today
    return savedDate.year == today.year && savedDate.month == today.month && savedDate.day == today.day;
  }

  Future<void> saveIsDarkMode(bool isDarkMode) {
    return _box.put(_isDarkMode, isDarkMode);
  }

  bool getIsDarkMode() {
    return _box.get(_isDarkMode) ?? false;
  }

  Future clear() => _box.clear();
}
